using System;

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

        //
        public DelayValue()
        {
        }

        //
        public DelayValue(T value)
        {
            _value = value;
            _delayValue = value;
        }

        //
        public void SetValue(T value, double ms = 0.0)
        {
            if (_delayValue.Equals(value)) return;

            _delayValue = value;
            _delayTime = DateTime.Now + TimeSpan.FromMilliseconds(ms);
            Tick(this, null);
        }

        //
        public void Tick(object sender, EventArgs e)
        {
            if (_delayTime <= DateTime.Now)
            {
                _value = _delayValue;
                _delayTime = DateTime.MaxValue;
                ValueChanged?.Invoke(this, null);
            }
        }

    }
}
