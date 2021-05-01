using System;
using System.Windows.Threading;

namespace NeeView.Threading
{
    public class SimpleDelayAction
    {
        private Action _action;
        private DispatcherTimer _timer;
        private object _lock = new object();

        public SimpleDelayAction()
        {
            _timer = new DispatcherTimer(DispatcherPriority.Normal, App.Current.Dispatcher);
            _timer.Tick += Timer_Tick;
        }

        public void Request(Action action, TimeSpan delay)
        {
            Cancel();

            lock (_lock)
            {
                _action = action ?? throw new ArgumentNullException(nameof(action));
                _timer.Interval = delay;
                _timer.Start();
            }
        }

        public void Cancel()
        {
            lock (_lock)
            {
                _timer.Stop();
                _action = null;
            }
        }

        public bool Flush()
        {
            lock (_lock)
            {
                if (_action is null)
                {
                    return false;
                }

                _timer.Stop();
                _action?.Invoke();
                _action = null;
                return true;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            lock (_lock)
            {
                _timer.Stop();
                _action?.Invoke();
                _action = null;
            }
        }
    }
}