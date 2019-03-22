using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace NeeView
{
    public enum JobState
    {
        None,
        Run,
        Closed,
    }

    public enum JobResult
    {
        None,
        Canceled,
        Completed,
    }

    /// <summary>
    /// ジョブ
    /// </summary>
    public class Job : BindableBase, IDisposable
    {
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


        private JobState _state;
        public JobState State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }

        private JobResult _result;
        public JobResult Result
        {
            get { return _result; }
            set { SetProperty(ref _result, value); }
        }




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

        #region Helper

        private static int _serialNumber;

        public static Job Create(IJobCommand command, CancellationToken token)
        {
            var job = new Job(_serialNumber++, command, token);
            return job;
        }

        #endregion

        #region 開発用

        private List<string> _debugLogs;

        public string LastLog => _debugLogs?.Last();

        public string AllLog => _debugLogs != null ? string.Join("\n", _debugLogs) : null;

        [Conditional("DEBUG")]
        public void Log(string msg)
        {
            _debugLogs = _debugLogs ?? new List<string>();
            _debugLogs.Add(msg);

            RaisePropertyChanged(nameof(LastLog));
            RaisePropertyChanged(nameof(AllLog));
        }

        #endregion
    }
}
