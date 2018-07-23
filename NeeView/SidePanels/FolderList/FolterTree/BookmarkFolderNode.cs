using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NeeView
{
    public class BookmarkFolderNode : FolderTreeNodeBase
    {
        public BookmarkFolderNode(TreeListNode<IBookmarkEntry> source, BookmarkFolderNode parent)
        {
            Source = source;
            Parent = parent;
        }

        public TreeListNode<IBookmarkEntry> BookmarkSource => (TreeListNode<IBookmarkEntry>)Source;

        public override string Name { get => BookmarkSource.Value.Name; set { } }

        public override string DispName { get => Name; set { } }

        public override ImageSource Icon => FileIconCollection.Current.CreateDefaultFolderIcon(16.0);

        public string Path => Parent is BookmarkFolderNode parent ? LoosePath.Combine(parent.Path, Name) : Name;


        public override ObservableCollection<FolderTreeNodeBase> Children
        {
            get
            {
                if (_children == null)
                {
                    _children = new ObservableCollection<FolderTreeNodeBase>(BookmarkSource.Children
                        .Where(e => e.Value is BookmarkFolder)
                        .OrderBy(e => e.Value, new NameComparer())
                        .Select(e => new BookmarkFolderNode(e, this)));
                }
                return _children;
            }
            set
            {
                SetProperty(ref _children, value);
            }
        }

        private class NameComparer : IComparer<IHasName>
        {
            public int Compare(IHasName x, IHasName y)
            {
                return NativeMethods.StrCmpLogicalW(x.Name, y.Name);
            }
        }
    }

}
