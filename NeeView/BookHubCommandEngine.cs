// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NeeView.Utility;
using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// BookHubコマンド引数基底
    /// </summary>
    public class BookHubCommandArgs
    {
    }
    
    /// <summary>
    /// BookHubコマンド基底
    /// </summary>
    public abstract class BookHubCommand : Utility.CommandBase
    {
        /// <summary>
        /// construcotr
        /// </summary>
        /// <param name="bookHub"></param>
        public BookHubCommand(BookHub bookHub) { _bookHub = bookHub; }

        /// <summary>
        /// ターゲット
        /// </summary>
        protected BookHub _bookHub { get; private set; }
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

        public BookHubCommandLoad(BookHub bookHub, BookHubCommandLoadArgs param) : base(bookHub)
        {
            _param = param;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            await _bookHub.LoadAsync(_param, token);
        }
    }


    /// <summary>
    /// CommandUnload引数
    /// </summary>
    public class BookHubCommandUnloadArgs : BookHubCommandArgs
    {
        public bool IsClearViewContent { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// CommandUnload
    /// </summary>
    public class BookHubCommandUnload : BookHubCommand
    {
        private BookHubCommandUnloadArgs _param;

        public BookHubCommandUnload(BookHub bookHub, BookHubCommandUnloadArgs param) : base(bookHub)
        {
            _param = param;

            // キャンセル不可
            CanBeCanceled = false;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            await _bookHub.UnloadAsync(_param);
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
        /// コマンド登録前処理
        /// </summary>
        /// <param name="command"></param>
        protected override bool OnEnqueueing(ICommand command)
        {
            // 現在コマンドはキャンセル
            _command?.Cancel();

            // 全コマンドキャンセル
            // ※ Unloadはキャンセルできないので残る
            foreach(var cmd in _queue)
            {
                cmd.Cancel();
            }

            return true;
        }

        /// <summary>
        /// コマンド登録後処理
        /// </summary>
        protected override void OnEnqueued(Utility.ICommand cmd)
        {
            // 最新コマンドから場所を取得 .. 使ってない？
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
