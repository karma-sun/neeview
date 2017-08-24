// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
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
        public static MemoryControl Current { get; private set; }

        /// <summary>
        /// 自動GCフラグ
        /// </summary>
        public bool IsAutoGC { get; set; } = true;

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
            Current = this;
            _delayAction = new DelayAction(dispatcher, TimeSpan.FromSeconds(0.2), GarbageCollectCore, TimeSpan.FromMilliseconds(100));
        }

        //
        private void GarbageCollectCore()
        {
            GC.Collect();
        }

        /// <summary>
        /// GCリクエスト
        /// </summary>
        public void GarbageCollect(bool force = false)
        {
            if (force)
            {
                _delayAction.Cancel();
                GarbageCollectCore();
                return;
            }

            if (IsAutoGC) return;

            _delayAction.Request();
        }


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public bool IsAutoGC { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsAutoGC = this.IsAutoGC;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.IsAutoGC = memento.IsAutoGC;
        }
        #endregion

    }
}
