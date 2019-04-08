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
    /// BookHubコマンド引数基底
    /// </summary>
    public class BookHubCommandArgs
    {
    }

    /// <summary>
    /// BookHubコマンド基底
    /// </summary>
    public abstract class BookHubCommand : CancelableJobBase
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
        public bool IsRefreshFolderList { get; set; }
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
            this.CanBeCanceled = false;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            _bookHub.Unload(_param);

            // ブックを閉じたときの移動履歴を表示するためにnullを履歴に登録
            BookHubHistory.Current.Add(null);

            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// BookHub用コマンドエンジン
    /// </summary>
    public class BookHubCommandEngine : SingleJobEngine
    {
        /// <summary>
        /// コマンド登録前処理
        /// </summary>
        /// <param name="job"></param>
        protected override bool OnEnqueueing(IJob job)
        {
            Debug.Assert(job is BookHubCommand);

            // 全コマンドキャンセル
            // ※ Unloadはキャンセルできないので残る
            foreach (BookHubCommand e in AllJobs())
            {
                e.Cancel();
            }

            return true;
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach (BookHubCommand e in AllJobs())
                    {
                        e.Cancel();
                    }
                }

                _disposedValue = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
