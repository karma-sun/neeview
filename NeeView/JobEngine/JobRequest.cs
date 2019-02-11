using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// 登録済みジョブ情報
    /// 登録後はこのインスタンスを介して制御する
    /// </summary>
    public class JobRequest : IDisposable
    {
        private JobEngine _jobEngine;
        private Job _job;
        private CancellationTokenSource _cancellationTokenSource;
        public QueueElementPriority Priority { get; private set; }

        public int Serial => _job.SerialNumber;

        public void SetDebug() => _job.IsDebug = true;

        // コンストラクタ
        public JobRequest(JobEngine jobEngine, Job job, QueueElementPriority priority)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            _jobEngine = jobEngine;
            _job = job;
            _job.CancellationToken = _cancellationTokenSource.Token;
            Priority = priority;

            _job.Logged += (s, e) => Logged?.Invoke(s, e);
        }

        // キャンセル
        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        // キャンセルリクエスト判定
        public bool IsCancellationRequested
        {
            get { return _cancellationTokenSource.IsCancellationRequested; }
        }

        // プライオリティ変更
        public void ChangePriority(QueueElementPriority priority)
        {
            if (Priority != priority)
            {
                _jobEngine.ChangePriority(_job, priority);
                Priority = priority;
            }
        }

        /// <summary>
        /// JOB完了判定
        /// </summary>
        public bool IsCompleted
        {
            get { return _job.Completed.IsSet; }
        }

        /// <summary>
        /// JOB完了まで待機
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task WaitAsync(CancellationToken token)
        {
            await Task.Run(() => _job.Completed.Wait(token));
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_cancellationTokenSource != null)
                    {
                        _cancellationTokenSource.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        #region 開発用

        public event EventHandler<JobLogEventArgs> Logged;

        #endregion
    }
}
