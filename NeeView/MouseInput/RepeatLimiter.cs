using System.Diagnostics;

namespace NeeView
{
    public class RepeatLimiter
    {
        private Stopwatch _stopwatch;

        public RepeatLimiter()
        {
            _stopwatch = Stopwatch.StartNew();
        }

        public bool IsLimit(int milliseconds)
        {
            if (milliseconds <= 0)
            {
                return false;
            }

            return _stopwatch.ElapsedMilliseconds < milliseconds;
        }

        public void Reset()
        {
            _stopwatch.Restart();
        }
    }

}
