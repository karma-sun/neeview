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
        private FolderList _model;
        private bool _isRenaming;


        public FolderListBoxViewModel(FolderList folderList)
        {
            _model = folderList;
            _model.BusyChanged += (s, e) => BusyChanged?.Invoke(s, e);
            _model.PropertyChanged += Model_PropertyChanged;
            _model.SelectedChanging += Model_SelectedChanging;
            _model.SelectedChanged += Model_SelectedChanged;
        }


        public event EventHandler<FolderListBusyChangedEventArgs> BusyChanged;
        public event EventHandler<FolderListSelectedChangedEventArgs> SelectedChanging;
        public event EventHandler<FolderListSelectedChangedEventArgs> SelectedChanged;


        public FolderCollection FolderCollection => _model.FolderCollection;

        public FolderOrder FolderOrder => _model.FolderCollection.FolderOrder;

        public bool IsFocusAtOnce
        {
            get => _model.IsFocusAtOnce;
            set => _model.IsFocusAtOnce = value;
        }

        public FolderList Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        // サムネイルが表示されている？
        public bool IsThumbnailVisibled => _model.IsThumbnailVisibled;

        public bool IsRenaming
        {
            get { return _isRenaming; }
            set { SetProperty(ref _isRenaming, value); }
        }


        #region RelayCommands

        private RelayCommand _toggleFolderRecursive;
        private RelayCommand _newFolderCommand;


        public RelayCommand ToggleFolderRecursive
        {
            get { return _toggleFolderRecursive = _toggleFolderRecursive ?? new RelayCommand(_model.ToggleFolderRecursive_Executed); }
        }

        // HACK: 未使用？
        public RelayCommand NewFolderCommand
        {
            get
            {
                return _newFolderCommand = _newFolderCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    _model.NewFolder();
                }
            }
        }

        #endregion RelayCommands


        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case null:
                case "":
                    RaisePropertyChanged(null);
                    break;

                case nameof(FolderList.FolderCollection):
                    RaisePropertyChanged(nameof(FolderCollection));
                    RaisePropertyChanged(nameof(FolderOrder));
                    break;

                case nameof(FolderList.IsFocusAtOnce):
                    RaisePropertyChanged(nameof(IsFocusAtOnce));
                    break;
            }
        }

        private void Model_SelectedChanging(object sender, FolderListSelectedChangedEventArgs e)
        {
            SelectedChanging?.Invoke(sender, e);
        }

        private void Model_SelectedChanged(object sender, FolderListSelectedChangedEventArgs e)
        {
            SelectedChanged?.Invoke(sender, e);
        }

        public bool IsLRKeyEnabled()
        {
            return Config.Current.Panels.IsLeftRightKeyEnabled && _model.FolderListConfig.PanelListItemStyle != PanelListItemStyle.Thumbnail;
        }

        public void MoveToHome()
        {
            _model.MoveToHome();
        }

        public void MoveToUp()
        {
            _model.MoveToParent();
        }

        /// <summary>
        /// 可能な場合のみ、フォルダー移動
        /// </summary>
        /// <param name="item"></param>
        public void MoveToSafety(FolderItem item)
        {
            if (item != null && item.CanOpenFolder())
            {
                _model.MoveTo(item.TargetPath);
            }
        }

        public void MoveToPrevious()
        {
            _model.MoveToPrevious();
        }

        public void MoveToNext()
        {
            _model.MoveToNext();
        }

        public void IsVisibleChanged(bool isVisible)
        {
            _model.IsVisibleChanged(isVisible);
        }

        public async Task RemoveAsync(IEnumerable<FolderItem> items)
        {
            if (items == null) return;

            await Model.RemoveAsync(items);
        }
    }
}
