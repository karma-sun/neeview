// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// ジョブ
    /// </summary>
    public class Job
    {
        // シリアル番号(開発用)
        public int SerialNumber { get; set; }

        // 発行者
        public object Sender { get; set; }

        // 完了フラグ
        public ManualResetEventSlim Completed { get; set; } = new ManualResetEventSlim();

        // キャンセルトークン
        public CancellationToken CancellationToken { get; set; }

        // コマンド
        public IJobCommand Command { get; set; }
    }

    /// <summary>
    /// JobCommand
    /// </summary>
    public interface IJobCommand
    {
        // メイン処理
        void Execute(ManualResetEventSlim completed, CancellationToken token);

        // キャンセル処理
        void Cancel();
    }


    /// <summary>
    /// 登録済みジョブ情報
    /// 登録後はこのインスタンスを介して制御する
    /// </summary>
    public class JobRequest
    {
        private JobEngine _jobEngine;
        private Job _job;
        private CancellationTokenSource _cancellationTokenSource;
        public QueueElementPriority Priority { get; private set; }

        // コンストラクタ
        public JobRequest(JobEngine jobEngine, Job job, QueueElementPriority priority)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            _jobEngine = jobEngine;
            _job = job;
            _job.CancellationToken = _cancellationTokenSource.Token;
            Priority = priority;
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

        // ワーカースレッド起動イベント
        public ManualResetEvent Event { get; private set; }

        // コンストラクト
        public JobContext()
        {
            JobQueue = new PriorityQueue<Job>();
            Lock = new object();
            Event = new ManualResetEvent(false);
        }
    }

    /// <summary>
    /// JobEngine
    /// </summary>
    public class JobEngine : IDisposable
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
        /// IsBusyChanged event
        /// </summary>
        public event EventHandler IsBusyChanged;

        /// <summary>
        /// IsBusy property.
        /// </summary>
        public bool IsBusy
        {
            get { return Workers.Any(e => e != null && e.IsBusy); }
        }

        // ジョブの製造番号用カウンタ
        private int _serialNumber;

        // コンテキスト
        private JobContext _context;

        // 最大ワーカー数
        public readonly int _MaxWorkerSize = 4;

        // ワーカー
        public JobWorker[] Workers { get; set; }


        // コンストラクタ
        public JobEngine()
        {
            _context = new JobContext();
            Workers = new JobWorker[_MaxWorkerSize];
        }

        // 開始
        public void Start(int workerSize)
        {
            if (workerSize < 1) workerSize = 1;
            if (workerSize > _MaxWorkerSize) workerSize = _MaxWorkerSize;

            ChangeWorkerSize(workerSize);
        }

        // 稼働ワーカー数変更
        public void ChangeWorkerSize(int size)
        {
            Debug.Assert(0 <= size && size <= _MaxWorkerSize);
            Debug.WriteLine("JobWorker: " + size);

            for (int i = 0; i < _MaxWorkerSize; ++i)
            {
                if (i < size)
                {
                    if (Workers[i] == null)
                    {
                        Workers[i] = new JobWorker(_context);
                        Workers[i].StatusChanged += (s, e) => StatusChanged?.Invoke(s, e);
                        Workers[i].IsBusyChanged += (s, e) => IsBusyChanged?.Invoke(s, e);
                        Workers[i].Run();
                        Message = $"Create Worker[{i}]";
                    }
                }
                else
                {
                    if (Workers[i] != null)
                    {
                        Workers[i].Cancel();
                        Workers[i] = null;
                        Message = $"Delete Worker[{i}]";
                    }
                }
            }

            // イベント待ち解除
            _context.Event.Set();

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
                    job.Command.Cancel(); // TODO: 処理しないのにキャンセルをしており後処理が異なる。しっくりこない。
                    job.Completed.Set();
                }
            }
        }

        /// <summary>
        /// Job登録
        /// </summary>
        /// <param name="command">処理</param>
        /// <param name="priority">優先度</param>
        /// <returns>JobRequest</returns>
        public JobRequest Add(object sender, IJobCommand command, QueueElementPriority priority, bool reverse = false)
        {
            var job = new Job();
            job.SerialNumber = _serialNumber++;
            job.Sender = sender;
            job.Command = command;

            var request = new JobRequest(this, job, priority);

            lock (_context.Lock)
            {
                _context.JobQueue.Enqueue(job, priority, reverse);
                _context.Event.Set();
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

        // 廃棄処理
        public void Dispose()
        {
            ChangeWorkerSize(0);
        }
    }


    /// <summary>
    /// ジョブワーカー
    /// </summary>
    public class JobWorker
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
        private CancellationTokenSource _cancellationTokenSource;


        // コンストラクタ
        public JobWorker(JobContext context)
        {
            _context = context;
            _cancellationTokenSource = new CancellationTokenSource();
        }


        // ワーカータスク開始
        public void Run()
        {
            Message = $"Run";
#if true
            // Task版
            var task = Task.Run(() =>
            {
                try
                {
                    WorkerExecute();
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception e)
                {
                    Message = e.Message;
                    Action<Exception> action = (exception) => { throw new ApplicationException("タスク内部エラー", exception); };
                    App.Current.Dispatcher.BeginInvoke(action, e);
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
        private async void WorkerExecute()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                Message = $"get Job ...";
                Job job;

                lock (_context.Lock)
                {
                    // ジョブ取り出し
                    job = _context.JobQueue.Dequeue();

                    // ジョブが無い場合はイベントリセット
                    if (job == null)
                    {
                        _context.Event.Reset();
                    }
                }

                // イベント待ち
                if (job == null)
                {
                    IsBusy = false;
                    Message = $"wait event ...";
                    await Task.Run(() => _context.Event.WaitOne());
                    continue;
                }

                IsBusy = true;

                if (!job.CancellationToken.IsCancellationRequested)
                {
                    Message = $"Job({job.SerialNumber}) execute ...";
                    job.Command.Execute(job.Completed, job.CancellationToken);
                    Message = $"Job({job.SerialNumber}) execute done.";
                }

                if (job.CancellationToken.IsCancellationRequested)
                {
                    job.Command.Cancel();
                    Message = $"Job({job.SerialNumber}) canceled.";
                }

                // JOB完了
                job.Completed.Set();
            }

            Debug.WriteLine("Task: Exit.");
        }
    }
}
