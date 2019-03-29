﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// ページ先読み
    /// </summary>
    public class BookAhead : IDisposable
    {
        private PageContentJobClient _jobClient = new PageContentJobClient("Ahead", JobCategories.PageAheadContentJobCategory);
        private BookMemoryService _bookMemoryService;

        public BookAhead(BookMemoryService bookMemoryService)
        {
            _bookMemoryService = bookMemoryService;
        }

        private List<Page> _pages;
        private int _index;
        private Page _page;
        private object _lock = new object();

        public void Order(List<Page> pages)
        {
            lock (_lock)
            {
                _pages = pages;
                _index = 0;
                _page = null;

                LoadNext();
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _pages = null;
                _index = 0;
                _page = null;
            }
        }

        /// <summary>
        /// ページ読み込み完了を通知して次の先読みを行う
        /// </summary>
        public void OnPageLoaded(object sender, PageChangedEventArgs e)
        {
            if (e.Page != _page) return;

            if (_bookMemoryService.IsFull) return;

            LoadNext();
        }

        private void LoadNext()
        {
            lock (_lock)
            {
                do
                {
                    if (_pages is null || _index >= _pages.Count) return;
                    _page = _pages[_index];
                    _page.State = PageContentStateExtension.Max(_page.State, PageContentState.Ahead);
                    _index++;
                }
                while (_page.IsContentAlived);

                _jobClient.Order(new List<Page>() { _page });
            }
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Clear();
                    _jobClient.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
