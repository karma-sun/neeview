using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

// TODO: Jobの状態パラメータ(Status?)

namespace NeeView
{
    #region 開発用
    public class LogEventArgs : EventArgs
    {
        public string Log { get; set; }
        public LogEventArgs(string log) => Log = log;
    }
    #endregion

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
        public event EventHandler<LogEventArgs> Logged;

        //
        public void Log(string msg)
        {
            Logged?.Invoke(this, new LogEventArgs(msg));
            if (IsDebug) Debug.WriteLine(msg);
        }

        #endregion
    }

    /// <summary>
    /// JobCommand
    /// </summary>
    public interface IJobCommand
    {
        // メイン処理
        Task ExecuteAsync(ManualResetEventSlim completed, CancellationToken token);
    }


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

        public event EventHandler<LogEventArgs> Logged;

        #endregion
    }


    /// <summary>
    /// Job環境
    /// Jobワーカータスクで共通のコンテキスト
    /// </summary>
    public class JobContext
    {
        // ジョブリスト
        public PriorityQueue<Job> JobQueue { get; private set; }

        // 排他処理用ロック
        public object Lock { get; private set; }

        // ジョブキュー変更通知
        public event EventHandler JobChanged;

        // コンストラクト
        public JobContext()
        {
            JobQueue = new PriorityQueue<Job>();
            Lock = new object();
        }

        // ジョブキュー変更通知
        public void RaiseJobChanged()
        {
            JobChanged?.Invoke(this, null);
        }
    }

    /// <summary>
    /// JobEngine
    /// </summary>
    public class JobEngine : BindableBase, IEngine
    {
        public static JobEngine Current { get; private set; }

        #region 開発用


        [Conditional("DEBUG")]
        private void NotifyStatusChanged()
        {
            RaisePropertyChanged(null);
        }

        // 状態メッセージ
        #region Property: Message
        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                //Debug.WriteLine($"JobEngine: {value}");
                _message = value;
                NotifyStatusChanged();
            }
        }
        #endregion

        #endregion


        /// <summary>
        /// IsBusy property.
        /// </summary>
        public bool IsBusy
        {
            get { return _IsBusy; }
            set { if (_IsBusy != value) { _IsBusy = value; RaisePropertyChanged(); } }
        }

        private bool _IsBusy;

        private void UpdateIsBusy()
        {
            this.IsBusy = this.Workers.Any(e => e != null && e.IsBusy);
        }


        // ジョブの製造番号用カウンタ
        private int _serialNumber;

        // コンテキスト
        private JobContext _context;

        // 最大ワーカー数
        public readonly int _MaxWorkerSize = 4;


        /// <summary>
        /// WorkerSize property.
        /// </summary>
        [PropertyMember("@ParamJobEngineWorkerSize", Tips = "@ParamJobEngineWorkerSizeTips")]
        public int WorkerSize
        {
            get { return _workerSize; }
            set
            {
                var size = MathUtility.Clamp(value, 1, _MaxWorkerSize);
                if (_workerSize != size)
                {
                    _workerSize = size;
                    if (_isActive) ChangeWorkerSize(_workerSize);
                    RaisePropertyChanged();
                }
            }
        }

        private int _workerSize = 2;

        // ワーカー
        public JobWorker[] Workers { get; set; }




        // コンストラクタ
        public JobEngine()
        {
            Current = this;

            _context = new JobContext();
            Workers = new JobWorker[_MaxWorkerSize];
        }

        private bool _isActive;

        // 開始
        public void StartEngine()
        {
            if (_isActive) return;

            _isActive = true;
            ChangeWorkerSize(_workerSize);
        }


        // 廃棄処理
        public void StopEngine()
        {
            ChangeWorkerSize(0);
            _isActive = false;
            Debug.WriteLine("JobEngine: Disposed.");
        }

        // 稼働ワーカー数変更
        public void ChangeWorkerSize(int size)
        {
            Debug.Assert(0 <= size && size <= _MaxWorkerSize);
            Debug.WriteLine("JobWorker: " + size);

            var primaryCount = (size > 2) ? 2 : size - 1;

            for (int i = 0; i < _MaxWorkerSize; ++i)
            {
                if (i < size)
                {
                    if (Workers[i] == null)
                    {
                        Workers[i] = new JobWorker(_context);
                        Workers[i].StatusChanged += (s, e) => NotifyStatusChanged(); //// StatusChanged?.Invoke(s, e);
                        Workers[i].IsBusyChanged += (s, e) => UpdateIsBusy(); ////  IsBusyChanged?.Invoke(s, e);
                        Workers[i].Run();
                        Message = $"Create Worker[{i}]";
                    }

                    // 現在のフォルダージョブのみ処理する設定
                    Workers[i].IsPrimary = i < primaryCount;
                }
                else
                {
                    if (Workers[i] != null)
                    {
                        Workers[i].Cancel();
                        Workers[i].Dispose();
                        Workers[i] = null;
                        Message = $"Delete Worker[{i}]";
                    }
                }
            }

            // イベント待ち解除
            _context.RaiseJobChanged();

            NotifyStatusChanged();
        }

        /// <summary>
        /// Jobクリア
        /// </summary>
        /// <param name="priority">クリアする優先度</param>
        public void Clear(QueueElementPriority priority)
        {
            lock (_context.Lock)
            {
                while (_context.JobQueue.CountAt(priority) > 0)
                {
                    var job = _context.JobQueue.Dequeue(priority);
                    job.Completed.Set(); // 終了
                }
            }
        }

        /// <summary>
        /// JOBクリア(未使用)
        /// </summary>
        /// <param name="priority">クリアする優先度</param>
        /// <param name="keyCode">識別子</param>
        public void Clear(QueueElementPriority priority, string keyCode)
        {
            lock (_context.Lock)
            {
                var jobs = _context.JobQueue.Where(e => e.KeyCode == keyCode, priority).ToList();
                foreach (var job in jobs)
                {
                    _context.JobQueue.Remove(priority, job);
                    job.Completed.Set(); // 終了
                }
            }
        }

        /// <summary>
        /// Job登録
        /// </summary>
        /// <param name="command">処理</param>
        /// <param name="priority">優先度</param>
        /// <param name="keyCode">識別子</param>
        /// <returns>JobRequest</returns>
        public JobRequest Add(object sender, IJobCommand command, QueueElementPriority priority, string keyCode, bool reverse = false)
        {
            var job = new Job();
            job.SerialNumber = _serialNumber++;
            job.KeyCode = keyCode;
            job.Sender = sender;
            job.Command = command;

            var request = new JobRequest(this, job, priority);

            lock (_context.Lock)
            {
                _context.JobQueue.Enqueue(job, priority, reverse);
                _context.RaiseJobChanged();
                Message = $"Add Job. {job.SerialNumber}";
            }

            NotifyStatusChanged();

            return request;
        }

        // 優先度変更
        public void ChangePriority(Job job, QueueElementPriority priority)
        {
            lock (_context.Lock)
            {
                _context.JobQueue.ChangePriority(job, priority);
            }
        }

        // 待機ジョブ数
        public int JobCount
        {
            get { return _context.JobQueue.Count; }
        }

        // 開発用遅延
        [Conditional("DEBUG")]
        private void __Delay(int ms)
        {
            Thread.Sleep(ms);
        }


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember, DefaultValue(2)]
            public int WorkerSize { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.WorkerSize = this.WorkerSize;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.WorkerSize = memento.WorkerSize;
        }
        #endregion


    }


    /// <summary>
    /// ジョブワーカー
    /// </summary>
    public class JobWorker : IDisposable
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
        #region Property: Message
        private string _message;
        public string Message
        {
            get { return _message; }
            set { _message = value; NotifyStatusChanged(); }
        }
        #endregion

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
#if true
            // Task版
            var task = Task.Run(async () =>
            {
                try
                {
                    await WorkerExecute();
                }
                catch (OperationCanceledException)
                {
                    Debug.WriteLine($"JOB TASK CANCELED.");
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"JOB EXCEPTION: {e.Message}");
                    Message = e.Message;
                    Action<Exception> action = (exception) => { throw new ApplicationException("タスク内部エラー", exception); };
                    App.Current?.Dispatcher.BeginInvoke(action, e);
                }
            },
            _cancellationTokenSource.Token);
#else
            // sample: Thread版
            Thread t1;
            t1 = new Thread(new ThreadStart(WorkerExecute));
            t1.Priority = ThreadPriority.Normal;
            t1.IsBackground = true;
            t1.Start();
#endif
        }


        // ワーカータスク廃棄
        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }


        // ワーカータスクメイン
        private async Task WorkerExecute()
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
                    await Task.Run(() => _event.Wait(_cancellationTokenSource.Token));
                    continue;
                }

                IsBusy = true;

                job.Log($"{job.SerialNumber}: Job...");

                if (!job.CancellationToken.IsCancellationRequested)
                {
                    Message = $"Job({job.SerialNumber}) execute ...";
                    try
                    {
                        await job.Command.ExecuteAsync(job.Completed, job.CancellationToken);
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
