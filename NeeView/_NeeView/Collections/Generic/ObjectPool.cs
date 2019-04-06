using System.Collections.Generic;
using System.Linq;

namespace NeeView.Collections.Generic
{
    /// <summary>
    /// オブジェクトのリサイクル
    /// </summary>
    public class ObjectPool<T> where T : new()
    {
        private Stack<T> _pool = new Stack<T>();
        private object _lock = new object();
        private int _count;

        public T Allocate()
        {
            lock (_lock)
            {
                if (_pool.Any())
                {
                    ////Debug.WriteLine($"{typeof(T)} Pool: Recycle");
                    return _pool.Pop();
                }
                else
                {
                    _count++;
                    ////Debug.WriteLine($"{typeof(T)} Pool: New #{_count}");
                    return new T();
                }
            }
        }

        public void Release(T element)
        {
            lock (_lock)
            {
                ////Debug.WriteLine($"{typeof(T)} Pool: Release");
                _pool.Push(element);
            }
        }
    }

}
