using NeeView.Collections.Generic;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    // とりあえずの現状のBookの置き換わりになるもの(V2)
    public partial class Book : IDisposable
    {
        public static Book Default { get; private set; }

        private BookMemoryService _bookMemoryService = new BookMemoryService();

        private BookSource _source;
        private BookPageViewer _viewer;
        private BookPageMarker _marker;
        private BookController _controller;

        public Book(BookSource source, Book.Memento memento)
        {
            Book.Default = this;

            _source = source;
            _viewer = new BookPageViewer(_source, _bookMemoryService, CreateBookViewerCreateSetting(memento));
            _marker = new BookPageMarker(_source, _viewer);
            _controller = new BookController(_source, _viewer, _marker);
        }

        public BookSource Source => _source;
        public BookPageCollection Pages => _source.Pages;
        public BookPageViewer Viewer => _viewer;
        public BookPageMarker Marker => _marker;
        public BookController Control => _controller;
        public BookMemoryService BookMemoryService => _bookMemoryService;

        public string Address => _source.Address;
        public bool IsMedia => _source.IsMedia;
        public bool IsPagemarkFolder => _source.IsPagemarkFolder;


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
                    _source.Dispose();
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

        public void StartPage(string start, BookLoadSetting setting)
        {
            // スタートページ取得
            PagePosition position = _source.Pages.FirstPosition();
            int direction = 1;
            if ((setting.Options & BookLoadOption.FirstPage) == BookLoadOption.FirstPage)
            {
                position = _source.Pages.FirstPosition();
                direction = 1;
            }
            else if ((setting.Options & BookLoadOption.LastPage) == BookLoadOption.LastPage)
            {
                position = _source.Pages.LastPosition();
                direction = -1;
            }
            else
            {
                int index = !string.IsNullOrEmpty(start) ? _source.Pages.FindIndex(e => e.EntryFullName == start) : 0;
                if (index < 0)
                {
                    this.NotFoundStartPage = start;
                }
                position = index >= 0 ? new PagePosition(index, 0) : _source.Pages.FirstPosition();
                direction = 1;
            }

            // 開始ページ記憶
            this.StartEntry = _source.Pages.Count > 0 ? _source.Pages[position.Index].EntryFullName : null;

            // 初期ページ設定 
            _controller.RequestSetPosition(this, position, direction);
        }

        #endregion


        #region Memento

        // bookの設定を取得する
        public Book.Memento CreateMemento()
        {
            var memento = new Book.Memento();

            memento.Place = _source.Address;
            memento.IsDirectorty = _source.IsDirectory;
            memento.Page = _source.Pages.SortMode != PageSortMode.Random ? _viewer.GetViewPage()?.EntryFullName : null;

            memento.PageMode = _viewer.PageMode;
            memento.BookReadOrder = _viewer.BookReadOrder;
            memento.IsSupportedDividePage = _viewer.IsSupportedDividePage;
            memento.IsSupportedSingleFirstPage = _viewer.IsSupportedSingleFirstPage;
            memento.IsSupportedSingleLastPage = _viewer.IsSupportedSingleLastPage;
            memento.IsSupportedWidePage = _viewer.IsSupportedWidePage;
            memento.IsRecursiveFolder = _source.IsRecursiveFolder;
            memento.SortMode = _source.Pages.SortMode;

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
            _source.IsRecursiveFolder = memento.IsRecursiveFolder;
            _source.Pages.SortMode = memento.SortMode;
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

    public static class BookFactory
    {
        public static async Task<Book> CreateAsync(BookAddress address, ArchiveEntryCollectionMode archiveRecursiveMode, BookLoadSetting setting, Book.Memento memento, CancellationToken token)
        {
            var factory = new BookSource.BookFactory();

            var createSetting = new BookSourceCreateSetting()
            {
                IsRecursiveFolder = BookLoadOptionHelper.IsRecursiveFolder(memento.IsRecursiveFolder, setting),
                ArchiveRecursiveMode = archiveRecursiveMode,
                BookPageCollectMode = setting.BookPageCollectMode,
                SortMode = memento.SortMode,
            };

            var bookSource = await factory.CreateAsync(address.Address, createSetting, token);

            if (bookSource.IsMedia)
            {
                foreach(var page in bookSource.Pages.OfType<MediaPage>())
                {
                    page.IsLastStart = setting.Options.HasFlag(BookLoadOption.LastPage);
                }
            }


            var book = new Book(bookSource, memento);

            // ## Start() で行いたい
            book.StartPage(address.EntryName, setting);

            return book;
        }
    }

}
