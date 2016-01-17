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

        // 処理
        public Action<CancellationToken> Execute { get; set; }

        // キャンセル時の処理
        public Action Cancel { get; set; }

        // キャンセルトークン
        public CancellationToken CancellationToken { get; set; }
    }


    /// <summary>
    /// 登録済みジョブ情報
    /// 登録後はこのインスタンスを介して制御する
    /// </summary>
    public class JobRequest
    {
        private JobEngine _JobEngine;
        private Job _Job;
        private CancellationTokenSource _CancellationTokenSource;
        public QueueElementPriority Priority { get; private set; }

        // コンストラクタ
        public JobRequest(JobEngine jobEngine, Job job, QueueElementPriority priority)
        {
            _CancellationTokenSource = new CancellationTokenSource();

            _JobEngine = jobEngine;
            _Job = job;
            _Job.CancellationToken = _CancellationTokenSource.Token;
            Priority = priority;
        }

        // キャンセル
        public void Cancel()
        {
            _CancellationTokenSource.Cancel();
        }

        // キャンセルリクエスト判定
        public bool IsCancellationRequested
        {
            get { return _CancellationTokenSource.IsCancellationRequested; }
        }

        // プライオリティ変更
        public void ChangePriority(QueueElementPriority priority)
        {
            if (Priority != priority)
            {
                _JobEngine.ChangePriority(_Job, priority);
                Priority = priority;
            }
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
        private string _Message;
        public string Message
        {
            get { return _Message; }
            set { _Message = value; NotifyStatusChanged(); }
        }
        #endregion

        #endregion

        // ジョブの製造番号用カウンタ
        private int _SerialNumber;

        // コンテキスト
        private JobContext _Context;

        // 最大ワーカー数
        public readonly int _MaxWorkerSize = 4;

        // ワーカー
        public JobWorker[] Workers { get; set; }


        // コンストラクタ
        public JobEngine()
        {
            _Context = new JobContext();
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
                        Workers[i] = new JobWorker(_Context);
                        Workers[i].StatusChanged += (s, e) => StatusChanged?.Invoke(s, e);
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
            _Context.Event.Set();

            NotifyStatusChanged();
        }

        /// <summary>
        /// Job登録
        /// </summary>
        /// <param name="action">処理</param>
        /// <param name="cancelAction">キャンセル時の処理</param>
        /// <param name="priority">優先度</param>
        /// <returns>JobRequest</returns>
        public JobRequest Add(Action<CancellationToken> action, Action cancelAction, QueueElementPriority priority)
        {
            var job = new Job();
            job.SerialNumber = _SerialNumber++;
            job.Execute = action;
            job.Cancel = cancelAction;

            var request = new JobRequest(this, job, priority);

            lock (_Context.Lock)
            {
                _Context.JobQueue.Enqueue(job, priority);
                _Context.Event.Set();
                Message = $"Add Job. {job.SerialNumber}";
            }

            NotifyStatusChanged();

            return request;
        }

        // 優先度変更
        public void ChangePriority(Job job, QueueElementPriority priority)
        {
            lock (_Context.Lock)
            {
                _Context.JobQueue.ChangePriority(job, priority);
            }
        }

        // 待機ジョブ数
        public int JobCount
        {
            get { return _Context.JobQueue.Count; }
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
        private string _Message;
        public string Message
        {
            get { return _Message; }
            set { _Message = value; NotifyStatusChanged(); }
        }
        #endregion

        #endregion

        // コンテキスト
        JobContext _Context;

        // ワーカータスクのキャンセルトークン
        CancellationTokenSource _CancellationTokenSource;


        // コンストラクタ
        public JobWorker(JobContext context)
        {
            _Context = context;
            _CancellationTokenSource = new CancellationTokenSource();
        }


        // ワーカータスク開始
        public void Run()
        {
            Message = $"Run";

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
            _CancellationTokenSource.Token);
        }


        // ワーカータスク廃棄
        public void Cancel()
        {
            _CancellationTokenSource.Cancel();
        }


        // ワーカータスクメイン
        private async void WorkerExecute()
        {
            while (!_CancellationTokenSource.Token.IsCancellationRequested)
            {
                Message = $"get Job ...";
                Job job;

                lock (_Context.Lock)
                {
                    // ジョブ取り出し
                    job = _Context.JobQueue.Decueue();

                    // ジョブが無い場合はイベントリセット
                    if (job == null)
                    {
                        _Context.Event.Reset();
                    }
                }

                // イベント待ち
                if (job == null)
                {
                    Message = $"wait event ...";
                    await Task.Run(()=>_Context.Event.WaitOne());
                    continue;
                }

                if (!job.CancellationToken.IsCancellationRequested)
                {
                    Message = $"Job({job.SerialNumber}) execute ...";
                    job.Execute(job.CancellationToken);
                    Message = $"Job({job.SerialNumber}) execute done.";
                }

                if (job.CancellationToken.IsCancellationRequested)
                {
                    job.Cancel?.Invoke();
                    Message = $"Job({job.SerialNumber}) canceled.";
                }
            }

            Debug.WriteLine("Task: Exit.");
        }
    }
}
