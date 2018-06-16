using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NeeView.Collections.Generic
{
    [DataContract]
    public class TreeListNode<T> : IEnumerable<TreeListNode<T>>
    {
        private TreeListNode<T> _parent;
        private List<TreeListNode<T>> _children;
        private bool _isExpanded;
        private T _value;

        public TreeListNode()
        {
            _children = new List<TreeListNode<T>>();
        }

        public TreeListNode(T value)
        {
            _children = new List<TreeListNode<T>>();
            _value = value;
        }

        public TreeListNode<T> Parent => _parent;

        public List<TreeListNode<T>> Children
        {
            get => _children;
            private set => _children = value;
        }

        [DataMember(Name = "Children", EmitDefaultValue = false)]
        private List<TreeListNode<T>> _NullableChildren
        {
            get => _children == null || _children.Count == 0 ? null : _children;
            set => _children = value ?? new List<TreeListNode<T>>();
        }

        [DataMember(EmitDefaultValue = false)]
        public bool IsExpanded
        {
            get => _isExpanded && IsExpandEnabled;
            set => _isExpanded = value;
        }

        public bool IsExpandEnabled => Children.Count > 0;

        [DataMember]
        public T Value
        {
            get => _value;
            set => _value = value;
        }

        public TreeListNode<T> Root => _parent == null ? this : _parent.Root;
        public int Depth => _parent == null ? 0 : _parent.Depth + 1;


        [OnDeserialized]
        private void Deserialized(StreamingContext c)
        {
            _children = _children ?? new List<TreeListNode<T>>();

            foreach (var child in _children)
            {
                child._parent = this;
            }
        }

        public bool ParentContains(TreeListNode<T> target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            return _parent == null ? false : _parent == target ? true : _parent.ParentContains(target);
        }

        public TreeListNode<T> Find(T value)
        {
            return _children.Find(e => EqualityComparer<T>.Default.Equals(e.Value, value));
        }

        public int GetIndex()
        {
            return _parent._children.IndexOf(this);
        }

        public void Add(T value)
        {
            Add(new TreeListNode<T>(value));
        }

        public void Add(TreeListNode<T> node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node._parent != null) throw new InvalidOperationException();

            node._parent = this;
            _children.Add(node);
        }

        public void Insert(int index, TreeListNode<T> node)
        {
            if (index < 0 || index > _children.Count) throw new ArgumentOutOfRangeException(nameof(index));
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node._parent != null) throw new InvalidOperationException();

            node._parent = this;
            _children.Insert(index, node);
        }

        public void Insert(TreeListNode<T> target, int direction, TreeListNode<T> node)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (target.Parent != this) throw new InvalidOperationException();
            if (direction != -1 && direction != +1) throw new ArgumentOutOfRangeException(nameof(direction));

            var index = _children.IndexOf(target);
            if (direction == +1)
            {
                index++;
            }

            Insert(index, node);
        }

        public bool Remove(T value)
        {
            var node = Find(value);
            if (node != null)
            {
                return Remove(node);
            }
            else
            {
                return false;
            }
        }

        public bool Remove(TreeListNode<T> node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node._parent != this) throw new InvalidOperationException();

            var isRemoved = _children.Remove(node);
            if (isRemoved)
            {
                node._parent = null;
            }

            return isRemoved;
        }

        internal bool RemoveSelf()
        {
            if (_parent == null) return false;

            return _parent.Remove(this);
        }

        public void Clear()
        {
            Value = default(T);
            if (_children.Count > 0)
            {
                _children.ForEach(e => e._parent = null);
                _children.Clear();
            }
        }

        public TreeListNode<T> GetPrev()
        {
            if (_parent == null) return null;

            var index = _parent._children.IndexOf(this);
            return _parent.Children.ElementAtOrDefault(index - 1);
        }

        public TreeListNode<T> GetNext()
        {
            if (_parent == null) return null;

            var index = _parent._children.IndexOf(this);
            return _parent.Children.ElementAtOrDefault(index + 1);
        }

        public IEnumerable<TreeListNode<T>> GetExpandedCollection()
        {
            foreach (var child in _children)
            {
                yield return child;

                if (child._isExpanded)
                {
                    foreach (var node in child.GetExpandedCollection())
                    {
                        yield return node;
                    }
                }
            }
        }


        #region IEnumerable support

        public IEnumerator<TreeListNode<T>> GetEnumerator()
        {
            // Note: 自身のインスタンスは含まない

            foreach (var child in _children)
            {
                yield return child;

                var enumerator = child.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }


    public class TreeListNodeMemento<T>
    {
        public TreeListNodeMemento(TreeListNode<T> node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Parent == null) throw new InvalidOperationException("Parent is null.");

            Node = node;
            Parent = node.Parent;
            Index = node.GetIndex();
        }

        public TreeListNode<T> Node { get; private set; }
        public TreeListNode<T> Parent { get; private set; }
        public int Index { get; private set; }
    }

}
