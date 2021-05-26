using System.Collections;
using System.Collections.Generic;

namespace NeeLaboratory.Collection
{
    // from https://www.hanachiru-blog.com/entry/2020/05/05/120000
    public class FixedQueue<T> : IEnumerable<T>
    {
        private Queue<T> _queue;

        public FixedQueue(int capacity)
        {
            Capacity = capacity;
            _queue = new Queue<T>(capacity);
        }

        public int Count => _queue.Count;

        public int Capacity { get; private set; }

        public void Enqueue(T item)
        {
            _queue.Enqueue(item);

            if (Count > Capacity) Dequeue();
        }

        public T Dequeue() => _queue.Dequeue();

        public T Peek() => _queue.Peek();

        public IEnumerator<T> GetEnumerator() => _queue.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _queue.GetEnumerator();
    }
}
