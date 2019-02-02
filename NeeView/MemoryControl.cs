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
        static MemoryControl() => Current = new MemoryControl();
        public static MemoryControl Current { get; }


        private DelayAction _delayAction;


        private MemoryControl()
        {
            _delayAction = new DelayAction(App.Current.Dispatcher, TimeSpan.FromSeconds(0.2), GarbageCollectCore, TimeSpan.FromMilliseconds(100));
        }


        /// <summary>
        /// 自動GCフラグ
        /// </summary>
        [PropertyMember("@ParamIsAutoGC", Tips = "@ParamIsAutoGCTips")]
        public bool IsAutoGC { get; set; } = true;


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
