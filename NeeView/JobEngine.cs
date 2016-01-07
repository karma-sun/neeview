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
    public enum JobPriority
    {
        Top,
        Hi,
        Default
    }

    //
    public class JobList<T>
    {
        private volatile Dictionary<JobPriority, List<T>> _Lists;

        public JobList()
        {
            _Lists = new Dictionary<JobPriority, List<T>>();
            foreach (JobPriority priority in Enum.GetValues(typeof(JobPriority)))
            {
                _Lists[priority] = new List<T>();
            }
        }

        public void Add(T item, JobPriority priority)
        {
            _Lists[priority].Add(item);
        }

        public void RemoveAt(int index, JobPriority priority)
        {
            _Lists[priority].RemoveAt(index);
        }

        public T this[int index, JobPriority priority]
        {
            set { _Lists[priority][index] = value; }
            get { return _Lists[priority][index]; }
        }

        public int Count
        {
            get
            {
                int sum = 0;
                foreach (JobPriority priority in Enum.GetValues(typeof(JobPriority)))
                {
                    sum += _Lists[priority].Count;
                }
                return sum;
            }
        }

        public T First()
        {
            foreach (JobPriority priority in Enum.GetValues(typeof(JobPriority)))
            {
                if (_Lists[priority].Count > 0) return _Lists[priority][0];
            }
            return default(T);
        }

        public void RemoveFirst()
        {
            foreach (JobPriority priority in Enum.GetValues(typeof(JobPriority)))
            {
                if (_Lists[priority].Count > 0)
                {
                    _Lists[priority].RemoveAt(0);
                    return;
                }
            }
        }

        public T Decueue()
        {
            foreach (JobPriority priority in Enum.GetValues(typeof(JobPriority)))
            {
                if (_Lists[priority].Count > 0)
                {
                    var item = _Lists[priority][0];
                    _Lists[priority].RemoveAt(0);
                    return item;
                }
            }
            return default(T);
        }

        public void ChangePriority(T item, JobPriority newPriority)
        {
            foreach (JobPriority priority in Enum.GetValues(typeof(JobPriority)))
            {
                if (priority == newPriority) continue;

                int index = _Lists[priority].IndexOf(item);
                if (index >= 0)
                {
                    _Lists[priority].RemoveAt(index);
                    this.Add(item, newPriority);
                    return;
                }
            }
        }
    }


    //
    public class JobRequest
    {
        private JobEngine _JobEngine;
        private Job _Job;
        private CancellationTokenSource _CancellationTokenSource;

        public JobRequest(JobEngine jobEngine, Job job)
        {
            _CancellationTokenSource = new CancellationTokenSource();

            _JobEngine = jobEngine;
            _Job = job;
            _Job.CancellationToken = _CancellationTokenSource.Token;
        }

        public void Cancel()
        {
            _CancellationTokenSource.Cancel();
        }

        public bool IsCancellationRequested
        {
            get { return _CancellationTokenSource.IsCancellationRequested; }
        }

        public void ChangePriority(JobPriority priority)
        {
            if (_Job.Priority != priority)
            {
                _JobEngine.ChangePriority(_Job, priority);
            }
        }
    }

    // job base
    public class Job
    {
        public int SerialNumber { get; set; }
        public JobPriority Priority; // これ違和感
        public Action<CancellationToken> Action;
        public Action CancelAction;
        public CancellationToken CancellationToken;
    }


    public class JobContext : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        public event EventHandler<Job> AddEvent;
        public event EventHandler<Job> RemoveEvent;

        public void NotifyAddEvent(Job job)
        {
            AddEvent?.Invoke(this, job);
        }

        public void NotifyRemoveEvent(Job job)
        {
            RemoveEvent?.Invoke(this, job);
        }

        // ジョブリスト
        public JobList<Job> JobList { get; private set; }

        // 排他処理用ロック
        public Object Lock { get; private set; }

        // ワーカースレッド起動イベント
        public ManualResetEvent Event { get; private set; }

        public JobContext()
        {
            JobList = new JobList<Job>();
            Lock = new Object();
            Event = new ManualResetEvent(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class JobEngine : INotifyPropertyChanged, IDisposable
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion


        #region Property: Message
        private string _Message;
        public string Message
        {
            get { return _Message; }
            set { _Message = value; OnPropertyChanged(); }
        }
        #endregion



        // ジョブの製造番号用カウンタ
        private int _SerialNumber;

        public JobContext Context { get; private set; }

        public readonly int _MaxWorkerSize = 2;
        private JobWorker[] _Workers;
        public JobWorker[] Workers => _Workers;

        public JobEngine()
        {
            Context = new JobContext();
            _Workers = new JobWorker[_MaxWorkerSize];
        }


        public void Start()
        {
            ChangeWorkerSize(_MaxWorkerSize);
        }

        public void ChangeWorkerSize(int size)
        {
            Debug.Assert(0 <= size && size <= _MaxWorkerSize);

            for (int i = 0; i < _MaxWorkerSize; ++i)
            {
                if (i < size)
                {
                    if (_Workers[i] == null)
                    {
                        _Workers[i] = new JobWorker(Context);
                        _Workers[i].Run();
                        Message = $"Create Worker[{i}]";
                    }
                }
                else
                {
                    if (_Workers[i] != null)
                    {
                        _Workers[i].Cancel();
                        _Workers[i] = null;
                        Message = $"Delete Worker[{i}]";
                    }
                }
            }

            // イベント待ち解除
            Context.Event.Set();

            OnPropertyChanged(nameof(Workers));
        }


        public JobRequest Add(Action<CancellationToken> action, Action cancelAction, JobPriority priority)
        {
            var job = new Job();
            job.SerialNumber = _SerialNumber++;
            job.Action = action;
            job.CancelAction = cancelAction;
            job.Priority = priority;
            var source = new JobRequest(this, job);

            lock (Context.Lock)
            {
                Context.JobList.Add(job, priority);
                Context.Event.Set();
                Message = $"Add Job. {job.SerialNumber}";
            }

            Context.NotifyAddEvent(job);

            return source;
        }


        public void ChangePriority(Job job, JobPriority priority)
        {
            lock (Context.Lock)
            {
                Context.JobList.ChangePriority(job, priority);
                job.Priority = priority;
            }
        }


        // 開発用遅延
        [Conditional("DEBUG")]
        private void __Delay(int ms)
        {
            Thread.Sleep(ms);
        }

        public void Dispose()
        {
            ChangeWorkerSize(0);
        }
    }




    /// <summary>
    /// 
    /// </summary>
    public class JobWorker : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        #region Property: Message
        private string _Message;
        public string Message
        {
            get { return _Message; }
            set { _Message = value; OnPropertyChanged(); }
        }
        #endregion

        JobContext _Context;

        CancellationTokenSource _CancellationTokenSource;

        public JobWorker(JobContext context)
        {
            _Context = context;
            _CancellationTokenSource = new CancellationTokenSource();
        }

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
                    Action<Exception> action = (exception) => { throw new ApplicationException("タスク内部エラー", exception); };
                    App.Current.Dispatcher.BeginInvoke(action, e);
                }
            },
            _CancellationTokenSource.Token);
        }

        public void Cancel()
        {
            _CancellationTokenSource.Cancel();
        }

        // ワーカータスク
        private void WorkerExecute()
        {
            while (!_CancellationTokenSource.Token.IsCancellationRequested)
            {
                Message = $"get Job ...";
                Job job;

                lock (_Context.Lock)
                {
                    // ジョブ取り出し
                    job = _Context.JobList.Decueue();

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
                    _Context.Event.WaitOne();
                    continue;
                }


                if (job.CancellationToken.IsCancellationRequested)
                {
                    job.CancelAction?.Invoke();
                    Message = $"Job({job.SerialNumber}) canceled.";
                }
                else
                {
                    Message = $"Job({job.SerialNumber}) execute ... {job.Priority}";
                    job.Action(job.CancellationToken);
                    Message = $"Job({job.SerialNumber}) execute done.";
                }

                //throw new ApplicationException("なんちゃって例外発生");

                _Context.NotifyRemoveEvent(job);
            }

            Debug.WriteLine("Task: Exit.");
        }
    }
}
