// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
    /// ロードオプションフラグ
    /// </summary>
    [Flags]
    public enum BookLoadOption
    {
        None = 0,
        Recursive = (1 << 0), // 再帰
        SupportAllFile = (1 << 1), // すべてのファイルをページとみなす
        FirstPage = (1 << 2), // 初期ページを先頭ページにする
        LastPage = (1 << 3), // 初期ページを最終ページにする
        ReLoad = (1 << 4), // 再読み込みフラグ(BookHubで使用)
        KeepHistoryOrder = (1 << 5), // 履歴の順番を変更しない
        SelectFoderListMaybe = (1 << 6), // 可能ならばフォルダリストで選択する
        SelectHistoryMaybe = (1 << 7), // 可能ならば履歴リストで選択する
        SkipSamePlace = (1 << 8), // 同じフォルダならば読み込まない
        AutoRecursive = (1 << 9), // 自動再帰
        Resume = (1 << 10), // 履歴情報から全て復元
        //Rename = (1 << 11), // 名前変更して開く
    };

    /// <summary>
    /// 
    /// </summary>
    public class PagemarkChangedEventArgs : EventArgs
    {
        public Page Page { get; set; }
        public bool IsMarked { get; set; }
    }

    /// <summary>
    /// 本
    /// </summary>
    public class Book : IDisposable
    {
        // テンポラリコンテンツ用ゴミ箱
        public TrashBox _trashBox { get; private set; } = new TrashBox();


        // 現在ページ変更(ページ番号)
        // タイトル、スライダーの更新を要求
        public event EventHandler<int> PageChanged;

        // 表示コンテンツ変更
        // 表示の更新を要求
        public event EventHandler<ViewSource> ViewContentsChanged;

        // ページ終端を超えて移動しようとした
        // 次の本への移動を要求
        public event EventHandler<int> PageTerminated;

        // 再読み込みを要求
        public event EventHandler DartyBook;

        // ソートされた
        public event EventHandler PagesSorted;

        // サムネイル更新
        public event EventHandler<Page> ThumbnailChanged;

        // Disposed
        private volatile bool _isDisposed;

        // 先読み許可フラグ
        private bool AllowPreLoad
        {
            get
            {
                switch (PreLoadMode)
                {
                    default:
                    case PreLoadMode.None:
                        return false;
                    case PreLoadMode.AutoPreLoad:
                        return _canPreLoad;
                    case PreLoadMode.PreLoad:
                        return true;
                }
            }
        }

        // 先読みモード
        public PreLoadMode PreLoadMode { get; set; }

        // 先読み可能フラグ
        private bool _canPreLoad = true;

        // 先読み解除フラグ
        private int _canPreLoadCount;


        // ファイル削除された
        public event EventHandler<Page> PageRemoved;


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
                    RequestReflesh(false);
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
                    RequestReflesh(false);
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
                    RequestReflesh(false);
                }
            }
        }

        // 横長ページは２ページとみなす
        private bool _isSupportedWidePage;
        public bool IsSupportedWidePage
        {
            get { return _isSupportedWidePage; }
            set
            {
                if (_isSupportedWidePage != value)
                {
                    _isSupportedWidePage = value;
                    RequestReflesh(false);
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
                    RequestReflesh(false);
                }
            }
        }

        // サブフォルダ読み込み
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
                    RequestReflesh(false);
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
                    RequestSort();
                }
            }
        }

        // ページ列を設定
        // プロパティと異なり、ランダムソートの場合はソートを再実行する
        public void SetSortMode(PageSortMode mode)
        {
            if (_sortMode != mode || mode == PageSortMode.Random)
            {
                _sortMode = mode;
                RequestSort();
            }
        }


        // この本の場所
        // nullの場合、この本は無効
        public string Place { get; private set; }

        // 開始ページ
        public string StartEntry { get; private set; }

        // アーカイバコレクション
        // Dispose処理のために保持
        private List<Archiver> _archivers = new List<Archiver>();

        // ページ コレクション
        public List<Page> Pages { get; private set; } = new List<Page>();

        // 表示されるページ番号(スライダー用)
        public int DisplayIndex { get; set; }

        // 表示ページコンテキスト
        private ViewPageContext _viewContext = new ViewPageContext();

        // 表示ページ番号
        public int GetViewPageindex() => _viewContext.Position.Index;

        // 表示ページ
        public Page GetViewPage() => GetPage(_viewContext.Position.Index);

        // 表示ページ群
        public List<Page> GetViewPages()
        {
            var pages = new List<Page>();
            for (int i = 0; i < _viewContext.Size; ++i)
            {
                pages.Add(GetPage(_viewContext.Position.Index + i));
            }
            return pages;
        }

        // ページ
        public Page GetPage(int index) => Pages.Count > 0 ? Pages[ClampPageNumber(index)] : null;

        //
        public Page GetPage(string name) => Pages.FirstOrDefault(e => e.FullPath == name);

        // ページ番号
        public int GetIndex(Page page) => Pages.IndexOf(page);

        // 先頭ページの場所
        private PagePosition _firstPosition => new PagePosition(0, 0);

        // 最終ページの場所
        private PagePosition _lastPosition => Pages.Count > 0 ? new PagePosition(Pages.Count - 1, 1) : _firstPosition;

        // リソースを保持しておくページ
        private List<Page> _keepPages = new List<Page>();

        //
        private int _keepPageNextSize => PageMode == PageMode.SinglePage ? 1 : 3;
        private int _keepPagePrevSize => PageMode == PageMode.SinglePage ? 1 : 3;

        // マーカー
        public List<Page> Markers = new List<Page>();

        // 排他処理用ロックオブジェクト
        private object _lock = new object();

        /// <summary>
        /// FastAction property.
        /// </summary>
        public bool FastAction { get; set; } = true;

        // 本の読み込み
        #region LoadBook

        // 読み込み対象外サブフォルダ数。リカーシブ確認に使用します。
        public int SubFolderCount { get; private set; }

        /// <summary>
        /// 自動先読み判定用画像サイズ
        /// </summary>
        public static int PreLoadLimitSize { get; internal set; } = 4096 * 4096;


        //
        public async Task LoadAsync(string path, string start, BookLoadOption option, CancellationToken token)
        {
            try
            {
                await LoadCoreAsync(path, start, option, token);
            }
            catch
            {
                Terminate();
                throw;
            }
        }

        // 本読み込み
        public async Task LoadCoreAsync(string path, string start, BookLoadOption option, CancellationToken token)
        {
            Debug.Assert(Place == null);

            // リカーシブフラグ
            if (IsRecursiveFolder)
            {
                option |= BookLoadOption.Recursive;
            }

            // アーカイバの選択
            Archiver archiver = ModelContext.ArchiverManager.CreateArchiver(path, null);
            if (archiver.IsFileSystem)
            {
                // 入力ファイルを最初のページにする
                if (path != archiver.GetPlace())
                {
                    start = Path.GetFileName(path);
                }
            }
            else
            {
                // 圧縮ファイルは再帰させる
                option |= BookLoadOption.Recursive;
            }

            PagePosition position = _firstPosition;
            int direction = 1;

            // 読み込み
            await ReadArchiveAsync(archiver, "", option, token);

            // Pages Prefix
            var prefix = GetPagesPrefix();
            foreach (var page in Pages)
            {
                page.Prefix = prefix;
            }

            // 初期ソート
            Sort();

            // スタートページ取得
            if ((option & BookLoadOption.FirstPage) == BookLoadOption.FirstPage)
            {
                position = _firstPosition;
                direction = 1;
            }
            else if ((option & BookLoadOption.LastPage) == BookLoadOption.LastPage)
            {
                position = _lastPosition;
                direction = -1;
            }
            else
            {
                int index = (start != null) ? Pages.FindIndex(e => e.FullPath == start) : 0;
                position = index >= 0 ? new PagePosition(index, 0) : _firstPosition;
                direction = 1;
            }

            // 開始ページ記憶
            StartEntry = Pages.Count > 0 ? Pages[position.Index].FullPath : null;

            // 有効化
            Place = archiver.FileName;


            // 初期ページ設定
            RequestSetPosition(position, direction, true);
        }


        // アーカイブからページ作成(再帰)
        private async Task<bool> ReadArchiveAsync(Archiver archiver, string place, BookLoadOption option, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            List<ArchiveEntry> entries = null;

            try
            {
                if (!archiver.IsSupported())
                {
                    archiver.Dispose();
                    return false;
                }
                //entries = archiver.GetEntries();
                entries = await archiver.GetEntriesAsync(token);
            }
            catch (OperationCanceledException)
            {
                archiver.Dispose();
                throw;
            }
            catch (Exception e)
            {
                archiver.Dispose();
                throw new ApplicationException(e.Message);
            }

            //
            _archivers.Add(archiver);

            //
            foreach (var entry in entries)
            {
                token.ThrowIfCancellationRequested();

                // 再帰設定、もしくは単一ファイルの場合、再帰を行う
                bool isRecursive = (option & BookLoadOption.Recursive) == BookLoadOption.Recursive;
                bool isAutoRecursive = (option & BookLoadOption.AutoRecursive) == BookLoadOption.AutoRecursive && entries.Count == 1 && archiver.IsFileSystem;
                if ((isRecursive || isAutoRecursive) && ModelContext.ArchiverManager.IsSupported(entry.EntryName))
                {
                    bool result = false;
                    if (archiver.IsFileSystem)
                    {
                        result = await ReadArchiveAsync(ModelContext.ArchiverManager.CreateArchiver(archiver.GetFileSystemPath(entry), entry), LoosePath.Combine(place, entry.EntryName), option, token);
                    }
                    else
                    {
                        // テンポラリにアーカイブを解凍する
                        try
                        {
                            string tempFileName = await ArchivenEntryExtractorService.Current.ExtractAsync(entry, token);
                            _trashBox.Add(new TempFile(tempFileName));

                            result = await ReadArchiveAsync(ModelContext.ArchiverManager.CreateArchiver(tempFileName, entry), LoosePath.Combine(place, entry.EntryName), option, token);
                            if (!result)
                            {
                                AddPage(archiver, entry, place, option);
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            // 展開に失敗した場合、エラーページとして登録する
                            Pages.Add(new FilePage(archiver, entry, place, FilePageIcon.Alart) { Text = "ファイルの抽出に失敗しました\n" + e.Message });
                        }
                    }
                }
                else
                {
                    // ファイルとして展開
                    AddPage(archiver, entry, place, option);
                }
            }
            return true;
        }


        // ページを追加する
        private void AddPage(Archiver archiver, ArchiveEntry entry, string place, BookLoadOption option)
        {
            Page page = null;

            bool isSupportAllFile = (option & BookLoadOption.SupportAllFile) == BookLoadOption.SupportAllFile;

            if (ModelContext.BitmapLoaderManager.IsSupported(entry.EntryName))
            {
                if (isSupportAllFile || !ModelContext.BitmapLoaderManager.IsExcludedPath(entry.EntryName))
                {
                    page = new BitmapPage(archiver, entry, place);
                }
            }
            else
            {
                var type = ModelContext.ArchiverManager.GetSupportedType(entry.EntryName);
                if (isSupportAllFile)
                {
                    switch (type)
                    {
                        case ArchiverType.None:
                            page = new FilePage(archiver, entry, place, FilePageIcon.File);
                            break;
                        case ArchiverType.FolderFiles:
                            page = new FilePage(archiver, entry, place, FilePageIcon.Folder);
                            break;
                        default:
                            page = new FilePage(archiver, entry, place, FilePageIcon.Archive);
                            break;
                    }
                }
                else if (type != ArchiverType.None)
                {
                    SubFolderCount++;
                }
            }

            if (page != null)
            {
                page.ThumbnailChanged += Page_ThumbnailChanged;
                Pages.Add(page);
            }
        }

        //
        private void Page_ThumbnailChanged(object sender, System.Windows.Media.Imaging.BitmapSource e)
        {
            var page = sender as Page;
            if (page != null && e != null)
            {
                ThumbnailChanged?.Invoke(this, page);
            }
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
            _commandEngine.Initialize();
        }

        #endregion


        // 廃棄処理
        public async Task DisposeAsync()
        {
            RequestDispose();
            while (!_isDisposed)
            {
                await Task.Delay(100);
            }
        }

        // 前のページに戻る
        public void PrevPage(int step = 0)
        {
            var s = (step == 0) ? PageMode.Size() : step;
            RequestMovePosition(-s);
        }

        // 次のページへ進む
        public void NextPage(int step = 0)
        {
            var s = (step == 0) ? PageMode.Size() : step;
            RequestMovePosition(+s);
        }

        // 最初のページに移動
        public void FirstPage()
        {
            RequestSetPosition(_firstPosition, 1, true);
        }

        // 最後のページに移動
        public void LastPage()
        {
            RequestSetPosition(_lastPosition, -1, true);
        }

        // 指定ページに移動
        public void JumpPage(Page page)
        {
            int index = Pages.IndexOf(page);
            if (index >= 0)
            {
                var position = new PagePosition(index, 0);
                RequestSetPosition(position, 1, false);
            }
        }



        // ページ指定移動
        public void RequestSetPosition(PagePosition position, int direction, bool isPreLoad)
        {
            Debug.Assert(direction == 1 || direction == -1);

            if (Place == null) return;

            DisplayIndex = position.Index;

            var command = new BookCommandSetPage(this, new BookCommandSetPageArgs()
            {
                Position = position,
                Direction = direction,
                Size = PageMode.Size(),
                IsPreLoad = isPreLoad,
            });
            _commandEngine.Enqueue(command);
        }

        // ページ相対移動
        public void RequestMovePosition(int step)
        {
            if (Place == null) return;

            var command = new BookCommandMovePage(this, new BookCommandMovePageArgs()
            {
                Step = step,
            });
            _commandEngine.Enqueue(command);
        }

        // リフレッシュ
        public void RequestReflesh(bool isClear)
        {
            if (Place == null) return;

            var command = new BookCommandReflesh(this, new BookCommandRefleshArgs()
            {
                IsClear = isClear,
            });
            _commandEngine.Enqueue(command);
        }

        // ソート
        public void RequestSort()
        {
            if (Place == null) return;

            var command = new BookCommandSort(this, new BookCommandSortArgs());
            _commandEngine.Enqueue(command);
        }

        // ページ削除
        public void RequestRemove(Page page)
        {
            if (Place == null) return;

            var command = new BookCommandRemove(this, new BookCommandRemoveArgs()
            {
                Page = page,
            });
            _commandEngine.Enqueue(command);
        }


        // 終了処理
        public void RequestDispose()
        {
            if (Place == null) return;

            var command = new BookCommandDispose(this, new BookCommandDisposeArgs());
            _commandEngine.Enqueue(command);
        }



        #region Marker

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
        /// マーカーに移動
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="isLoop"></param>
        /// <returns></returns>
        public bool RequestJumpToMarker(int direction, bool isLoop, bool isIncludeTerminal)
        {
            Debug.Assert(direction == 1 || direction == -1);

            if (Place == null) return false;
            if (Pages == null || Pages.Count < 2) return false;

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

            if (list.Count == 0) return false;

            var index = GetViewPageindex();

            var target =
                direction > 0
                ? list.FirstOrDefault(e => e.Index > index) ?? (isLoop ? list.First() : null)
                : list.LastOrDefault(e => e.Index < index) ?? (isLoop ? list.Last() : null);

            if (target == null) return false;

            RequestSetPosition(new PagePosition(target.Index, 0), direction, false);
            return true;
        }

        #endregion





        // 表示ページ情報
        public class PageInfo
        {
            public PagePosition Position;
            public int Size;
        }

        // 表示ページコンテキスト
        public class ViewPageContext
        {
            // 基準となるページの場所
            public PagePosition Position { get; set; }

            // 進行方向
            public int Direction { get; set; } = 1;

            // 表示ページ数
            public int Size { get; set; }

            // 表示ページ情報
            public List<PageInfo> Infos { get; set; }

            // 表示ページコンテンツソース
            public List<ViewContentSource> ViewContentsSource { get; set; }
        }

        // 表示ページコンテキストのソース
        public class ViewPageContextSource
        {
            public PagePosition Position { get; set; }
            public int Direction { get; set; } = 1;
            public int Size { get; set; }
        }


        // コマンド処理
        internal async Task Dispose_Executed(BookCommandDisposeArgs param, CancellationToken token)
        {
            Dispose();
            await Task.Yield();
        }

        public void Dispose()
        {
            Terminate();
            _commandEngine.Terminate();
            _commandEngine = null;
            _isDisposed = true;
        }

        internal async Task Remove_Executed(BookCommandRemoveArgs param, CancellationToken token)
        {
            Remove(param.Page);
            await Task.Yield();
        }

        internal async Task Sort_Executed(BookCommandSortArgs param, CancellationToken token)
        {
            Sort();
            RequestSetPosition(_firstPosition, 1, true);
            await Task.Yield();
        }

        internal async Task Reflesh_Executed(BookCommandRefleshArgs param, CancellationToken token)
        {
            Reflesh(param.IsClear);
            await Task.Yield();
        }

        internal async Task SetPage_Executed(BookCommandSetPageArgs param, CancellationToken token)
        {
            var source = new ViewPageContextSource()
            {
                Position = param.Position,
                Direction = param.Direction,
                Size = param.Size,
            };
            await UpdateViewPageAsync(source, param.IsPreLoad, token);
        }


        private ViewPageContextSource GetViewPageContextSource(int step)
        {
            int delta = 0;

            if (Pages.Count == 0)
            {
                delta = step < 0 ? -1 : 1;
            }
            else if (step > 0)
            {
                for (int i = 0; i < step && i < _viewContext.Infos.Count; ++i)
                {
                    delta += _viewContext.Infos[i].Size;
                }
            }
            else if (_viewContext.Size == 2)
            {
                delta = step + 1;
            }
            else
            {
                delta = step;
            }

            return new ViewPageContextSource()
            {
                Position = _viewContext.Position + delta,
                Direction = step < 0 ? -1 : 1,
                Size = PageMode.Size(),
            };
        }

        private ViewPageContextSource GetViewPageContextSourceSimple(int step)
        {
            int index = _viewContext.Position.Index;
            int newIndex = ClampPageNumber(index + step);

            int direction = step < 0 ? -1 : 1;

            if (index == newIndex)
            {
                newIndex += direction;
            }

            var position = new PagePosition(newIndex, 0);

            return new ViewPageContextSource()
            {
                Position = position,
                Direction = direction,
                Size = PageMode.Size(),
            };
        }


        internal async Task MovePage_Executed(BookCommandMovePageArgs param, CancellationToken token)
        {
            if (param.Step < -2 || param.Step > 2)
            {
                await UpdateViewPageAsync(GetViewPageContextSourceSimple(param.Step), false, token);
            }
            else
            {
                await UpdateViewPageAsync(GetViewPageContextSource(param.Step), true, token);
            }
        }


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
            return (_firstPosition <= position && position <= _lastPosition);
        }

        // 表示ページ更新
        private async Task UpdateViewPageAsync(ViewPageContextSource source, bool isPreLoad, CancellationToken token)
        {
            // ページ終端を越えたか判定
            if (source.Position < _firstPosition)
            {
                App.Current.Dispatcher.Invoke(() => PageTerminated?.Invoke(this, -1));
                return;
            }
            else if (source.Position > _lastPosition)
            {
                App.Current.Dispatcher.Invoke(() => PageTerminated?.Invoke(this, +1));
                return;
            }

            // ページ数０の場合は表示コンテンツなし
            if (Pages.Count == 0)
            {
                App.Current.Dispatcher.Invoke(() => ViewContentsChanged?.Invoke(this, new ViewSource()));
                return;
            }

            if (FastAction)
            {
                _currentSource = source;
            }

            // view pages
            var viewPages = new List<Page>();
            for (int i = 0; i < source.Size; ++i)
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
            if (FastAction)
            {
                await Task.Run(() => Task.WaitAll(tlist.ToArray(), 100, token));
                //await Task.Yield();
            }
            else
            {
                await Task.WhenAll(tlist);
                //await Task.Run(() => Task.WaitAll(tlist.ToArray(), token));
            }

            // update contents
            _viewContext = CreateViewPageContext(source);

            // task cancel?
            token.ThrowIfCancellationRequested();

            // notice ViewContentsChanged
            App.Current?.Dispatcher.Invoke(() => ViewContentsChanged?.Invoke(this, new ViewSource()
            {
                Type = ViewSourceType.Content,
                Sources = _viewContext.ViewContentsSource,
                Direction = _viewContext.Direction
            }));

            // change page
            DisplayIndex = _viewContext.Position.Index;

            // notice PropertyChanged
            PageChanged?.Invoke(this, _viewContext.Position.Index);

            // ページ破棄
            if (!FastAction)
            {
                if (!AllowPreLoad) ClearAllPages();
            }
            else
            {
                if (!AllowPreLoad) ClearAllPages(viewPages.Where(e => !e.IsContentAlived).ToList());
            }

            // 間に合わなかった表示更新を登録
            if (FastAction)
            {
                _oldSource = _currentSource;
                foreach (var item in _viewContext.ViewContentsSource.Where(i => i.Source == null))
                {
                    Debug.WriteLine($"Delay! {item.Page.FileName}");

                    var page = Pages[item.Page.Index];
                    // TODO: 厳密なスレッドタイミングの対処
                    if (page.IsContentAlived)
                    {
                        UpdateViewContents(source);
                    }
                    else
                    {
                        // TODO: キャンセルされた時にイベントが発行されない問題
                        // 発行されなくてもいいがイベント登録が解除されないとまずい
                        // LoadTaskComplete?
                        // グローバルなイベントにしてそれで判断？
                        page.Loaded += Page_Loaded;
                    }
                }
            }
        }

        ViewPageContextSource _currentSource;
        ViewPageContextSource _oldSource;

        private void Page_Loaded(object sender, bool e)
        {
            var page = (Page)sender;
            page.Loaded -= Page_Loaded;

            Debug.WriteLine("DelayLoad..");

            if (!e) return;

            UpdatePreLoadStatus(page);

            if (_currentSource != _oldSource)
            {
                Debug.WriteLine("DelayLoad Canceled!");
                return;
            }

            UpdateViewContents(_oldSource);
        }

        //
        public void UpdateViewContents(ViewPageContextSource source)
        {
            // update contents
            _viewContext = CreateViewPageContext(source);

            // notice ViewContentsChanged
            // TODO: 表示の更新だけという通知。必要？
            App.Current?.Dispatcher.Invoke(() => ViewContentsChanged?.Invoke(this, new ViewSource()
            {
                Type = ViewSourceType.Content,
                Sources = _viewContext.ViewContentsSource,
                Direction = _viewContext.Direction
            }));

            // TODO: ページ番号とかの整合性
            // change page
            //DisplayIndex = _viewContext.Position.Index;

            // notice PropertyChanged
            //PageChanged?.Invoke(this, _viewContext.Position.Index);
        }

        // 見開きモードでも単独表示するべきか判定
        private bool IsSoloPage(int index)
        {
            if (IsSupportedWidePage && Pages[index].IsWide) return true;
            if (IsSupportedSingleFirstPage && index == 0) return true;
            if (IsSupportedSingleLastPage && index == Pages.Count - 1) return true;
            return false;
        }

        // 分割モード有効判定
        private bool IsEnableDividePage(int index)
        {
            return (PageMode == PageMode.SinglePage && IsSupportedDividePage && Pages[index].IsWide);
        }

        // 表示コンテンツソースと、それに対応したコンテキスト作成
        private ViewPageContext CreateViewPageContext(ViewPageContextSource source)
        {
            var infos = new List<PageInfo>();

            {
                PagePosition position = source.Position;

                for (int id = 0; id < source.Size; ++id)
                {
                    if (!IsValidPosition(position) || Pages[position.Index] == null) break;

                    int size = 2;
                    if (IsEnableDividePage(position.Index))
                    {
                        size = 1;
                    }
                    else
                    {
                        position.Part = 0;
                    }

                    infos.Add(new PageInfo() { Position = position, Size = size });

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
            var contentsSource = new List<ViewContentSource>();
            foreach (var v in infos)
            {
                contentsSource.Add(new ViewContentSource(Pages[v.Position.Index], v.Position, v.Size, BookReadOrder));
            }

            // 先読み可能判定
            UpdatePreLoadStatus(contentsSource);

#if false
            if (PreLoadMode == PreLoadMode.AutoPreLoad)
            {
                foreach (var content in contentsSource)
                {
                    if (content.Page.Width * content.Page.Height > PreLoadLimitSize)
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
            }
#endif

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
                contentsSource.Add(new ViewContentSource(Pages[position.Index], position, 2, BookReadOrder));
            }

            // 新しいコンテキスト
            var context = new ViewPageContext();
            context.Position = infos[0].Position;
            context.Size = infos.Count;
            context.Direction = source.Direction;
            context.Infos = infos;
            context.ViewContentsSource = contentsSource;

            return context;
        }

        // 先読み判定更新
        private void UpdatePreLoadStatus(List<ViewContentSource> contentsSource)
        {
            if (PreLoadMode != PreLoadMode.AutoPreLoad) return;

            foreach (var content in contentsSource)
            {
                UpdatePreLoadStatus(content.Page);
            }
        }

        //
        private void UpdatePreLoadStatus(Page page)
        {
            if (PreLoadMode != PreLoadMode.AutoPreLoad) return;

            // 判定
            if (page.Width * page.Height > PreLoadLimitSize)
            {
                Debug.WriteLine("PreLoad: Disabled");
                _canPreLoadCount = 0;
                _canPreLoad = false;
            }
            else if (page.IsContentAlived)
            {
                _canPreLoadCount++;
                if (!_canPreLoad && _canPreLoadCount > 3) // 一定回数連続で規定サイズ以下なら先読み有効
                {
                    Debug.WriteLine("PreLoad: Enabled");
                    _canPreLoad = true;
                }
            }
        }


        // 不要ページコンテンツの削除を行う
        private void CleanupPages(ViewPageContextSource source)
        {
            // コンテンツを保持するページ収集
            var keepPages = new List<Page>();
            for (int offset = -_keepPagePrevSize; offset <= _keepPageNextSize; ++offset)
            {
                int index = source.Position.Index + offset;
                if (0 <= index && index < Pages.Count)
                {
                    keepPages.Add(Pages[index]);
                }
            }

            // 不要コンテンツ破棄
            foreach (var page in _keepPages)
            {
                if (!keepPages.Contains(page))
                {
                    page.Close();
                }
            }

            // 保持ページ更新
            _keepPages = keepPages;
        }


        // 全ページコンテンツの削除を行う
        private void ClearAllPages()
        {
            foreach (var page in _keepPages)
            {
                page.Close();
            }

            // 保持ページ更新
            _keepPages = new List<Page>();
        }

        // 全ページコンテンツの削除を行う
        private void ClearAllPages(List<Page> keeps)
        {
            foreach (var page in _keepPages.Where(e => !keeps.Contains(e)))
            {
                page.Close();
            }

            // 保持ページ更新
            _keepPages = keeps; // new List<Page>();
        }


        // 先読み
        private void PreLoad(ViewPageContextSource source)
        {
            if (!AllowPreLoad) return;

            var preLoadPages = new List<Page>();

            var size = source.Direction < 0 ? _keepPagePrevSize : _keepPageNextSize;

            for (int offset = 0; offset <= size; offset++)
            {
                int index = source.Position.Index + (source.Direction < 0 ? -offset : offset);
                if (0 <= index && index < Pages.Count)
                {
                    Debug.Assert(_keepPages.Contains(Pages[index])); // 念のため
                    Pages[index].Open(QueueElementPriority.Default, Page.OpenOption.WeakPriority);

                    if (!_keepPages.Contains(Pages[index]))
                    {
                        _keepPages.Add(Pages[index]);
                    }
                }
            }
        }

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



        // ページの削除
        private void Remove(Page page)
        {
            if (Pages.Count <= 0) return;

            int index = Pages.IndexOf(page);
            if (index < 0) return;

            Pages.RemoveAt(index);

            PagesNumbering();

            PageRemoved?.Invoke(this, page);

            index = ClampPageNumber(index);
            RequestSetPosition(new PagePosition(index, 0), 1, true);
        }


        // 表示の再構築
        private void Reflesh(bool clear)
        {
            if (Place == null) return;

            if (clear)
            {
                _keepPages.ForEach(e => e?.Close());
            }

            RequestSetPosition(_viewContext.Position, 1, true);
        }


        // 廃棄処理
        private void Terminate()
        {
            _viewContext = null;

            Pages?.ForEach(e => e?.Close());
            Pages?.Clear();

            _archivers?.ForEach(e => e.Dispose());
            _archivers?.Clear();

            _trashBox?.Clear();
        }


        #region Memento

        /// <summary>
        /// 保存設定
        /// </summary>
        [DataContract]
        public class Memento
        {
            // フォルダの場所
            [DataMember(EmitDefaultValue = false)]
            public string Place { get; set; }

            // 名前
            public string Name => Place.EndsWith(@":\") ? Place : System.IO.Path.GetFileName(Place);

            // 現在ページ(旧)
            [DataMember(EmitDefaultValue = false)]
            public string BookMark { get; set; } // no used

            // 現在ページ
            [DataMember(EmitDefaultValue = false)]
            public string Page { get; set; }

            // 1ページ表示 or 2ページ表示
            [DataMember(Name = "PageModeV2")]
            public PageMode PageMode { get; set; }

            // 右開き or 左開き
            [DataMember]
            public PageReadOrder BookReadOrder { get; set; }

            // 横長ページ分割 (1ページモード)
            [DataMember]
            public bool IsSupportedDividePage { get; set; }

            // 最初のページを単独表示 
            [DataMember]
            public bool IsSupportedSingleFirstPage { get; set; }

            // 最後のページを単独表示
            [DataMember]
            public bool IsSupportedSingleLastPage { get; set; }

            // 横長ページを2ページ分とみなす(2ページモード)
            [DataMember]
            public bool IsSupportedWidePage { get; set; }

            // フォルダの再帰
            [DataMember]
            public bool IsRecursiveFolder { get; set; }

            // ページ並び順
            [DataMember]
            public PageSortMode SortMode { get; set; }

            // 最終アクセス日
            [DataMember(Order = 12, EmitDefaultValue = false)]
            public DateTime LastAccessTime { get; set; }

            /// <summary>
            /// construcgtor
            /// </summary>
            private void Constructor()
            {
                PageMode = PageMode.SinglePage;
                IsSupportedWidePage = true;
            }

            //
            public Memento()
            {
                Constructor();
            }

            //
            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
            }

            [OnDeserialized]
            private void Deserialized(StreamingContext c)
            {
                Page = Page ?? BookMark;
                BookMark = null;
            }

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
                // フォルダの再帰
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
            // このmementoは履歴とデフォルト設定の２つに使われるが、デフォルト設定には本の場所やページは不要
            public void ValidateForDefault()
            {
                Place = null;
                Page = null;
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
            PageMode = memento.PageMode;
            BookReadOrder = memento.BookReadOrder;
            IsSupportedDividePage = memento.IsSupportedDividePage;
            IsSupportedSingleFirstPage = memento.IsSupportedSingleFirstPage;
            IsSupportedSingleLastPage = memento.IsSupportedSingleLastPage;
            IsSupportedWidePage = memento.IsSupportedWidePage;
            IsRecursiveFolder = memento.IsRecursiveFolder;
            SortMode = memento.SortMode;
        }
    }

    #endregion


    /// <summary>
    /// Book設定項目番号
    /// </summary>
    public enum BookMementoBit
    {
        // 現在ページ
        Page,

        // 1ページ表示 or 2ページ表示
        PageMode,

        // 右開き or 左開き
        BookReadOrder,

        // 横長ページ分割 (1ページモード)
        IsSupportedDividePage,

        // 最初のページを単独表示 
        IsSupportedSingleFirstPage,

        // 最後のページを単独表示
        IsSupportedSingleLastPage,

        // 横長ページを2ページ分とみなす(2ページモード)
        IsSupportedWidePage,

        // フォルダの再帰
        IsRecursiveFolder,

        // ページ並び順
        SortMode,
    };



    /// <summary>
    /// Book設定フィルタ
    /// </summary>
    [DataContract]
    public class BookMementoFilter
    {
        [DataMember]
        public Dictionary<BookMementoBit, bool> Flags { get; set; }

        //
        public BookMementoFilter(bool def = false)
        {
            Flags = Enum.GetValues(typeof(BookMementoBit)).OfType<BookMementoBit>().ToDictionary(e => e, e => def);
        }

        /// <summary>
        /// デシリアライズ終端処理
        /// </summary>
        /// <param name="c"></param>
        [OnDeserialized]
        private void Deserialized(StreamingContext c)
        {
            // 項目数の保証
            foreach (BookMementoBit key in Enum.GetValues(typeof(BookMementoBit)))
            {
                if (!Flags.ContainsKey(key)) Flags.Add(key, true);
            }
        }
    }

}
