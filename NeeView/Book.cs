// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// 本
    /// </summary>
    public class Book : IDisposable
    {
        #region 開発用

        public static TraceSource Log = Logger.CreateLogger(nameof(Book));

        #endregion

        #region Fields

        // シリアル
        public static int _serial = 0;

        // テンポラリコンテンツ用ゴミ箱
        public TrashBox _trashBox = new TrashBox();

        // Disposed
        private volatile bool _isDisposed;

        // 先読み可能フラグ
        private bool _canPreLoad = true;

        // 先読み解除フラグ
        private int _canPreLoadCount;

        // アーカイバーコレクション
        // Dispose処理のために保持
        private List<Archiver> _archivers = new List<Archiver>();

        /// <summary>
        /// 要求中の表示範囲
        /// </summary>
        volatile PageDirectionalRange _viewPageRange;

        /// <summary>
        /// ページ要求発行者.
        /// スライダーの挙動制御用
        /// </summary>
        volatile object _viewPageSender;

        // 表示ページコンテキスト
        private volatile ViewPageCollection _viewPageCollection = new ViewPageCollection();

        // 先読みページコンテキスト
        private volatile ViewPageCollection _nextPageCollection = new ViewPageCollection();

        // リソースを保持しておくページ
        private List<Page> _keepPages = new List<Page>();

        // サムネイル寿命管理
        private PageThumbnailPool _thumbnaulPool = new PageThumbnailPool();


        #endregion

        #region Constructors

        /// <summary>
        /// constructor
        /// </summary>
        public Book()
        {
            Serial = ++_serial;
        }

        #endregion

        #region Events

        // 現在ページ変更(ページ番号)
        // タイトル、スライダーの更新を要求
        public event EventHandler<PageChangedEventArgs> PageChanged;

        // 表示コンテンツ変更
        // 表示の更新を要求
        public event EventHandler<ViewPageCollectionChangedEventArgs> ViewContentsChanged;

        // 先読みコンテンツ変更
        public event EventHandler<ViewPageCollectionChangedEventArgs> NextContentsChanged;

        // ページ終端を超えて移動しようとした
        // 次の本への移動を要求
        public event EventHandler<PageTerminatedEventArgs> PageTerminated;

        // 再読み込みを要求
        public event EventHandler DartyBook;

        // ソートされた
        public event EventHandler PagesSorted;

        // サムネイル更新
        public event EventHandler<PageChangedEventArgs> ThumbnailChanged;

        // ファイル削除された
        public event EventHandler<PageChangedEventArgs> PageRemoved;


        #endregion

        #region Properties

        // Log用 シリアル番号
        public int Serial { get; private set; }

        // 最初のコンテンツ表示フラグ
        public ManualResetEventSlim ContentLoaded { get; } = new ManualResetEventSlim();

        // 見つからなかった開始ページ名。通知用。
        public string NotFoundStartPage { get; private set; }

        // 横長ページを分割する
        private bool _isSupportedDividePage;
        public bool IsSupportedDividePage
        {
            get { return _isSupportedDividePage; }
            set
            {
                if (_isSupportedDividePage != value)
                {
                    _isSupportedDividePage = value;
                    RequestReflesh(this, false);
                }
            }
        }

        // 最初のページは単独表示
        private bool _isSupportedSingleFirstPage;
        public bool IsSupportedSingleFirstPage
        {
            get { return _isSupportedSingleFirstPage; }
            set
            {
                if (_isSupportedSingleFirstPage != value)
                {
                    _isSupportedSingleFirstPage = value;
                    RequestReflesh(this, false);
                }
            }
        }

        // 最後のページは単独表示
        private bool _isSupportedSingleLastPage;
        public bool IsSupportedSingleLastPage
        {
            get { return _isSupportedSingleLastPage; }
            set
            {
                if (_isSupportedSingleLastPage != value)
                {
                    _isSupportedSingleLastPage = value;
                    RequestReflesh(this, false);
                }
            }
        }

        // 横長ページは２ページとみなす
        private bool _isSupportedWidePage = true;
        public bool IsSupportedWidePage
        {
            get { return _isSupportedWidePage; }
            set
            {
                if (_isSupportedWidePage != value)
                {
                    _isSupportedWidePage = value;
                    RequestReflesh(this, false);
                }
            }
        }

        // 右開き、左開き
        private PageReadOrder _bookReadOrder = PageReadOrder.RightToLeft;
        public PageReadOrder BookReadOrder
        {
            get { return _bookReadOrder; }
            set
            {
                if (_bookReadOrder != value)
                {
                    _bookReadOrder = value;
                    RequestReflesh(this, false);
                }
            }
        }

        // サブフォルダー読み込み
        private bool _isRecursiveFolder;
        public bool IsRecursiveFolder
        {
            get { return _isRecursiveFolder; }
            set
            {
                if (_isRecursiveFolder != value)
                {
                    _isRecursiveFolder = value;
                    DartyBook?.Invoke(this, null);
                }
            }
        }

        // 単ページ/見開き
        private PageMode _pageMode = PageMode.SinglePage;
        public PageMode PageMode
        {
            get { return _pageMode; }
            set
            {
                if (_pageMode != value)
                {
                    _pageMode = value;
                    RequestReflesh(this, false);
                }
            }
        }

        // ページ列
        private PageSortMode _sortMode = PageSortMode.FileName;
        public PageSortMode SortMode
        {
            get { return _sortMode; }
            set
            {
                if (_sortMode != value)
                {
                    _sortMode = value;
                    RequestSort(this);
                }
            }
        }

        // この本の場所
        // nullの場合、この本は無効
        public string Place { get; private set; }

        // この本のアーカイバ
        public Archiver Archiver { get; private set; }

        // メディアアーカイバ？
        public bool IsMedia => Archiver is MediaArchiver; 

        // 開始ページ
        public string StartEntry { get; private set; }

        // ページ コレクション
        public List<Page> Pages { get; private set; } = new List<Page>();

        // 表示されるページ番号(スライダー用)
        public int DisplayIndex { get; set; }

        // 読み込み対象外サブフォルダー数。リカーシブ確認に使用します。
        public int SubFolderCount { get; private set; }

        // マーカー
        public List<Page> Markers { get; private set; } = new List<Page>();


        // 表示ページ変更回数
        public int PageChangeCount { get; private set; }

        // 終端ページ表示
        public bool IsPageTerminated { get; private set; }

        #endregion

        #region Medhots

        #region 本の初期化

        /// <summary>
        /// フォルダーの読込
        /// </summary>
        /// <param name="path"></param>
        /// <param name="start"></param>
        /// <param name="option"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task LoadAsync(BookAddress address, BookLoadOption option, CancellationToken token)
        {
            try
            {
                Log.TraceEvent(TraceEventType.Information, Serial, $"Load: {address.Place}");
                Log.Flush();

                await LoadCoreAsync(address, option, token);
            }
            catch (Exception e)
            {
                Log.TraceEvent(TraceEventType.Warning, Serial, $"Load Failed: {e.Message}");
                Log.Flush();

                Dispose();
                throw;
            }
        }

        // 本読み込み
        public async Task LoadCoreAsync(BookAddress address, BookLoadOption option, CancellationToken token)
        {
            Debug.Assert(Place == null);
            ////Debug.WriteLine($"OPEN: {address.Place}, {address.EntryName}, {address.Archiver.Path}");

            // ソリッド書庫の事前展開を許可してアーカイバ再生性
            var archiver = ArchiverManager.Current.CreateArchiver(address.Archiver.Path, address.Archiver.Source, true, true);
            archiver.TempFile = archiver.TempFile ?? address.Archiver.TempFile; // TEMPファイル引き継ぎ

            var start = address.EntryName;

            // リカーシブオプションフラグ
            if (option.HasFlag(BookLoadOption.NotRecursive))
            {
                IsRecursiveFolder = false;
                option &= ~BookLoadOption.Recursive;
            }
            else if (option.HasFlag(BookLoadOption.Recursive))
            {
                IsRecursiveFolder = true;
            }

            // リカーシブフラグ
            if (IsRecursiveFolder)
            {
                option |= BookLoadOption.Recursive;
            }

            // 圧縮ファイル再帰
            if (!address.Archiver.IsFileSystem && option.HasFlag(BookLoadOption.ArchiveRecursive))
            {
                option |= BookLoadOption.Recursive;
            }

            PagePosition position = FirstPosition();
            int direction = 1;

            this.Archiver = archiver;
            _trashBox.Add(archiver);

            this.Pages = await ReadArchiveAsync2(archiver, option, token);


            // Pages initialize
            var prefix = GetPagesPrefix();
            foreach (var page in Pages)
            {
                page.Prefix = prefix;
                page.Loaded += Page_Loaded;
                page.Thumbnail.Touched += Thumbnail_Touched;
            }

            // 初期ソート
            Sort();

            // スタートページ取得
            if ((option & BookLoadOption.FirstPage) == BookLoadOption.FirstPage)
            {
                position = FirstPosition();
                direction = 1;
            }
            else if ((option & BookLoadOption.LastPage) == BookLoadOption.LastPage)
            {
                position = LastPosition();
                direction = -1;
            }
            else
            {
                int index = (start != null) ? Pages.FindIndex(e => e.FullPath == start) : 0;
                if (index < 0)
                {
                    this.NotFoundStartPage = start;
                }
                position = index >= 0 ? new PagePosition(index, 0) : FirstPosition();
                direction = 1;
            }

            // 開始ページ記憶
            StartEntry = Pages.Count > 0 ? Pages[position.Index].FullPath : null;

            // 有効化
            Place = archiver.FullPath;

            // 初期ページ設定
            RequestSetPosition(this, position, direction, true);
        }

        /// <summary>
        /// アーカイブファイルロック解除
        /// </summary>
        /// <returns></returns>
        public void Unlock()
        {
            var archivers = this.Pages.Select(e => e.Entry.Archiver).Distinct().Where(e => e != null);
            foreach (var archiver in archivers)
            {
                archiver.Unlock();
            }
        }

        /// <summary>
        /// サムネイル参照イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Thumbnail_Touched(object sender, EventArgs e)
        {
            var thumb = (Thumbnail)sender;
            _thumbnaulPool.Add(thumb);
        }


        /// <summary>
        /// ページ収集
        /// </summary>
        /// <param name="archiver"></param>
        /// <param name="option"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<List<Page>> ReadArchiveAsync2(Archiver archiver, BookLoadOption option, CancellationToken token)
        {
            try
            {
                var collection = new EntryCollection(archiver, option.HasFlag(BookLoadOption.Recursive), option.HasFlag(BookLoadOption.SupportAllFile));
                _trashBox.Add(collection);

                await collection.CollectAsync(token);

                SubFolderCount = collection.SkippedArchiveCount;

                return collection.Collection.Select(e => CreatePage(e)).ToList();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw;
            }
        }

        /// <summary>
        /// ページ作成
        /// </summary>
        /// <param name="entry">ファイルエントリ</param>
        /// <returns></returns>
        private Page CreatePage(ArchiveEntry entry)
        {
            Page page;

            if (entry.IsImage())
            {
                if (entry.Archiver is MediaArchiver)
                {
                    page = new MediaPage(entry);
                }
                else if (entry.Archiver is PdfArchiver)
                {
                    page = new PdfPage(entry);
                }
                else if (BookProfile.Current.IsEnableAnimatedGif && LoosePath.GetExtension(entry.EntryName) == ".gif")
                {
                    page = new AnimatedPage(entry);
                }
                else
                {
                    page = new BitmapPage(entry);
                }
            }
            else
            {
                var type = entry.IsDirectory ? ArchiverType.FolderArchive : ArchiverManager.Current.GetSupportedType(entry.EntryName);
                switch (type)
                {
                    case ArchiverType.None:
                        page = new FilePage(entry, FilePageIcon.File);
                        break;
                    case ArchiverType.FolderArchive:
                        page = new FilePage(entry, FilePageIcon.Folder);
                        break;
                    default:
                        page = new FilePage(entry, FilePageIcon.Archive);
                        break;
                }
            }

            //
            page.Thumbnail.Changed += (s, e) =>
            {
                ThumbnailChanged?.Invoke(this, new PageChangedEventArgs(page));
            };

            return page;
        }

        // 名前の最長一致文字列取得
        private string GetPagesPrefix()
        {
            if (Pages == null || Pages.Count == 0) return "";

            string s = Pages[0].FullPath;
            foreach (var page in Pages)
            {
                s = GetStartsWith(s, page.FullPath);
                if (string.IsNullOrEmpty(s)) break;
            }

            // １ディレクトリだけの場合に表示が消えないようにする
            if (Pages.Count == 1)
            {
                s = s.TrimEnd('\\', '/');
            }

            // 最初の区切り記号
            for (int i = s.Length - 1; i >= 0; --i)
            {
                if (s[i] == '\\' || s[i] == '/')
                {
                    return s.Substring(0, i + 1);
                }
            }

            // ヘッダとして認識できなかった
            return "";
        }

        //
        private string GetStartsWith(string s0, string s1)
        {
            if (s0 == null || s1 == null) return "";

            if (s0.Length > s1.Length)
            {
                var temp = s0;
                s0 = s1;
                s1 = temp;
            }

            for (int i = 0; i < s0.Length; ++i)
            {
                char a0 = s0[i];
                char a1 = s1[i];
                if (s0[i] != s1[i])
                {
                    return i > 0 ? s0.Substring(0, i) : "";
                }
            }

            return s0;
        }

        // コマンドエンジン
        private BookCommandEngine _commandEngine = new BookCommandEngine();

        // 開始
        // ページ設定を行うとコンテンツ読み込みが始まるため、ロードと分離した
        public void Start()
        {
            Debug.Assert(Place != null);
            _commandEngine.Log = new NeeLaboratory.Diagnostics.Log(nameof(BookCommandEngine), 0);
            _commandEngine.StartEngine();
        }

        #endregion

        #region コマンド発行

        // 廃棄処理
        public async Task DisposeAsync()
        {
            var command = RequestDispose(this);
            if (command == null) return;

            await command.WaitAsync();
        }

        // ページ列を設定
        // プロパティと異なり、ランダムソートの場合はソートを再実行する
        public void SetSortMode(PageSortMode mode)
        {
            if (_sortMode != mode || mode == PageSortMode.Random)
            {
                _sortMode = mode;
                RequestSort(this);
            }
        }

        // 前のページに戻る
        public void PrevPage(int step = 0)
        {
            var s = (step == 0) ? PageMode.Size() : step;
            RequestMovePosition(this, -s);
        }

        // 次のページへ進む
        public void NextPage(int step = 0)
        {
            var s = (step == 0) ? PageMode.Size() : step;
            RequestMovePosition(this, +s);
        }

        // 最初のページに移動
        public void FirstPage()
        {
            RequestSetPosition(this, FirstPosition(), 1, true);
        }

        // 最後のページに移動
        public void LastPage()
        {
            RequestSetPosition(this, LastPosition(), -1, true);
        }

        // 指定ページに移動
        public void JumpPage(Page page)
        {
            int index = Pages.IndexOf(page);
            if (index >= 0)
            {
                var position = new PagePosition(index, 0);
                RequestSetPosition(this, position, 1, false);
            }
        }


        /// <summary>
        /// ページ指定移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="position">ページ位置</param>
        /// <param name="direction">読む方向(+1 or -1)</param>
        /// <param name="isPreLoad">この移動で先読みを行う</param>
        public void RequestSetPosition(object sender, PagePosition position, int direction, bool isPreLoad)
        {
            Debug.Assert(direction == 1 || direction == -1);

            if (Place == null) return;

            DisplayIndex = position.Index;

            var command = new BookCommandSetPage(sender, this, new BookCommandSetPageArgs()
            {
                Position = position,
                Direction = direction,
                Size = PageMode.Size(),
                IsPreLoad = isPreLoad,
            });
            _commandEngine.Enqueue(command);
        }

        // ページ相対移動
        public void RequestMovePosition(object sender, int step)
        {
            if (Place == null) return;

            var command = new BookCommandMovePage(sender, this, new BookCommandMovePageArgs()
            {
                Step = step,
            });

            _commandEngine.Enqueue(command);
        }

        // リフレッシュ
        public void RequestReflesh(object sender, bool isClear)
        {
            if (Place == null) return;

            var command = new BookCommandReflesh(sender, this, new BookCommandRefleshArgs()
            {
                IsClear = isClear,
            });
            _commandEngine.Enqueue(command);
        }

        // ソート
        public void RequestSort(object sender)
        {
            if (Place == null) return;

            var command = new BookCommandSort(sender, this, new BookCommandSortArgs());
            _commandEngine.Enqueue(command);
        }

        // ページ削除
        public void RequestRemove(object sender, Page page)
        {
            if (Place == null) return;

            var command = new BookCommandRemove(sender, this, new BookCommandRemoveArgs()
            {
                Page = page,
            });
            _commandEngine.Enqueue(command);
        }

        // 表示の再構築
        private void Reflesh(bool clear)
        {
            if (Place == null) return;

            if (clear)
            {
                _keepPages.ForEach(e => e?.Unload());
            }

            RequestSetPosition(this, _viewPageCollection.Range.Min, 1, true);
        }

        // 終了処理
        private BookCommand RequestDispose(object sender)
        {
            if (Place == null) return null;

            var command = new BookCommandDispose(sender, this, new BookCommandDisposeArgs());
            _commandEngine.Enqueue(command);

            return command;
        }

        #endregion

        #region コマンド実行

        // コマンド処理
        internal async Task Dispose_Executed(BookCommandDisposeArgs param, CancellationToken token)
        {
            Dispose();
            await Task.Yield();
        }

        internal async Task Remove_Executed(BookCommandRemoveArgs param, CancellationToken token)
        {
            Remove(param.Page);
            await Task.Yield();
        }

        internal async Task Sort_Executed(BookCommandSortArgs param, CancellationToken token)
        {
            Sort();
            RequestSetPosition(this, FirstPosition(), 1, true);
            await Task.Yield();
        }

        internal async Task Reflesh_Executed(BookCommandRefleshArgs param, CancellationToken token)
        {
            Reflesh(param.IsClear);
            await Task.Yield();
        }

        internal async Task SetPage_Executed(object sender, BookCommandSetPageArgs param, CancellationToken token)
        {
            var source = new PageDirectionalRange(param.Position, param.Direction, param.Size);
            await UpdateViewPageAsync(source, param.IsPreLoad, sender, token);
        }


        internal async Task MovePage_Executed(BookCommandMovePageArgs param, CancellationToken token)
        {
            var viewRange = _viewPageCollection.Range;

            var direction = param.Step < 0 ? -1 : 1;

            var pos = Math.Abs(param.Step) == PageMode.Size() ? viewRange.Next(direction) : viewRange.Move(param.Step);
            if (pos < FirstPosition() && !viewRange.IsContains(FirstPosition()))
            {
                pos = new PagePosition(0, direction < 0 ? 1 : 0);
            }
            else if (pos > LastPosition() && !viewRange.IsContains(LastPosition()))
            {
                pos = new PagePosition(Pages.Count - 1, direction < 0 ? 1 : 0);
            }

            var range = new PageDirectionalRange(pos, direction, PageMode.Size());

            var isPreLoad = Math.Abs(param.Step) <= PageMode.Size();

            await UpdateViewPageAsync(range, isPreLoad, null, token);
        }

        #endregion

        #region 表示ページ処理

        // 表示ページ番号
        public int GetViewPageindex() => _viewPageCollection.Range.Min.Index;

        // 表示ページ
        public Page GetViewPage() => GetPage(_viewPageCollection.Range.Min.Index);

        // 表示ページ群
        public List<Page> GetViewPages() => _viewPageCollection.Collection.Select(e => e.Page).ToList();

        // ページ
        public Page GetPage(int index) => Pages.Count > 0 ? Pages[ClampPageNumber(index)] : null;

        //
        public Page GetPage(string name) => Pages.FirstOrDefault(e => e.FullPath == name);

        // ページ番号
        public int GetIndex(Page page) => Pages.IndexOf(page);

        // 先頭ページの場所
        private PagePosition FirstPosition() => PagePosition.Zero;

        // 最終ページの場所
        private PagePosition LastPosition() => Pages.Count > 0 ? new PagePosition(Pages.Count - 1, 1) : FirstPosition();

        //
        private int KeepPageNextSize() => PageMode == PageMode.SinglePage ? 1 : 3;
        private int KeepPagePrevSize() => PageMode == PageMode.SinglePage ? 1 : 2;

        // 先読み許可フラグ
        private bool AllowPreLoad()
        {
            switch (BookProfile.Current.PreLoadMode)
            {
                default:
                case PreLoadMode.None:
                    return false;
                case PreLoadMode.AutoPreLoad:
                    return _canPreLoad;
                case PreLoadMode.PreLoad:
                    return true;
                case PreLoadMode.PreLoadNoUnload:
                    return true;
            }
        }

        // 開放許可フラグ
        private bool AllowUnload() => BookProfile.Current.PreLoadMode != PreLoadMode.PreLoadNoUnload;

        // ページ番号のクランプ
        private int ClampPageNumber(int index)
        {
            if (index > Pages.Count - 1) index = Pages.Count - 1;
            if (index < 0) index = 0;
            return index;
        }

        // ページ場所の有効判定
        private bool IsValidPosition(PagePosition position)
        {
            return (FirstPosition() <= position && position <= LastPosition());
        }

        // 表示ページ更新
        private async Task UpdateViewPageAsync(PageDirectionalRange source, bool isPreLoad, object sender, CancellationToken token)
        {
            // ページ終端を越えたか判定
            if (source.Position < FirstPosition())
            {
                App.Current?.Dispatcher.Invoke(() => PageTerminated?.Invoke(this, new PageTerminatedEventArgs(-1)));
                return;
            }
            else if (source.Position > LastPosition())
            {
                App.Current?.Dispatcher.Invoke(() => PageTerminated?.Invoke(this, new PageTerminatedEventArgs(+1)));
                return;
            }

            // ページ数０の場合は表示コンテンツなし
            if (Pages.Count == 0)
            {
                App.Current?.Dispatcher.Invoke(() => ViewContentsChanged?.Invoke(this, new ViewPageCollectionChangedEventArgs(new ViewPageCollection())));
                return;
            }

            // 先読みページコンテンツ無効
            _nextPageCollection = new ViewPageCollection();

            // view pages
            var viewPages = new List<Page>();
            for (int i = 0; i < PageMode.Size(); ++i)
            {
                var page = Pages[ClampPageNumber(source.Position.Index + source.Direction * i)];
                if (!viewPages.Contains(page))
                {
                    viewPages.Add(page);
                }
            }

            // cleanup pages
            _keepPages.AddRange(viewPages.Where(e => !_keepPages.Contains(e)));
            CleanupPages(source);

            // start load
            var tlist = new List<Task>();
            foreach (var page in viewPages)
            {
                tlist.Add(page.LoadAsync(QueueElementPriority.Top));
            }

            // pre load
            if (isPreLoad) PreLoad(source);

            // wait load
            if (BookProfile.Current.CanPrioritizePageMove())
            {
                await Task.Run(() => Task.WaitAll(tlist.ToArray(), 100, token));
            }
            else
            {
                await Task.WhenAll(tlist.ToArray());
            }
            // task cancel?
            token.ThrowIfCancellationRequested();

            // update contents
            this.PageChangeCount++;
            this.IsPageTerminated = source.Max >= LastPosition();
            _viewPageSender = sender;
            _viewPageRange = source;
            UpdateViewContents();
            UpdateNextContents();

            // ページ破棄
            if (!AllowPreLoad()) ClearAllPages(viewPages);
        }

        /// <summary>
        /// ページロード完了イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_Loaded(object sender, EventArgs e)
        {
            if (!BookProfile.Current.CanPrioritizePageMove()) return;

            // 非同期なので一旦退避
            var now = _viewPageCollection;
            var next = _nextPageCollection;

            if (now?.Collection == null) return;

            var page = (Page)sender;

            // 現在表示に含まれているページ？
            if (page.IsContentAlived && now.Collection.Any(item => !item.IsValid && item.Page == page))
            {
                // 再更新
                UpdateViewContents();
            }

            UpdateNextContents();
        }

        /// <summary>
        /// 表示コンテンツ更新
        /// </summary>
        public void UpdateViewContents()
        {
            if (_isDisposed) return;

            // update contents
            var sender = _viewPageSender;
            var viewContent = CreateViewPageContext(_viewPageRange);
            if (viewContent == null) return;

            _viewPageCollection = viewContent;
            ////Debug.WriteLine($"now: {_viewPageCollection.Range}");

            // notice ViewContentsChanged
            App.Current?.Dispatcher.Invoke(() => ViewContentsChanged?.Invoke(this, new ViewPageCollectionChangedEventArgs(_viewPageCollection)));

            // change page
            this.DisplayIndex = viewContent.Range.Min.Index;

            // notice PropertyChanged
            // sender を命令発行者にする
            PageChanged?.Invoke(sender, new PageChangedEventArgs(GetPage(this.DisplayIndex)));

            // コンテンツ準備完了
            ContentLoaded.Set();
        }

        /// <summary>
        /// 先読みコンテンツ更新
        /// </summary>
        public void UpdateNextContents()
        {
            if (_isDisposed) return;

            // 表示コンテンツ確定？
            if (!_viewPageCollection.IsValid) return;

            // 既に先読みコンテンツは確定している？
            if (_nextPageCollection.IsValid) return;

            // 先読みコンテンツ領域計算
            var position = _viewPageCollection.Range.Next();
            var direction = _viewPageCollection.Range.Direction;
            var range = new PageDirectionalRange(position, direction, PageMode.Size());

            // create contents
            var next = CreateViewPageContext(range);
            _nextPageCollection = next;
            if (next == null) return;
            if (!next.IsValid) return;

            ////Debug.WriteLine($"next: {next.Range}");
            NextContentsChanged?.Invoke(this, new ViewPageCollectionChangedEventArgs(next));
        }

        // ページのワイド判定
        private bool IsWide(Page page)
        {
            return page.Width > page.Height * BookProfile.Current.WideRatio;
        }

        // 見開きモードでも単独表示するべきか判定
        private bool IsSoloPage(int index)
        {
            if (IsSupportedWidePage && IsWide(Pages[index])) return true;
            if (IsSupportedSingleFirstPage && index == 0) return true;
            if (IsSupportedSingleLastPage && index == Pages.Count - 1) return true;
            return false;
        }

        // 分割モード有効判定
        private bool IsEnableDividePage(int index)
        {
            return (PageMode == PageMode.SinglePage && IsSupportedDividePage && IsWide(Pages[index]));
        }

        // 表示コンテンツソースと、それに対応したコンテキスト作成
        private ViewPageCollection CreateViewPageContext(PageDirectionalRange source)
        {
            var infos = new List<PagePart>();

            {
                PagePosition position = source.Position;

                for (int id = 0; id < PageMode.Size(); ++id)
                {
                    if (!IsValidPosition(position) || Pages[position.Index] == null) break;

                    int size = 2;
                    if (IsEnableDividePage(position.Index))
                    {
                        size = 1;
                    }
                    else
                    {
                        position = new PagePosition(position.Index, 0);
                    }

                    infos.Add(new PagePart(position, size, this.BookReadOrder));

                    position = position + ((source.Direction > 0) ? size : -1);
                }
            }

            // 見開き補正
            if (PageMode == PageMode.WidePage && infos.Count >= 2)
            {
                if (IsSoloPage(infos[0].Position.Index) || IsSoloPage(infos[1].Position.Index))
                {
                    infos = infos.GetRange(0, 1);
                }
            }

            // コンテンツソース作成
            var contentsSource = new List<ViewPage>();
            foreach (var v in infos)
            {
                var page = Pages[v.Position.Index];
                contentsSource.Add(new ViewPage(page, v));
            }



            // 並び順補正
            if (source.Direction < 0 && infos.Count >= 2)
            {
                contentsSource.Reverse();
                infos.Reverse();
            }

            // 左開き
            if (BookReadOrder == PageReadOrder.LeftToRight)
            {
                contentsSource.Reverse();
            }

            // 単一ソースならコンテンツは１つにまとめる
            if (infos.Count == 2 && infos[0].Position.Index == infos[1].Position.Index)
            {
                var position = new PagePosition(infos[0].Position.Index, 0);
                contentsSource.Clear();
                contentsSource.Add(new ViewPage(Pages[position.Index], new PagePart(position, 2, BookReadOrder)));
            }

            // 先読み可能判定
            UpdatePreLoadStatus(contentsSource);

            // 新しいコンテキスト
            var context = new ViewPageCollection(new PageDirectionalRange(infos, source.Direction), contentsSource);
            return context;
        }

        // 先読み判定更新
        private void UpdatePreLoadStatus(List<ViewPage> contentsSource)
        {
            if (BookProfile.Current.PreLoadMode != PreLoadMode.AutoPreLoad) return;

            UpdatePreLoadStatus(contentsSource.Select(e => e.Page));
        }

        /// <summary>
        /// 先読み自動判定
        /// </summary>
        /// <param name="page"></param>
        private void UpdatePreLoadStatus(IEnumerable<Page> pages)
        {
            if (BookProfile.Current.PreLoadMode != PreLoadMode.AutoPreLoad) return;

            // 集計
            double size = 0;
            foreach (var page in pages)
            {
                if (!page.IsContentInfoAlive) return;
                size += page.Content.Size.Width * page.Content.Size.Height;
            }

            // 判定
            if (size > BookProfile.Current.PreLoadLimitSize)
            {
                //Debug.WriteLine("PreLoad: Disabled");
                _canPreLoadCount = 0;
                _canPreLoad = false;
            }
            else
            {
                _canPreLoadCount++;
                if (!_canPreLoad && _canPreLoadCount > 3) // 一定回数連続で規定サイズ以下なら先読み有効
                {
                    //Debug.WriteLine("PreLoad: Enabled");
                    _canPreLoad = true;
                }
            }
        }

        // 不要ページコンテンツの削除を行う
        private void CleanupPages(PageDirectionalRange source)
        {
            // コンテンツを保持するページ収集
            var keepPages = new List<Page>();
            int prevSize = source.Direction < 0 ? KeepPageNextSize() : KeepPagePrevSize();
            int nextSize = source.Direction < 0 ? KeepPagePrevSize() : KeepPageNextSize();
            for (int offset = -prevSize; offset <= nextSize; ++offset)
            {
                int index = source.Position.Index + offset;
                if (0 <= index && index < Pages.Count)
                {
                    keepPages.Add(Pages[index]);
                }
            }

            // 不要コンテンツ破棄
            ClearAllPages(keepPages);
        }

        // 全ページコンテンツの削除を行う
        private void ClearAllPages(List<Page> keeps)
        {
            if (AllowUnload())
            {
                foreach (var page in _keepPages.Where(e => !keeps.Contains(e)))
                {
                    page.Unload();
                }
            }

            // 保持ページ更新
            _keepPages = keeps;
        }

        // 先読み
        private void PreLoad(PageDirectionalRange source)
        {
            if (!AllowPreLoad()) return;

            var preLoadPages = new List<Page>();

            for (int offset = 0; offset <= KeepPageNextSize(); offset++)
            {
                int index = source.Position.Index + (source.Direction < 0 ? -offset : offset);
                if (0 <= index && index < Pages.Count)
                {
                    Debug.Assert(_keepPages.Contains(Pages[index])); // 念のため
                    Pages[index].Load(QueueElementPriority.Default, PageJobOption.WeakPriority);

                    if (!_keepPages.Contains(Pages[index]))
                    {
                        _keepPages.Add(Pages[index]);
                    }
                }
            }
        }

        #endregion

        #region ページの並び替え

        // ページの並び替え
        private void Sort()
        {
            if (Pages.Count <= 0) return;

            switch (SortMode)
            {
                case PageSortMode.FileName:
                    Pages.Sort((a, b) => CompareFileNameOrder(a, b, Win32Api.StrCmpLogicalW));
                    break;
                case PageSortMode.FileNameDescending:
                    Pages.Sort((a, b) => CompareFileNameOrder(b, a, Win32Api.StrCmpLogicalW));
                    break;
                case PageSortMode.TimeStamp:
                    Pages.Sort((a, b) => CompareDateTimeOrder(a, b, Win32Api.StrCmpLogicalW));
                    break;
                case PageSortMode.TimeStampDescending:
                    Pages.Sort((a, b) => CompareDateTimeOrder(b, a, Win32Api.StrCmpLogicalW));
                    break;
                case PageSortMode.Random:
                    var random = new Random();
                    Pages = Pages.OrderBy(e => random.Next()).ToList();
                    break;
                default:
                    throw new NotImplementedException();
            }

            // ページ ナンバリング
            PagesNumbering();

            PagesSorted?.Invoke(this, null);
        }

        /// <summary>
        /// ページ番号設定
        /// </summary>
        private void PagesNumbering()
        {
            for (int i = 0; i < Pages.Count; ++i) Pages[i].Index = i;
        }

        // ファイル名, 日付, ID の順で比較
        private static int CompareFileNameOrder(Page p1, Page p2, Func<string, string, int> compare)
        {
            if (p1.FullPath != p2.FullPath)
                return CompareFileName(p1.FullPath, p2.FullPath, compare);
            else if (p1.Entry.LastWriteTime != p2.Entry.LastWriteTime)
                return CompareDateTime(p1.Entry.LastWriteTime, p2.Entry.LastWriteTime);
            else
                return p1.Entry.Id - p2.Entry.Id;
        }

        // 日付, ファイル名, ID の順で比較
        private static int CompareDateTimeOrder(Page p1, Page p2, Func<string, string, int> compare)
        {
            if (p1.Entry.LastWriteTime != p2.Entry.LastWriteTime)
                return CompareDateTime(p1.Entry.LastWriteTime, p2.Entry.LastWriteTime);
            else if (p1.FullPath != p2.FullPath)
                return CompareFileName(p1.FullPath, p2.FullPath, compare);
            else
                return p1.Entry.Id - p2.Entry.Id;
        }

        // ファイル名比較. ディレクトリを優先する
        private static int CompareFileName(string s1, string s2, Func<string, string, int> compare)
        {
            string d1 = LoosePath.GetDirectoryName(s1);
            string d2 = LoosePath.GetDirectoryName(s2);

            if (d1 == d2)
                return compare(s1, s2);
            else
                return compare(d1, d2);
        }

        // 日付比較。null対応
        private static int CompareDateTime(DateTime? _t1, DateTime? _t2)
        {
            DateTime t1 = _t1 ?? DateTime.MinValue;
            DateTime t2 = _t2 ?? DateTime.MinValue;
            return (t1.Ticks - t2.Ticks < 0) ? -1 : 1;
        }

        #endregion

        #region ページの削除

        // ページの削除
        private void Remove(Page page)
        {
            if (Pages.Count <= 0) return;

            int index = Pages.IndexOf(page);
            if (index < 0) return;

            Pages.RemoveAt(index);

            PagesNumbering();

            PageRemoved?.Invoke(this, new PageChangedEventArgs(page));

            index = ClampPageNumber(index);
            RequestSetPosition(this, new PagePosition(index, 0), 1, true);
        }

        #endregion

        #region マーカー処理

        /// <summary>
        /// マーカー判定
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public bool IsMarked(Page page)
        {
            return Markers.Contains(page);
        }

        /// <summary>
        /// マーカー群設定
        /// </summary>
        /// <param name="pageNames"></param>
        public void SetMarkers(IEnumerable<string> pageNames)
        {
            this.Markers = pageNames.Select(e => Pages.FirstOrDefault(page => page.FullPath == e)).Where(e => e != null).ToList();
        }

        /// <summary>
        /// マーカー移動可能判定
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="isLoop"></param>
        /// <returns></returns>
        public bool CanJumpToMarker(int direction, bool isLoop)
        {
            if (Place == null) return false;
            if (Markers == null || Markers.Count == 0) return false;

            if (isLoop) return true;

            var list = Markers.OrderBy(e => e.Index).ToList();
            var index = GetViewPageindex();

            return direction > 0
                ? list.Last().Index > index
                : list.First().Index < index;
        }

        /// <summary>
        /// ブック内のマーカーを移動
        /// </summary>
        /// <param name="direction">移動方向(+1 or -1)</param>
        /// <param name="isLoop">ループ移動</param>
        /// <returns></returns>
        public Page RequestJumpToMarker(object sender, int direction, bool isLoop, bool isIncludeTerminal)
        {
            Debug.Assert(direction == 1 || direction == -1);

            if (Place == null) return null;
            if (Pages == null || Pages.Count < 2) return null;

            var list = Markers != null ? Markers.OrderBy(e => e.Index).ToList() : new List<Page>();

            if (isIncludeTerminal)
            {
                if (list.FirstOrDefault() != Pages.First())
                {
                    list.Insert(0, Pages.First());
                }
                if (list.LastOrDefault() != Pages.Last())
                {
                    list.Add(Pages.Last());
                }
            }

            if (list.Count == 0) return null;

            var index = GetViewPageindex();

            var target =
                direction > 0
                ? list.FirstOrDefault(e => e.Index > index) ?? (isLoop ? list.First() : null)
                : list.LastOrDefault(e => e.Index < index) ?? (isLoop ? list.Last() : null);

            if (target == null) return null;

            RequestSetPosition(sender, new PagePosition(target.Index, 0), +1, false);
            return target;
        }

        #endregion

        #region 終了処理

        /// <summary>
        /// 終了処理
        /// </summary>
        public void Dispose()
        {
            _isDisposed = true;

            // さまざまなイベント停止
            this.DartyBook = null;
            this.PageChanged = null;
            this.PageRemoved = null;
            this.PagesSorted = null;
            this.PageTerminated = null;
            this.ThumbnailChanged = null;
            this.ViewContentsChanged = null;

            _viewPageCollection = new ViewPageCollection();

            Pages?.ForEach(e => e?.Dispose());
            _archivers?.ForEach(e => e.Dispose());
            _trashBox?.CleanUp();

            _commandEngine.StopEngine();

            MemoryControl.Current.GarbageCollect();

            ////Debug.WriteLine("Book: Disposed.");
        }

        #endregion

        #endregion

        #region Memento

        /// <summary>
        /// 保存設定
        /// </summary>
        [DataContract]
        public class Memento
        {
            // フォルダーの場所
            [DataMember(EmitDefaultValue = false)]
            public string Place { get; set; }

            // 名前
            public string Name => Place.EndsWith(@":\") ? Place : System.IO.Path.GetFileName(Place);

            // 現在ページ
            [DataMember(EmitDefaultValue = false)]
            public string Page { get; set; }

            // 1ページ表示 or 2ページ表示
            [DataMember(Name = "PageModeV2")]
            [PropertyMember("ページ表示")]
            public PageMode PageMode { get; set; }

            // 右開き or 左開き
            [DataMember]
            [PropertyMember("本を開く方向")]
            public PageReadOrder BookReadOrder { get; set; }

            // 横長ページ分割 (1ページモード)
            [DataMember]
            [PropertyMember("横長ページを分割する")]
            public bool IsSupportedDividePage { get; set; }

            // 最初のページを単独表示 
            [DataMember]
            [PropertyMember("最初のページを単独表示")]
            public bool IsSupportedSingleFirstPage { get; set; }

            // 最後のページを単独表示
            [DataMember]
            [PropertyMember("最後のページを単独表示")]
            public bool IsSupportedSingleLastPage { get; set; }

            // 横長ページを2ページ分とみなす(2ページモード)
            [DataMember]
            [PropertyMember("横長ページを2ページとみなす")]
            public bool IsSupportedWidePage { get; set; } = true;

            // フォルダーの再帰
            [DataMember]
            [PropertyMember("サブフォルダーを読み込む", Tips = "開くフォルダー以下を全て検索するため、フォルダーの場所によっては処理が重くなります。")]
            public bool IsRecursiveFolder { get; set; }

            // ページ並び順
            [DataMember]
            [PropertyMember("ページの並び順")]
            public PageSortMode SortMode { get; set; }

            // 最終アクセス日
            [DataMember(Order = 12, EmitDefaultValue = false)]
            public DateTime LastAccessTime { get; set; }


            //
            public Memento Clone()
            {
                return (Memento)this.MemberwiseClone();
            }


            /// <summary>
            /// 項目のフィルタリング。フラグの立っている項目を上書き
            /// </summary>
            /// <param name="filter">フィルタービット列</param>
            /// <param name="overwrite">上書き既定値</param>
            public void Write(BookMementoFilter filter, Memento overwrite)
            {
                // 現在ページ
                if (filter.Flags[BookMementoBit.Page])
                {
                    this.Page = overwrite.Page;
                }
                // 1ページ表示 or 2ページ表示
                if (filter.Flags[BookMementoBit.PageMode])
                {
                    this.PageMode = overwrite.PageMode;
                }
                // 右開き or 左開き
                if (filter.Flags[BookMementoBit.BookReadOrder])
                {
                    this.BookReadOrder = overwrite.BookReadOrder;
                }
                // 横長ページ分割 (1ページモード)
                if (filter.Flags[BookMementoBit.IsSupportedDividePage])
                {
                    this.IsSupportedDividePage = overwrite.IsSupportedDividePage;
                }
                // 最初のページを単独表示 
                if (filter.Flags[BookMementoBit.IsSupportedSingleFirstPage])
                {
                    this.IsSupportedSingleFirstPage = overwrite.IsSupportedSingleFirstPage;
                }
                // 最後のページを単独表示
                if (filter.Flags[BookMementoBit.IsSupportedSingleLastPage])
                {
                    this.IsSupportedSingleLastPage = overwrite.IsSupportedSingleLastPage;
                }
                // 横長ページを2ページ分とみなす(2ページモード)
                if (filter.Flags[BookMementoBit.IsSupportedWidePage])
                {
                    this.IsSupportedWidePage = overwrite.IsSupportedWidePage;
                }
                // フォルダーの再帰
                if (filter.Flags[BookMementoBit.IsRecursiveFolder])
                {
                    this.IsRecursiveFolder = overwrite.IsRecursiveFolder;
                }
                // ページ並び順
                if (filter.Flags[BookMementoBit.SortMode])
                {
                    this.SortMode = overwrite.SortMode;
                }
            }


            // 保存用バリデート
            // このmementoは履歴とデフォルト設定の２つに使われるが、デフォルト設定には本の場所やページ等は不要
            public void ValidateForDefault()
            {
                Place = null;
                Page = null;
                LastAccessTime = default(DateTime);
            }

            // バリデートされたクローン
            public Memento ValidatedClone()
            {
                var clone = this.Clone();
                clone.ValidateForDefault();
                return clone;
            }
        }

        // 重複チェック用
        public class MementoPlaceCompare : IEqualityComparer<Memento>
        {
            public bool Equals(Memento m1, Memento m2)
            {
                if (m1 == null && m2 == null)
                    return true;
                else if (m1 == null | m2 == null)
                    return false;
                else if (m1.Place == m2.Place)
                    return true;
                else
                    return false;
            }

            public int GetHashCode(Memento m)
            {
                return m.Place.GetHashCode();
            }
        }


        // bookの設定を取得する
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.Place = Place;
            memento.Page = SortMode != PageSortMode.Random ? GetViewPage()?.FullPath : null;

            memento.PageMode = PageMode;
            memento.BookReadOrder = BookReadOrder;
            memento.IsSupportedDividePage = IsSupportedDividePage;
            memento.IsSupportedSingleFirstPage = IsSupportedSingleFirstPage;
            memento.IsSupportedSingleLastPage = IsSupportedSingleLastPage;
            memento.IsSupportedWidePage = IsSupportedWidePage;
            memento.IsRecursiveFolder = IsRecursiveFolder;
            memento.SortMode = SortMode;
            //memento.LastAccessTime = DateTime.Now;

            return memento;
        }

        // bookに設定を反映させる
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            PageMode = memento.PageMode;
            BookReadOrder = memento.BookReadOrder;
            IsSupportedDividePage = memento.IsSupportedDividePage;
            IsSupportedSingleFirstPage = memento.IsSupportedSingleFirstPage;
            IsSupportedSingleLastPage = memento.IsSupportedSingleLastPage;
            IsSupportedWidePage = memento.IsSupportedWidePage;
            IsRecursiveFolder = memento.IsRecursiveFolder;
            SortMode = memento.SortMode;
        }

        #endregion
    }

    // ページ関係のイベントパラメータ
    public class PageChangedEventArgs : EventArgs
    {
        public PageChangedEventArgs(Page page)
        {
            Page = page;
        }

        public Page Page { get; set; }
    }

    // 表示コンテンツ変更イベント
    public class ViewPageCollectionChangedEventArgs : EventArgs
    {
        public ViewPageCollectionChangedEventArgs(ViewPageCollection viewPageCollection)
        {
            ViewPageCollection = viewPageCollection;
        }

        public ViewPageCollection ViewPageCollection { get; set; }
    }

    // ページ終端イベント
    public class PageTerminatedEventArgs : EventArgs
    {
        public PageTerminatedEventArgs(int direction)
        {
            Direction = direction;
        }

        public int Direction { get; set; }
    }

    // ページサムネイル寿命管理
    public class PageThumbnailPool : ThumbnailPool
    {
        public override int Limit => ThumbnailProfile.Current.PageCapacity;
    }


}
