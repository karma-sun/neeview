// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public static MemoryControl Current { get; private set; }

        #region Fields

        private DelayAction _delayAction;

        #endregion

        #region Constructors

        public MemoryControl(Dispatcher dispatcher)
        {
            Current = this;
            _delayAction = new DelayAction(dispatcher, TimeSpan.FromSeconds(0.2), GarbageCollectCore, TimeSpan.FromMilliseconds(100));
        }

        #endregion

        #region Properties

        /// <summary>
        /// 自動GCフラグ
        /// </summary>
        [PropertyMember("メモリ開放をシステムに任せる", Tips = "OFFの時はページ切り替え毎にメモリ開放を行います")]
        public bool IsAutoGC { get; set; } = true;

        #endregion

        #region Methods

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

        #endregion

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
