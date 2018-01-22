// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.Threading.Jobs
{
    /// <summary>
    /// Jobエンジン
    /// TODO: Logger
    /// </summary>
    public class JobEngine : IDisposable
    {
        #region Fields

        /// <summary>
        /// ワーカータスクのキャンセルトークン
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// エンジン稼働フラグ
        /// </summary>
        private ManualResetEventSlim _isEnabled = new ManualResetEventSlim(false);

        /// <summary>
        /// 予約Job存在通知
        /// </summary>
        private ManualResetEventSlim _ready = new ManualResetEventSlim(false);

        /// <summary>
        /// lock
        /// </summary>
        private object _lock = new object();

        /// <summary>
        /// 予約Jobリスト
        /// </summary>
        protected Queue<IJob> _queue = new Queue<IJob>();

        /// <summary>
        /// 実行中Job
        /// </summary>
        protected IJob _job;

        #endregion

        #region Constructors

        /// <summary>
        /// constructor
        /// </summary>
        public JobEngine()
        {
            Initialize();
        }

        #endregion

        #region Events

        /// <summary>
        /// エラー発生時のイベント
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;

        #endregion

        #region Properties

        /// <summary>
        /// エンジン稼働フラグ
        /// </summary>
        public bool IsEnabled
        {
            get { return _isEnabled.IsSet; }
            set { if (value) _isEnabled.Set(); else _isEnabled.Reset(); }
        }

        /// <summary>
        /// 現在のJob数
        /// </summary>
        public int Count
        {
            get { return _queue.Count + (_job != null ? 1 : 0); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Job登録
        /// </summary>
        /// <param name="command"></param>
        public virtual void Enqueue(IJob command)
        {
            lock (_lock)
            {
                if (OnEnqueueing(command))
                {
                    _queue.Enqueue(command);
                    OnEnqueued(command);
                    _ready.Set();
                }
            }
        }

        /// <summary>
        /// Queue登録前の処理
        /// </summary>
        /// <param name="command"></param>
        protected virtual bool OnEnqueueing(IJob command)
        {
            return true;
        }

        /// <summary>
        /// Queue登録後の処理
        /// </summary>
        protected virtual void OnEnqueued(IJob command)
        {
            // nop.
        }


        /// <summary>
        /// 初期化
        /// ワーカータスク起動
        /// </summary>
        private void Initialize()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(async () =>
            {
                try
                {
                    await WorkerAsync(_cancellationTokenSource.Token);
                }
                catch (Exception e)
                {
                    // 例外停止はイベントで発行
                    Error?.Invoke(this, new ErrorEventArgs(e));
                }
            });
        }

        /// <summary>
        /// ワーカータスク終了
        /// </summary>
        public virtual void Dispose()
        {
            lock (_lock)
            {
                // 停止命令発行
                _cancellationTokenSource?.Cancel();
            }
        }

        /// <summary>
        /// ワーカータスク
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task WorkerAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    _ready.Wait(token);
                    _isEnabled.Wait(token);

                    while (!token.IsCancellationRequested)
                    {
                        lock (_lock)
                        {
                            if (_queue.Count <= 0)
                            {
                                _job = null;
                                _ready.Reset();
                                break;
                            }

                            _job = _queue.Dequeue();
                        }

                        ////Logger.Trace($"{_command}: start... :rest={_queue.Count}");
                        await _job?.ExecuteAsync();
                        ////Logger.Trace($"{_command}: done.");
                        ////if (_command is JobBase cmd)
                        ////{
                        ////    Logger.Trace($"{cmd}: result={cmd.Result}");
                        ////}

                        _job = null;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _job = null;
            }
        }

        #endregion
    }
}
