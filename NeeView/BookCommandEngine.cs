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
using NeeView.Utility;
using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// Bookコマンドパラメータ基底
    /// </summary>
    internal class BookCommandArgs
    {
    }

    /// <summary>
    /// Bookコマンド基底
    /// </summary>
    internal abstract class BookCommand : Utility.CommandBase
    {
        /// <summary>
        /// construcotr
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="book"></param>
        /// <param name="priority"></param>
        public BookCommand(object sender, Book book, int priority) { _sender = sender; _book = book; Priority = priority; }

        /// <summary>
        /// 送信者
        /// </summary>
        protected object _sender;

        /// <summary>
        /// コマンド優先度
        /// </summary>
        public int Priority { get; private set; }

        /// <summary>
        /// ターゲット
        /// </summary>
        protected Book _book;

        //
        protected sealed override async Task ExecuteAsync(CancellationToken token)
        {
            Book.Log.TraceEvent(TraceEventType.Information, _book.Serial, $"{this} ...");
            await OnExecuteAsync(token);
            Book.Log.TraceEvent(TraceEventType.Information, _book.Serial, $"{this} done.");
        }

        //
        protected abstract Task OnExecuteAsync(CancellationToken token);

        //
        protected override void OnCanceled()
        {
            Book.Log.TraceEvent(TraceEventType.Information, _book.Serial, $"{this} canceled.");
        }

        //
        protected override void OnException(Exception e)
        {
            Book.Log.TraceEvent(TraceEventType.Error, _book.Serial, $"{this} exception: {e.Message}\n{e.StackTrace}");
            Book.Log.Flush();
        }
    }


    /// <summary>
    /// 廃棄処理コマンドパラメータ
    /// </summary>
    internal class BookCommandDisposeArgs : BookCommandArgs
    {
    }

    /// <summary>
    /// 廃棄処理コマンド
    /// </summary>
    internal class BookCommandDispose : BookCommand
    {
        private BookCommandDisposeArgs _param;

        public BookCommandDispose(object sender, Book book, BookCommandDisposeArgs param) : base(sender, book, 4)
        {
            _param = param;
        }

        protected override async Task OnExecuteAsync(CancellationToken token)
        {
            await _book.Dispose_Executed(_param, token);
        }
    }


    /// <summary>
    /// 削除コマンドパラメータ
    /// </summary>
    internal class BookCommandRemoveArgs : BookCommandArgs
    {
        public Page Page { get; set; }
    }

    /// <summary>
    /// 削除コマンド
    /// </summary>
    internal class BookCommandRemove : BookCommand
    {
        private BookCommandRemoveArgs _param;

        public BookCommandRemove(object sender, Book book, BookCommandRemoveArgs param) : base(sender, book, 3)
        {
            _param = param;
        }

        protected override async Task OnExecuteAsync(CancellationToken token)
        {
            await _book.Remove_Executed(_param, token);
        }
    }


    /// <summary>
    /// ソートコマンドパラメータ
    /// </summary>
    internal class BookCommandSortArgs : BookCommandArgs
    {
    }

    /// <summary>
    /// ソートコマンド
    /// </summary>
    internal class BookCommandSort : BookCommand
    {
        private BookCommandSortArgs _param;

        public BookCommandSort(object sender, Book book, BookCommandSortArgs param) : base(sender, book, 2)
        {
            _param = param;
        }

        protected override async Task OnExecuteAsync(CancellationToken token)
        {
            await _book.Sort_Executed(_param, token);
        }
    }



    /// <summary>
    /// リフレッシュコマンドパラメータ
    /// </summary>
    internal class BookCommandRefleshArgs : BookCommandArgs
    {
        public bool IsClear { get; set; }
    }

    /// <summary>
    /// リフレッシュコマンド
    /// </summary>
    internal class BookCommandReflesh : BookCommand
    {
        private BookCommandRefleshArgs _param;

        public BookCommandReflesh(object sender, Book book, BookCommandRefleshArgs param) : base(sender, book, 1)
        {
            _param = param;
        }

        protected override async Task OnExecuteAsync(CancellationToken token)
        {
            await _book.Reflesh_Executed(_param, token);
        }
    }


    /// <summary>
    /// ページ指定移動コマンドパラメータ
    /// </summary>
    internal class BookCommandSetPageArgs : BookCommandArgs
    {
        public PagePosition Position { get; set; }
        public int Direction { get; set; }
        public int Size { get; set; }
        public bool IsPreLoad { get; set; }
    }

    /// <summary>
    /// ページ指定移動コマンド
    /// </summary>
    internal class BookCommandSetPage : BookCommand
    {
        private BookCommandSetPageArgs _param;

        public BookCommandSetPage(object sender, Book book, BookCommandSetPageArgs param) : base(sender, book, 0)
        {
            _param = param;
        }

        protected override async Task OnExecuteAsync(CancellationToken token)
        {
            await _book.SetPage_Executed(_sender, _param, token);
        }
    }


    /// <summary>
    /// ページ相対移動コマンドパラメータ
    /// </summary>
    internal class BookCommandMovePageArgs : BookCommandArgs
    {
        /// <summary>
        /// Step property.
        /// </summary>
        private volatile int _step;
        public int Step
        {
            get { return _step; }
            set { if (_step != value) { _step = value; } }
        }
    }

    /// <summary>
    /// ページ相対移動コマンド
    /// </summary>
    internal class BookCommandMovePage : BookCommand
    {
        private BookCommandMovePageArgs _param;

        public BookCommandMovePage(object sender, Book book, BookCommandMovePageArgs param) : base(sender, book, 0)
        {
            _param = param;
        }

        protected override async Task OnExecuteAsync(CancellationToken token)
        {
            await _book.MovePage_Executed(_param, token);
        }

        public void Add(BookCommandMovePage a)
        {
            _param.Step += a._param.Step;
        }
    }


    /// <summary>
    /// Bookコマンドエンジン
    /// </summary>
    internal class BookCommandEngine : Utility.CommandEngine
    {
        /// <summary>
        /// Book挙動設定
        /// </summary>
        public BookEnvironment BookEnvironment { get; set; }

        /// <summary>
        /// コマンド登録前処理
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        protected override bool OnEnqueueing(ICommand command)
        {
            if (_queue.Count == 0) return true;

            // ページ移動コマンドはまとめる
            if (BookEnvironment.AllowMultiplePageMove)
            {
                var mc0 = command as BookCommandMovePage;
                var mc1 = _queue.Peek() as BookCommandMovePage;
                if (mc0 != null && mc1 != null)
                {
                    mc1.Add(mc0);
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return true;
        }

        /// <summary>
        /// コマンド登録後処理
        /// </summary>
        /// <param name="cmd"></param>
        protected override void OnEnqueued(Utility.ICommand cmd)
        {
            // 優先度の高い、最新のコマンドのみ残す
            if (_queue.Count > 1)
            {
                // 選択コマンド
                var select = _queue.Reverse().Cast<BookCommand>().OrderByDescending(e => e.Priority).First();

                // それ以外のコマンドは廃棄
                foreach(var command in _queue.Where(e => e != select))
                {
                    command.Cancel();
                }

                // 新しいコマンド列
                _queue.Clear();
                _queue.Enqueue(select);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void StopEngine()
        {
            Book.Log.Flush();
            base.StopEngine();
        }
    }
}
