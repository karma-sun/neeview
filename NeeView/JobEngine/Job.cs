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
        #region Helper

        private static int _serialNumber;

        public static Job Create(IJobCommand command, CancellationToken token)
        {
            var job = new Job(_serialNumber++, command, token);
            return job;
        }

        #endregion

        private ManualResetEventSlim _completed = new ManualResetEventSlim();

        private Job(int serialNumber, IJobCommand command, CancellationToken token)
        {
            SerialNumber = serialNumber;
            Command = command;
            CancellationToken = token;
        }

        // シリアル番号(開発用..HashCodeで代用可能か)
        public int SerialNumber { get; private set; }

        // コマンド
        public IJobCommand Command { get; private set; }

        // キャンセルトークン
        public CancellationToken CancellationToken { get; private set; }


        public void SetCompleted()
        {
            _completed.Set();
        }

        public bool WaitCompleted(int millisecondsTimeout, CancellationToken token)
        {
            using (var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, CancellationToken))
            {
                return _completed.Wait(millisecondsTimeout, linkedTokenSource.Token);
            }
        }

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

        private string _debugLog;
        public void Log(string msg)
        {
            _debugLog = _debugLog + msg + "\n";
        }

        #endregion
    }
}
