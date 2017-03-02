﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
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
        /// <param name="book"></param>
        /// <param name="priority"></param>
        public BookCommand(Book book, int priority) { _book = book; Priority = priority; }

        /// <summary>
        /// コマンド優先度
        /// </summary>
        public int Priority { get; private set; }

        /// <summary>
        /// ターゲット
        /// </summary>
        protected Book _book { get; private set; }
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

        public BookCommandDispose(Book book, BookCommandDisposeArgs param) : base(book, 4)
        {
            _param = param;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
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

        public BookCommandRemove(Book book, BookCommandRemoveArgs param) : base(book, 3)
        {
            _param = param;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
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

        public BookCommandSort(Book book, BookCommandSortArgs param) : base(book, 2)
        {
            _param = param;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
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

        public BookCommandReflesh(Book book, BookCommandRefleshArgs param) : base(book, 1)
        {
            _param = param;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
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

        public BookCommandSetPage(Book book, BookCommandSetPageArgs param) : base(book, 0)
        {
            _param = param;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            await _book.SetPage_Executed(_param, token);
        }
    }


    /// <summary>
    /// ページ相対移動コマンドパラメータ
    /// </summary>
    internal class BookCommandMovePageArgs : BookCommandArgs
    {
        public int Step { get; set; }
    }

    /// <summary>
    /// ページ相対移動コマンド
    /// </summary>
    internal class BookCommandMovePage : BookCommand
    {
        private BookCommandMovePageArgs _param;

        public BookCommandMovePage(Book book, BookCommandMovePageArgs param) : base(book, 0)
        {
            _param = param;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            await _book.MovePage_Executed(_param, token);
        }
    }


    /// <summary>
    /// Bookコマンドエンジン
    /// </summary>
    internal class BookCommandEngine : Utility.CommandEngine
    {
        protected override void OnEnqueued(Utility.ICommand cmd)
        {
            // 優先度の高い、最新のコマンドのみ残す
            if (_queue.Count > 1)
            {
                var command = _queue.Reverse().Cast<BookCommand>().OrderBy(e => e.Priority).First();
                _queue.Clear();
                _queue.Enqueue(command);
            }
        }
    }
}