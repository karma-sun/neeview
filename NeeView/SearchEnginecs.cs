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
    class SearchEngine : IDisposable
    {
        #region Fields

        private NeeLaboratory.IO.Search.SearchEngine _engine;

        #endregion

        #region Constructors

        //
        public SearchEngine(string path)
        {
            // Windowsフォルダを検索対象からはずす
            Node.IgnorePathCollection = new List<string>()
            {
                System.Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                System.Environment.GetFolderPath(Environment.SpecialFolder.Windows) + ".old"
            };

            // 検索から除外するファイル属性設定
            Node.IgnoreFileAttributes = FileAttributes.System | FileAttributes.ReparsePoint | FileAttributes.Temporary | FileAttributes.Hidden;


            Debug.WriteLine($"SearchEngine: {path}");
            _engine = new NeeLaboratory.IO.Search.SearchEngine();
            _engine.SetSearchAreas(new List<string> { path });
            _engine.Start();
        }

        #endregion

        #region Properties

        //
        public bool IsBusy => _engine != null && _engine.State != SearchCommandEngineState.Idle;

        #endregion

        #region Methods

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
