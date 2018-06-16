using NeeView.Collections.Generic;
using System;
using System.Collections.Specialized;

namespace NeeView
{
    public class BookmarkCollectionChangedEventArgs : EventArgs
    {
        public NotifyCollectionChangedAction Action { get; set; }
        public TreeListNode<IBookmarkEntry> Item { get; set; }

        public BookmarkCollectionChangedEventArgs(NotifyCollectionChangedAction action)
        {
            Action = action;
        }

        public BookmarkCollectionChangedEventArgs(NotifyCollectionChangedAction action, TreeListNode<IBookmarkEntry> item)
        {
            Action = action;
            Item = item;
        }
    }
}
