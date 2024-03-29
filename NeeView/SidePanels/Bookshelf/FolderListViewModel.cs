﻿using Microsoft.Win32;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// FolderList : ViewModel
    /// </summary>
    public class FolderListViewModel : BindableBase
    {
        private BookshelfFolderList _model;
        private Dictionary<FolderOrder, string> _folderOrderList = AliasNameExtensions.GetAliasNameDictionary<FolderOrder>();
        private double _dpi = 1.0;


        public FolderListViewModel(BookshelfFolderList model)
        {
            _model = model;

            _model.History.Changed +=
                (s, e) => UpdateCommandCanExecute();

            _model.PlaceChanged +=
                (s, e) => MoveToUp.RaiseCanExecuteChanged();

            _model.CollectionChanged +=
                Model_CollectionChanged;

            MoreMenuDescription = new FolderListMoreMenuDescription(this);
        }



        public FolderCollection FolderCollection => _model.FolderCollection;


        public BookshelfFolderList Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// コンボボックス用リスト
        /// </summary>
        public Dictionary<FolderOrder, string> FolderOrderList
        {
            get { return _folderOrderList; }
            set { SetProperty(ref _folderOrderList, value); }
        }

        public FolderOrder FolderOrder
        {
            get { return FolderCollection != null ? FolderCollection.FolderParameter.FolderOrder : default; }
            set { if (FolderCollection != null) { FolderCollection.FolderParameter.FolderOrder = value; } }
        }

        public double Dpi
        {
            get { return _dpi; }
            set { SetProperty(ref _dpi, value); }
        }


        #region Commands

        private RelayCommand _setHome;
        private RelayCommand _moveToHome;
        private RelayCommand<QueryPath> _moveTo;
        private RelayCommand _moveToPrevious;
        private RelayCommand _moveToNext;
        private RelayCommand<KeyValuePair<int, QueryPath>> _moveToHistory;
        private RelayCommand _moveToUp;
        private RelayCommand _sync;
        private RelayCommand _toggleFolderRecursive;
        private RelayCommand _search;
        private RelayCommand _clearSearch;
        private RelayCommand _addQuickAccess;
        private RelayCommand<FolderTreeLayout> _setFolderTreeLayout;
        private RelayCommand _newFolderCommand;
        private RelayCommand _addBookmarkCommand;
        private RelayCommand<PanelListItemStyle> _setListItemStyle;
        private RelayCommand _toggleVisibleFoldersTree;

        public RelayCommand ToggleVisibleFoldersTree
        {
            get { return _toggleVisibleFoldersTree = _toggleVisibleFoldersTree ?? new RelayCommand(_model.ToggleVisibleFoldersTree); }
        }

        public RelayCommand SetHome
        {
            get { return _setHome = _setHome ?? new RelayCommand(_model.SetHome, _model.CanSetHome); }
        }

        public RelayCommand MoveToHome
        {
            get { return _moveToHome = _moveToHome ?? new RelayCommand(_model.MoveToHome); }
        }

        public RelayCommand<QueryPath> MoveTo
        {
            get { return _moveTo = _moveTo ?? new RelayCommand<QueryPath>(_model.MoveTo); }
        }

        public RelayCommand MoveToPrevious
        {
            get { return _moveToPrevious = _moveToPrevious ?? new RelayCommand(_model.MoveToPrevious, _model.CanMoveToPrevious); }
        }

        public RelayCommand MoveToNext
        {
            get { return _moveToNext = _moveToNext ?? new RelayCommand(_model.MoveToNext, _model.CanMoveToNext); }
        }

        public RelayCommand<KeyValuePair<int, QueryPath>> MoveToHistory
        {
            get { return _moveToHistory = _moveToHistory ?? new RelayCommand<KeyValuePair<int, QueryPath>>(_model.MoveToHistory); }
        }

        public RelayCommand MoveToUp
        {
            get { return _moveToUp = _moveToUp ?? new RelayCommand(_model.MoveToParent, _model.CanMoveToParent); }
        }

        public RelayCommand Sync
        {
            get { return _sync = _sync ?? new RelayCommand(_model.Sync); }
        }

        public RelayCommand ToggleFolderRecursive
        {
            get { return _toggleFolderRecursive = _toggleFolderRecursive ?? new RelayCommand(_model.ToggleFolderRecursive); }
        }

        public RelayCommand Search
        {
            get
            {
                return _search = _search ?? new RelayCommand(Execute);

                void Execute()
                {
                    _model.RequestSearchPlace(true);
                }
            }
        }

        public RelayCommand ClearSearch
        {
            get
            {
                return _clearSearch = _clearSearch ?? new RelayCommand(Execute);

                void Execute()
                {
                    _model.InputKeyword = "";
                    _model.SetSearchKeywordAndSearch("");
                }
            }
        }

        public RelayCommand AddQuickAccess
        {
            get
            {
                return _addQuickAccess = _addQuickAccess ?? new RelayCommand(Execute, CanExecute);

                bool CanExecute()
                {
                    return _model.Place != null;
                }

                void Execute()
                {
                    _model.AddQuickAccess();
                }
            }
        }

        public RelayCommand<FolderTreeLayout> SetFolderTreeLayout
        {
            get
            {
                return _setFolderTreeLayout = _setFolderTreeLayout ?? new RelayCommand<FolderTreeLayout>(Execute);

                void Execute(FolderTreeLayout layout)
                {
                    _model.FolderListConfig.FolderTreeLayout = layout;
                    SidePanelFrame.Current.SetVisibleBookshelfFolderTree(true, true);
                }
            }
        }

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

        public RelayCommand AddBookmarkCommand
        {
            get
            {
                return _addBookmarkCommand = _addBookmarkCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    _model.AddBookmark();
                }

            }
        }

        public RelayCommand<PanelListItemStyle> SetListItemStyle
        {
            get
            {
                return _setListItemStyle = _setListItemStyle ?? new RelayCommand<PanelListItemStyle>(Execute);

                void Execute(PanelListItemStyle style)
                {
                    _model.FolderListConfig.PanelListItemStyle = style;
                }
            }
        }


        /// <summary>
        /// コマンド実行可能状態を更新
        /// </summary>
        private void UpdateCommandCanExecute()
        {
            this.MoveToPrevious.RaiseCanExecuteChanged();
            this.MoveToNext.RaiseCanExecuteChanged();
        }

        #endregion Commands

        #region MoreMenu

        public FolderListMoreMenuDescription MoreMenuDescription { get; }

        public class FolderListMoreMenuDescription : ItemsListMoreMenuDescription
        {
            private FolderListViewModel _vm;

            public FolderListMoreMenuDescription(FolderListViewModel vm)
            {
                _vm = vm;
            }

            public override ContextMenu Create()
            {
                return Update(new ContextMenu());
            }

            public override ContextMenu Update(ContextMenu menu)
            {
                var items = menu.Items;

                items.Clear();
                items.Add(CreateListItemStyleMenuItem(Properties.Resources.Word_StyleList, PanelListItemStyle.Normal));
                items.Add(CreateListItemStyleMenuItem(Properties.Resources.Word_StyleContent, PanelListItemStyle.Content));
                items.Add(CreateListItemStyleMenuItem(Properties.Resources.Word_StyleBanner, PanelListItemStyle.Banner));
                items.Add(CreateListItemStyleMenuItem(Properties.Resources.Word_StyleThumbnail, PanelListItemStyle.Thumbnail));
                items.Add(new Separator());
                items.Add(CreateCommandMenuItem(Properties.Resources.Bookshelf_MoreMenu_AddQuickAccess, _vm.AddQuickAccess));
                items.Add(CreateCommandMenuItem(Properties.Resources.Bookshelf_MoreMenu_ClearHistory, "ClearHistoryInPlace"));

                switch (_vm._model.FolderCollection)
                {
                    case FolderEntryCollection folderEntryCollection:
                        items.Add(new Separator());
                        items.Add(CreateCommandMenuItem(Properties.Resources.Bookshelf_MoreMenu_Subfolder, _vm.ToggleFolderRecursive, new Binding("FolderCollection.FolderParameter.IsFolderRecursive") { Source = _vm._model }));
                        break;

                    case FolderArchiveCollection folderArchiveCollection:
                        break;

                    case FolderSearchCollection folderSearchCollection:
                        break;

                    case BookmarkFolderCollection bookmarFolderCollection:
                        items.Add(new Separator());
                        items.Add(CreateCommandMenuItem(Properties.Resources.Word_NewFolder, _vm.NewFolderCommand));
                        items.Add(CreateCommandMenuItem(Properties.Resources.FolderTree_Menu_AddBookmark, _vm.AddBookmarkCommand));
                        break;
                }

                if (_vm._model.IsFolderSearchEnabled)
                {
                    var subItem = new MenuItem() { Header = Properties.Resources.Bookshelf_MoreMenu_SearchOptions };
                    subItem.Items.Add(CreateCheckMenuItem(Properties.Resources.Bookshelf_MoreMenu_SearchIncremental, new Binding(nameof(BookshelfConfig.IsIncrementalSearchEnabled)) { Source = Config.Current.Bookshelf }));
                    subItem.Items.Add(CreateCheckMenuItem(Properties.Resources.Bookshelf_MoreMenu_SearchIncludeSubdirectories, new Binding(nameof(BookshelfConfig.IsSearchIncludeSubdirectories)) { Source = Config.Current.Bookshelf }));
                    items.Add(new Separator());
                    items.Add(subItem);
                }

                return menu;
            }

            private MenuItem CreateListItemStyleMenuItem(string header, PanelListItemStyle style)
            {
                return CreateListItemStyleMenuItem(header, _vm.SetListItemStyle, style, _vm._model.FolderListConfig);
            }
        }

        #endregion MoreMenu


        /// <summary>
        /// Model CollectionChanged event
        /// </summary>
        private void Model_CollectionChanged(object sender, EventArgs e)
        {
            UpdateFolderOrerList();
            RaisePropertyChanged(nameof(FolderCollection));
        }

        /// <summary>
        /// 履歴取得
        /// </summary>
        internal List<KeyValuePair<int, QueryPath>> GetHistory(int direction, int size)
        {
            return _model.History.GetHistory(direction, size);
        }

        /// <summary>
        /// 並び順リスト更新
        /// </summary>
        public void UpdateFolderOrerList()
        {
            FolderOrderList = FolderCollection.FolderOrderClass.GetFolderOrderMap();
            RaisePropertyChanged(nameof(FolderOrder));
        }

        /// <summary>
        /// 検索履歴更新
        /// </summary>
        public void UpdateSearchHistory()
        {
            _model.UpdateSearchHistory();
        }

        /// <summary>
        /// 可能な場合のみ、フォルダー移動
        /// </summary>
        /// <param name="folderInfo"></param>
        public void MoveToSafety(FolderItem folderInfo)
        {
            if (folderInfo != null && folderInfo.CanOpenFolder())
            {
                _model.MoveTo(folderInfo.TargetPath);
            }
        }

        public bool IsLRKeyEnabled()
        {
            return Config.Current.Panels.IsLeftRightKeyEnabled || _model.PanelListItemStyle == PanelListItemStyle.Thumbnail;
        }
    }
}
