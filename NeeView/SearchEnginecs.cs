// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeLaboratory.IO.Search;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    class SearchEngine : IDisposable
    {
        private NeeLaboratory.IO.Search.SearchEngine _engine;

        //
        public SearchEngine(string path)
        {
            Debug.WriteLine($"SearchEngine: {path}");
            _engine = new NeeLaboratory.IO.Search.SearchEngine();
            _engine.SetSearchAreas(new List<string> { path });
            _engine.Start();
        }

        //
        public bool IsBusy => _engine != null &&  _engine.State != SearchEngineState.Idle;

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

            if (_engine.State == SearchEngineState.Idle)
            {
                return "待機中";
            }
            else if (_engine.State == SearchEngineState.Collect)
            {
                return $"{_engine.NodeCountMaybe:#,0} 個のインデックス作成中...";
            }
            else
            {
                return "処理中...";
            }
        }

        //
        public async Task<SearchResultWatcher> SearchAsync(string keyword, SearchOption option = null)
        {
            if (_engine == null) throw new InvalidOperationException();

            // 検索
            option = option ?? new SearchOption();
            var result = await _engine.SearchAsync(keyword.Trim(), option);

            // 監視開始
            var watcher = new SearchResultWatcher(_engine, result);
            watcher.Start();

            return watcher;
        }

        //
        public void Dispose()
        {
            Stop();
        }
    }
}
