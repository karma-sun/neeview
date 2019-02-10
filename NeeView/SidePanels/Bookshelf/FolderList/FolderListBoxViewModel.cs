using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    public class FolderListBoxViewModel : BindableBase
    {
        public FolderListBoxViewModel(FolderList folderList, FolderListBoxModel model)
        {
            _folderList = folderList;
            _model = model;
        }

        public event EventHandler<SelectedChangedEventArgs> SelectedChanging;
        public event EventHandler<SelectedChangedEventArgs> SelectedChanged;


        public SidePanelProfile Profile => SidePanelProfile.Current;
        public FolderCollection FolderCollection => _model.FolderCollection;
        public FolderOrder FolderOrder => _model.FolderCollection.FolderOrder;
        public string PlaceRaw => _model.FolderCollection?.Place.SimplePath;

        private FolderList _folderList;
        public FolderList FolderList
        {
            get { return _folderList; }
            set { SetProperty(ref _folderList, value); }
        }

        private FolderListBoxModel _model;
        public FolderListBoxModel Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        // サムネイルが表示されている？
        public bool IsThumbnailVisibled => _folderList.IsThumbnailVisibled;

        public bool IsRenaming
        {
            get => _folderList.IsRenaming;
            set => _folderList.IsRenaming = value;
        }


        #region RelayCommands

        /// <summary>
        /// ToggleFolderRecursive command.
        /// </summary>
        private RelayCommand _ToggleFolderRecursive;
        public RelayCommand ToggleFolderRecursive
        {
            get { return _ToggleFolderRecursive = _ToggleFolderRecursive ?? new RelayCommand(_model.ToggleFolderRecursive_Executed); }
        }

        // HACK: 未使用？
        private RelayCommand _NewFolderCommand;
        public RelayCommand NewFolderCommand
        {
            get { return _NewFolderCommand = _NewFolderCommand ?? new RelayCommand(NewFolderCommand_Executed); }
        }

        private void NewFolderCommand_Executed()
        {
            _model.NewFolder();
        }

        #endregion RelayCommands


        public void Loaded()
        {
            _model.Loaded();
            _model.SelectedChanging += Model_SelectedChanging;
            _model.SelectedChanged += Model_SelectedChanged;
        }

        public void Unloaded()
        {
            _model.Unloaded();
            _model.SelectedChanging -= Model_SelectedChanging;
            _model.SelectedChanged -= Model_SelectedChanged;
        }

        private void Model_SelectedChanging(object sender, SelectedChangedEventArgs e)
        {
            SelectedChanging?.Invoke(sender, e);
        }

        private void Model_SelectedChanged(object sender, SelectedChangedEventArgs e)
        {
            SelectedChanged?.Invoke(sender, e);
        }

        public bool IsLRKeyEnabled()
        {
            return SidePanelProfile.Current.IsLeftRightKeyEnabled && _folderList.PanelListItemStyle != PanelListItemStyle.Thumbnail;
        }

        public void MoveToHome()
        {
            _folderList.MoveToHome();
        }

        public void MoveToUp()
        {
            _folderList.MoveToParent();
        }

        /// <summary>
        /// 可能な場合のみ、フォルダー移動
        /// </summary>
        /// <param name="item"></param>
        public void MoveToSafety(FolderItem item)
        {
            if (item != null && item.CanOpenFolder())
            {
                _folderList.MoveTo(item.TargetPath);
            }
        }

        public void MoveToPrevious()
        {
            _folderList.MoveToPrevious();
        }

        public void MoveToNext()
        {
            _folderList.MoveToNext();
        }

        public void IsVisibleChanged(bool isVisible)
        {
            _folderList.IsVisibleChanged(isVisible);
        }
    }
}
