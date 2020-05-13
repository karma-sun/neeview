using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class BookController : IDisposable
    {
        // コマンドエンジン
        private BookCommandEngine _commandEngine = new BookCommandEngine();

        private BookSource _book;
        private BookPageViewer _viewer;
        private BookPageMarker _marker;


        public BookController(BookSource book, BookPageViewer viewer, BookPageMarker marker)
        {
            _book = book ?? throw new ArgumentNullException(nameof(book));
            _viewer = viewer ?? throw new ArgumentNullException(nameof(viewer));
            _marker = marker ?? throw new ArgumentNullException(nameof(marker));

            _book.Pages.AddPropertyChanged(nameof(BookPageCollection.SortMode), (s, e) => RequestSort(this));
            _viewer.SettingChanged += (s, e) => RequestRefresh(this, false);
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

        // 前のページに戻る
        public void PrevPage(object sender, int step)
        {
            var s = (step == 0) ? _viewer.PageMode.Size() : step;
            RequestMovePosition(sender, -s);
        }

        // 次のページへ進む
        public void NextPage(object sender, int step)
        {
            var s = (step == 0) ? _viewer.PageMode.Size() : step;
            RequestMovePosition(sender, +s);
        }

        // 前のフォルダーに戻る
        public int PrevFolderPage(object sender)
        {
            var index = _book.Pages.GetPrevFolderIndex(_viewer.GetViewPageIndex());
            if (index < 0) return -1;
            RequestSetPosition(sender, new PagePosition(index, 0), 1);
            return index;
        }

        // 前のフォルダーへ進む
        public int NextFolderPage(object sender)
        {
            var index = _book.Pages.GetNextFolderIndex(_viewer.GetViewPageIndex());
            if (index < 0) return -1;
            RequestSetPosition(sender, new PagePosition(index, 0), 1);
            return index;
        }

        // 最初のページに移動
        public void FirstPage(object sender)
        {
            RequestSetPosition(sender, _book.Pages.FirstPosition(), 1);
        }

        // 最後のページに移動
        public void LastPage(object sender)
        {
            RequestSetPosition(sender, _book.Pages.LastPosition(), -1);
        }

        // 指定ページに移動
        public void JumpPage(object sender, Page page)
        {
            int index = _book.Pages.IndexOf(page);
            if (index >= 0)
            {
                var position = new PagePosition(index, 0);
                RequestSetPosition(sender, position, 1);
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


        [Conditional("DEBUG")]
        private void __CommandWriteLine(string message)
        {
            ////Debug.WriteLine("Command> " + message);
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

            var range = new PageRange(position, direction, _viewer.PageMode.Size());
            var command = new BookCommandAction(sender, Execute, 0);
            _commandEngine.Enqueue(command);

            async Task Execute(object s, CancellationToken token)
            {
                __CommandWriteLine($"Set: {s}, {range}");
                await _viewer.UpdateViewPageAsync(s, range, token);
            }
        }

        // ページ相対移動
        public void RequestMovePosition(object sender, int step)
        {
            var command = new BookCommandJoinAction(sender, Execute, step, 0);
            _commandEngine.Enqueue(command);

            async Task Execute(object s, int value, CancellationToken token)
            {
                __CommandWriteLine($"Move: {s}, {value}");
                await _viewer.MoveViewPageAsync(s, value, token);
            }
        }

        // リフレッシュ
        public void RequestRefresh(object sender, bool isClear)
        {
            var command = new BookCommandAction(sender, Execute, 1);
            _commandEngine.Enqueue(command);

            async Task Execute(object s, CancellationToken token)
            {
                __CommandWriteLine($"Refresh: {s}");
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
                __CommandWriteLine($"Sort: {_book.Pages.SortMode}");
                var page = _viewer.GetViewPage();

                _book.Pages.Sort();

                var index = (_book.Pages.SortMode == PageSortMode.Random && Config.Current.Book.ResetPageWhenRandomSort) ? 0 : _book.Pages.GetIndex(page);
                var pagePosition = new PagePosition(index, 0);
                RequestSetPosition(this, pagePosition, 1);

                await Task.CompletedTask;
            }
        }

        // ページ削除
        public void RequestRemove(object sender, Page page)
        {
            RequestRemove(sender, new List<Page>() { page });
        }

        // ページ削除
        public void RequestRemove(object sender, List<Page> pages)
        {
            if (pages == null) return;
            if (pages.Count == 0) return;

            var command = new BookCommandAction(sender, Execute, 3);
            _commandEngine.Enqueue(command);

            async Task Execute(object s, CancellationToken token)
            {
                __CommandWriteLine($"Remove: Count={pages.Count}");

                foreach (var page in pages)
                {
                    page.IsDeleted = true;
                }

                var next = _book.Pages.GetValidPage(_viewer.GetViewPage());

                _book.Pages.Remove(pages);

                var index = next != null ? next.Index : 0;
                RequestSetPosition(sender, new PagePosition(index, 0), 1);

                await Task.CompletedTask;
            }
        }

    }
}
