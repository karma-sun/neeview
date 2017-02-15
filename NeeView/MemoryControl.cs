// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// ヒープ管理
    /// </summary>
    public class MemoryControl
    {
        /// <summary>
        /// 標準インスタンス
        /// </summary>
        public static MemoryControl Current { get; set; }

        /// <summary>
        /// 自動GCフラグ
        /// </summary>
        public bool IsAutoGC { get; set; }

        /// <summary>
        /// GC要求カウンタ
        /// </summary>
        private int _CollectRequest;

        /// <summary>
        /// 最後のGC要求時間
        /// </summary>
        private DateTime _lastRequestTime;

        /// <summary>
        /// GC遅延実行のためのタイマー
        /// </summary>
        private DispatcherTimer _timer;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="dispatcher"></param>
        public MemoryControl(Dispatcher dispatcher)
        {
            // timer for delay GC
            _timer = new DispatcherTimer(DispatcherPriority.Normal, dispatcher);
            _timer.Interval = TimeSpan.FromSeconds(0.1);
            _timer.Tick += new EventHandler(DispatcherTimer_Tick);
        }

        /// <summary>
        /// GCリクエスト
        /// </summary>
        public void GarbageCollect()
        {
            if (IsAutoGC) return;

            _CollectRequest++;
            _lastRequestTime = DateTime.Now;
            _timer.Start();
        }

        /// <summary>
        /// timer callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {

            if (_CollectRequest > 0 && (DateTime.Now - _lastRequestTime).TotalMilliseconds > 100)
            {
                //Debug.WriteLine($"GC : {_isCollectRequest}");
                _CollectRequest = 0;
                _timer.Stop();
                GC.Collect();
            }
        }

    }
}
