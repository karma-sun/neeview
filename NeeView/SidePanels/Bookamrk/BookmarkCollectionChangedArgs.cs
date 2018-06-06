using System;
using System.Collections.Specialized;

namespace NeeView
{
    public class BookmarkCollectionChangedEventArgs : EventArgs
    {
        public NotifyCollectionChangedAction Action { get; set; }
        public IBookmarkEntry Item { get; set; }

        public BookmarkCollectionChangedEventArgs(NotifyCollectionChangedAction action)
        {
            Action = action;
        }

        public BookmarkCollectionChangedEventArgs(NotifyCollectionChangedAction action, IBookmarkEntry item)
        {
            Action = action;
            Item = item;
        }
    }
}
