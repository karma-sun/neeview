using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    public static class DebugTimer
    {
        private static Stopwatch _sw;

        private static string _label;
        private static Dictionary<string, long> _timetable;
        private static int _count;
        private static bool _isSlient;

        [Conditional("DEBUG")]
        public static void Start(string message = null, bool isSilent = false)
        {
            _label = message ?? "DebugTimer";
            _isSlient = isSilent;

            if (!_isSlient) Debug.WriteLine(message);

            _timetable = new Dictionary<string, long>();
            _count = 0;

            _sw = Stopwatch.StartNew();
        }

        [Conditional("DEBUG")]
        public static void Stop()
        {
            _sw?.Stop();
            _sw = null;
        }

        [Conditional("DEBUG")]
        public static void CheckRestart()
        {
            _count++;
            _sw.Restart();
        }

        [Conditional("DEBUG")]
        public static void Check(string message)
        {
            if (_sw == null)
            {
                //Debug.WriteLine($"DebugTimer is not active.");
                return;
            }

            var ms = _sw.ElapsedMilliseconds;
            if (!_isSlient) Debug.WriteLine($">{message}: {ms:#,0}ms");
            if (_timetable.ContainsKey(message))
            {
                _timetable[message] += ms;
            }
            else
            {
                _timetable.Add(message, ms);
            }

            _sw.Restart();
        }

        [Conditional("DEBUG")]
        public static void Result()
        {
            if (_sw == null)
            {
                Debug.WriteLine($"DebugTimer is not active.");
                return;
            }

            _sw.Stop();

            Debug.WriteLine($"\n[{_label}] ({_count:#,0})");
            foreach (var item in _timetable)
            {
                Debug.WriteLine($"{item.Key}: {item.Value:#,0}ms");
            }
            Debug.WriteLine($"Total: {_timetable.Values.Sum():#,0}ms");

            _sw.Start();
        }
    }

    public static class StopWatchExtensions
    {
        public static void Check(this Stopwatch self, string label)
        {
            Debug.WriteLine($"> {label}: {self.ElapsedMilliseconds:#,0}ms");
        }
    }
}
