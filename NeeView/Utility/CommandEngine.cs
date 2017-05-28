// Copyright (c) 2016 Mitsuhiro Ito (nee)
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
    /// <summary>
    /// コマンドインターフェイス
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// コマンドキャンセル
        /// </summary>
        void Cancel();

        /// <summary>
        /// コマンド実行
        /// </summary>
        /// <returns></returns>
        Task ExecuteAsync();
    }


    /// <summary>
    /// コマンド実行結果
    /// </summary>
    public enum CommandResult
    {
        None,
        Completed,
        Canceled,
    }

    /// <summary>
    /// コマンド基底
    /// キャンセル、終了待機対応
    /// </summary>
    public abstract class CommandBase : ICommand
    {
        // キャンセルトークン
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        // コマンド終了通知
        private ManualResetEventSlim _complete = new ManualResetEventSlim(false);

        // コマンド実行結果
        private CommandResult _result;
        public CommandResult Result
        {
            get { return _result; }
            set { _result = value; _complete.Set(); }
        }

        // キャンセル可能フラグ
        public bool CanBeCanceled { get; set; } = true;

        /// <summary>
        /// キャンセル要求
        /// </summary>
        public virtual void Cancel()
        {
            if (!CanBeCanceled) return;

            _cancellationTokenSource.Cancel();

            Result = CommandResult.Canceled;
        }


        /// <summary>
        /// コマンド実行
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteAsync()
        {
            if (_complete.IsSet) return;

            // cancel ?
            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                Result = CommandResult.Canceled;
                return;
            }

            // execute
            try
            {
                await ExecuteAsync(_cancellationTokenSource.Token);
                Result = CommandResult.Completed;
            }
            catch (OperationCanceledException)
            {
                Result = CommandResult.Canceled;
                OnCanceled();
            }
            catch (Exception e)
            {
                OnException(e);
                throw;
            }
        }

        /// <summary>
        /// コマンド終了待機
        /// </summary>
        /// <returns></returns>
        public async Task WaitAsync()
        {
            await Task.Run(() => _complete.Wait());
        }


        /// <summary>
        /// コマンド実行(abstract)
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected abstract Task ExecuteAsync(CancellationToken token);


        /// <summary>
        /// コマンドキャンセル時
        /// </summary>
        protected virtual void OnCanceled()
        {
        }

        /// <summary>
        /// コマンド例外時
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnException(Exception e)
        {
        }
    }



    /// <summary>
    /// コマンドエンジン
    /// </summary>
    public class CommandEngine : IEngine
    {
        // ワーカータスクのキャンセルトークン
        private CancellationTokenSource _cancellationTokenSource;

        // 予約コマンド存在通知
        private ManualResetEventSlim _ready = new ManualResetEventSlim(false);

        // 排他処理用ロックオブジェクト
        private object _lock = new object();

        // コマンドリスト
        protected Queue<ICommand> _queue = new Queue<ICommand>();

        // 実行中コマンド
        protected ICommand _command;

        /// <summary>
        /// コマンド登録
        /// </summary>
        /// <param name="command"></param>
        public virtual void Enqueue(ICommand command)
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
        protected virtual bool OnEnqueueing(ICommand command)
        {
            return true;
        }

        /// <summary>
        /// Queue登録後の処理
        /// </summary>
        protected virtual void OnEnqueued(ICommand command)
        {
            // nop.
        }

        /// <summary>
        /// 現在のコマンド数
        /// </summary>
        public int Count
        {
            get { return _queue.Count + (_command != null ? 1 : 0); }
        }

        //
        private bool _isActive;

        /// <summary>
        /// 初期化
        /// ワーカータスク起動
        /// </summary>
        private void Initialize()
        {
            if (_isActive) return;
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(async () => await WorkerAsync(_cancellationTokenSource.Token));
        }

        /// <summary>
        /// ワーカータスク終了
        /// </summary>
        private void Dispose()
        {
            if (!_isActive) return;

            lock (_lock)
            {
                _cancellationTokenSource?.Cancel();
                _command?.Cancel();
                _isActive = false;
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

                    while (!token.IsCancellationRequested)
                    {
                        lock (_lock)
                        {
                            if (_queue.Count <= 0)
                            {
                                _command = null;
                                _ready.Reset();
                                break;
                            }

                            _command = _queue.Dequeue();
                        }

                        await _command?.ExecuteAsync();
                        _command = null;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Action<Exception> action = (exception) => { throw new ApplicationException("CommandEngineException", exception); };
                await App.Current?.Dispatcher.BeginInvoke(action, e);
            }
            finally
            {
                _command = null;
            }
        }

        public virtual void StartEngine()
        {
            Initialize();
        }

        public virtual void StopEngine()
        {
            Dispose();
        }
    }

}
