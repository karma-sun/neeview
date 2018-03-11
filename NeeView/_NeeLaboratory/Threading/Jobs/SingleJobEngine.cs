using NeeLaboratory.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.Threading.Jobs
{
    /// <summary>
    /// SingleJobエンジン
    /// </summary>
    public class SingleJobEngine : IEngine, IDisposable
    {
        #region Fields

        /// <summary>
        /// ワーカータスクのキャンセルトークン
        /// </summary>
        private CancellationTokenSource _engineCancellationTokenSource;

        /// <summary>
        /// 予約Job存在通知
        /// </summary>
        private ManualResetEventSlim _readyQueue = new ManualResetEventSlim(false);

        /// <summary>
        /// 予約Jobリスト
        /// </summary>
        protected Queue<IJob> _queue = new Queue<IJob>();

        /// <summary>
        /// 実行中Job
        /// </summary>
        protected volatile IJob _currentJob;

        /// <summary>
        /// エンジン動作中
        /// </summary>
        private bool _isEngineActive;

        /// <summary>
        /// 排他処理用オブジェクト
        /// </summary>
        private object _lock = new object();

        /// <summary>
        /// 開発用：ログ
        /// </summary>
        private Log _log;

        #endregion

        #region Constructors

        public SingleJobEngine()
        {
        }

        #endregion

        #region Events

        /// <summary>
        /// JOBエラー発生時のイベント
        /// </summary>
        public event EventHandler<JobErrorEventArgs> JobError;

        /// <summary>
        /// 例外によってJobEngineが停止した時に発生するイベント
        /// </summary>
        public event EventHandler<ErrorEventArgs> JobEngineError;

        #endregion

        #region Properties

        /// <summary>
        /// 現在のJob数
        /// </summary>
        public int Count
        {
            get { return _queue.Count + (_currentJob != null ? 1 : 0); }
        }

        /// <summary>
        /// 開発用：ログ
        /// </summary>
        public Log Log
        {
            get { return _log; }
            set { _log = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// 全てのJobを走査
        /// </summary>
        public IEnumerable<IJob> AllJobs()
        {
            if (_currentJob != null)
            {
                yield return _currentJob;
            }
            foreach (var job in _queue)
            {
                yield return job;
            }
        }

        /// <summary>
        /// Job登録
        /// </summary>
        public virtual void Enqueue(IJob job)
        {
            if (!_isEngineActive)
            {
                _log?.Trace(TraceEventType.Warning, $"enqueue when engine not actived,");
            }

            lock (_lock)
            {
                if (OnEnqueueing(job))
                {
                    _log?.Trace($"Job entry: {job}");
                    _queue.Enqueue(job);
                    OnEnqueued(job);
                    _readyQueue.Set();
                }
                else
                {
                    _log?.Trace($"Job entry canceled: {job}");
                }
            }
        }

        /// <summary>
        /// JOBで発生した例外の処理
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="job"></param>
        private void HandleJobException(Exception exception, IJob job)
        {
            var args = new JobErrorEventArgs(exception, job);
            JobError?.Invoke(this, args);
            if (!args.Handled)
            {
                throw new JobException($"Job Exception: {job}", exception, job);
            }
        }

        /// <summary>
        /// Queue登録前の処理
        /// </summary>
        /// <param name="job"></param>
        /// <returns>falseの場合、登録しない</returns>
        protected virtual bool OnEnqueueing(IJob job)
        {
            return true;
        }

        /// <summary>
        /// Queue登録後の処理
        /// </summary>
        /// <param name="job"></param>
        protected virtual void OnEnqueued(IJob job)
        {
        }

        /// <summary>
        /// ワーカータスク
        /// </summary>
        private async Task WorkerAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    _readyQueue.Wait(token);

                    while (!token.IsCancellationRequested)
                    {
                        lock (_lock)
                        {
                            if (_queue.Count <= 0)
                            {
                                _currentJob = null;
                                _readyQueue.Reset();
                                break;
                            }
                            _currentJob = _queue.Dequeue();
                        }

                        try
                        {
                            _log?.Trace($"Job execute: {_currentJob}");
                            await _currentJob?.ExecuteAsync();
                        }
                        catch (OperationCanceledException)
                        {
                            _log?.Trace(TraceEventType.Information, $"Job canceled: {_currentJob}");
                        }
                        catch (Exception ex)
                        {
                            _log?.Trace(TraceEventType.Error, $"Job excepted: {_currentJob}");
                            HandleJobException(ex, _currentJob);
                        }

                        _currentJob = null;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _currentJob = null;
            }
        }

        #endregion

        #region IEngine Support

        /// <summary>
        /// エンジン始動
        /// </summary>
        public virtual void StartEngine()
        {
            if (_isEngineActive) return;

            _log?.Trace($"start...");
            _isEngineActive = true;
            _engineCancellationTokenSource = new CancellationTokenSource();

            Task.Run(async () =>
            {
                try
                {
                    await WorkerAsync(_engineCancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    _log?.Trace(TraceEventType.Critical, $"excepted: {ex.Message}");
                    JobEngineError?.Invoke(this, new ErrorEventArgs(ex));
                }
                finally
                {
                    _isEngineActive = false;
                    _log?.Trace($"stopped.");
                }
            });
        }

        /// <summary>
        /// エンジン停止
        /// </summary>
        public virtual void StopEngine()
        {
            _engineCancellationTokenSource?.Cancel();
        }

        #endregion

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    StopEngine();
                    _log?.Dispose();
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
