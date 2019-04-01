using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#if true

// TODO: Bookman で差し替え

namespace NeeView
{
    // とりあえずの現状のBookの置き換わりになるもの(V2)
    public partial class Book : IDisposable
    {
        // TODO: あまりよろしくない
        public static Book Default { get; private set; }

        private BookContext _book;
        private BookPageViewer _viewer;
        private BookPageMarker _marker;
        private BookController _controller;


        public static async Task<Book> CreateAsync(BookAddress address, ArchiveEntryCollectionMode archiveRecursiveMode, BookLoadSetting setting, Book.Memento memento, CancellationToken token)
        {
            var factory = new BookContext.BookFactory();
            var book = await factory.LoadAsync(address, archiveRecursiveMode, setting, memento.IsRecursiveFolder, memento.SortMode, token);

            var viewer = new BookPageViewer(book, CreateBookViewerCreateSetting(memento));
            var marker = new BookPageMarker(book, viewer);
            var controller = new BookController(book, viewer, marker);

            var bookman = new Book();
            bookman._book = book;
            bookman._viewer = viewer;
            bookman._marker = marker;
            bookman._controller = controller;

            // ## Start() で行いたい
            bookman.StartPage(address, setting);

            return bookman;
        }

        private Book()
        {
            Book.Default = this;
        }

        public BookContext Context => _book;
        public BookPageCollection Pages => _book.Pages;
        public BookPageViewer Viewer => _viewer;
        public BookPageMarker Marker => _marker;
        public BookController Control => _controller;

        // 見つからなかった開始ページ名。通知用。
        // TODO: 不要？
        public string NotFoundStartPage { get; private set; }

        // 開始ページ
        // TODO: 再読込時に必要だが、なくすことできそう？
        public string StartEntry { get; private set; }


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

                    _controller.Dispose();
                    _viewer.Dispose();
                    _book.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion


        #region Methods

        public void Start()
        {
            // TODO: スタートページへ移動

            _controller.Start();
        }

        private void StartPage(BookAddress address, BookLoadSetting setting)
        {
            var start = address.EntryName;

            // スタートページ取得
            PagePosition position = _book.Pages.FirstPosition();
            int direction = 1;
            if ((setting.Options & BookLoadOption.FirstPage) == BookLoadOption.FirstPage)
            {
                position = _book.Pages.FirstPosition();
                direction = 1;
            }
            else if ((setting.Options & BookLoadOption.LastPage) == BookLoadOption.LastPage)
            {
                position = _book.Pages.LastPosition();
                direction = -1;
            }
            else
            {
                int index = !string.IsNullOrEmpty(start) ? _book.Pages.FindIndex(e => e.EntryFullName == start) : 0;
                if (index < 0)
                {
                    this.NotFoundStartPage = start;
                }
                position = index >= 0 ? new PagePosition(index, 0) : _book.Pages.FirstPosition();
                direction = 1;
            }

            // 開始ページ記憶
            this.StartEntry = _book.Pages.Count > 0 ? _book.Pages[position.Index].EntryFullName : null;

            /*
            // 有効化
            book.Address = book.ArchiveEntryCollection.Path;
            book.IsDirectory = book.ArchiveEntryCollection.Archiver is FolderArchive;
            */

            // 初期ページ設定 
            _controller.RequestSetPosition(this, position, direction);
        }

        #endregion


        #region Memento

        // bookの設定を取得する
        public Book.Memento CreateMemento()
        {
            var memento = new Book.Memento();

            memento.Place = _book.Address;
            memento.IsDirectorty = _book.IsDirectory;
            memento.Page = _book.Pages.SortMode != PageSortMode.Random ? _viewer.GetViewPage()?.EntryFullName : null;

            memento.PageMode = _viewer.PageMode;
            memento.BookReadOrder = _viewer.BookReadOrder;
            memento.IsSupportedDividePage = _viewer.IsSupportedDividePage;
            memento.IsSupportedSingleFirstPage = _viewer.IsSupportedSingleFirstPage;
            memento.IsSupportedSingleLastPage = _viewer.IsSupportedSingleLastPage;
            memento.IsSupportedWidePage = _viewer.IsSupportedWidePage;
            memento.IsRecursiveFolder = _book.IsRecursiveFolder;
            memento.SortMode = _book.Pages.SortMode;

            return memento;
        }

        // bookに設定を反映させる
        public void Restore(Book.Memento memento)
        {
            if (memento == null) return;

            // TODO: インスタンス生成前によばれても適用不可。生成時に渡すのであれば別実装が必要

            _viewer.PageMode = memento.PageMode;
            _viewer.BookReadOrder = memento.BookReadOrder;
            _viewer.IsSupportedDividePage = memento.IsSupportedDividePage;
            _viewer.IsSupportedSingleFirstPage = memento.IsSupportedSingleFirstPage;
            _viewer.IsSupportedSingleLastPage = memento.IsSupportedSingleLastPage;
            _viewer.IsSupportedWidePage = memento.IsSupportedWidePage;
            _book.IsRecursiveFolder = memento.IsRecursiveFolder;
            _book.Pages.SortMode = memento.SortMode;
        }

        private static BookViewerCreateSetting CreateBookViewerCreateSetting(Book.Memento memento)
        {
            var setting = new BookViewerCreateSetting
            {
                PageMode = memento.PageMode,
                BookReadOrder = memento.BookReadOrder,
                IsSupportedDividePage = memento.IsSupportedDividePage,
                IsSupportedSingleFirstPage = memento.IsSupportedSingleFirstPage,
                IsSupportedSingleLastPage = memento.IsSupportedSingleLastPage,
                IsSupportedWidePage = memento.IsSupportedWidePage
            };
            return setting;
        }

        #endregion
    }



    public class BookContext : IDisposable
    {
        // 再読み込みを要求
        public event EventHandler DartyBook;



        // 見つからなかった開始ページ名。通知用。
        ////public string NotFoundStartPage { get; private set; }

        // この本の場所
        // nullの場合、この本は無効
        public string Address { get; private set; }

        // 開始ページ
        ////public string StartEntry { get; private set; }

        // この本はディレクトリ？
        public bool IsDirectory { get; private set; }


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


        // この本のアーカイバ
        public ArchiveEntryCollection ArchiveEntryCollection { get; private set; }

        // メディアアーカイバ？
        public bool IsMedia => ArchiveEntryCollection?.Archiver is MediaArchiver;

        // メディア、最終フレームから再生フラグ
        // TODO: 生成時に生成パラメータを保持してそこから参照する？
        //// if (_book.IsMedia && _setting.Options.HasFlag(BookLoadOption.LastPage))
        public bool IsMediaLastPlay { get; private set; }

        // ページマークアーカイバ？
        public bool IsPagemarkFolder => ArchiveEntryCollection?.Archiver is PagemarkArchiver;

        public BookPageCollection Pages { get; private set; }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    this.DartyBook = null;
                    Pages.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion


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

        public class BookFactory
        {
            #region 開発用

            ////public static TraceSource Log = Logger.CreateLogger(nameof(Book));

            // シリアル
            ////public static int _serial = 0;

            // Log用 シリアル番号
            ////public int Serial { get; private set; }

            #endregion

            // 初期化オプション
            private BookLoadSetting _setting;


            #region 本の初期化

#if false
            /// <summary>
            /// フォルダーの読込。これ意味ないよね
            /// </summary>
            public async Task<BookContext> LoadAsync(BookAddress address, ArchiveEntryCollectionMode archiveRecursiveMode, BookLoadSetting setting, CancellationToken token)
            {
                try
                {
                    Log.TraceEvent(TraceEventType.Information, Serial, $"Load: {address.Address}");
                    Log.Flush();

                    return await LoadCoreAsync(address, archiveRecursiveMode, setting, token);
                }
                catch (Exception e)
                {
                    Log.TraceEvent(TraceEventType.Warning, Serial, $"Load Failed: {e.Message}");
                    Log.Flush();

                    ////Dispose();
                    throw;
                }
            }
#endif

            // 本読み込み
            // TODO: BookMementoを引数で渡して初期値とする
            public async Task<BookContext> LoadAsync(BookAddress address, ArchiveEntryCollectionMode archiveRecursiveMode, BookLoadSetting setting, bool isRecursiveFolder, PageSortMode sortMode, CancellationToken token)
            {
                ////Debug.Assert(Address == null);
                ////Debug.WriteLine($"OPEN: {address.Place}, {address.EntryName}, {address.Archiver.Path}");

                var book = new BookContext();
                book.IsRecursiveFolder = isRecursiveFolder;

                _setting = setting.Clone();

                var start = address.EntryName;

                // リカーシブオプションフラグ
                if (_setting.Options.HasFlag(BookLoadOption.NotRecursive))
                {
                    book.IsRecursiveFolder = false;
                    _setting.Options &= ~BookLoadOption.Recursive;
                }
                else if (_setting.Options.HasFlag(BookLoadOption.Recursive))
                {
                    book.IsRecursiveFolder = true;
                }

                // リカーシブフラグ
                if (book.IsRecursiveFolder)
                {
                    _setting.Options |= BookLoadOption.Recursive;
                }

                // ページ生成
                ////var pageList = await CreatePageCollection(address.Address.SimplePath, archiveRecursiveMode, token);
                var archiveEntryCollection = CreateArchiveEntryCollection(address.Address.SimplePath, archiveRecursiveMode);
                var pageList = await CreatePageCollection(archiveEntryCollection, token);


                // 自動再帰処理
                if (archiveEntryCollection.Mode != ArchiveEntryCollectionMode.IncludeSubArchives && pageList.Count == 0 && _setting.Options.HasFlag(BookLoadOption.AutoRecursive))
                {
                    var entries = await archiveEntryCollection.GetEntriesWhereBookAsync(token);
                    if (entries.Count == 1)
                    {
                        _setting.Options |= BookLoadOption.Recursive;
                        ////pageList = await CreatePageCollection(address.Address.SimplePath, archiveRecursiveMode, token);
                        archiveEntryCollection = CreateArchiveEntryCollection(address.Address.SimplePath, archiveRecursiveMode);
                        pageList = await CreatePageCollection(archiveEntryCollection, token);
                    }
                }

                // 事前展開処理
                await PreExtractAsync(pageList, token);

                // Pages initialize
                // TODO: ページ生成と同時に行うべき?
                ////_pageMap.Clear();
                var prefix = GetPagesPrefix(pageList);
                foreach (var page in pageList)
                {
                    page.Prefix = prefix;
                    ////page.Loaded += Page_Loaded;
                    ////page.Thumbnail.Touched += Thumbnail_Touched;
                    ////_pageMap[page.EntryFullName] = page;
                }


                var pages = new BookPageCollection(pageList, sortMode);

                // 初期ソート
                ////pages.Sort();

#if false
                #region スタートページ表示 ## これは生成の仕事ではない。生成後に開始ページを設定しページ移動を行わせる。

                // スタートページ取得
                PagePosition position = pages.FirstPosition();
                int direction = 1;
                if ((_setting.Options & BookLoadOption.FirstPage) == BookLoadOption.FirstPage)
                {
                    position = pages.FirstPosition();
                    direction = 1;
                }
                else if ((_setting.Options & BookLoadOption.LastPage) == BookLoadOption.LastPage)
                {
                    position = pages.LastPosition();
                    direction = -1;
                }
                else
                {
                    int index = !string.IsNullOrEmpty(start) ? Pages.FindIndex(e => e.EntryFullName == start) : 0;
                    if (index < 0)
                    {
                        book.NotFoundStartPage = start;
                    }
                    position = index >= 0 ? new PagePosition(index, 0) : pages.FirstPosition();
                    direction = 1;
                }

                // 開始ページ記憶
                book.StartEntry = Pages.Count > 0 ? Pages[position.Index].EntryFullName : null;

                // 有効化
                book.Address = book.ArchiveEntryCollection.Path;
                book.IsDirectory = book.ArchiveEntryCollection.Archiver is FolderArchive;

                // 初期ページ設定 
                ////RequestSetPosition(this, position, direction);
                #endregion
#endif

                // 有効化
                book.Address = archiveEntryCollection.Path;
                book.IsDirectory = archiveEntryCollection.Archiver is FolderArchive;

                book.ArchiveEntryCollection = archiveEntryCollection;
                book.IsMediaLastPlay = book.IsMedia && _setting.Options.HasFlag(BookLoadOption.LastPage); // TODO: 設定をまるごと保存してそこから参照する形へ

                book.Pages = pages;

                return book;
            }


            private ArchiveEntryCollection CreateArchiveEntryCollection(string place, ArchiveEntryCollectionMode archiveRecursiveMode)
            {
                var collectMode = _setting.Options.HasFlag(BookLoadOption.Recursive) ? ArchiveEntryCollectionMode.IncludeSubArchives : ArchiveEntryCollectionMode.CurrentDirectory;
                var collectModeIfArchive = _setting.Options.HasFlag(BookLoadOption.Recursive) ? ArchiveEntryCollectionMode.IncludeSubArchives : archiveRecursiveMode;
                var collectOption = ArchiveEntryCollectionOption.None;
                return new ArchiveEntryCollection(place, collectMode, collectModeIfArchive, collectOption);
            }

            /// <summary>
            /// ページ生成
            /// </summary>
            private async Task<List<Page>> CreatePageCollection(ArchiveEntryCollection archiveEntryCollection, CancellationToken token)
            {
                List<ArchiveEntry> entries;
                switch (_setting.BookPageCollectMode)
                {
                    case BookPageCollectMode.Image:
                        entries = await archiveEntryCollection.GetEntriesWhereImageAsync(token);
                        break;
                    case BookPageCollectMode.ImageAndBook:
                        entries = await archiveEntryCollection.GetEntriesWhereImageAndArchiveAsync(token);
                        break;
                    case BookPageCollectMode.All:
                    default:
                        entries = await archiveEntryCollection.GetEntriesWherePageAllAsync(token);
                        break;
                }

                var bookPrefix = LoosePath.TrimDirectoryEnd(archiveEntryCollection.Path);
                ////this.Pages = entries.Select(e => CreatePage(bookPrefix, e)).ToList();
                return entries.Select(e => CreatePage(bookPrefix, e)).ToList();
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
            private string GetPagesPrefix(List<Page> Pages)
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
            private async Task PreExtractAsync(List<Page> Pages, CancellationToken token)
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

            #endregion
        }


    }

    public class BookPageCollection : BindableBase, IEnumerable<Page>, IDisposable
    {
        // サムネイル寿命管理
        private PageThumbnailPool _thumbnaulPool = new PageThumbnailPool();



        // ソートされた
        public event EventHandler PagesSorted;

        // ファイル削除された
        public event EventHandler<PageChangedEventArgs> PageRemoved;



        // この本のアーカイバ
        public ArchiveEntryCollection ArchiveEntryCollection { get; private set; }

        // メディアアーカイバ？
        public bool IsMedia => ArchiveEntryCollection?.Archiver is MediaArchiver;

        // ページマークアーカイバ？
        public bool IsPagemarkFolder => ArchiveEntryCollection?.Archiver is PagemarkArchiver;


        public List<Page> Pages { get; private set; }

        public int Count => Pages.Count;

        public int IndexOf(Page page) => Pages.IndexOf(page);

        public Page First() => Pages.First();

        public Page Last() => Pages.Last();


        // ページ列
        private PageSortMode _sortMode = PageSortMode.FileName;


        public BookPageCollection(List<Page> pages, PageSortMode sortMode)
        {
            Pages = pages;
            _sortMode = sortMode;

            foreach (var page in Pages)
            {
                page.Thumbnail.Touched += Thumbnail_Touched;
            }

            Sort();
        }


        public PageSortMode SortMode
        {
            get => _sortMode;
            set => SetProperty(ref _sortMode, value);
            ////RequestSort(this); TODO: PropertyChangedイベントで処理する
        }

        public Page this[int index]
        {
            get { return Pages[index]; }
            set { Pages[index] = value; }
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    this.ResetPropertyChanged();
                    this.PageRemoved = null;
                    this.PagesSorted = null;

                    if (Pages != null)
                    {
                        Pages.ForEach(e => e?.Dispose());
                    }
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        #region IEnumerable<Page> Support

        public IEnumerator<Page> GetEnumerator()
        {
            return Pages.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

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

        // ページ
        public Page GetPage(int index) => Pages.Count > 0 ? Pages[ClampPageNumber(index)] : null;

        //
        public Page GetPage(string name) => Pages.FirstOrDefault(e => e.EntryFullName == name);

        // ページ番号
        public int GetIndex(Page page) => Pages.IndexOf(page);

        // 先頭ページの場所
        public PagePosition FirstPosition() => PagePosition.Zero;

        // 最終ページの場所
        public PagePosition LastPosition() => Pages.Count > 0 ? new PagePosition(Pages.Count - 1, 1) : FirstPosition();

        // ページ番号のクランプ
        public int ClampPageNumber(int index)
        {
            if (index > Pages.Count - 1) index = Pages.Count - 1;
            if (index < 0) index = 0;
            return index;
        }

        // ページ場所の有効判定
        public bool IsValidPosition(PagePosition position)
        {
            return (FirstPosition() <= position && position <= LastPosition());
        }


        #region ページの並び替え

        // ページの並び替え
        public void Sort()
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
        public void Remove(Page page)
        {
            if (Pages.Count <= 0) return;

            int index = Pages.IndexOf(page);
            if (index < 0) return;

            Pages.RemoveAt(index);

            PagesNumbering();

            PageRemoved?.Invoke(this, new PageChangedEventArgs(page));

#if false
            index = ClampPageNumber(index);

            // TODO: ## BookCommandEngine へ。
            RequestSetPosition(this, new PagePosition(index, 0), 1);

            // TODO: ## BookPageMarkerへ
            if (_pageMap.TryGetValue(page.EntryFullName, out Page target) && page == target)
            {
                _pageMap.Remove(page.EntryFullName);
            }
#endif
        }

        #endregion


    }

    public class BookViewerCreateSetting
    {
        public PageMode PageMode { get; set; }
        public PageReadOrder BookReadOrder { get; set; }
        public bool IsSupportedDividePage { get; set; }
        public bool IsSupportedSingleFirstPage { get; set; }
        public bool IsSupportedSingleLastPage { get; set; }
        public bool IsSupportedWidePage { get; set; }
    }

    public class BookPageViewer : BindableBase, IDisposable
    {
        private BookContext _book;

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

        // JOBリクエスト
        private PageContentJobClient _jobClient = new PageContentJobClient("View", JobCategories.PageViewContentJobCategory);

        // メモリ管理
        private BookMemoryService _bookMemoryService = new BookMemoryService();

        // 先読み
        private BookAhead _ahead;

        private object _lock = new object();



        public BookPageViewer(BookContext book, BookViewerCreateSetting setting)
        {
            _book = book ?? throw new ArgumentNullException(nameof(book));

            this.PageMode = setting.PageMode;
            this.BookReadOrder = setting.BookReadOrder;
            this.IsSupportedDividePage = setting.IsSupportedDividePage;
            this.IsSupportedSingleFirstPage = setting.IsSupportedSingleFirstPage;
            this.IsSupportedSingleLastPage = setting.IsSupportedSingleLastPage;
            this.IsSupportedWidePage = setting.IsSupportedWidePage;

            foreach (var page in _book.Pages)
            {
                page.Loaded += Page_Loaded;
            }

            _ahead = new BookAhead(_bookMemoryService);
        }



        // 表示コンテンツ変更
        // 表示の更新を要求
        public event EventHandler<ViewPageCollectionChangedEventArgs> ViewContentsChanged;

        // 先読みコンテンツ変更
        public event EventHandler<ViewPageCollectionChangedEventArgs> NextContentsChanged;

        // ページ終端を超えて移動しようとした
        // 次の本への移動を要求
        public event EventHandler<PageTerminatedEventArgs> PageTerminated;




        // 横長ページを分割する
        private bool _isSupportedDividePage;
        public bool IsSupportedDividePage
        {
            get => _isSupportedDividePage;
            set => SetProperty(ref _isSupportedDividePage, value);
        }

        // 最初のページは単独表示
        private bool _isSupportedSingleFirstPage;
        public bool IsSupportedSingleFirstPage
        {
            get => _isSupportedSingleFirstPage;
            set => SetProperty(ref _isSupportedSingleFirstPage, value);
        }

        // 最後のページは単独表示
        private bool _isSupportedSingleLastPage;
        public bool IsSupportedSingleLastPage
        {
            get => _isSupportedSingleLastPage;
            set => SetProperty(ref _isSupportedSingleLastPage, value);
        }

        // 横長ページは２ページとみなす
        private bool _isSupportedWidePage = true;
        public bool IsSupportedWidePage
        {
            get => _isSupportedWidePage;
            set => SetProperty(ref _isSupportedWidePage, value);
        }

        // 右開き、左開き
        private PageReadOrder _bookReadOrder = PageReadOrder.RightToLeft;
        public PageReadOrder BookReadOrder
        {
            get => _bookReadOrder;
            set => SetProperty(ref _bookReadOrder, value);
        }

        // 単ページ/見開き
        private PageMode _pageMode = PageMode.SinglePage;
        public PageMode PageMode
        {
            get => _pageMode;
            set => SetProperty(ref _pageMode, value);
        }


        // 表示されるページ番号(スライダー用)
        public int DisplayIndex { get; set; }


        // 表示ページ変更回数
        public int PageChangeCount { get; private set; }

        // 終端ページ表示
        public bool IsPageTerminated { get; private set; }

        // ##
        public ViewPageCollection ViewPageCollection => _viewPageCollection;
        public BookMemoryService BookMemoryService => _bookMemoryService;


        // TODO: イベントでよくないか？待機が必要なら受け側で実装
        // 最初のコンテンツ表示フラグ
        private ManualResetEventSlim _contentLoaded = new ManualResetEventSlim();
        public ManualResetEventSlim ContentLoaded => _contentLoaded;


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    this.ResetPropertyChanged();
                    this.PageTerminated = null;
                    this.ViewContentsChanged = null;
                    this.NextContentsChanged = null;

                    _ahead.Dispose();
                    CancelUpdateNextContents();

                    _jobClient.Dispose();

                    _contentLoaded.Set();
                    _contentLoaded.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion


        // 動画用：外部から終端イベントを発行
        public void RaisePageTerminatedEvent(int direction)
        {
            PageTerminated?.Invoke(this, new PageTerminatedEventArgs(direction));
        }

        #region 表示ページ処理

        // 表示ページ番号
        public int GetViewPageindex() => _viewPageCollection.Range.Min.Index;

        // 表示ページ
        public Page GetViewPage() => _book.Pages.GetPage(_viewPageCollection.Range.Min.Index);

        // 表示ページ群
        public List<Page> GetViewPages() => _viewPageCollection.Collection.Select(e => e.Page).ToList();



        // 先読み許可フラグ
        private bool AllowPreLoad()
        {
            return BookProfile.Current.PreLoadSize > 0;
        }

        // 表示ページ再読込
        public async Task RefreshViewPageAsync(object sender, CancellationToken token)
        {
            var range = new PageDirectionalRange(_viewPageCollection.Range.Min, 1, PageMode.Size());
            await UpdateViewPageAsync(range, sender, token);
        }

        // 表示ページ移動
        public async Task MoveViewPageAsync(int step, object sender, CancellationToken token)
        {
            var viewRange = _viewPageCollection.Range;

            var direction = step < 0 ? -1 : 1;

            var pos = Math.Abs(step) == PageMode.Size() ? viewRange.Next(direction) : viewRange.Move(step);
            if (pos < _book.Pages.FirstPosition() && !viewRange.IsContains(_book.Pages.FirstPosition()))
            {
                pos = new PagePosition(0, direction < 0 ? 1 : 0);
            }
            else if (pos > _book.Pages.LastPosition() && !viewRange.IsContains(_book.Pages.LastPosition()))
            {
                pos = new PagePosition(_book.Pages.Count - 1, direction < 0 ? 1 : 0);
            }

            var range = new PageDirectionalRange(pos, direction, PageMode.Size());

            await UpdateViewPageAsync(range, sender, token);
        }

        // 表示ページ更新
        public async Task UpdateViewPageAsync(PageDirectionalRange source, object sender, CancellationToken token)
        {
            // ページ終端を越えたか判定
            if (source.Position < _book.Pages.FirstPosition())
            {
                AppDispatcher.Invoke(() => PageTerminated?.Invoke(this, new PageTerminatedEventArgs(-1)));
                return;
            }
            else if (source.Position > _book.Pages.LastPosition())
            {
                AppDispatcher.Invoke(() => PageTerminated?.Invoke(this, new PageTerminatedEventArgs(+1)));
                return;
            }

            // ページ数０の場合は表示コンテンツなし
            if (_book.Pages.Count == 0)
            {
                AppDispatcher.Invoke(() => ViewContentsChanged?.Invoke(this, new ViewPageCollectionChangedEventArgs(new ViewPageCollection())));
                return;
            }

            // view pages
            var viewPages = new List<Page>();
            for (int i = 0; i < PageMode.Size(); ++i)
            {
                var page = _book.Pages[_book.Pages.ClampPageNumber(source.Position.Index + source.Direction * i)];
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
            this.IsPageTerminated = source.Max >= _book.Pages.LastPosition();
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
                    ////Debug.WriteLine($"> CancelUpdateViewContents");
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
                catch (Exception)
                {
                    ////Debug.WriteLine($"> RunUpdateViewContents: {ex.Message}");
                }
            });
        }

        private void UpdateNextContentsInner(CancellationToken token)
        {
            if (!_nextPageCollection.IsValid) return;

            ////int turn = 0;
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
            if (IsSupportedSingleLastPage && index == _book.Pages.Count - 1) return true;
            if (_book.Pages[index] is ArchivePage) return true;
            if (IsSupportedWidePage && IsWide(_book.Pages[index])) return true;
            return false;
        }

        // 分割モード有効判定
        private bool IsEnableDividePage(int index)
        {
            return (PageMode == PageMode.SinglePage && !_book.IsMedia && IsSupportedDividePage && IsWide(_book.Pages[index]));
        }

        // 表示コンテンツソースと、それに対応したコンテキスト作成
        private ViewPageCollection CreateViewPageContext(PageDirectionalRange source)
        {
            var infos = new List<PagePart>();

            {
                PagePosition position = source.Position;

                for (int id = 0; id < PageMode.Size(); ++id)
                {
                    if (!_book.Pages.IsValidPosition(position) || _book.Pages[position.Index] == null) break;

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
                var viewPage = new ViewPage(_book.Pages[v.Position.Index], v);

                // メディア用。最終ページからの表示指示の場合のフラグ設定
                ////if (_book.IsMedia && _setting.Options.HasFlag(BookLoadOption.LastPage))
                if (_book.IsMediaLastPlay)
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
                contentsSource.Add(new ViewPage(_book.Pages[position.Index], new PagePart(position, 2, BookReadOrder)));
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
            var pos1 = new PagePosition(_book.Pages.ClampPageNumber(index + (BookProfile.Current.PreLoadSize - 1) * source.Direction), 0);
            var range = _book.Pages.IsValidPosition(pos0) ? new PageDirectionalRange(pos0, pos1) : PageDirectionalRange.Empty;

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
                .Where(e => 0 <= e && e < _book.Pages.Count)
                .Select(e => _book.Pages[e])
                .Except(excepts)
                .ToList();
        }

        #endregion

        #region ページリスト用現在ページ表示フラグ
        // TODO: BookPageCollection or Book に移動？

        // 表示中ページ
        private List<Page> _viewPages = new List<Page>();

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


        #endregion
    }

    public class BookPageMarker
    {
        private BookContext _book;
        private BookPageViewer _viewer;

        // ページマップ
        private Dictionary<string, Page> _pageMap = new Dictionary<string, Page>();

        public BookPageMarker(BookContext book, BookPageViewer viewer)
        {
            _book = book;
            _viewer = viewer;

            // TODO: ページ生成と同時に行うべき
            _pageMap.Clear();
            foreach (var page in _book.Pages)
            {
                _pageMap[page.EntryFullName] = page;
            }

            _book.Pages.PageRemoved += Pages_PageRemoved;
        }



        // マーカー
        public List<Page> Markers { get; private set; } = new List<Page>();


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
            ////if (Address == null) return false;
            if (Markers == null || Markers.Count == 0) return false;

            if (isLoop) return true;

            var list = Markers.OrderBy(e => e.Index).ToList();
            var index = _viewer.GetViewPageindex();

            return direction > 0
                ? list.Last().Index > index
                : list.First().Index < index;
        }

        /// <summary>
        /// ブック内のマーカーを取得
        /// </summary>
        /// <param name="direction">移動方向(+1 or -1)</param>
        /// <param name="isLoop">ループ移動</param>
        /// <param name="isIncludeTerminal">終端を含める</param>
        /// <returns>一致するページ。見つからなければnull</returns>
        ////public Page RequestJumpToMarker(object sender, int direction, bool isLoop, bool isIncludeTerminal)
        public Page GetNearMarkedPage(int direction, bool isLoop, bool isIncludeTerminal)
        {
            Debug.Assert(direction == 1 || direction == -1);

            ////if (Address == null) return null;
            ////if (Pages == null || _pages.Count < 2) return null;
            if (_book.Pages.Count < 2) return null;

            var list = Markers != null ? Markers.OrderBy(e => e.Index).ToList() : new List<Page>();

            if (isIncludeTerminal)
            {
                if (list.FirstOrDefault() != _book.Pages.First())
                {
                    list.Insert(0, _book.Pages.First());
                }
                if (list.LastOrDefault() != _book.Pages.Last())
                {
                    list.Add(_book.Pages.Last());
                }
            }

            if (list.Count == 0) return null;

            var index = _viewer.GetViewPageindex();

            var target =
                direction > 0
                ? list.FirstOrDefault(e => e.Index > index) ?? (isLoop ? list.First() : null)
                : list.LastOrDefault(e => e.Index < index) ?? (isLoop ? list.Last() : null);

            ////if (target == null) return null;
            // TODO コマンド側で処理するべきか。このメソッドはそのパラメータ生成用にする。
            ////RequestSetPosition(sender, new PagePosition(target.Index, 0), 1);

            return target;
        }

        private void Pages_PageRemoved(object sender, PageChangedEventArgs e)
        {
            if (_pageMap.TryGetValue(e.Page.EntryFullName, out Page target) && e.Page == target)
            {
                _pageMap.Remove(e.Page.EntryFullName);
            }
        }

        #endregion
    }



    public class BookController : IDisposable
    {
        // コマンドエンジン
        private BookCommandEngine _commandEngine = new BookCommandEngine();

        private BookContext _book;
        private BookPageViewer _viewer;
        private BookPageMarker _marker;


        public BookController(BookContext book, BookPageViewer viewer, BookPageMarker marker)
        {
            _book = book ?? throw new ArgumentNullException(nameof(book));
            _viewer = viewer ?? throw new ArgumentNullException(nameof(viewer));
            _marker = marker ?? throw new ArgumentNullException(nameof(marker));

            _book.Pages.PropertyChanged += (s, e) => RequestSort(this);
            _viewer.PropertyChanged += (s, e) => RequestRefresh(this, false);
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _commandEngine.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion


        // 開始
        // ページ設定を行うとコンテンツ読み込みが始まるため、ロードと分離した
        public void Start()
        {
            ////Debug.Assert(Address != null);
            ////_commandEngine.Name = $"BookJobEngine: {this.Address}";
            _commandEngine.Name = $"BookJobEngine: start.";
            _commandEngine.Log = new NeeLaboratory.Diagnostics.Log(nameof(BookCommandEngine), 0);
            _commandEngine.StartEngine();
        }

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
            var s = (step == 0) ? _viewer.PageMode.Size() : step;
            RequestMovePosition(this, -s);
        }

        // 次のページへ進む
        public void NextPage(int step = 0)
        {
            var s = (step == 0) ? _viewer.PageMode.Size() : step;
            RequestMovePosition(this, +s);
        }

        // 最初のページに移動
        public void FirstPage()
        {
            RequestSetPosition(this, _book.Pages.FirstPosition(), 1);
        }

        // 最後のページに移動
        public void LastPage()
        {
            RequestSetPosition(this, _book.Pages.LastPosition(), -1);
        }

        // 指定ページに移動
        public void JumpPage(Page page)
        {
            int index = _book.Pages.IndexOf(page);
            if (index >= 0)
            {
                var position = new PagePosition(index, 0);
                RequestSetPosition(this, position, 1);
            }
        }

        // ページマーク移動
        // TODO: もっと上のレベルでページマークの取得と移動の発行を行う
        public Page RequestJumpToMarker(object sender, int direction, bool isLoop, bool isIncludeTerminal)
        {
            Debug.Assert(direction == 1 || direction == -1);

            var target = _marker.GetNearMarkedPage(direction, isLoop, isIncludeTerminal);
            if (target == null) return null;

            RequestSetPosition(sender, new PagePosition(target.Index, 0), 1);
            return target;
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

            _viewer.DisplayIndex = position.Index;

            var range = new PageDirectionalRange(position, direction, _viewer.PageMode.Size());
            var command = new BookCommandAction(sender, Execute, 0);
            _commandEngine.Enqueue(command);

            async Task Execute(object s, CancellationToken token)
            {
                await _viewer.UpdateViewPageAsync(range, s, token);
            }
        }

        // ページ相対移動
        public void RequestMovePosition(object sender, int step)
        {
            var command = new BookCommandJoinAction(sender, Execute, step, 0);
            _commandEngine.Enqueue(command);

            async Task Execute(object s, int value, CancellationToken token)
            {
                await _viewer.MoveViewPageAsync(value, s, token);
            }
        }

        // リフレッシュ
        public void RequestRefresh(object sender, bool isClear)
        {
            var command = new BookCommandAction(sender, Execute, 1);
            _commandEngine.Enqueue(command);

            async Task Execute(object s, CancellationToken token)
            {
                await _viewer.RefreshViewPageAsync(s, token);
            }
        }

        // ソート
        public void RequestSort(object sender)
        {
            var command = new BookCommandAction(sender, Execute, 2);
            _commandEngine.Enqueue(command);

            async Task Execute(object s, CancellationToken token)
            {
                var page = _viewer.GetViewPage();

                _book.Pages.Sort();

                var pagePosition = new PagePosition(_book.Pages.GetIndex(page), 0);
                RequestSetPosition(this, pagePosition, 1);

                await Task.CompletedTask;
            }
        }

        // ページ削除
        public void RequestRemove(object sender, Page page)
        {
            var command = new BookCommandAction(sender, Execute, 3);
            _commandEngine.Enqueue(command);

            async Task Execute(object s, CancellationToken token)
            {
                var index = _book.Pages.ClampPageNumber(page.Index);
                _book.Pages.Remove(page);
                RequestSetPosition(this, new PagePosition(index, 0), 1);
                await Task.CompletedTask;
            }
        }


        // 終了処理
        private BookCommand RequestDispose(object sender)
        {
            var command = new BookCommandAction(sender, Execute, 4);
            _commandEngine.Enqueue(command);
            return command;

            async Task Execute(object s, CancellationToken token)
            {
                Dispose();
                await Task.CompletedTask;
            }
        }

        #endregion


#if false
        #region コマンド実行


        internal async Task Remove_Executed(BookCommandRemoveArgs param, CancellationToken token)
        {
            var index = _book.Pages.ClampPageNumber(param.Page.Index);
            _book.Pages.Remove(param.Page);
            RequestSetPosition(this, new PagePosition(index, 0), 1);
            await Task.CompletedTask;
        }

        internal async Task Sort_Executed(BookCommandSortArgs param, CancellationToken token)
        {
            var page = _viewer.GetViewPage();

            _book.Pages.Sort();

            var pagePosition = new PagePosition(_book.Pages.GetIndex(page), 0);
            RequestSetPosition(this, pagePosition, 1);

            await Task.CompletedTask;
        }

        internal async Task Refresh_Executed(BookCommandRefreshArgs param, CancellationToken token)
        {
            ////Refresh(param.IsClear);

            await _viewer.RefreshViewPageAsync(null, token);
            await Task.CompletedTask;
        }

        internal async Task SetPage_Executed(object sender, BookCommandSetPageArgs param, CancellationToken token)
        {
            var source = new PageDirectionalRange(param.Position, param.Direction, param.Size);
            await _viewer.UpdateViewPageAsync(source, sender, token);
        }


        internal async Task MovePage_Executed(BookCommandMovePageArgs param, CancellationToken token)
        {
            await _viewer.MoveViewPageAsync(param.Step, null, token);
        }


        #endregion
#endif
    }


}

#endif
