using System.Collections;
using System.Collections.Generic;

namespace NeeView
{
    /// <summary>
    /// LinkedList + Dictionary
    /// 検索を高速化したLinkedList
    /// </summary>
    /// <typeparam name="TKey">検索用キー</typeparam>
    /// <typeparam name="TValue">収納型</typeparam>
    public class LinkedDicionary<TKey, TValue> : IEnumerable<TValue>
    {
        private Dictionary<TKey, LinkedListNode<TValue>> _map = new Dictionary<TKey, LinkedListNode<TValue>>();
        private LinkedList<TValue> _list = new LinkedList<TValue>();

        public int Count => _map.Count;
        public LinkedListNode<TValue> First => _list.First;
        public LinkedListNode<TValue> Last => _list.Last;

        public void AddFirst(TKey key, TValue item)
        {
            var node = Find(key);
            if (node == null)
            {
                node = new LinkedListNode<TValue>(item);
                _map.Add(key, node);
                _list.AddFirst(node);
            }
            else
            {
                node.Value = item;
                _list.Remove(node);
                _list.AddFirst(node);
            }
        }

        public void AddLast(TKey key, TValue item)
        {
            var node = Find(key);
            if (node == null)
            {
                node = new LinkedListNode<TValue>(item);
                _map.Add(key, node);
                _list.AddLast(node);
            }
            else
            {
                node.Value = item;
                _list.Remove(node);
                _list.AddLast(node);
            }
        }

        // no check version
        public void AddLastRaw(TKey key, TValue item)
        {
            var node = new LinkedListNode<TValue>(item);
            _map.Add(key, node);
            _list.AddLast(node);
        }

        public LinkedListNode<TValue> Find(TKey key)
        {
            _map.TryGetValue(key, out var node);
            return node;
        }

        public bool Remove(TKey key)
        {
            if (key == null) return false;

            var node = Find(key);
            if (node != null)
            {
                _list.Remove(node);
                _map.Remove(key);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Remap(TKey src, TKey dst)
        {
            var node = Find(src);
            if (node != null)
            {
                _map.Remove(src);
                _map.Add(dst, node);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Clear()
        {
            _list.Clear();
            _map.Clear();
        }

        public bool ContainsKey(TKey key)
        {
            return _map.ContainsKey(key);
        }

        #region IEnumerable support

        public IEnumerator<TValue> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion
    }
}
