using NeeLaboratory.IO.Search;
using NeeView.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// 検索エンジン
    /// </summary>
    public class SearchEngine : IDisposable
    {
        #region Fields

        /// <summary>
        /// インデックスフィルタ用無効パス
        /// </summary>
        private static List<string> _ignores = new List<string>()
        {
            // Windows フォルダを除外
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.Windows),
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.Windows) + ".old",
        };


        /// <summary>
        /// 検索エンジン
        /// </summary>
        private NeeLaboratory.IO.Search.SearchEngine _engine;

        #endregion

        #region Constructors

        //
        public SearchEngine(string path, bool includeSubdirectories)
        {
            Path = path;
            IncludeSubdirectories = includeSubdirectories;

            ////Debug.WriteLine($"SearchEngine: {path}");
            _engine = new NeeLaboratory.IO.Search.SearchEngine();
            _engine.Context.NodeFilter = NodeFilter;
            _engine.SetSearchAreas(new List<SearchArea> { new SearchArea(path, includeSubdirectories) });
            _engine.Start();
        }

        #endregion

        #region Properties

        public bool IsBusy => _engine != null && _engine.State != SearchCommandEngineState.Idle;

        public string Path { get; private set; }

        public bool IncludeSubdirectories { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// インデックスフィルタ
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private static bool NodeFilter(FileSystemInfo info)
        {
            // 属性フィルター
            if ((info.Attributes & (FileAttributes.ReparsePoint | FileAttributes.System | FileAttributes.Temporary)) != 0)
            {
                return false;
            }

            if ((info.Attributes & FileAttributes.Hidden) != 0 && !Config.Current.System.IsHiddenFileVisibled)
            {
                return false;
            }

            // ディレクトリ無効フィルター
            if ((info.Attributes & FileAttributes.Directory) != 0)
            {
                var infoFullName = info.FullName;
                var infoLen = infoFullName.Length;

                foreach (var ignore in _ignores)
                {
                    var ignoreLen = ignore.Length;

                    if (ignoreLen == infoLen || (ignoreLen < infoLen && infoFullName[ignoreLen] == '\\'))
                    {
                        if (infoFullName.StartsWith(ignore, true, null))
                        {
                            return false;
                        }
                    }
                }
            }

            // 対応アーカイブ判定。ショートカットもアーカイブの可能性があるため有効とする
            else
            {
                if (!ArchiverManager.Current.IsSupported(info.Name, false) && !FileShortcut.IsShortcut(info.Name))
                {
                    return false;
                }
            }

            return true;
        }


        //
        public void Stop()
        {
            _engine?.Stop();
            _engine = null;
        }

        //
        public async Task<SearchResultWatcher> SearchAsync(string keyword, NeeLaboratory.IO.Search.SearchOption option = null)
        {
            if (_engine == null) throw new InvalidOperationException();

            // 検索
            option = option ?? new NeeLaboratory.IO.Search.SearchOption();
            var result = await _engine.SearchAsync(keyword.Trim(), option);

            // 監視開始
            var watcher = new SearchResultWatcher(_engine, result);
            watcher.Start();

            return watcher;
        }

        //
        public void CancelSearch()
        {
            _engine?.CancelSearch();
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
                    Stop();

                    if (_engine != null)
                    {
                        _engine.Dispose();
                    }
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
