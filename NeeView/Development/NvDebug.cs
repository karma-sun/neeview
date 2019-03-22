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
