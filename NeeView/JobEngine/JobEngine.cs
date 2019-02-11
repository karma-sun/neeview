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
using System.Windows;
using System.Windows.Threading;

// TODO: Jobの状態パラメータ(Status?)

namespace NeeView
{
    /// <summary>
    /// JobEngine
    /// </summary>
    public class JobEngine : BindableBase, IDisposable
    {
        static JobEngine() => Current = new JobEngine();
        public static JobEngine Current { get; }

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
        private readonly int _maxWorkerSize;


        /// <summary>
        /// WorkerSize property.
        /// </summary>
        [PropertyMember("@ParamJobEngineWorkerSize", Tips = "@ParamJobEngineWorkerSizeTips")]
        public int WorkerSize
        {
            get { return _workerSize; }
            set
            {
                var size = MathUtility.Clamp(value, 1, _maxWorkerSize);
                if (_workerSize != size)
                {
                    _workerSize = size;
                    ChangeWorkerSize(_workerSize);
                    RaisePropertyChanged();
                }
            }
        }

        private int _workerSize = 2;

        // ワーカー
        public JobWorker[] Workers { get; set; }


        // コンストラクタ
        private JobEngine()
        {
            _maxWorkerSize = Math.Max(4, Environment.ProcessorCount);

            _context = new JobContext();
            Workers = new JobWorker[_maxWorkerSize];

            ChangeWorkerSize(_workerSize);

            // アプリ終了前の開放予約
            ApplicationDisposer.Current.Add(this);
        }

        // 廃棄処理
        private void Stop()
        {
            ChangeWorkerSize(0);
            Debug.WriteLine("JobEngine: Disposed.");
        }

        // 稼働ワーカー数変更
        public void ChangeWorkerSize(int size)
        {
            Debug.Assert(0 <= size && size <= _maxWorkerSize);
            Debug.WriteLine("JobWorker: " + size);

            var primaryCount = (size > 2) ? 2 : size - 1;

            for (int i = 0; i < _maxWorkerSize; ++i)
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

            RaisePropertyChanged(nameof(Workers));
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

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Stop();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

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
}
