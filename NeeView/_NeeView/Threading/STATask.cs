// from https://qiita.com/tricogimmick/items/f3afc94e7133e9c641a7

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView.Threading
{
    class STATask
    {
        public static Task Run<T>(Func<T> func)
        {
            var tcs = new TaskCompletionSource<T>();
            var thread = new Thread(() =>
            {
                try
                {
                    tcs.SetResult(func());
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        public static Task Run(Action act)
        {
            return Run(() =>
            {
                act();
                return true;
            });
        }

        public static void Sleep(int ms)
        {
            Thread.Sleep(ms);
        }
    }
}
