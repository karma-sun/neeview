﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
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
            App.Current.Dispatcher.Invoke(action);
#endif
        }

        public static TResult Invoke<TResult>(Func<TResult> callback)
        {
            if (Application.Current == null) throw new InvalidOperationException();

            return App.Current.Dispatcher.Invoke(callback);
        }

        public static void InvokeSTA(Action action)
        {
            if (System.Threading.Thread.CurrentThread.GetApartmentState() == System.Threading.ApartmentState.STA)
            {
                action.Invoke();
            }
            else
            {
                if (Application.Current == null) throw new InvalidOperationException();
                App.Current.Dispatcher.Invoke(action);
            }
        }

        public static TResult InvokeSTA<TResult>(Func<TResult> callback)
        {
            if (System.Threading.Thread.CurrentThread.GetApartmentState() == System.Threading.ApartmentState.STA)
            {
                return callback.Invoke();
            }
            else
            {
                if (Application.Current == null) throw new InvalidOperationException();
                return App.Current.Dispatcher.Invoke(callback);
            }
        }

        public static async Task InvokeAsync(Action action)
        {
            if (Application.Current == null) return;

#if DEBUG
            var callStack = new StackFrame(1, true);
            var sourceFile = System.IO.Path.GetFileName(callStack.GetFileName());
            int sourceLine = callStack.GetFileLineNumber();
            var sw = Stopwatch.StartNew();
            await App.Current.Dispatcher.InvokeAsync(action);
            ////Debug.WriteLine($"App.Dispatcher.Invoke: {sourceFile}({sourceLine}):  {sw.ElapsedMilliseconds}ms");
#else
            await App.Current.Dispatcher.InvokeAsync(action);
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
            App.Current.Dispatcher.BeginInvoke(action);
#endif
        }
    }
}
