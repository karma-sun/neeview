// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeLaboratory.IO.Search;
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
    class SearchEngine : IDisposable
    {
        #region Fields

        /// <summary>
        /// インデックスフィルタ用無効属性
        /// </summary>
        private static FileAttributes _ignoreAttributes = FileAttributes.ReparsePoint | FileAttributes.Hidden | FileAttributes.System | FileAttributes.Temporary;

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
        public SearchEngine(string path)
        {
            Debug.WriteLine($"SearchEngine: {path}");
            _engine = new NeeLaboratory.IO.Search.SearchEngine();
            _engine.Context.NodeFilter = NodeFilter;
            _engine.SetSearchAreas(new List<string> { path });
            _engine.Start();
        }

        #endregion

        #region Properties

        //
        public bool IsBusy => _engine != null && _engine.State != SearchCommandEngineState.Idle;

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
            if ((info.Attributes & _ignoreAttributes) != 0)
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

            // 対応アーカイブ判定
            else
            {
                if (!ArchiverManager.Current.IsSupported(info.Name, false))
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
        public string GetStatuMessage()
        {
            if (_engine == null) return "停止";

            if (_engine.State == SearchCommandEngineState.Idle)
            {
                return "待機中";
            }
            else if (_engine.State == SearchCommandEngineState.Collect)
            {
                return $"{_engine.NodeCountMaybe:#,0} 個のインデックス作成中...";
            }
            else
            {
                return "処理中...";
            }
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

        //
        public void Dispose()
        {
            Stop();
        }

        #endregion
    }
}
