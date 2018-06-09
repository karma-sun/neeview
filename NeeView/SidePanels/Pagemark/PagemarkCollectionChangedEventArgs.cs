using System;
using System.Collections.Specialized;

namespace NeeView
{
    public class PagemarkCollectionChangedEventArgs : EventArgs
    {
        public NotifyCollectionChangedAction Action { get; set; }
        public IPagemarkEntry Item { get; set; }

        public PagemarkCollectionChangedEventArgs(NotifyCollectionChangedAction action)
        {
            Action = action;
        }

        public PagemarkCollectionChangedEventArgs(NotifyCollectionChangedAction action, IPagemarkEntry item)
        {
            Action = action;
            Item = item;
        }
    }

}
