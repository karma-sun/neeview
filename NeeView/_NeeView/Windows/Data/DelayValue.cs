using System;
using System.Diagnostics;
using System.Windows.Threading;

namespace NeeView.Windows.Data
{
    //
    public class DelayValue<T> where T : IComparable
    {
        public event EventHandler ValueChanged;

        private T _value;
        public T Value
        {
            get { return _value; }
            set { SetValue(value, 0); }
        }

        private T _delayValue;

        private DateTime _delayTime = DateTime.MaxValue;

        private DispatcherTimer _timer;

        //
        public DelayValue()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(0.1);
            _timer.Tick += Tick;
        }

        //
        public DelayValue(T value) : this()
        {
            _value = value;
            _delayValue = value;
        }

        //
        public void SetValue(T value, double ms = 0.0, bool isForce = false)
        {
            if (!isForce && _delayValue.Equals(value)) return;

            _delayValue = value;

            if (ms <= 0.0)
            {
                Flush();
            }
            else
            {
                _delayTime = DateTime.Now + TimeSpan.FromMilliseconds(ms);
                _timer.Start();
            }
        }

        //
        private void Flush()
        {
            _timer.Stop();

            if (!_delayValue.Equals(_value))
            {
                _value = _delayValue;
                ValueChanged?.Invoke(this, null);
            }
        }

        //
        private void Tick(object sender, EventArgs e)
        {
            if (_delayTime <= DateTime.Now)
            {
                Flush();
            }
        }

        //
        public string ToDetail()
        {
            return _timer.IsEnabled ? $"{_value} ({_delayValue}, {(_delayTime - DateTime.Now).TotalMilliseconds}ms)" : $"{_value}";
        }
    }
}
