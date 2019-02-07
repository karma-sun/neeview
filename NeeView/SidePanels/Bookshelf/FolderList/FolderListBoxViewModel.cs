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
    }
}
