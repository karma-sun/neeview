using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using NeeLaboratory.Threading.Jobs;

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
    internal abstract class BookCommand : CancelableJobBase
    {
        public BookCommand(object sender, int priority)
        {
            _sender = sender;
            Priority = priority;
        }

        /// <summary>
        /// 送信者
        /// </summary>
        protected object _sender;

        /// <summary>
        /// コマンド優先度
        /// </summary>
        public int Priority { get; private set; }


        protected sealed override async Task ExecuteAsync(CancellationToken token)
        {
            ////Book.Log.TraceEvent(TraceEventType.Information, 0, $"{this} ...");
            await OnExecuteAsync(token);
            ////Book.Log.TraceEvent(TraceEventType.Information, 0, $"{this} done.");
        }

        protected abstract Task OnExecuteAsync(CancellationToken token);

        protected override void OnCanceled()
        {
            ////Book.Log.TraceEvent(TraceEventType.Information, 0, $"{this} canceled.");
        }

        protected override void OnException(Exception e)
        {
            ////Book.Log.TraceEvent(TraceEventType.Error, 0, $"{this} exception: {e.Message}\n{e.StackTrace}");
            ////Book.Log.Flush();
        }
    }



    /// <summary>
    /// 一般コマンド
    /// </summary>
    internal class BookCommandAction : BookCommand
    {
        private Func<object, CancellationToken, Task> _taskAction;

        public BookCommandAction(object sender, Func<object, CancellationToken, Task> taskAction, int priority) : base(sender, priority)
        {
            _taskAction = taskAction;
        }

        protected override async Task OnExecuteAsync(CancellationToken token)
        {
            await _taskAction(_sender, token);
        }
    }


    /// <summary>
    /// 重複コマンド
    /// </summary>
    internal class BookCommandJoinAction : BookCommand
    {
        private Func<object, int, CancellationToken, Task> _taskAction;
        private int _value;

        public BookCommandJoinAction(object sender, Func<object, int, CancellationToken, Task> taskAction, int value, int priority) : base(sender, priority)
        {
            _taskAction = taskAction;
            _value = value;
        }

        protected override async Task OnExecuteAsync(CancellationToken token)
        {
            await _taskAction(_sender, _value, token);
        }

        public void Join(BookCommandJoinAction other)
        {
            _value += other._value;
        }
    }



    /// <summary>
    /// Bookコマンドエンジン
    /// </summary>
    internal class BookCommandEngine : SingleJobEngine
    {
        /// <summary>
        /// コマンド登録前処理
        /// </summary>
        protected override bool OnEnqueueing(IJob command)
        {
            Debug.Assert(command is BookCommand);

            if (_queue.Count == 0) return true;

            // JoinActionコマンドはまとめる
            if (BookProfile.Current.CanMultiplePageMove())
            {
                var mc0 = command as BookCommandJoinAction;
                var mc1 = _queue.Peek() as BookCommandJoinAction;
                if (mc0 != null && mc1 != null)
                {
                    mc1.Join(mc0);
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
        /// <param name="job"></param>
        protected override void OnEnqueued(IJob job)
        {
            // 優先度の高い、最新のコマンドのみ残す
            if (_queue.Count > 1)
            {
                // 選択コマンド
                var select = _queue.Reverse().Cast<BookCommand>().OrderByDescending(e => e.Priority).First();

                // それ以外のコマンドは廃棄
                foreach (BookCommand command in _queue.Where(e => e != select))
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
            ////Book.Log.Flush();
            base.StopEngine();
        }
    }
}
