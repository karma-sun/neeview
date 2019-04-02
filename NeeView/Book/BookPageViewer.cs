using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class BookPageViewer : BindableBase, IDisposable
    {
        private BookSource _book;

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
        private BookMemoryService _bookMemoryService;

        // 先読み
        private BookAhead _ahead;

        private object _lock = new object();



        public BookPageViewer(BookSource book, BookMemoryService memoryService, BookViewerCreateSetting setting)
        {
            _book = book ?? throw new ArgumentNullException(nameof(book));
            _bookMemoryService = memoryService;

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
                /*
                if (_book.IsMediaLastPlay)
                {
                    viewPage.IsLastStart = true;
                }
                */

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


    public class BookViewerCreateSetting
    {
        public PageMode PageMode { get; set; }
        public PageReadOrder BookReadOrder { get; set; }
        public bool IsSupportedDividePage { get; set; }
        public bool IsSupportedSingleFirstPage { get; set; }
        public bool IsSupportedSingleLastPage { get; set; }
        public bool IsSupportedWidePage { get; set; }
    }


}
