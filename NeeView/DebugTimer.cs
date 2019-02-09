using System.Diagnostics;

namespace NeeView
{
    public static class DebugTimer
    {
        private static Stopwatch _sw;

        [Conditional("DEBUG")]
        public static void Start(string message = null)
        {
            if (message != null)
            {
                Debug.WriteLine(message);
            }
            _sw = Stopwatch.StartNew();
        }

        [Conditional("DEBUG")]
        public static void Check(string message)
        {
            var ms = _sw != null ? _sw.ElapsedMilliseconds : 0L;
            Debug.WriteLine($">{message}: {ms}ms");
        }
    }
}
