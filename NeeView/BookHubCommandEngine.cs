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

namespace NeeView
{
    /// <summary>
    /// コマンド実行結果
    /// </summary>
    public enum BookHubCommandResult
    {
        None,
        Completed,
        Canceled,
    }

    /// <summary>
    /// BookHubコマンド基底
    /// </summary>
    public abstract class BookHubCommand : Utility.ICommand
    {
        // キャンセルトークン
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        // コマンド終了通知
        private ManualResetEventSlim _complete = new ManualResetEventSlim(false);

        // コマンド実行結果
        private BookHubCommandResult _result;
        public BookHubCommandResult Result
        {
            get { return _result; }
            set { _result = value; _complete.Set(); }
        }

        // コマンド実行フラグ
        private bool _isActive;


        /// <summary>
        /// キャンセル要求
        /// </summary>
        public void Cancel()
        {
            //Debug.WriteLine($"{this} cancel.");

            _cancellationTokenSource.Cancel();

            if (!_isActive)
            {
                Result = BookHubCommandResult.Canceled;
            }
        }

        /// <summary>
        /// キャンセル要求判定
        /// </summary>
        public bool IsCancellationRequested => _cancellationTokenSource.Token.IsCancellationRequested;


        /// <summary>
        /// コマンド実行
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteAsync()
        {
            _isActive = true;

            try
            {
                _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                //Debug.WriteLine($"{this} ...");
                await ExecuteAsync(_cancellationTokenSource.Token);
                Result = BookHubCommandResult.Completed;
            }
            catch (OperationCanceledException)
            {
                Result = BookHubCommandResult.Canceled;
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
    }


    /// <summary>
    /// コマンド引数基底
    /// </summary>
    public class BookHubCommandArgs
    {
        public BookHub BookHub { get; set; }
    }
    

    /// <summary>
    /// CommandLoad 引数
    /// </summary>
    public class BookHubCommandLoadArgs : BookHubCommandArgs
    {
        public string Path { get; set; }
        public string StartEntry { get; set; }
        public BookLoadOption Option { get; set; }
        public bool IsRefleshFolderList { get; set; }
    }

    /// <summary>
    /// CommandLoad
    /// </summary>
    public class BookHubCommandLoad : BookHubCommand
    {
        private BookHubCommandLoadArgs _param;

        public string Path => _param?.Path;

        public BookHubCommandLoad(BookHubCommandLoadArgs param)
        {
            _param = param;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            await _param.BookHub.LoadAsync(_param, token);
        }
    }


    /// <summary>
    /// CommandUnload引数
    /// </summary>
    public class BookHubCommandUnloadArgs : BookHubCommandArgs
    {
        public bool IsClearViewContent { get; set; }
    }

    /// <summary>
    /// CommandUnload
    /// </summary>
    public class BookHubCommandUnload : BookHubCommand
    {
        private BookHubCommandUnloadArgs _param;

        public BookHubCommandUnload(BookHubCommandUnloadArgs param)
        {
            _param = param;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            await _param.BookHub.UnloadAsync(_param); // Unloadはキャンセルできない
        }
    }

    /// <summary>
    /// BookHub用コマンドエンジン
    /// </summary>
    public class BookHubCommandEngine : Utility.CommandEngine
    {
        /// <summary>
        /// 最新の場所
        /// </summary>
        public string Place { get; private set; }

        /// <summary>
        /// コマンド登録後処理
        /// </summary>
        protected override void OnRegistered()
        {
            // 最新コマンド以外はキャンセル
            _command?.Cancel();
            while (_queue.Count > 1)
            {
                _queue.Dequeue().Cancel();
            }

            // 最新コマンドから場所を取得
            if (_queue.Any())
            {
                var command = _queue.Peek();
                if (command is BookHubCommandLoad)
                {
                    this.Place = ((BookHubCommandLoad)command).Path;
                }
                else if (command is BookHubCommandUnload)
                {
                    this.Place = null;
                }
            }
        }
    }
}
