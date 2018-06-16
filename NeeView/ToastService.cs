using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace NeeView
{
    public class ToastService : BindableBase
    {
        public static ToastService Current { get; } = new ToastService();


        private Queue<Toast> _queue;
        private ToastCard _toastCard;
        private DispatcherTimer _timer;
        private DateTime _timeLimit;


        public ToastService()
        {
            _queue = new Queue<Toast>();

            _timer = new System.Windows.Threading.DispatcherTimer();
            _timer.Tick += Timer_Tick;
        }


        public ToastCard ToastCard
        {
            get { return _toastCard; }
            set { SetProperty(ref _toastCard, value); }
        }

        public void Show(Toast toast)
        {
            _queue.Enqueue(toast);

            // ひとまず１枚だけに限定する
            if (ToastCard != null)
            {
                ToastCard.IsCanceled = true;
            }

            Update();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Update();
        }

        public void Update()
        {
            App.Current.Dispatcher.BeginInvoke((Action)(() => UpdateCore()));
        }

        private void UpdateCore()
        {
            if (ToastCard != null)
            {
                if (ToastCard.IsCanceled || ToastCard.Toast.IsCanceled || (!ToastCard.IsMouseOver && DateTime.Now > _timeLimit))
                {
                    Close();
                }
            }

            while (ToastCard == null && _queue.Count > 0)
            {
                var toast = _queue.Dequeue();
                Open(toast);
            }
        }

        private void Open(Toast toast)
        {
            if (toast.IsCanceled)
            {
                return;
            }

            ToastCard = new ToastCard() { Toast = toast };
            _timeLimit = DateTime.Now + toast.DisplayTime;
            _timer.Interval = new TimeSpan(0, 0, 1);
            _timer.Start();
        }

        private void Close()
        {
            if (ToastCard != null)
            {
                _timer.Stop();
                ToastCard = null;
            }
        }

    }
}
