// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView.Utility
{
    public static class Process
    {
        //
        public static Task ActionAsync(Action action, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return Task.Run(() => action());
        }

        //
        public static Task ActionAsync(Action<CancellationToken> action, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return Task.Run(() => action(token));
        }

        //
        public static Task WaitAsync(Task task)
        {
            return ActionAsync(() => task.Wait(), CancellationToken.None);
        }

        //
        public static Task WaitAsync(Task task, CancellationToken token)
        {
            return ActionAsync(() => task.Wait(token), token);
        }

        //
        public static Task<T> FuncAsync<T>(Func<T> func)
        {
            return Task.Run(() => func());
        }

        //
        public static Task<T> FuncAsync<T>(Func<T> func, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return Task.Run(() => func());
        }
        
        //
        public static Task<T> FuncAsync<T>(Func<CancellationToken, T> func, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return Task.Run(() => func(token));
        }

        //
        public static Task<T> WaitAsync<T>(Task<T> task, CancellationToken token)
        {
            return FuncAsync((t) =>
            {
                task.Wait(token);
                return task.Result;
            }
            , token);
        }
    }
}
