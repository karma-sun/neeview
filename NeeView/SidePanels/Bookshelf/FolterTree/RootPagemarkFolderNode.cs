using NeeView.Collections;
using NeeView.Collections.Generic;
using System;
using System.Diagnostics;
using System.Windows.Media;

namespace NeeView
{
    public class RootPagemarkFolderNode : PagemarkFolderNode
    {
        public RootPagemarkFolderNode(FolderTreeNodeBase parent) : base(PagemarkCollection.Current.Items, parent)
        {
            PagemarkCollection.Current.PagemarkChanged += PagemarkCollection_PagemarkChanged;
        }


        public override string Name => QueryScheme.Pagemark.ToSchemeString();

        public override string DispName { get => Properties.Resources.WordPagemark; set { } }

        public override ImageSource Icon => MainWindow.Current.Resources["ic_bookmark_24px"] as ImageSource;


        private void PagemarkCollection_PagemarkChanged(object sender, PagemarkCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case EntryCollectionChangedAction.Reset:
                case EntryCollectionChangedAction.Replace:
                    Source = PagemarkCollection.Current.Items;
                    RefreshChildren();
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

        private void Directory_Creaded(TreeListNode<IPagemarkEntry> parent, TreeListNode<IPagemarkEntry> item)
        {
            if (!(item.Value is PagemarkFolder folder))
            {
                return;
            }

            Debug.WriteLine("Create: " + item.CreateQuery(QueryScheme.Pagemark));

            var node = GetDirectoryNode(parent.CreateQuery(QueryScheme.Pagemark));
            if (node != null)
            {
                var newNode = new PagemarkFolderNode(item, null);
                node.Add(newNode);
            }
            else
            {
                Debug.WriteLine("Skip create");
            }
        }

        private void Directory_Deleted(TreeListNode<IPagemarkEntry> parent, TreeListNode<IPagemarkEntry> item)
        {
            if (!(item.Value is PagemarkFolder folder))
            {
                return;
            }

            Debug.WriteLine("Delete: " + item.CreateQuery(QueryScheme.Pagemark));

            var node = GetDirectoryNode(parent.CreateQuery(QueryScheme.Pagemark));
            if (node != null)
            {
                node.Remove(item);
            }
            else
            {
                Debug.WriteLine("Skip delete");
            }
        }

        private void Directory_Renamed(TreeListNode<IPagemarkEntry> parent, TreeListNode<IPagemarkEntry> item)
        {
            if (!(item.Value is PagemarkFolder folder))
            {
                return;
            }

            Debug.WriteLine("Rename: " + item.CreateQuery(QueryScheme.Pagemark));

            var node = GetDirectoryNode(parent.CreateQuery(QueryScheme.Pagemark));
            if (node != null)
            {
                node.Renamed(item);
            }
            else
            {
                Debug.WriteLine("Skip rename");
            }
        }

        private PagemarkFolderNode GetDirectoryNode(QueryPath path)
        {
            return GetDirectoryNode(path.Path);
        }

        private PagemarkFolderNode GetDirectoryNode(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return this;
            }

            return GetFolderTreeNode(path, false, false) as PagemarkFolderNode;
        }

    }
}
