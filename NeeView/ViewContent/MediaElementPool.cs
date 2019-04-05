using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace NeeView
{
    public class MediaElementPool
    {
        private Stack<MediaElement> _pool = new Stack<MediaElement>();
        private object _lock = new object();
        ////private int _count;

        public MediaElement Allocate()
        {
            lock (_lock)
            {
                if (_pool.Any())
                {
                    ////Debug.WriteLine($"MediaElementRecycle: Recycle");
                    return _pool.Pop();
                }
                else
                {
                    ////Debug.WriteLine($"MediaElementRecycle: New #{++_count}");
                    return new MediaElement();
                }
            }
        }

        public void Release(MediaElement element)
        {
            lock (_lock)
            {
                _pool.Push(element);
            }
        }
    }
}
