﻿using NeeView.Collections.Generic;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public partial class Book : IDisposable
    {
        public static Book Default { get; private set; }

        private BookMemoryService _bookMemoryService = new BookMemoryService();

        private BookSource _source;
        private BookPageViewer _viewer;
        private BookPageMarker _marker;
        private BookController _controller;
        private string _sourceAddress;
        private BookLoadOption _loadOption;

        public Book(BookSource source, QueryPath sourceAddress, Book.Memento memento, BookLoadOption option)
        {
            Book.Default = this;

            _source = source;
            _sourceAddress = sourceAddress.SimplePath;
            _viewer = new BookPageViewer(_source, _bookMemoryService, CreateBookViewerCreateSetting(memento));
            _marker = new BookPageMarker(_source, _viewer);
            _controller = new BookController(_source, _viewer, _marker);
            _loadOption = option;
        }

        public BookSource Source => _source;
        public BookPageCollection Pages => _source.Pages;
        public BookPageViewer Viewer => _viewer;
        public BookPageMarker Marker => _marker;
        public BookController Control => _controller;
        public BookMemoryService BookMemoryService => _bookMemoryService;

        public string Address => _source.Address;
        public string SourceAddress => _sourceAddress;
        public bool IsMedia => _source.IsMedia;
        public bool IsPlaylist => _source.IsPlaylist;
        public bool IsTemporary => _source.Address.StartsWith(Temporary.Current.TempDirectory);

        public PageSortModeClass PageSortModeClass => IsPlaylist ? PageSortModeClass.WithEntry : PageSortModeClass.Normal;
        public BookLoadOption LoadOption => _loadOption;

        // はじめて開く本
        public bool IsNew { get; set; }

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

        public void SetStartPage(object sender, BookStartPage startPage)
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
                int index = !string.IsNullOrEmpty(startPage.PageName) ? _source.Pages.FindIndex(e => e.EntryName == startPage.PageName) : 0;
                if (index < 0)
                {
                    this.NotFoundStartPage = startPage.PageName;
                }
                position = index >= 0 ? new PagePosition(index, 0) : _source.Pages.FirstPosition();
                direction = 1;

                // 最終ページリセット
                // NOTE: ワイドページ判定は行わないため、2ページモードの場合に不正確な場合がある
                int lastPageOffset = (_viewer.PageMode == PageMode.WidePage && !_viewer.IsSupportedSingleLastPage) ? 1 : 0;
                if (startPage.IsResetLastPage && index >= _source.Pages.LastPosition().Index - lastPageOffset)
                {
                    position = _source.Pages.FirstPosition();
                }
            }

            // 開始ページ記憶
            this.StartEntry = _source.Pages.Count > 0 ? _source.Pages[position.Index].EntryName : null;

            // 初期ページ設定 
            _controller.RequestSetPosition(sender, position, direction);
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

            memento.Path = _source.Address;
            memento.IsDirectorty = _source.IsDirectory;
            memento.Page = _source.Pages.SortMode != PageSortMode.Random ? _viewer.GetViewPage()?.EntryName : null;

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

            _viewer.PageMode = memento.PageMode;
            _viewer.BookReadOrder = memento.BookReadOrder;
            _viewer.IsSupportedDividePage = memento.IsSupportedDividePage;
            _viewer.IsSupportedSingleFirstPage = memento.IsSupportedSingleFirstPage;
            _viewer.IsSupportedSingleLastPage = memento.IsSupportedSingleLastPage;
            _viewer.IsSupportedWidePage = memento.IsSupportedWidePage;
            _source.IsRecursiveFolder = memento.IsRecursiveFolder;
            _source.Pages.SortMode = memento.SortMode;
        }

        private static BookPageViewSetting CreateBookViewerCreateSetting(Book.Memento memento)
        {
            var setting = new BookPageViewSetting
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

        /// <summary>
        /// キャッシュ無効
        /// </summary>
        public bool IsIgnoreCache { get; set; }

        /// <summary>
        /// ロード設定フラグ
        /// </summary>
        public BookLoadOption LoadOption { get; set; }
    }


    public static class BookFactory
    {
        public static async Task<Book> CreateAsync(object sender, QueryPath address, QueryPath sourceAddress, BookCreateSetting setting, Book.Memento memento, CancellationToken token)
        {
            var factory = new BookSourceFactory();
            var bookSource = await factory.CreateAsync(address, setting, token);

            if (bookSource.IsMedia)
            {
                foreach (var mediaContent in bookSource.Pages.Select(e => e.ContentAccessor).OfType<MediaContent>())
                {
                    mediaContent.IsLastStart = setting.StartPage.StartPageType == BookStartPageType.LastPage;
                }
            }

            var book = new Book(bookSource, sourceAddress, memento, setting.LoadOption);

            // HACK: Start() で行いたい
            book.SetStartPage(sender, setting.StartPage);

            return book;
        }
    }

}
