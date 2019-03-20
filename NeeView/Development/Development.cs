using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class Development : BindableBase
    {
        static Development() => Current = new Development();
        public static Development Current { get; }

        private Development()
        {
        }

        /// <summary>
        /// IsVisibleDevPageList property.
        /// </summary>
        private bool _IsVisibleDevPageList;
        public bool IsVisibleDevPageList
        {
            get { return _IsVisibleDevPageList; }
            set { if (_IsVisibleDevPageList != value) { _IsVisibleDevPageList = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// IsVisibleDevInfo property.
        /// </summary>
        private bool _IsVisibleDevInfo;
        public bool IsVisibleDevInfo
        {
            get { return _IsVisibleDevInfo; }
            set { if (_IsVisibleDevInfo != value) { _IsVisibleDevInfo = value; RaisePropertyChanged(); } }
        }
    }


    public static class NvDebug
    {
        [Conditional("DEBUG")]
        public static void __DumpThread(string s = null)
        {
            Debug.WriteLine($"> ThreadId: {Thread.CurrentThread.ManagedThreadId}: {s}");
        }

        [Conditional("DEBUG")]
        private static void __Delay(int ms)
        {
            Thread.Sleep(ms);
        }
    }
}
