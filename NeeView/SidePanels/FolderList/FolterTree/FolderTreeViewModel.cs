using NeeLaboratory.ComponentModel;
using NeeView.Windows;
using System;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class FolderTreeViewModel : BindableBase
    {
        private bool _isFirstVisibled;

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

        public void MoveQuickAccess(QuickAccessNode src, QuickAccessNode dst)
        {
            Model.MoveQuickAccess(src, dst);
        }

        public void RefreshFolder()
        {
            Model.RefreshDirectory();
        }

        public void IsVisibleChanged(bool isVisible)
        {
            if (isVisible && !_isFirstVisibled)
            {
                _isFirstVisibled = true;
                Model.ExpandRoot();
            }
        }
    }
}
