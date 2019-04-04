﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// ViewPageCollection Generate Process
    /// </summary>
    public class BookViewGenerater : IDisposable
    {
        private BookSource _book;
        private BookViewSetting _setting;

        private object _viewPageSender;
        private PageDirectionalRange _viewRange;
        private PageDirectionalRange _nextRange;
        private PageDirectionalRange _contentRange;
        private int _contentsCount;

        private Task _task;
        private CancellationTokenSource _cancellationTokenSource;
        private object _lock = new object();
        private SemaphoreSlim _semaphore;

        public BookViewGenerater(BookSource book, BookViewSetting setting, object sender, PageDirectionalRange viewPageRange, PageDirectionalRange aheadPageRange)
        {
            _book = book;
            _setting = setting;

            _viewPageSender = sender;
            _viewRange = viewPageRange;
            _nextRange = viewPageRange;
            _contentRange = viewPageRange.Add(aheadPageRange);
            _contentsCount = 0;

            _cancellationTokenSource = new CancellationTokenSource();
            _semaphore = new SemaphoreSlim(0);
            _task = TasnEntry(_cancellationTokenSource.Token);
        }

        // 表示コンテンツ変更
        // 表示の更新を要求
        public event EventHandler<ViewPageCollectionChangedEventArgs> ViewContentsChanged;

        // 先読みコンテンツ変更
        public event EventHandler<ViewPageCollectionChangedEventArgs> NextContentsChanged;


        #region IDisposable Support
        private bool _disposedValue = false;

        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId ="_semaphore")]
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    ViewContentsChanged = null;
                    NextContentsChanged = null;
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
                    // _semaphore は Taskの最後で Disposeされる
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion


        public void UpdateNextContents()
        {
            if (_disposedValue) return;

            _semaphore.Release();
        }

        private async Task TasnEntry(CancellationToken token)
        {
            try
            {
                await UpdateNextContentsAsync(token);
                ////Debug.WriteLine($"> RunUpdateViewContents: done.");
            }
            catch (Exception)
            {
                ////Debug.WriteLine($"> RunUpdateViewContents: {ex.Message}");
            }

            _semaphore.Dispose();
        }

        private async Task UpdateNextContentsAsync(CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                await _semaphore.WaitAsync(token);

                while (true)
                {
                    ViewPageCollection collection;

                    lock (_lock)
                    {
                        // get next collecton.
                        collection = CreateViewPageCollection(_nextRange);

                        // if out of range, return;
                        if (!_contentRange.IsContains(new PagePosition(collection.Range.Last.Index, 0)))
                        {
                            return;
                        }

                        // if collection is not valid, break;
                        if (!collection.IsValid)
                        {
                            break;
                        }

                        // update next range.
                        _nextRange = GetNextRange(collection.Range);
                        _contentsCount++;
                    }

                    token.ThrowIfCancellationRequested();
                    NextContentsChanged?.Invoke(_viewPageSender, new ViewPageCollectionChangedEventArgs(collection) { CancellationToken = token });

                    if (_contentsCount == 1)
                    {
                        ////Debug.WriteLine($"UpdateNextContentsInner: ViewContentChanged");
                        token.ThrowIfCancellationRequested();
                        UpdateViewContentsInner(_viewPageSender, collection, token);
                    }
                }
            }
        }

        public void UpdateViewContents(CancellationToken token)
        {
            ViewPageCollection collection;
            lock (_lock)
            {
                if (_contentsCount > 0)
                {
                    return;
                }

                collection = CreateViewPageCollection(_viewRange);
            }

            ////Debug.WriteLine($"UpdateViewContents: ViewContentChanged");
            UpdateViewContentsInner(_viewPageSender, collection, token);
        }

        private void UpdateViewContentsInner(object sender, ViewPageCollection collection, CancellationToken token)
        {
            var page = collection.Collection[0].Page;
            ////Debug.WriteLine($"UpdateViewContentsInner: {page.EntryName}, {page.Content.IsAllLoaded}");

            var args = new ViewPageCollectionChangedEventArgs(collection) { IsForceResize = true, CancellationToken = token };
            ViewContentsChanged?.Invoke(sender, args);
        }


        private PageDirectionalRange GetNextRange(PageDirectionalRange previous)
        {
            // 先読みコンテンツ領域計算
            var position = previous.Next();
            var direction = previous.Direction;
            var range = new PageDirectionalRange(position, direction, _setting.PageMode.Size());

            return range;
        }


        // ページのワイド判定
        private bool IsWide(Page page)
        {
            return page.Width > page.Height * BookProfile.Current.WideRatio;
        }

        // 見開きモードでも単独表示するべきか判定
        private bool IsSoloPage(int index)
        {
            if (_setting.IsSupportedSingleFirstPage && index == 0) return true;
            if (_setting.IsSupportedSingleLastPage && index == _book.Pages.Count - 1) return true;
            if (_book.Pages[index] is ArchivePage) return true;
            if (_setting.IsSupportedWidePage && IsWide(_book.Pages[index])) return true;
            return false;
        }

        // 分割モード有効判定
        private bool IsEnableDividePage(int index)
        {
            return (_setting.PageMode == PageMode.SinglePage && !_book.IsMedia && _setting.IsSupportedDividePage && IsWide(_book.Pages[index]));
        }

        // 表示コンテンツソースと、それに対応したコンテキスト作成
        private ViewPageCollection CreateViewPageCollection(PageDirectionalRange source)
        {
            var infos = new List<PagePart>();

            {
                PagePosition position = source.Position;
                for (int id = 0; id < _setting.PageMode.Size(); ++id)
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

                    infos.Add(new PagePart(position, size, _setting.BookReadOrder));
                    position = position + ((source.Direction > 0) ? size : -1);
                }
            }

            // 見開き補正
            if (_setting.PageMode == PageMode.WidePage && infos.Count >= 2)
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
                contentsSource.Add(viewPage);
            }

            // 並び順補正
            if (source.Direction < 0 && infos.Count >= 2)
            {
                contentsSource.Reverse();
                infos.Reverse();
            }

            // 左開き
            if (_setting.BookReadOrder == PageReadOrder.LeftToRight)
            {
                contentsSource.Reverse();
            }

            // 単一ソースならコンテンツは１つにまとめる
            if (infos.Count == 2 && infos[0].Position.Index == infos[1].Position.Index)
            {
                var position = new PagePosition(infos[0].Position.Index, 0);
                contentsSource.Clear();
                contentsSource.Add(new ViewPage(_book.Pages[position.Index], new PagePart(position, 2, _setting.BookReadOrder)));
            }

            // 新しいコンテキスト
            var context = new ViewPageCollection(new PageDirectionalRange(infos, source.Direction), contentsSource);
            return context;
        }

    }
}
