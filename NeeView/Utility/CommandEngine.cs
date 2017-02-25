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
    /// コマンドエンジン
    /// </summary>
    public class CommandEngine
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
        public void Regist(ICommand command)
        {
            lock (_lock)
            {
                _queue.Enqueue(command);
                OnRegistered();
                _ready.Set();
            }
        }

        /// <summary>
        /// Queueの修正
        /// オーバーライドして使用する
        /// </summary>
        protected virtual void OnRegistered()
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


        /// <summary>
        /// 初期化
        /// ワーカータスク起動
        /// </summary>
        public void Initialize()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(async () => await WorkerAsync(_cancellationTokenSource.Token));
        }

        /// <summary>
        /// ワーカータスク終了
        /// </summary>
        public void Terminate()
        {
            lock (_lock)
            {
                _cancellationTokenSource.Cancel();
                _command?.Cancel();
            }
        }

        /// <summary>
        /// ワーカータスク
        /// </summary>
        /// <param name="cancellatioinTolen"></param>
        /// <returns></returns>
        private async Task WorkerAsync(CancellationToken cancellatioinTolen)
        {
            try
            {
                while (true)
                {
                    _ready.Wait(cancellatioinTolen);

                    while (true)
                    {
                        cancellatioinTolen.ThrowIfCancellationRequested();

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
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Action<Exception> action = (exception) => { throw new ApplicationException("CommandEngineException", exception); };
                await App.Current.Dispatcher.BeginInvoke(action, e);
            }
        }
    }

}
