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
        /// ワーカースレッド
        /// </summary>
        private Thread _thread;

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
        public event EventHandler<JobErrorEventArgs> JobEngineError;

        #endregion

        #region Properties

        /// <summary>
        /// 名前
        /// </summary>
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    if (_thread != null)
                    {
                        _thread.Name = _name;
                    }
                }
            }
        }


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
        private void WorkerAsync(CancellationToken token)
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
                            _currentJob?.ExecuteAsync().Wait();
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

            _thread = new Thread(() =>
            {
                try
                {
                    WorkerAsync(_engineCancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    _log?.Trace(TraceEventType.Critical, $"excepted: {ex.Message}");
                    var args = new JobErrorEventArgs(ex, null);
                    JobEngineError?.Invoke(this, args);
                    if (!args.Handled)
                    {
                        throw;
                    }
                }
                finally
                {
                    _isEngineActive = false;
                    _log?.Trace($"stopped.");

                    Debug.WriteLine($"{this}: worker thread terminated.");
                }
            });

            _thread.IsBackground = true;
            _thread.Name = _name;
            _thread.Start();
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

                    if (_log != null)
                    {
                        _log.Dispose();
                    }

                    if (_engineCancellationTokenSource != null)
                    {
                        _engineCancellationTokenSource.Dispose();
                    }

                    if (_readyQueue != null)
                    {
                        _readyQueue.Dispose();
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
