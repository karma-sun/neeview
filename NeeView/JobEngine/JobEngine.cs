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

        private bool _isBusy;
        private readonly int _maxWorkerSize;
        private int _workerSize = 2;


        // コンストラクタ
        private JobEngine()
        {
            InitializeScheduler();

            _maxWorkerSize = Math.Max(4, Environment.ProcessorCount);

            Workers = new JobWorker[_maxWorkerSize];

            ChangeWorkerSize(_workerSize);

            // アプリ終了前の開放予約
            ApplicationDisposer.Current.Add(this);
        }


        public bool IsBusy
        {
            get { return _isBusy; }
            set { if (_isBusy != value) { _isBusy = value; RaisePropertyChanged(); } }
        }

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

        public JobWorker[] Workers { get; set; }


        private void UpdateIsBusy()
        {
            this.IsBusy = this.Workers.Any(e => e != null && e.IsBusy);
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
                        Workers[i] = new JobWorker(_scheduler);
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
            _scheduler.RaiseQueueChanged();

            RaisePropertyChanged(nameof(Workers));
            NotifyStatusChanged();
        }


        #region Scheduler
        private JobScheduler _scheduler;

        private void InitializeScheduler()
        {
            _scheduler = new JobScheduler();
        }

        public void RegistClient(JobClient client)
        {
            _scheduler.RegistClent(client);
        }

        public void UnregistClient(JobClient client)
        {
            _scheduler.Order(client, new List<JobOrder>());
            _scheduler.UnregistClient(client);
        }

        public List<JobSource> Order(JobClient sender, List<JobOrder> orders)
        {
            return _scheduler.Order(sender, orders);
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
