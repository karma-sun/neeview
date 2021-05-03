using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    public class BookPageMarker
    {
        private BookSource _book;
        private BookPageViewer _viewer;

        // ページマップ
        private Dictionary<string, Page> _pageMap = new Dictionary<string, Page>();

        public BookPageMarker(BookSource book, BookPageViewer viewer)
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
                page.IsMarked = false;
            }
            foreach (var page in Markers)
            {
                page.IsMarked = true;
            }
        }


        public void SetMarkers(IEnumerable<Page> pages)
        {
            var oldies = Markers;
            Markers = pages.ToList();

            foreach (var page in oldies.Where(e => !Markers.Contains(e)))
            {
                page.IsMarked = false;
            }
            foreach (var page in Markers)
            {
                page.IsMarked = true;
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
            var index = _viewer.GetViewPageIndex();

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

            var index = _viewer.GetViewPageIndex();

            var target =
                direction > 0
                ? list.FirstOrDefault(e => e.Index > index) ?? (isLoop ? list.First() : null)
                : list.LastOrDefault(e => e.Index < index) ?? (isLoop ? list.Last() : null);

            ////if (target == null) return null;
            // TODO コマンド側で処理するべきか。このメソッドはそのパラメータ生成用にする。
            ////RequestSetPosition(sender, new PagePosition(target.Index, 0), 1);

            return target;
        }

        private void Pages_PageRemoved(object sender, PageRemovedEventArgs e)
        {
            foreach (var page in e.Pages)
            {
                if (_pageMap.TryGetValue(page.EntryFullName, out Page target) && page == target)
                {
                    _pageMap.Remove(page.EntryFullName);
                }
            }
        }

        #endregion
    }
}
