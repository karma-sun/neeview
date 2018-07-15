using NeeLaboratory.ComponentModel;
using NeeView.Windows;
using System;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class FolderTreeViewModel : BindableBase
    {
        public FolderTreeViewModel()
        {
            Model = FolderTreeModel.Current;

            Model.SelectedItemChanged += (s, e) => SelectedItemChanged?.Invoke(s, e);
        }

        public event EventHandler SelectedItemChanged;

        public FolderTreeModel Model { get; set; }


        public void SelectRootQuickAccess()
        {
            Model.SelectRootQuickAccess();
        }

        public void Decide(object item)
        {
            Model.Decide(item);
        }

        public void AddQuickAccess(object item)
        {
            Model.AddQuickAccess(item);
        }

        public void Remove(object item)
        {
            Model.Remove(item);
        }

        public void MoveQuickAccess(QuickAccess src, QuickAccess dst)
        {
            Model.MoveQuickAccess(src, dst);
        }

        internal void RefreshFolder()
        {
            Model.RefreshDirectory();
        }
    }
}
