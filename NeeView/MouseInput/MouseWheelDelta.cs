using System;
using System.Diagnostics;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// ホイール回転をノッチ回数に変更
    /// </summary>
    public class MouseWheelDelta
    {
        private int _delta;
        private int _timestamp;

        private readonly int _resettime = 1000;
        private readonly int _notch = 120;

        public void Reset()
        {
            _delta = 0;
        }

        public int NotchCount(MouseWheelEventArgs e)
        {
            if (e.Timestamp - _timestamp > _resettime)
            {
                ////System.Diagnostics.Debug.WriteLine($"WheelCount: reset.");
                _delta = 0;
            }

            _delta += e.Delta;
            int count = _delta / _notch;
            _delta -= count * _notch;
            _timestamp = e.Timestamp;

            ////System.Diagnostics.Debug.WriteLine($"WheelCount: {count}, {_delta}");
            return count;
        }
    }
}
