using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

}
