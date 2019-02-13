using NeeLaboratory.IO.Search;
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

        public bool IncludeSubdirectories { get; set; } = true;
        public bool IsRegularExpression { get; set; }

        #region Methods

        public async Task<SearchResultWatcher> SearchAsync(string path, string keyword)
        {
            try
            {
                var searchEngine = GetSearchEngine(path, IncludeSubdirectories);
                searchEngine.CancelSearch();
                var option = new SearchOption()
                {
                    AllowFolder = true,
                    SearchMode = IsRegularExpression ? SearchMode.RegularExpression : SearchMode.Advanced,
                };
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

        private SearchEngine GetSearchEngine(string path, bool includeSubdirectories)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            else if (_searchEngine != null && _searchEngine.Path == path && _searchEngine.IncludeSubdirectories == includeSubdirectories)
            {
                return _searchEngine;
            }
            else
            {
                _searchEngine?.Dispose();
                _searchEngine = new SearchEngine(path, includeSubdirectories);
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
