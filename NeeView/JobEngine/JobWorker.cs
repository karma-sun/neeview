﻿using NeeLaboratory.ComponentModel;
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

        public DebugSimpleLog DebugLog { get; private set; }

        [Conditional("DEBUG")]
        public void Log(string message)
        {
            DebugLog = DebugLog ?? new DebugSimpleLog();
            DebugLog.WriteLine(Thread.CurrentThread.Priority + ": " + $"{_jobPriorityMin}-{_jobPriorityMax}: " + message);
            RaisePropertyChanged(nameof(DebugLog));
        }

        #endregion

        // スケジューラー
        private JobScheduler _scheduler;

        // ワーカータスクのキャンセルトークン
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        // ジョブ待ちフラグ
        private ManualResetEventSlim _event = new ManualResetEventSlim(false);

        // ワーカースレッド
        private Thread _thread;

        private bool _isBusy;
        private bool _isPrimary;
        private bool _isLimited;
        private int _jobPriorityMin;
        private int _jobPriorityMax;


        // コンストラクタ
        public JobWorker(JobScheduler scheduler)
        {
            _scheduler = scheduler;
            _scheduler.QueueChanged += Context_JobChanged;

            UpdateJobPriorityRange();
        }


        public event EventHandler IsBusyChanged;


        /// <summary>
        /// IsBusy property.
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }
            set { if (_isBusy != value) { _isBusy = value; IsBusyChanged?.Invoke(this, null); } }
        }

        /// <summary>
        /// 優先ワーカー.
        /// 現在開いているフォルダーに対してのジョブのみ処理する
        /// </summary>
        public bool IsPrimary
        {
            get { return _isPrimary; }
            set
            {
                if (SetProperty(ref _isPrimary, value))
                {
                    UpdateThreadPriority();
                    UpdateJobPriorityRange();
                }
            }
        }

        /// <summary>
        /// JobWorkerの総数が少ない状態
        /// </summary>
        public bool IsLimited
        {
            get { return _isLimited; }
            set
            {
                if (SetProperty(ref _isLimited, value))
                {
                    UpdateJobPriorityRange();
                }
            }
        }


        private void Context_JobChanged(object sender, EventArgs e)
        {
            _event.Set();
        }

        private void UpdateJobPriorityRange()
        {
            if (IsPrimary)
            {
                _jobPriorityMin = 10;
                _jobPriorityMax = 99;
            }
            else
            {
                _jobPriorityMin = 0;
                _jobPriorityMax = IsLimited ? 99 : 9;
            }
        }

        private void UpdateThreadPriority()
        {
            if (_thread != null)
            {
                _thread.Priority = IsPrimary ? ThreadPriority.Normal : ThreadPriority.BelowNormal;
            }
        }

        // ワーカータスク開始
        public void Run()
        {
            Log($"Run");

            _thread = new Thread(() => WorkerExecuteAsync(_cancellationTokenSource.Token));
            _thread.IsBackground = true;
            _thread.Name = "JobWorker";
            _thread.Start();

            UpdateThreadPriority();
        }

        // ワーカータスク廃棄
        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        // ワーカータスクメイン
        private void WorkerExecuteAsync(CancellationToken token)
        {
            try
            {
                WorkerExecuteAsyncCore(token);
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine($"JOB TASK CANCELED.");
            }
            catch (ObjectDisposedException)
            {
                Debug.WriteLine($"JOB TASK DISPOSED.");
            }
        }


        // ワーカータスクメイン
        private void WorkerExecuteAsyncCore(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Log($"get Job ...");
                Job job;

                lock (_scheduler.Lock)
                {
                    ThrowIfDisposed();

                    // ジョブ取り出し
                    job = _scheduler.FetchNextJob(_jobPriorityMin, _jobPriorityMax);

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
                    Log($"wait event ...");
                    _event.Wait(token);
                    continue;
                }

                IsBusy = true;

                if (!job.CancellationToken.IsCancellationRequested)
                {
                    job.Log("Run...");
                    job.State = JobState.Run;
                    Log($"Job({job.SerialNumber}) execute ...");
                    try
                    {
                        job.Command.Execute(job.CancellationToken);
                        job.Log($"Done: Cancel={job.CancellationToken.IsCancellationRequested}");
                    }
                    catch (AggregateException ex)
                    {
                        foreach (var iex in ex.InnerExceptions)
                        {
                            job.Log($"Exception: {iex.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        job.Log($"Exception: {ex.Message}");
                    }
                    Log($"Job({job.SerialNumber}) execute done. : {job.CancellationToken.IsCancellationRequested}");
                }
                else
                {
                    job.Log($"Already canceled.");
                }

                // JOB完了
                job.Result = job.CancellationToken.IsCancellationRequested ? JobResult.Canceled : JobResult.Completed;
                job.State = JobState.Closed;
                job.SetCompleted();
            }

            Debug.WriteLine("Task: Exit.");
        }

        #region IDisposable Support
        private int _disposedValue;

        private void ThrowIfDisposed()
        {
            if (_disposedValue != 0) throw new ObjectDisposedException(nameof(JobWorker));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Interlocked.Exchange(ref _disposedValue, 1) == 0)
            {
                if (disposing)
                {
                    if (_scheduler != null)
                    {
                        _scheduler.QueueChanged -= Context_JobChanged;
                    }

                    if (_cancellationTokenSource != null)
                    {
                        _cancellationTokenSource.Cancel();
                        _cancellationTokenSource.Dispose();
                    }

                    if (_event != null)
                    {
                        _event.Dispose();
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

    }
}
