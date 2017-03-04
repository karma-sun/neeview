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
    // PageJob Requestフラグ
    [Flags]
    public enum PageJobOption
    {
        None = 0,
        WeakPriority = (1 << 0), // 高優先度の場合のみ上書き
    };

    /// <summary>
    /// Page Job
    /// 同じ命令に対して重複しないように吸収する
    /// </summary>
    public class PageJob
    {
        Page _page;
        PageJobCommand _command;

        JobRequest _jobRequest;

        public bool IsActive => _jobRequest != null && !_jobRequest.IsCompleted;

        /// <summary>
        /// JobCommand ラップ
        /// </summary>
        private class PageJobCommand : IJobCommand
        {
            /// <summary>
            /// 親となるPageJob
            /// </summary>
            private PageJob _pageJob;

            /// <summary>
            /// JobCommand
            /// </summary>
            private IJobCommand _command;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="pageJob"></param>
            /// <param name="command"></param>
            public PageJobCommand(PageJob pageJob, IJobCommand command)
            {
                _pageJob = pageJob;
                _command = command;
            }

            /// <summary>
            /// キャンセル命令
            /// </summary>
            public void Cancel()
            {
                _command.Cancel();
                _pageJob._jobRequest = null;
            }

            /// <summary>
            /// 実行命令
            /// </summary>
            /// <param name="completed"></param>
            /// <param name="token"></param>
            public void Execute(ManualResetEventSlim completed, CancellationToken token)
            {
                _command.Execute(completed, token);
            }
        }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="page"></param>
        /// <param name="command"></param>
        public PageJob(Page page, IJobCommand command)
        {
            _page = page;
            _command = new PageJobCommand(this, command);
        }

        /// <summary>
        /// JOB要求(async)
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="option"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task RequestAsync(QueueElementPriority priority, PageJobOption option, CancellationToken token)
        {
            await Request(priority, option).WaitAsync(token);
        }

        /// <summary>
        /// ジョブ要求
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public JobRequest Request(QueueElementPriority priority, PageJobOption option)
        {
            // ジョブ登録済の場合、優先度変更
            if (_jobRequest != null && !_jobRequest.IsCompleted) //  !_jobRequest.IsCancellationRequested)
            {
                if (!option.HasFlag(PageJobOption.WeakPriority) || priority < _jobRequest.Priority)
                {
                    _jobRequest.ChangePriority(priority);
                }
            }
            else
            {
                _jobRequest = ModelContext.JobEngine.Add(this, _command, priority);
            }

            return _jobRequest;
        }

        /// <summary>
        /// ジョブ完了待機
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task WaitAsync(CancellationToken token)
        {
            if (_jobRequest != null)
            {
                await _jobRequest.WaitAsync(token);
            }
        }

        /// <summary>
        /// ジョブキャンセル
        /// </summary>
        public void Cancel()
        {
            if (_jobRequest != null)
            {
                _jobRequest.Cancel();
                _jobRequest = null;
            }
        }
    }
}