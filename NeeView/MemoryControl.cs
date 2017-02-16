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
        /// 遅延実行系
        /// </summary>
        private DelayAction _delayAction;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="dispatcher"></param>
        public MemoryControl(Dispatcher dispatcher)
        {
            _delayAction = new DelayAction(dispatcher, () => GC.Collect(), TimeSpan.FromMilliseconds(100));
        }

        /// <summary>
        /// GCリクエスト
        /// </summary>
        public void GarbageCollect()
        {
            if (IsAutoGC) return;

            _delayAction.Request();
        }
    }
}
