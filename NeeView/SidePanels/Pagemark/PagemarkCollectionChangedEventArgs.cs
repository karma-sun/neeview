using NeeView.Collections;
using NeeView.Collections.Generic;
using System;
using System.Collections.Specialized;

namespace NeeView
{
    public class PagemarkCollectionChangedEventArgs : EventArgs
    {
        public EntryCollectionChangedAction Action { get; set; }
        public TreeListNode<IPagemarkEntry> Parent { get; set; }
        public TreeListNode<IPagemarkEntry> Item { get; set; }

        public int OldIndex { get; set; } = -1;
        public string OldName { get; set; }

        public PagemarkCollectionChangedEventArgs(EntryCollectionChangedAction action)
        {
            Action = action;
        }

        public PagemarkCollectionChangedEventArgs(EntryCollectionChangedAction action, TreeListNode<IPagemarkEntry> parent, TreeListNode<IPagemarkEntry> item)
        {
            Action = action;
            Parent = parent;
            Item = item;
        }
    }

}
