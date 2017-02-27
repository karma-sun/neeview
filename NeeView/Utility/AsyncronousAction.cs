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
        private ManualResetEventSlim _completed = new ManualResetEventSlim(false);

        /// <summary>
        /// 実行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task ExecuteAsync(Action<CancellationToken> action, CancellationToken token)
        {
            // 非同期で実行
            Run(action, token);

            // 待機
            await WaitAsync(token);
        }

        /// <summary>
        /// 非同期実行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="token"></param>
        public void Run(Action<CancellationToken> action, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            // 非同期で実行
            Task.Run(() =>
            {
                action(token);
                _completed.Set();
            });
        }

        /// <summary>
        /// 実行完了待ち
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task WaitAsync(CancellationToken token)
        {
            await Task.Yield();
            _completed.Wait(token);
        }
    }
}
