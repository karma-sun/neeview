using System;
using System.Diagnostics;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// App.Current.Dispatcher.Invoke系のラッパー
    /// </summary>
    public static class AppDispatcher
    {
        public static void Invoke(Action action)
        {
            if (Application.Current == null) return;

#if DEBUG
            var callStack = new StackFrame(1, true);
            var sourceFile = System.IO.Path.GetFileName(callStack.GetFileName());
            int sourceLine = callStack.GetFileLineNumber();
            var sw = Stopwatch.StartNew();
            App.Current.Dispatcher.Invoke(action);
            ////Debug.WriteLine($"App.Dispatcher.Invoke: {sourceFile}({sourceLine}):  {sw.ElapsedMilliseconds}ms");
#else
            this.Dispatcher.Invoke(action);
#endif
        }

        public static void BeginInvoke(Action action)
        {
            if (Application.Current == null) return;

#if DEBUG
            var callStack = new StackFrame(1, true);
            var sourceFile = System.IO.Path.GetFileName(callStack.GetFileName());
            int sourceLine = callStack.GetFileLineNumber();
            App.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                var sw = Stopwatch.StartNew();
                action();
                ////Debug.WriteLine($"App.Dispatcher.BeginInvoke: {sourceFile}({sourceLine}):  {sw.ElapsedMilliseconds}ms");
            }));
#else
            this.Dispatcher.BeginInvoke(action);
#endif
        }
    }
}
