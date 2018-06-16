using System;
using System.ComponentModel;
using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using NeeView.Windows;

namespace NeeView
{
    public class BookmarkListBoxViewModel : BindableBase
    {
        public BookmarkListBoxViewModel(BookmarkListBoxModel model)
        {
            Model = model;
        }

        public event CollectionChangeEventHandler Changing;
        public event CollectionChangeEventHandler Changed;


        public BookmarkListBoxModel Model { get; private set; }


        internal void Loaded()
        {
            Model.Changing += Model_Changing;
            Model.Changed += Model_Changed;
        }

        internal void Unloaded()
        {
            Model.Changing -= Model_Changing;
            Model.Changed -= Model_Changed;
        }

        private void Model_Changing(object sender, CollectionChangeEventArgs e)
        {
            Changing?.Invoke(sender, e);
        }

        private void Model_Changed(object sender, CollectionChangeEventArgs e)
        {
            Changed?.Invoke(sender, e);
        }


        public void Decide(TreeListNode<IBookmarkEntry> item)
        {
            Model.Decide(item);
        }

        public void Expand(TreeListNode<IBookmarkEntry> item, bool isExpanded)
        {
            Model.Expand(item, isExpanded);
        }

        public void Remove(TreeListNode<IBookmarkEntry> item)
        {
            Model.Remove(item);
        }

        public void Move(DropInfo<TreeListNode<IBookmarkEntry>> dropInfo)
        {
            Model.Move(dropInfo);
        }

    }
}
