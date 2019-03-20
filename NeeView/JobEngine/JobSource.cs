using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// JOB発行管理用単位
    /// </summary>
    public class JobSource : IDisposable
    {
        private Job _job;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public JobSource(JobCategory category, object key)
        {
            Category = category;
            Key = key;
        }

        /// <summary>
        /// JOBの種類。スケジューラの区分に使用される。
        /// </summary>
        public JobCategory Category { get; private set; }

        /// <summary>
        /// JOBに関連付けられたキー
        /// </summary>
        public object Key { get; private set; }

        /// <summary>
        /// JOB。
        /// 初回アクセス時に生成する
        /// </summary>
        public Job Job
        {
            get
            {
                if (_job== null)
                {
                    _job = Category.CreateJob(Key, _cancellationTokenSource.Token);
                }
                return _job;
            }
        }

        /// <summary>
        /// JobWorkerで処理が開始された
        /// </summary>
        public bool IsProcessed { get; set; }


        /// <summary>
        /// キャンセル
        /// </summary>
        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// キャンセルリクエスト判定
        /// </summary>
        public bool IsCancellationRequested
        {
            get { return _cancellationTokenSource.IsCancellationRequested; }
        }

        /// <summary>
        /// JOB完了まで待機
        /// </summary>
        public async Task WaitAsync(int milisecondTimeout, CancellationToken token)
        {
            await Task.Run(() => Job.WaitCompleted(milisecondTimeout, token));
        }

        public override string ToString()
        {
            return Category.ToString() + "." + Key.ToString();
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_cancellationTokenSource != null)
                    {
                        _cancellationTokenSource.Cancel();
                        _cancellationTokenSource.Dispose();
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
