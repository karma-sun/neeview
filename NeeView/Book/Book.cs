using NeeView.Collections.Generic;
using System;
using System.Diagnostics;
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

        public void SetStartPage(BookStartPage startPage)
        {
            // スタートページ取得
            PagePosition position = _source.Pages.FirstPosition();
            int direction = 1;
            if (startPage.StartPageType == BookStartPageType.FirstPage)
            {
                position = _source.Pages.FirstPosition();
                direction = 1;
            }
            else if (startPage.StartPageType == BookStartPageType.LastPage)
            {
                position = _source.Pages.LastPosition();
                direction = -1;
            }
            else
            {
                int index = !string.IsNullOrEmpty(startPage.PageName) ? _source.Pages.FindIndex(e => e.EntryFullName == startPage.PageName) : 0;
                if (index < 0)
                {
                    this.NotFoundStartPage = startPage.PageName;
                }
                position = index >= 0 ? new PagePosition(index, 0) : _source.Pages.FirstPosition();
                direction = 1;
            }

            // 開始ページ記憶
            this.StartEntry = _source.Pages.Count > 0 ? _source.Pages[position.Index].EntryFullName : null;

            // 初期ページ設定 
            _controller.RequestSetPosition(this, position, direction);
        }

        public void Start()
        {
            // TODO: スタートページへ移動
            _controller.Start();
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




    /// <summary>
    /// Book生成設定
    /// </summary>
    public class BookCreateSetting
    {
        /// <summary>
        /// 開始ページ
        /// </summary>
        public BookStartPage StartPage { get; set; }

        /// <summary>
        /// フォルダー再帰
        /// </summary>
        public bool IsRecursiveFolder { get; set; }

        /// <summary>
        /// 圧縮ファイルの再帰モード
        /// </summary>
        public ArchiveEntryCollectionMode ArchiveRecursiveMode { get; set; }

        /// <summary>
        /// ページ収集モード
        /// </summary>
        public BookPageCollectMode BookPageCollectMode { get; set; }

        /// <summary>
        /// ページの並び順
        /// </summary>
        public PageSortMode SortMode { get; set; }
    }


    public static class BookFactory
    {
        public static async Task<Book> CreateAsync(QueryPath address, BookCreateSetting setting, Book.Memento memento, CancellationToken token)
        {
            var factory = new BookSourceFactory();
            var bookSource = await factory.CreateAsync(address, setting, token);

            if (bookSource.IsMedia)
            {
                foreach (var page in bookSource.Pages.OfType<MediaPage>())
                {
                    page.IsLastStart = setting.StartPage.StartPageType == BookStartPageType.LastPage;
                }
            }

            var book = new Book(bookSource, memento);

            // ## Start() で行いたい
            book.SetStartPage(setting.StartPage);

            return book;
        }
    }

}
