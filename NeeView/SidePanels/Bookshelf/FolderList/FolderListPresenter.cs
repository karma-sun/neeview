using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace NeeView
{
    public interface IHasFolderListBox
    {
        void SetFolderListBoxContent(FolderListBox content);
    }

    public class FolderListPresenter
    {
        private IHasFolderListBox _folderListView;
        private FolderList _folderList;

        private FolderListBox _folderListBox;
        private FolderListBoxViewModel _folderListBoxViewModel;


        public FolderListPresenter(IHasFolderListBox folderListView, FolderList folderList)
        {
            _folderListView = folderListView;
            _folderList = folderList;
            _folderList.FolderListConfig.AddPropertyChanged(nameof(FolderListConfig.PanelListItemStyle), (s, e) => UpdateFolderListBox());

            _folderListBoxViewModel = new FolderListBoxViewModel(folderList);
            UpdateFolderListBox();
        }


        protected FolderListBox FolderListBox => _folderListBox;


        public void UpdateFolderListBox()
        {
            _folderListBox = new FolderListBox(_folderListBoxViewModel);
            _folderListView.SetFolderListBoxContent(_folderListBox);
        }

        public void Refresh()
        {
            _folderListBox?.Refresh();
        }

        public void FocusAtOnce()
        {
            _folderList.FocusAtOnce();
            _folderListBox?.FocusSelectedItem(false);
        }
    }
}
