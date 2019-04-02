using NeeView.Collections.Generic;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;



namespace NeeView
{
    /// <summary>
    /// 本
    /// </summary>
    public partial class Book : IDisposable
    {
#if false

        #region 開発用

        public static TraceSource Log = Logger.CreateLogger(nameof(Book));

        #endregion

        public static Book Default { get; private set; }

        #region Fields

        // シリアル
        public static int _serial = 0;

        // テンポラリコンテンツ用ゴミ箱
        public TrashBox _trashBox = new TrashBox();

        // 初期化オプション
        private BookLoadSetting _setting;

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

        // 表示中ページ
        private List<Page> _viewPages = new List<Page>();

        // リソースを保持しておくページ
        private List<Page> _keepPages = new List<Page>();

        // サムネイル寿命管理
        private PageThumbnailPool _thumbnaulPool = new PageThumbnailPool();

        // ページマップ
        private Dictionary<string, Page> _pageMap = new Dictionary<string, Page>();

        // JOBリクエスト
        private PageContentJobClient _jobClient = new PageContentJobClient("View", JobCategories.PageViewContentJobCategory);

        // メモリ管理
        private BookMemoryService _bookMemoryService = new BookMemoryService();

        // 先読み
        private BookAhead _ahead;


        private object _lock = new object();

        #endregion

        #region Constructors

        /// <summary>
        /// constructor
        /// </summary>
        public Book()
        {
            Serial = ++_serial;
            _ahead = new BookAhead(_bookMemoryService);

            Book.Default = this;
        }

        #endregion

        #region Events

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

        // ファイル削除された
        public event EventHandler<PageChangedEventArgs> PageRemoved;

        #endregion

        #region Properties

        // Log用 シリアル番号
        public int Serial { get; private set; }

        // 最初のコンテンツ表示フラグ
        private ManualResetEventSlim _contentLoaded = new ManualResetEventSlim();
        public ManualResetEventSlim ContentLoaded => _contentLoaded;

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
                    RequestRefresh(this, false);
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
                    RequestRefresh(this, false);
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
                    RequestRefresh(this, false);
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
                    RequestRefresh(this, false);
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
                    RequestRefresh(this, false);
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
                    RequestRefresh(this, false);
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
        public string Address { get; private set; }

        // この本はディレクトリ？
        public bool IsDirectory { get; private set; }

        // この本のアーカイバ
        public ArchiveEntryCollection ArchiveEntryCollection { get; private set; }

        // メディアアーカイバ？
        public bool IsMedia => ArchiveEntryCollection?.Archiver is MediaArchiver;

        // ページマークアーカイバ？
        public bool IsPagemarkFolder => ArchiveEntryCollection?.Archiver is PagemarkArchiver;

        // 開始ページ
        public string StartEntry { get; private set; }

        // ページ コレクション
        public List<Page> Pages { get; private set; } = new List<Page>();

        // 表示されているページコレクション
        public ViewPageCollection ViewPageCollection => _viewPageCollection;

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

        // メモリ管理
        public BookMemoryService BookMemoryService => _bookMemoryService;

        #endregion

        #region Methods

        public string GetArchiverDetail()
        {
            var archiver = ArchiveEntryCollection?.Archiver;
            if (archiver == null)
            {
                return null;
            }

            var inner = archiver.Parent != null ? Properties.Resources.WordInner + " " : "";

            var extension = LoosePath.GetExtension(archiver.EntryName);

            var archiverType = ArchiverManager.Current.GetArchiverType(archiver);
            switch (archiverType)
            {
                case ArchiverType.FolderArchive:
                    return Properties.Resources.ArchiveFormatFolder;
                case ArchiverType.ZipArchiver:
                case ArchiverType.SevenZipArchiver:
                case ArchiverType.SusieArchiver:
                    return inner + Properties.Resources.ArchiveFormatCompressedFile + $"({extension})";
                case ArchiverType.PdfArchiver:
                    return inner + Properties.Resources.ArchiveFormatPdf + $"({extension})";
                case ArchiverType.MediaArchiver:
                    return inner + Properties.Resources.ArchiveFormatMedia + $"({extension})";
                case ArchiverType.PagemarkArchiver:
                    return Properties.Resources.ArchiveFormatPagemark;
                default:
                    return Properties.Resources.ArchiveFormatUnknown;
            }
        }

        public string GetDetail()
        {
            string text = "";
            text += GetArchiverDetail() + "\n";
            text += string.Format(Properties.Resources.BookAddressInfoPage, Pages.Count);
            return text;
        }

        public string GetFolderPlace()
        {
            return ArchiveEntryCollection.GetFolderPlace();
        }

        #region 本の初期化

        /// <summary>
        /// フォルダーの読込
        /// </summary>
        public async Task LoadAsync(BookAddress address, ArchiveEntryCollectionMode archiveRecursiveMode, BookLoadSetting setting, CancellationToken token)
        {
            try
            {
                Log.TraceEvent(TraceEventType.Information, Serial, $"Load: {address.Address}");
                Log.Flush();

                await LoadCoreAsync(address, archiveRecursiveMode, setting, token);
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
        public async Task LoadCoreAsync(BookAddress address, ArchiveEntryCollectionMode archiveRecursiveMode, BookLoadSetting setting, CancellationToken token)
        {
            Debug.Assert(Address == null);
            ////Debug.WriteLine($"OPEN: {address.Place}, {address.EntryName}, {address.Archiver.Path}");

            _setting = setting.Clone();

            var start = address.EntryName;

            // リカーシブオプションフラグ
            if (_setting.Options.HasFlag(BookLoadOption.NotRecursive))
            {
                IsRecursiveFolder = false;
                _setting.Options &= ~BookLoadOption.Recursive;
            }
            else if (_setting.Options.HasFlag(BookLoadOption.Recursive))
            {
                IsRecursiveFolder = true;
            }

            // リカーシブフラグ
            if (IsRecursiveFolder)
            {
                _setting.Options |= BookLoadOption.Recursive;
            }

            // ページ生成
            await CreatePageCollection(address.Address.SimplePath, archiveRecursiveMode, token);

            // 自動再帰処理
            if (ArchiveEntryCollection.Mode != ArchiveEntryCollectionMode.IncludeSubArchives && this.Pages.Count == 0 && _setting.Options.HasFlag(BookLoadOption.AutoRecursive))
            {
                var entries = await ArchiveEntryCollection.GetEntriesWhereBookAsync(token);
                if (entries.Count == 1)
                {
                    _setting.Options |= BookLoadOption.Recursive;
                    await CreatePageCollection(address.Address.SimplePath, archiveRecursiveMode, token);
                }
            }

            // 事前展開処理
            await PreExtractAsync(token);

            // Pages initialize
            // TODO: ページ生成と同時に行うべき
            _pageMap.Clear();
            var prefix = GetPagesPrefix();
            foreach (var page in Pages)
            {
                page.Prefix = prefix;
                page.Loaded += Page_Loaded;
                page.Thumbnail.Touched += Thumbnail_Touched;
                _pageMap[page.EntryFullName] = page;
            }

            // 初期ソート
            Sort();

            // スタートページ取得
            PagePosition position = FirstPosition();
            int direction = 1;
            if ((_setting.Options & BookLoadOption.FirstPage) == BookLoadOption.FirstPage)
            {
                position = FirstPosition();
                direction = 1;
            }
            else if ((_setting.Options & BookLoadOption.LastPage) == BookLoadOption.LastPage)
            {
                position = LastPosition();
                direction = -1;
            }
            else
            {
                int index = !string.IsNullOrEmpty(start) ? Pages.FindIndex(e => e.EntryFullName == start) : 0;
                if (index < 0)
                {
                    this.NotFoundStartPage = start;
                }
                position = index >= 0 ? new PagePosition(index, 0) : FirstPosition();
                direction = 1;
            }

            // 開始ページ記憶
            StartEntry = Pages.Count > 0 ? Pages[position.Index].EntryFullName : null;

            // 有効化
            Address = ArchiveEntryCollection.Path;
            IsDirectory = ArchiveEntryCollection.Archiver is FolderArchive;

            // 初期ページ設定
            RequestSetPosition(this, position, direction);
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
        /// ページ生成
        /// </summary>
        private async Task CreatePageCollection(string place, ArchiveEntryCollectionMode archiveRecursiveMode, CancellationToken token)
        {
            var collectMode = _setting.Options.HasFlag(BookLoadOption.Recursive) ? ArchiveEntryCollectionMode.IncludeSubArchives : ArchiveEntryCollectionMode.CurrentDirectory;
            var collectModeIfArchive = _setting.Options.HasFlag(BookLoadOption.Recursive) ? ArchiveEntryCollectionMode.IncludeSubArchives : archiveRecursiveMode;
            var collectOption = ArchiveEntryCollectionOption.None;
            this.ArchiveEntryCollection = new ArchiveEntryCollection(place, collectMode, collectModeIfArchive, collectOption);

            List<ArchiveEntry> entries;
            switch (_setting.BookPageCollectMode)
            {
                case BookPageCollectMode.Image:
                    entries = await ArchiveEntryCollection.GetEntriesWhereImageAsync(token);
                    break;
                case BookPageCollectMode.ImageAndBook:
                    entries = await ArchiveEntryCollection.GetEntriesWhereImageAndArchiveAsync(token);
                    break;
                case BookPageCollectMode.All:
                default:
                    entries = await ArchiveEntryCollection.GetEntriesWherePageAllAsync(token);
                    break;
            }

            var bookPrefix = LoosePath.TrimDirectoryEnd(place);
            this.Pages = entries.Select(e => CreatePage(bookPrefix, e)).ToList();
        }

        /// <summary>
        /// ページ作成
        /// </summary>
        /// <param name="entry">ファイルエントリ</param>
        /// <returns></returns>
        private Page CreatePage(string bookPrefix, ArchiveEntry entry)
        {
            Page page;

            if (entry.IsImage())
            {
                if (entry.Archiver is MediaArchiver)
                {
                    page = new MediaPage(bookPrefix, entry);
                }
                else if (entry.Archiver is PdfArchiver)
                {
                    page = new PdfPage(bookPrefix, entry);
                }
                else if (BookProfile.Current.IsEnableAnimatedGif && LoosePath.GetExtension(entry.EntryName) == ".gif")
                {
                    page = new AnimatedPage(bookPrefix, entry);
                }
                else
                {
                    page = new BitmapPage(bookPrefix, entry);
                }
            }
            else if (entry.IsBook())
            {
                page = new ArchivePage(bookPrefix, entry);
            }
            else
            {
                var type = entry.IsDirectory ? ArchiverType.FolderArchive : ArchiverManager.Current.GetSupportedType(entry.EntryName);
                switch (type)
                {
                    case ArchiverType.None:
                        if (BookProfile.Current.IsAllFileAnImage)
                        {
                            entry.IsIgnoreFileExtension = true;
                            page = new BitmapPage(bookPrefix, entry);
                        }
                        else
                        {
                            page = new FilePage(bookPrefix, entry, FilePageIcon.File);
                        }
                        break;
                    case ArchiverType.FolderArchive:
                        page = new FilePage(bookPrefix, entry, FilePageIcon.Folder);
                        break;
                    default:
                        page = new FilePage(bookPrefix, entry, FilePageIcon.Archive);
                        break;
                }
            }

            return page;
        }

        // 名前の最長一致文字列取得
        private string GetPagesPrefix()
        {
            if (Pages == null || Pages.Count == 0) return "";

            string s = Pages[0].EntryFullName;
            foreach (var page in Pages)
            {
                s = GetStartsWith(s, page.EntryFullName);
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


        // 事前展開(仮)
        // TODO: 事前展開の非同期化。ページアクセスをトリガーにする
        private async Task PreExtractAsync(CancellationToken token)
        {
            var archivers = Pages
                .Select(e => e.Entry.Archiver)
                .Distinct()
                .Where(e => e != null && !e.IsFileSystem)
                .ToList();

            foreach (var archiver in archivers)
            {
                if (archiver.CanPreExtract())
                {
                    Debug.WriteLine($"PreExtract: EXTRACT {archiver.EntryName}");
                    await archiver.PreExtractAsync(token);
                }
            }
        }



        // コマンドエンジン
        private BookCommandEngine _commandEngine = new BookCommandEngine();

        // 開始
        // ページ設定を行うとコンテンツ読み込みが始まるため、ロードと分離した
        public void Start()
        {
            Debug.Assert(Address != null);
            _commandEngine.Name = $"BookJobEngine: {this.Address}";
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
            RequestSetPosition(this, FirstPosition(), 1);
        }

        // 最後のページに移動
        public void LastPage()
        {
            RequestSetPosition(this, LastPosition(), -1);
        }

        // 指定ページに移動
        public void JumpPage(Page page)
        {
            int index = Pages.IndexOf(page);
            if (index >= 0)
            {
                var position = new PagePosition(index, 0);
                RequestSetPosition(this, position, 1);
            }
        }


        /// <summary>
        /// ページ指定移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="position">ページ位置</param>
        /// <param name="direction">読む方向(+1 or -1)</param>
        public void RequestSetPosition(object sender, PagePosition position, int direction)
        {
            Debug.Assert(direction == 1 || direction == -1);

            if (Address == null) return;

            DisplayIndex = position.Index;

            var range = new PageDirectionalRange(position, direction, PageMode.Size());
            var command = new BookCommandAction(sender, async (s, token) => await SetPage_Executed(s, range, token), 0);
            _commandEngine.Enqueue(command);
        }

        // ページ相対移動
        public void RequestMovePosition(object sender, int step)
        {
            if (Address == null) return;

            var command = new BookCommandJoinAction(sender, async (s, value, token) => await MovePage_Executed(s, value, token), step, 0);
            _commandEngine.Enqueue(command);
        }

        // リフレッシュ
        public void RequestRefresh(object sender, bool isClear)
        {
            if (Address == null) return;

            var command = new BookCommandAction(sender, Refresh_Executed, 1);
            _commandEngine.Enqueue(command);
        }

        // ソート
        public void RequestSort(object sender)
        {
            if (Address == null) return;

            var command = new BookCommandAction(sender, Sort_Executed, 2);
            _commandEngine.Enqueue(command);
        }

        // ページ削除
        public void RequestRemove(object sender, Page page)
        {
            if (Address == null) return;

            var command = new BookCommandAction(sender, async (s, token) => await Remove_Executed(s, page, token), 3);
            _commandEngine.Enqueue(command);
        }

        // 表示の再構築
        private void Refresh(bool clear)
        {
            if (Address == null) return;
            RequestSetPosition(this, _viewPageCollection.Range.Min, 1);
        }

        // 終了処理
        private BookCommand RequestDispose(object sender)
        {
            if (Address == null) return null;

            var command = new BookCommandAction(sender, Dispose_Executed, 4);
            _commandEngine.Enqueue(command);

            return command;
        }

        #endregion

        #region コマンド実行

        private async Task Dispose_Executed(object sender, CancellationToken token)
        {
            Dispose();
            await Task.CompletedTask;
        }

        internal async Task Remove_Executed(object sender, Page page, CancellationToken token)
        {
            Remove(page);
            await Task.CompletedTask;
        }

        internal async Task Sort_Executed(object sender, CancellationToken token)
        {
            var page = GetViewPage();

            Sort();

            var pagePosition = new PagePosition(GetIndex(page), 0);
            RequestSetPosition(this, pagePosition, 1);

            await Task.CompletedTask;
        }

        internal async Task Refresh_Executed(object sender, CancellationToken token)
        {
            Refresh(false);
            await Task.CompletedTask;
        }

        internal async Task SetPage_Executed(object sender, PageDirectionalRange source, CancellationToken token)
        {
            await UpdateViewPageAsync(source, sender, token);
        }


        internal async Task MovePage_Executed(object sender, int step, CancellationToken token)
        {
            var viewRange = _viewPageCollection.Range;

            var direction = step < 0 ? -1 : 1;

            var pos = Math.Abs(step) == PageMode.Size() ? viewRange.Next(direction) : viewRange.Move(step);
            if (pos < FirstPosition() && !viewRange.IsContains(FirstPosition()))
            {
                pos = new PagePosition(0, direction < 0 ? 1 : 0);
            }
            else if (pos > LastPosition() && !viewRange.IsContains(LastPosition()))
            {
                pos = new PagePosition(Pages.Count - 1, direction < 0 ? 1 : 0);
            }

            var range = new PageDirectionalRange(pos, direction, PageMode.Size());

            await UpdateViewPageAsync(range, sender, token);
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
        public Page GetPage(string name) => Pages.FirstOrDefault(e => e.EntryFullName == name);

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
            return BookProfile.Current.PreLoadSize > 0;
        }

        // ページ番号のクランプ
        public int ClampPageNumber(int index)
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
        private async Task UpdateViewPageAsync(PageDirectionalRange source, object sender, CancellationToken token)
        {
            // ページ終端を越えたか判定
            if (source.Position < FirstPosition())
            {
                AppDispatcher.Invoke(() => PageTerminated?.Invoke(this, new PageTerminatedEventArgs(-1)));
                return;
            }
            else if (source.Position > LastPosition())
            {
                AppDispatcher.Invoke(() => PageTerminated?.Invoke(this, new PageTerminatedEventArgs(+1)));
                return;
            }

            // ページ数０の場合は表示コンテンツなし
            if (Pages.Count == 0)
            {
                AppDispatcher.Invoke(() => ViewContentsChanged?.Invoke(this, new ViewPageCollectionChangedEventArgs(new ViewPageCollection())));
                return;
            }

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

            // pre load
            _ahead.Clear();
            CancelUpdateNextContents();
            _aheadPageRange = CreateAheadPageRange(source);
            var aheadPages = CreatePagesFromRange(_aheadPageRange, viewPages);

            var loadPages = viewPages.Concat(aheadPages).Distinct().ToList();

            // update content lock
            var unloadPages = _keepPages.Except(viewPages).ToList();
            foreach (var page in unloadPages)
            {
                page.State = PageContentState.None;
            }
            foreach (var (page, index) in viewPages.ToTuples())
            {
                page.State = PageContentState.View;
            }
            _keepPages = loadPages;

            // update contents
            this.PageChangeCount++;
            this.IsPageTerminated = source.Max >= LastPosition();
            _viewPageSender = sender;
            _viewPageRange = source;

            _bookMemoryService.SetReference(viewPages.First().Index);
            _jobClient.Order(viewPages);
            _ahead.Order(aheadPages);

            using (var loadWaitCancellation = new CancellationTokenSource())
            using (var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, loadWaitCancellation.Token))
            {
                // wait load (max 5sec.)
                var timeout = BookProfile.Current.CanPrioritizePageMove() ? 100 : 5000;
                await _jobClient.WaitAsync(viewPages, timeout, linkedTokenSource.Token);
                loadWaitCancellation.Cancel();
            }

            // task cancel?
            token.ThrowIfCancellationRequested();

            UpdateViewContents();
            UpdateNextContents();
        }


        /// <summary>
        /// ページロード完了イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_Loaded(object sender, EventArgs e)
        {
            var page = (Page)sender;

            _bookMemoryService.AddPageContent(page);

            _ahead.OnPageLoaded(this, new PageChangedEventArgs(page));

            ////if (!BookProfile.Current.CanPrioritizePageMove()) return;

            // 非同期なので一旦退避
            var now = _viewPageCollection;

            if (now?.Collection == null) return;

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
            if (_disposedValue)
            {
                return;
            }

            lock (_lock)
            {
                // update contents
                var sender = _viewPageSender;
                var viewContent = CreateViewPageContext(_viewPageRange);
                if (viewContent == null)
                {
                    return;
                }

                _viewPageCollection = viewContent;
                _nextPageCollection = viewContent;
                ////Debug.WriteLine($"now: {_viewPageCollection.Range}");

                // change page
                this.DisplayIndex = viewContent.Range.Min.Index;

                // notice ViewContentsChanged
                AppDispatcher.Invoke(() => ViewContentsChanged?.Invoke(sender, new ViewPageCollectionChangedEventArgs(viewContent)));

                // コンテンツ準備完了
                ContentLoaded.Set();
            }
        }

        /// <summary>
        /// 表示中ページフラグ更新
        /// </summary>
        public void UpdateViewPages(object sender, ViewPageCollectionChangedEventArgs args)
        {
            var viewPages = args?.ViewPageCollection?.Collection.Where(e => e != null).Select(e => e.Page) ?? new List<Page>();
            var hidePages = _viewPages.Where(e => !viewPages.Contains(e));

            foreach (var page in viewPages)
            {
                page.IsVisibled = true;
            }

            foreach (var page in hidePages)
            {
                page.IsVisibled = false;
            }

            _viewPages = viewPages.ToList();
        }


        #region 先読みコンテンツ更新

        private PageDirectionalRange _aheadPageRange;
        private CancellationTokenSource _aheadCancellationTokenSource;
        private Task _aheadTask;

        public void CancelUpdateNextContents()
        {
            lock (_lock)
            {
                if (_aheadTask != null)
                {
                    Debug.WriteLine($"> CancelUpdateViewContents");
                    _aheadCancellationTokenSource?.Cancel();
                    _aheadTask = null;
                }
                _nextPageCollection = new ViewPageCollection();
            }
        }

        public void UpdateNextContents()
        {
            if (_aheadTask != null && !_aheadTask.IsCompleted)
            {
                ////Debug.WriteLine($"> RunUpdateViewContents: skip.");
                return;
            }

            ////Debug.WriteLine($"> RunUpdateViewContents: run...");
            _aheadCancellationTokenSource?.Cancel();
            _aheadCancellationTokenSource = new CancellationTokenSource();
            var token = _aheadCancellationTokenSource.Token;
            _aheadTask = Task.Run(() =>
            {
                try
                {
                    UpdateNextContentsInner(token);
                    ////Debug.WriteLine($"> RunUpdateViewContents: done.");
                }
                catch (Exception ex)
                {
                    ////Debug.WriteLine($"> RunUpdateViewContents: {ex.Message}");
                }
            });
        }

        private void UpdateNextContentsInner(CancellationToken token)
        {
            if (!_nextPageCollection.IsValid) return;

            int turn = 0;
            RETRY:

            if (_disposedValue) return;
            if (token.IsCancellationRequested) return;

            ViewPageCollection next;
            lock (_lock)
            {
                next = CreateViewPageContext(GetNextRange(_nextPageCollection.Range));
                var range = _aheadPageRange.Add(_viewPageRange.Position);
                if (next.IsValid && range.IsContains(new PagePosition(next.Range.Position.Index, 0)))
                {
                    _nextPageCollection = next;
                }
                else
                {
                    return;
                }
            }

            ////Debug.WriteLine($"next({turn++}): {next.Range}");
            NextContentsChanged?.Invoke(this, new ViewPageCollectionChangedEventArgs(next));

            ////Thread.Sleep(1000); // ##

            goto RETRY;
        }

        private PageDirectionalRange GetNextRange(PageDirectionalRange previous)
        {
            // 先読みコンテンツ領域計算
            var position = previous.Next();
            var direction = previous.Direction;
            var range = new PageDirectionalRange(position, direction, PageMode.Size());

            return range;
        }

        #endregion 先読みコンテンツ更新


        // ページのワイド判定
        private bool IsWide(Page page)
        {
            return page.Width > page.Height * BookProfile.Current.WideRatio;
        }

        // 見開きモードでも単独表示するべきか判定
        private bool IsSoloPage(int index)
        {
            if (IsSupportedSingleFirstPage && index == 0) return true;
            if (IsSupportedSingleLastPage && index == Pages.Count - 1) return true;
            if (Pages[index] is ArchivePage) return true;
            if (IsSupportedWidePage && IsWide(Pages[index])) return true;
            return false;
        }

        // 分割モード有効判定
        private bool IsEnableDividePage(int index)
        {
            return (PageMode == PageMode.SinglePage && !IsMedia && IsSupportedDividePage && IsWide(Pages[index]));
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
                var viewPage = new ViewPage(Pages[v.Position.Index], v);

                // メディア用。最終ページからの表示指示の場合のフラグ設定
                if (IsMedia && _setting.Options.HasFlag(BookLoadOption.LastPage))
                {
                    viewPage.IsLastStart = true;
                }

                contentsSource.Add(viewPage);
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

            // 新しいコンテキスト
            var context = new ViewPageCollection(new PageDirectionalRange(infos, source.Direction), contentsSource);
            return context;
        }

        /// <summary>
        /// 先読みページ範囲を求める
        /// </summary>
        private PageDirectionalRange CreateAheadPageRange(PageDirectionalRange source)
        {
            if (!AllowPreLoad() || BookProfile.Current.PreLoadSize < 1)
            {
                return PageDirectionalRange.Empty;
            }

            int index = source.Next().Index;
            var pos0 = new PagePosition(index, 0);
            var pos1 = new PagePosition(ClampPageNumber(index + (BookProfile.Current.PreLoadSize - 1) * source.Direction), 0);
            var range = IsValidPosition(pos0) ? new PageDirectionalRange(pos0, pos1) : PageDirectionalRange.Empty;

            return range;
        }

        /// <summary>
        /// ページ範囲からページ列を生成
        /// </summary>
        /// <param name="range"></param>
        /// <param name="excepts">除外するページ</param>
        /// <returns></returns>
        private List<Page> CreatePagesFromRange(PageDirectionalRange range, List<Page> excepts)
        {
            if (range.IsEmpty())
            {
                return new List<Page>();
            }

            return Enumerable.Range(0, range.PageSize)
                .Select(e => range.Position.Index + e * range.Direction)
                .Where(e => 0 <= e && e < Pages.Count)
                .Select(e => Pages[e])
                .Except(excepts)
                .ToList();
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
                    Pages = Pages.OrderBy(e => e.PageType).ThenBy(e => e, new ComparerFileName()).ToList();
                    break;
                case PageSortMode.FileNameDescending:
                    Pages = Pages.OrderBy(e => e.PageType).ThenByDescending(e => e, new ComparerFileName()).ToList();
                    break;
                case PageSortMode.TimeStamp:
                    Pages = Pages.OrderBy(e => e.PageType).ThenBy(e => e.Entry.LastWriteTime).ThenBy(e => e, new ComparerFileName()).ToList();
                    break;
                case PageSortMode.TimeStampDescending:
                    Pages = Pages.OrderBy(e => e.PageType).ThenByDescending(e => e.Entry.LastWriteTime).ThenBy(e => e, new ComparerFileName()).ToList();
                    break;
                case PageSortMode.Size:
                    Pages = Pages.OrderBy(e => e.PageType).ThenBy(e => e.Entry.Length).ThenBy(e => e, new ComparerFileName()).ToList();
                    break;
                case PageSortMode.SizeDescending:
                    Pages = Pages.OrderBy(e => e.PageType).ThenByDescending(e => e.Entry.Length).ThenBy(e => e, new ComparerFileName()).ToList();
                    break;
                case PageSortMode.Random:
                    var random = new Random();
                    Pages = Pages.OrderBy(e => e.PageType).ThenBy(e => random.Next()).ToList();
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

        /// <summary>
        /// ファイル名ソート用比較クラス
        /// </summary>
        private class ComparerFileName : IComparer<Page>
        {
            public int Compare(Page x, Page y)
            {
                var xName = x.GetEntryFullNameTokens();
                var yName = y.GetEntryFullNameTokens();

                var limit = Math.Min(xName.Length, yName.Length);
                for (int i = 0; i < limit; ++i)
                {
                    if (xName[i] != yName[i])
                    {
                        var xIsDirectory = i + 1 == xName.Length ? x.Entry.IsDirectory : true;
                        var yIsDirectory = i + 1 == yName.Length ? y.Entry.IsDirectory : true;
                        if (xIsDirectory != yIsDirectory)
                        {
                            return xIsDirectory ? -1 : 1;
                        }
                        return NativeMethods.StrCmpLogicalW(xName[i], yName[i]);
                    }
                }

                return xName.Length - yName.Length;
            }
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
            RequestSetPosition(this, new PagePosition(index, 0), 1);

            if (_pageMap.TryGetValue(page.EntryFullName, out Page target) && page == target)
            {
                _pageMap.Remove(page.EntryFullName);
            }
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
            var oldies = Markers;
            Markers = pageNames.Select(e => _pageMap.TryGetValue(e, out Page page) ? page : null).Where(e => e != null).ToList();

            foreach (var page in oldies.Where(e => !Markers.Contains(e)))
            {
                page.IsPagemark = false;
            }
            foreach (var page in Markers)
            {
                page.IsPagemark = true;
            }
        }

        /// <summary>
        /// マーカー移動可能判定
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="isLoop"></param>
        /// <returns></returns>
        public bool CanJumpToMarker(int direction, bool isLoop)
        {
            if (Address == null) return false;
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

            if (Address == null) return null;
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

            RequestSetPosition(sender, new PagePosition(target.Index, 0), 1);
            return target;
        }

        #endregion

        #region 動画再生用

        public void RaisePageTerminatedEvent(int direction)
        {
            PageTerminated?.Invoke(this, new PageTerminatedEventArgs(direction));
        }

        #endregion

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (Book.Default == this)
                    {
                        Book.Default = null;
                    }

                    // さまざまなイベント停止
                    this.DartyBook = null;
                    this.PageRemoved = null;
                    this.PagesSorted = null;
                    this.PageTerminated = null;
                    this.ViewContentsChanged = null;

                    _viewPageCollection = new ViewPageCollection();

                    _ahead.Dispose();
                    CancelUpdateNextContents();

                    if (Pages != null)
                    {
                        Pages.ForEach(e => e?.Dispose());
                    }

                    if (_trashBox != null)
                    {
                        _trashBox.Dispose();
                    }

                    _commandEngine.Dispose();

                    if (_contentLoaded != null)
                    {
                        _contentLoaded.Dispose();
                    }

                    if (_jobClient != null)
                    {
                        _jobClient.Dispose();
                    }

                    MemoryControl.Current.GarbageCollect();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        #endregion

#endif

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

            // ディレクトリ？
            [DataMember(EmitDefaultValue = false)]
            public bool IsDirectorty { get; set; }

            // 名前
            public string Name => Place.EndsWith(@":\") ? Place : System.IO.Path.GetFileName(Place);

            // 現在ページ
            [DataMember(EmitDefaultValue = false)]
            public string Page { get; set; }

            // 1ページ表示 or 2ページ表示
            [DataMember(Name = "PageModeV2")]
            [PropertyMember("@ParamBookPageMode")]
            public PageMode PageMode { get; set; }

            // 右開き or 左開き
            [DataMember]
            [PropertyMember("@ParamBookBookReadOrder")]
            public PageReadOrder BookReadOrder { get; set; }

            // 横長ページ分割 (1ページモード)
            [DataMember]
            [PropertyMember("@ParamBookIsSupportedDividePage")]
            public bool IsSupportedDividePage { get; set; }

            // 最初のページを単独表示 
            [DataMember]
            [PropertyMember("@ParamBookIsSupportedSingleFirstPage")]
            public bool IsSupportedSingleFirstPage { get; set; }

            // 最後のページを単独表示
            [DataMember]
            [PropertyMember("@ParamBookIsSupportedSingleLastPage")]
            public bool IsSupportedSingleLastPage { get; set; }

            // 横長ページを2ページ分とみなす(2ページモード)
            [DataMember]
            [PropertyMember("@ParamBookIsSupportedWidePage")]
            public bool IsSupportedWidePage { get; set; } = true;

            // フォルダーの再帰
            [DataMember]
            [PropertyMember("@ParamBookIsRecursiveFolder", Tips = "@ParamBookIsRecursiveFolderTips")]
            public bool IsRecursiveFolder { get; set; }

            // ページ並び順
            [DataMember]
            [PropertyMember("@ParamBookSortMode")]
            public PageSortMode SortMode { get; set; }

            // 最終アクセス日
            [Obsolete]
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
                IsDirectorty = false;
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


#if false
        // bookの設定を取得する
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.Place = Address;
            memento.IsDirectorty = IsDirectory;
            memento.Page = SortMode != PageSortMode.Random ? GetViewPage()?.EntryFullName : null;

            memento.PageMode = PageMode;
            memento.BookReadOrder = BookReadOrder;
            memento.IsSupportedDividePage = IsSupportedDividePage;
            memento.IsSupportedSingleFirstPage = IsSupportedSingleFirstPage;
            memento.IsSupportedSingleLastPage = IsSupportedSingleLastPage;
            memento.IsSupportedWidePage = IsSupportedWidePage;
            memento.IsRecursiveFolder = IsRecursiveFolder;
            memento.SortMode = SortMode;

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
#endif

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

