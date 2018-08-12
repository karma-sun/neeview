using NeeView.Collections;
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
    public class PagemarkFolderNode : FolderTreeNodeBase
    {
        public PagemarkFolderNode(TreeListNode<IPagemarkEntry> source, FolderTreeNodeBase parent)
        {
            Source = source;
            Parent = parent;
        }

        public TreeListNode<IPagemarkEntry> PagemarkSource => (TreeListNode<IPagemarkEntry>)Source;

        public override string Name { get => PagemarkSource.Value.Name; set { } }

        public override string DispName
        {
            get { return PagemarkSource.Value is DefaultPagemarkFolder ? Properties.Resources.WordDefaultPagemark : Name; }
            set { }
        }

        public override ImageSource Icon => FileIconCollection.Current.CreateDefaultFolderIcon(16.0);

        public string Path => Parent is PagemarkFolderNode parent ? LoosePath.Combine(parent.Path, Name) : Name;


        public override ObservableCollection<FolderTreeNodeBase> Children
        {
            get
            {
                if (_children == null)
                {
                    _children = new ObservableCollection<FolderTreeNodeBase>(PagemarkSource.Children
                        .Where(e => e.Value is PagemarkFolder)
                        .OrderBy(e => e.Value, new HasNameComparer())
                        .Select(e => new PagemarkFolderNode(e, this)));
                }
                return _children;
            }
            set
            {
                SetProperty(ref _children, value);
            }
        }

    }

}
