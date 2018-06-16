using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using NeeView.Windows;
using System;
using System.ComponentModel;

namespace NeeView
{
    public class PagemarkListBoxViewModel : BindableBase
    {
        public PagemarkListBoxViewModel(PagemarkListBoxModel model)
        {
            Model = model;
        }

        public event CollectionChangeEventHandler Changing;
        public event CollectionChangeEventHandler Changed;


        public PagemarkListBoxModel Model { get; private set; }


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


        public void Decide(TreeListNode<IPagemarkEntry> item)
        {
            Model.Decide(item);
        }

        public void Expand(TreeListNode<IPagemarkEntry> item, bool isExpanded)
        {
            Model.Expand(item, isExpanded);
        }

        public void Remove(TreeListNode<IPagemarkEntry> item)
        {
            Model.Remove(item);
        }

        public void Move(DropInfo<TreeListNode<IPagemarkEntry>> dropInfo)
        {
            Model.Move(dropInfo);
        }
    }
}
