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


        private bool _isBusy;
        private readonly int _maxWorkerSize;
        private int _workerSize = 2;


        // コンストラクタ
        private JobEngine()
        {
            InitializeScheduler();

            _maxWorkerSize = Config.Current.Performance.GetMaxJobWorkerSzie();
            _workerSize = Config.Current.Performance.JobWorkerSize;

            Config.Current.Performance.AddPropertyChanged(nameof(PerformanceConfig.JobWorkerSize), (s, e) =>
            {
                _workerSize = Config.Current.Performance.JobWorkerSize;
                ChangeWorkerSize(_workerSize);
            });


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
            Debug.WriteLine("JobEngine: WorkerSize=" + size);

            var primaryCount = (size > 2) ? 2 : size - 1;
            var isLimited = primaryCount <= 1;

            for (int i = 0; i < _maxWorkerSize; ++i)
            {
                if (i < size)
                {
                    if (Workers[i] == null)
                    {
                        Workers[i] = new JobWorker(_scheduler);
                        Workers[i].IsBusyChanged += (s, e) => UpdateIsBusy(); ////  IsBusyChanged?.Invoke(s, e);
                        Workers[i].Run();
                        Debug.WriteLine($"JobEngine: Create Worker[{i}]");
                    }

                    Workers[i].IsPrimary = i < primaryCount;
                    Workers[i].IsLimited = isLimited;
                }
                else
                {
                    if (Workers[i] != null)
                    {
                        Workers[i].Cancel();
                        Workers[i].Dispose();
                        Workers[i] = null;
                        Debug.WriteLine($"JobEngine: Delete Worker[{i}]");
                    }
                }
            }

            // イベント待ち解除
            _scheduler.RaiseQueueChanged();

            RaisePropertyChanged(nameof(Workers));
        }


        #region Scheduler
        private JobScheduler _scheduler;

        public JobScheduler Scheduler => _scheduler;

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
        public class Memento : IMemento
        {
            [DataMember, DefaultValue(2)]
            public int WorkerSize { get; set; }

            public void RestoreConfig(Config config)
            {
                config.Performance.JobWorkerSize = WorkerSize;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.WorkerSize = Config.Current.Performance.JobWorkerSize;
            return memento;
        }

        #endregion
    }
}
