using System;
using System.Diagnostics;
using System.Threading;

namespace NeeView
{
    /// <summary>
    /// ジョブ
    /// </summary>
    public class Job : IDisposable
    {
        // シリアル番号(開発用)
        public int SerialNumber { get; set; }

        // 識別コード. 一括削除に使用される(未使用)
        public string KeyCode { get; set; }

        // 発行者
        public object Sender { get; set; }

        // 完了フラグ
        private ManualResetEventSlim _completed = new ManualResetEventSlim();
        public ManualResetEventSlim Completed => _completed;

        // キャンセルトークン
        public CancellationToken CancellationToken { get; set; }

        // コマンド
        public IJobCommand Command { get; set; }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _completed.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        #region 開発用

        //
        public bool IsDebug { get; set; }

        //
        public event EventHandler<JobLogEventArgs> Logged;

        //
        public void Log(string msg)
        {
            Logged?.Invoke(this, new JobLogEventArgs(msg));
            if (IsDebug) Debug.WriteLine(msg);
        }

        #endregion
    }
}
