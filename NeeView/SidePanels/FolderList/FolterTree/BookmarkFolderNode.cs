using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class BookmarkFolderNode : DirectoryNodeBase
    {
        internal static partial class NativeMethods
        {
            [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
            public static extern int StrCmpLogicalW(string psz1, string psz2);
        }

        private BookmarkFolderNode _parent;
        protected TreeListNode<IBookmarkEntry> _source;

        public BookmarkFolderNode(BookmarkFolderNode parent, TreeListNode<IBookmarkEntry> source)
        {
            _parent = parent;
            _source = source;
        }

        public TreeListNode<IBookmarkEntry> Source => _source;

        public BookmarkFolderNode Parent
        {
            get { return _parent; }
            set { SetProperty(ref _parent, value); }
        }

        public virtual string Name => _source.Value.Name;
        public override string Key => Name;

        public string Path => _parent != null ? LoosePath.Combine(_parent.Path, Name) : Name;
        public string KeyPath => _parent != null ? LoosePath.Combine(_parent.Path, Key) : Key;

        public override void RefreshChildren(bool isForce)
        {
            Children = new ObservableCollection<IFolderTreeNode>(_source.Children
                .Where(e => e.Value is BookmarkFolder)
                .OrderBy(e => e.Value, new ComparerFileName())
                .Select(e => new BookmarkFolderNode(this, e)));
        }

        public class ComparerFileName : IComparer<IHasName>
        {
            public int Compare(IHasName x, IHasName y)
            {
                return NativeMethods.StrCmpLogicalW(x.Name, y.Name);
            }
        }

        public void Add(TreeListNode<IBookmarkEntry> source)
        {
            if (_children == null) return;

            ////Debug.WriteLine($"ADD: " + name);

            var directory = _children.Cast<BookmarkFolderNode>().FirstOrDefault(e => e.Name == source.Value.Name);
            if (directory == null)
            {
                directory = new BookmarkFolderNode(this, source);
                _children.Add(directory);
                Sort(directory);
            }
        }

        public void Remove(TreeListNode<IBookmarkEntry> source)
        {
            if (_children == null) return;

            ////Debug.WriteLine($"REMOVE: " + name);

            var directory = _children.Cast<BookmarkFolderNode>().FirstOrDefault(e => e.Name == source.Value.Name);
            if (directory != null)
            {
                _children.Remove(directory);
                directory.Parent = null;
            }
        }

        public void Rename(TreeListNode<IBookmarkEntry> item)
        {
            if (_children == null) return;

            ////Debug.WriteLine($"RENAME: " + oldName + " -> " + name);

            var directory = _children.Cast<BookmarkFolderNode>().FirstOrDefault(e => e.Source == item);
            if (directory != null)
            {
                directory.RaisePropertyChanged(nameof(Name));
                Sort(directory);
            }
        }

        /// <summary>
        /// 指定した子を適切な位置に並び替える
        /// </summary>
        /// <param name="child"></param>
        private void Sort(BookmarkFolderNode child)
        {
            if (_children == null) return;

            var oldIndex = _children.IndexOf(child);
            if (oldIndex < 0) return;

            for (int index = 0; index < _children.Count; ++index)
            {
                var directory = (BookmarkFolderNode)_children[index];
                if (directory == child) continue;

                if (NativeMethods.StrCmpLogicalW(child.Name, directory.Name) < 0)
                {
                    if (oldIndex != index - 1)
                    {
                        _children.Move(oldIndex, index);
                    }
                    return;
                }
            }

        }
    }


    public class RootBookmarkFolderNode : BookmarkFolderNode
    {
        public override string Key => "";

        public override string Name => Bookmark.Scheme + "\\";

        public string SchemeName => "@WordBookmark";

        public RootBookmarkFolderNode() : base(null, BookmarkCollection.Current.Items)
        {
            BookmarkCollection.Current.BookmarkChanged += BookmarkCollection_BookmarkChanged;
        }

        private void BookmarkCollection_BookmarkChanged(object sender, BookmarkCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case EntryCollectionChangedAction.Reset:
                case EntryCollectionChangedAction.Replace:
                    _source = BookmarkCollection.Current.Items;
                    RefreshChildren(true);
                    RaisePropertyChanged(nameof(Children));
                    break;

                case EntryCollectionChangedAction.Add:
                    Directory_Creaded(e.Parent, e.Item);
                    break;

                case EntryCollectionChangedAction.Remove:
                    Directory_Deleted(e.Parent, e.Item);
                    break;

                case EntryCollectionChangedAction.Rename:
                    Directory_Renamed(e.Parent, e.Item);
                    break;

            }
        }

        private void Directory_Creaded(TreeListNode<IBookmarkEntry> parent, TreeListNode<IBookmarkEntry> item)
        {
            if (!(item.Value is BookmarkFolder folder))
            {
                return;
            }

            Debug.WriteLine("Create: " + item.CreatePath(null));

            var node = GetDirectoryNode(parent.CreatePath(null));
            if (node != null)
            {
                App.Current.Dispatcher.BeginInvoke((Action)(() => node.Add(item)));
            }
            else
            {
                Debug.WriteLine("Skip create");
            }
        }

        private void Directory_Deleted(TreeListNode<IBookmarkEntry> parent, TreeListNode<IBookmarkEntry> item)
        {
            if (!(item.Value is BookmarkFolder folder))
            {
                return;
            }

            Debug.WriteLine("Delete: " + item.CreatePath(null));

            var node = GetDirectoryNode(parent.CreatePath(null));
            if (node != null)
            {
                App.Current.Dispatcher.BeginInvoke((Action)(() => node.Remove(item)));
            }
            else
            {
                Debug.WriteLine("Skip delete");
            }
        }

        private void Directory_Renamed(TreeListNode<IBookmarkEntry> parent, TreeListNode<IBookmarkEntry> item)
        {
            if (!(item.Value is BookmarkFolder folder))
            {
                return;
            }

            Debug.WriteLine("Rename: " + item.CreatePath(null));

            var node = GetDirectoryNode(parent.CreatePath(null));
            if (node != null)
            {
                App.Current.Dispatcher.BeginInvoke((Action)(() => node.Rename(item)));
            }
            else
            {
                Debug.WriteLine("Skip rename");
            }
        }

        private BookmarkFolderNode GetDirectoryNode(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return this;
            }

            return GetDirectoryNode(path, false, false) as BookmarkFolderNode;
        }


    }

}
