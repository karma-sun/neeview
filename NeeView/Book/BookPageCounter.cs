using System.Threading;

namespace NeeView
{
    /// <summary>
    /// Bookで使用されるカウンター型
    /// </summary>
    public class BookPageCounter
    {
        public int _counter;

        public int Counter => _counter;

        public int Increment()
        {
            return Interlocked.Increment(ref _counter);
        }
    }
}
