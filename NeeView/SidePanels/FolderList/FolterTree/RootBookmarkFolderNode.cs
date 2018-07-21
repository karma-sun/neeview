using NeeView.Collections.Generic;
using System;
using System.Diagnostics;
using System.Windows.Media;

namespace NeeView
{
    public class RootBookmarkFolderNode : BookmarkFolderNode
    {
        public override string DispName { get => Properties.Resources.WordBookmark; set { } }

        public override ImageSource Icon => App.Current.Resources["ic_grade_24px"] as ImageSource;

        public override string Key => "";

        public override string Name => Bookmark.Scheme + "\\";

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
                ////App.Current.Dispatcher.BeginInvoke((Action)(() => node.Add(item)));
                node.Add(item);
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
