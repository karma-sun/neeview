using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// フォルダーリスト用検索エンジン
    /// 検索結果は同時に１つのみ存在
    /// </summary>
    public class FolderSearchEngine : IDisposable
    {
        #region Fields

        private SearchEngine _searchEngine;

        #endregion

        #region Methods

        public async Task<NeeLaboratory.IO.Search.SearchResultWatcher> SearchAsync(string path, string keyword)
        {
            try
            {
                var searchEngine = GetSearchEngine(path);
                searchEngine.CancelSearch();
                var option = new NeeLaboratory.IO.Search.SearchOption() { AllowFolder = true, SearchMode = NeeLaboratory.IO.Search.SearchMode.Advanced };
                var result = await _searchEngine.SearchAsync(keyword, option);
                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Search Exception: {ex.Message}");
                Reset();
                throw;
            }
        }

        private SearchEngine GetSearchEngine(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            else if (_searchEngine?.Path == path)
            {
                return _searchEngine;
            }
            else
            {
                _searchEngine?.Dispose();
                _searchEngine = new SearchEngine(path);
                return _searchEngine;
            }
        }

        public void CancelSearch()
        {
            _searchEngine?.CancelSearch();
        }

        public void Reset()
        {
            if (_disposedValue) return;

            if (_searchEngine != null)
            {
                _searchEngine.Dispose();
                _searchEngine = null;
            }
        }

        #endregion

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Reset();
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
