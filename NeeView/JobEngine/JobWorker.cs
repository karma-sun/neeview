using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics;
using System.Threading;

namespace NeeView
{
    /// <summary>
    /// ジョブワーカー
    /// </summary>
    public class JobWorker : BindableBase, IDisposable
    {
        #region 開発用

        // 状態変化通知
        public event EventHandler StatusChanged;

        [Conditional("DEBUG")]
        public void NotifyStatusChanged()
        {
            StatusChanged?.Invoke(this, null);
        }

        // 状態メッセージ
        private string _message;
        public string Message
        {
            get { return _message; }
            set { if (SetProperty(ref _message, value)) NotifyStatusChanged(); }
        }

        #endregion

        public event EventHandler IsBusyChanged;

        /// <summary>
        /// IsBusy property.
        /// </summary>
        private bool _IsBusy;
        public bool IsBusy
        {
            get { return _IsBusy; }
            set { if (_IsBusy != value) { _IsBusy = value; IsBusyChanged?.Invoke(this, null); } }
        }


        // コンテキスト
        private JobContext _context;

        // ワーカータスクのキャンセルトークン
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        // ジョブ待ちフラグ
        private ManualResetEventSlim _event = new ManualResetEventSlim(false);


        /// <summary>
        /// 優先ワーカー.
        /// 現在開いているフォルダーに対してのジョブのみ処理する
        /// </summary>
        public bool IsPrimary { get; set; }


        // コンストラクタ
        public JobWorker(JobContext context)
        {
            _context = context;
            _context.JobChanged += Context_JobChanged;
        }

        //
        private void Context_JobChanged(object sender, EventArgs e)
        {
            _event.Set();
        }


        // ワーカータスク開始
        public void Run()
        {
            Message = $"Run";

            // Thread版
            var thread = new Thread(new ThreadStart(() => WorkerExecuteAsync()));
            thread.Priority = IsPrimary ? ThreadPriority.Normal : ThreadPriority.BelowNormal;
            thread.IsBackground = true;
            thread.Name = "JobWorker";
            thread.Start();
            // Task版
            //// var task = Task.Run(() => WorkerExecuteAsync(), _cancellationTokenSource.Token);
        }

        // ワーカータスク廃棄
        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        // ワーカータスクメイン
        private void WorkerExecuteAsync()
        {
            try
            {
                WorkerExecuteAsyncCore();
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine($"JOB TASK CANCELED.");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"JOB EXCEPTION: {e.Message}");
                Message = e.Message;
                AppDispatcher.BeginInvoke(() => throw new ApplicationException("JobEngine internal exception", e));
            }
        }

        // ワーカータスクメイン
        private void WorkerExecuteAsyncCore()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                Message = $"get Job ...";
                Job job;

                lock (_context.Lock)
                {
                    // ジョブ取り出し
                    job = IsPrimary ? _context.JobQueue.DequeueAll(QueueElementPriority.Default) : _context.JobQueue.DequeueAll();

                    // ジョブが無い場合はイベントリセット
                    if (job == null)
                    {
                        _event.Reset();
                    }
                }

                // イベント待ち
                if (job == null)
                {
                    IsBusy = false;
                    Message = $"wait event ...";
                    _event.Wait(_cancellationTokenSource.Token);
                    continue;
                }

                IsBusy = true;

                job.Log($"{job.SerialNumber}: Job...");

                if (!job.CancellationToken.IsCancellationRequested)
                {
                    Message = $"Job({job.SerialNumber}) execute ...";
                    try
                    {
                        job.Command.ExecuteAsync(job.Completed, job.CancellationToken).Wait();
                        job.Log($"{job.SerialNumber}: Job done.");
                    }
                    catch (OperationCanceledException)
                    {
                        job.Log($"{job.SerialNumber}: Job canceled");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"EXCEPTION!!: {ex.Message}");
                    }
                    Message = $"Job({job.SerialNumber}) execute done.";
                }
                else
                {
                    job.Log($"{job.SerialNumber}: Job canceled");
                }


                // JOB完了
                job.Completed.Set();
            }

            Debug.WriteLine("Task: Exit.");
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_context != null)
                    {
                        _context.JobChanged -= Context_JobChanged;
                    }

                    if (_cancellationTokenSource != null)
                    {
                        _cancellationTokenSource.Dispose();
                    }

                    if (_event != null)
                    {
                        _event.Dispose();
                    }
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

    }
}
