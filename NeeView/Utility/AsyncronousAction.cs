// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView.Utility
{
    /// <summary>
    /// 同期処理のキャンセル対応
    /// </summary>
    public class AsynchronousAction
    {
        private Task _task;

        /// <summary>
        /// 実行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task ExecuteAsync(Action<CancellationToken> action, CancellationToken token)
        {
            Run(action, token);
            await WaitAsync(token);
        }

        /// <summary>
        /// 非同期実行(結果有り)
        /// </summary>
        /// <param name="action"></param>
        /// <param name="token"></param>
        public void Run(Action<CancellationToken> action, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            _task = Task.Run(() => action(token));
        }

        /// <summary>
        /// 実行完了待ち
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task WaitAsync(CancellationToken token)
        {
            await Task.Run(() => _task?.Wait(token));
        }
    }

    /// <summary>
    /// 同期処理のキャンセル対応
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AsynchronousAction<T>
    {
        private Task<T> _task;

        /// <summary>
        /// 実行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<T> ExecuteAsync(Func<CancellationToken, T> action, CancellationToken token)
        {
            Run(action, token);
            return await WaitAsync(token);
        }

        /// <summary>
        /// 非同期実行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="token"></param>
        public void Run(Func<CancellationToken, T> action, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            _task = Task.Run(() => { return action(token); });
        }

        /// <summary>
        /// 実行完了待ち
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<T> WaitAsync(CancellationToken token)
        {
            await Task.Run(() => _task.Wait(token));
            token.ThrowIfCancellationRequested();
            return _task.Result;
        }
    }
}
