﻿// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.Threading.Jobs
{
    /// <summary>
    /// Job実行結果
    /// </summary>
    public enum JobResult
    {
        None,
        Completed,
        Canceled,
    }

    /// <summary>
    /// Job基底
    /// キャンセル、終了待機対応
    /// </summary>
    public abstract class JobBase : IJob
    {
        #region Fields

        /// <summary>
        /// キャンセルトークン
        /// </summary>
        private CancellationToken _cancellationToken;

        /// <summary>
        /// 実行完了待ち用フラグ
        /// </summary>
        private ManualResetEventSlim _complete = new ManualResetEventSlim(false);

        /// <summary>
        /// 実行結果
        /// </summary>
        private JobResult _result;

        #endregion

        #region Properties

        /// <summary>
        /// 実行結果
        /// </summary>
        public JobResult Result
        {
            get { return _result; }
            private set { _result = value; _complete.Set(); }
        }

        // キャンセル可能フラグ
        public bool CanBeCanceled => _cancellationToken.CanBeCanceled;

        #endregion

        #region Constructoes

        /// <summary>
        /// constructor
        /// </summary>
        public JobBase()
        {
            _cancellationToken = CancellationToken.None;
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="token"></param>
        public JobBase(CancellationToken token)
        {
            _cancellationToken = token;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Job実行
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteAsync()
        {
            if (_complete.IsSet) return;

            // cancel ?
            if (_cancellationToken.IsCancellationRequested)
            {
                this.Result = JobResult.Canceled;
                return;
            }

            // execute
            try
            {
                await ExecuteAsync(_cancellationToken);
                this.Result = JobResult.Completed;
            }
            catch (OperationCanceledException)
            {
                this.Result = JobResult.Canceled;
                Debug.WriteLine($"command {this}: canceled.");
                OnCanceled();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"command {this}: excepted!!");
                OnException(e);
                throw;
            }
        }

        /// <summary>
        /// Job終了待機
        /// </summary>
        /// <returns></returns>
        public async Task WaitAsync()
        {
            await Task.Run(() => _complete.Wait());
        }

        /// <summary>
        /// Job終了待機
        /// </summary>
        /// <returns></returns>
        public async Task WaitAsync(CancellationToken token)
        {
            await Task.Run(async () =>
            {
                await Task.Yield();
                _complete.Wait(token);
            });
        }

        /// <summary>
        /// Job実行(abstract)
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected abstract Task ExecuteAsync(CancellationToken token);

        /// <summary>
        /// Jobキャンセル時
        /// </summary>
        protected virtual void OnCanceled()
        {
        }

        /// <summary>
        /// Job例外時
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnException(Exception e)
        {
        }

        #endregion
    }
}