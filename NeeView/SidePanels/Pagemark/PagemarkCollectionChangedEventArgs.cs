using NeeView.Collections.Generic;
using System;
using System.Collections.Specialized;

namespace NeeView
{
    public class PagemarkCollectionChangedEventArgs : EventArgs
    {
        public NotifyCollectionChangedAction Action { get; set; }
        public TreeListNode<IPagemarkEntry> Item { get; set; }

        public PagemarkCollectionChangedEventArgs(NotifyCollectionChangedAction action)
        {
            Action = action;
        }

        public PagemarkCollectionChangedEventArgs(NotifyCollectionChangedAction action, TreeListNode<IPagemarkEntry> item)
        {
            Action = action;
            Item = item;
        }
    }

}
