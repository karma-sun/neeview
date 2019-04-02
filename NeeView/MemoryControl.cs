using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
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
        public bool IsAutoGC { get; set; } = false;


        /// <summary>
        /// GCメイン
        /// </summary>
        private void GarbageCollectCore()
        {
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce; // cost +20ms
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

        /// <summary>
        /// OutOfMemory発生でメモリクリーンアップしてリトライ
        /// </summary>
        public void RetryActionWithMemoryCleanup(Action action)
        {
            try
            {
                action();
            }
            catch (OutOfMemoryException)
            {
                CleanupDeep();
                action();
            }
        }

        /// <summary>
        /// OutOfMemory発生でメモリクリーンアップしてリトライ
        /// </summary>
        public T RetryFuncWithMemoryCleanup<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch (OutOfMemoryException)
            {
                CleanupDeep();
                return func();
            }
        }

        /// <summary>
        /// キャッシュメモリクリーンアップ
        /// </summary>
        private void CleanupDeep()
        {
            // TODO: サムネイルキャッシュ開放
            Debug.WriteLine($">> OutOfMemory -> CleanUp");

            Book.Default?.BookMemoryService.CleanupDeep();
            GarbageCollect(true);
        }

        #region Memento
        [DataContract]
        public class Memento
        {
            [Obsolete]
            [DataMember(EmitDefaultValue = false)]
            public bool IsAutoGC { get; set; }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            ////memento.IsAutoGC = this.IsAutoGC;
            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;
            ////this.IsAutoGC = memento.IsAutoGC;
        }
        #endregion

    }
}
