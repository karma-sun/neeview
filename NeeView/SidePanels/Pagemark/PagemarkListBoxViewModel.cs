using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using NeeView.Windows;
using System;

namespace NeeView
{
    public class PagemarkListBoxViewModel : BindableBase
    {
        public PagemarkListBoxViewModel(PagemarkListBoxModel model)
        {
            Model = model;
        }

        public event EventHandler Changing;
        public event EventHandler Changed;


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

        private void Model_Changing(object sender, EventArgs e)
        {
            Changing?.Invoke(sender, e);
        }

        private void Model_Changed(object sender, EventArgs e)
        {
            Changed?.Invoke(sender, e);
        }


        public void Decide(TreeListNode<IPagemarkEntry> item)
        {
            Model.Decide(item);
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
