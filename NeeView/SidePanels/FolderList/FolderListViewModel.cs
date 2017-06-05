﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using NeeView.Windows.Input;
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
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// Messanger: MoveFolderメッセージのパラメータ
    /// </summary>
    public class MoveFolderParams
    {
        public int Distance { get; set; }
        public BookLoadOption BookLoadOption { get; set; }
    }

    /// <summary>
    /// Messenger: FolderOrderメッセージのパラメータ
    /// </summary>
    public class FolderOrderParams
    {
        public FolderOrder FolderOrder { get; set; }
    }

    [Flags]
    public enum FolderSetPlaceOption
    {
        None,
        IsFocus = (1 << 0),
        IsUpdateHistory = (1 << 1),
        IsTopSelect = (1 << 3),
    }

    /// <summary>
    /// FolderList : ViewModel
    /// </summary>
    public class FolderListViewModel : BindableBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public FolderListViewModel(FolderList model)
        {
            _model = model;

            _model.History.Changed +=
                (s, e) => UpdateCommandCanExecute();

            _model.PropertyChanged +=
                Model_PropertyChanged;

            _model.PlaceChanged +=
                (s, e) => MoveToUp.RaiseCanExecuteChanged();

            _model.SelectedChanged +=
                (s, e) => this.ListContent?.FocusSelectedItem(e.IsFocus);

            _model.CollectionChanged +=
                Model_CollectionChanged;

            InitializeMoreMenu(_model.FolderPanel);
            InitializeDragStart();

            UpdateListContent();
        }

        //
        internal void IsVisibleChanged(bool isVisibled)
        {
            if (isVisibled)
            {
                this.ListContent?.FocusSelectedItem(true);
            }
        }


        //
        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_model.IsVisibleHistoryMark):
                    FolderItem.IsVisibleHistoryMark = _model.IsVisibleHistoryMark;
                    break;
                case nameof(_model.IsVisibleBookmarkMark):
                    FolderItem.IsVisibleBookmarkMark = _model.IsVisibleBookmarkMark;
                    break;
                case nameof(_model.FolderIconLayout):
                case nameof(_model.PanelListItemStyle):
                    UpdateListContent();
                    break;
            }
        }

        //
        private void Model_CollectionChanged(object sender, EventArgs e)
        {
            UpdateListContent();
            RaisePropertyChanged(nameof(FolderCollection));
            RaisePropertyChanged(nameof(IsFolderRecursive));
            RaisePropertyChanged(nameof(Place));
        }

        //
        public FolderCollection FolderCollection => _model.FolderCollection;
        public bool IsFolderRecursive => _model.FolderCollection != null ? _model.FolderCollection.FolderParameter.IsFolderRecursive : false;
        public string Place => _model.FolderCollection?.PlaceDispString;

        /// <summary>
        /// Model property.
        /// </summary>
        public FolderList Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        //
        private FolderList _model;

        /// <summary>
        /// コンボボックス用リスト
        /// </summary>
        public Dictionary<FolderOrder, string> FolderOrderList => FolderOrderExtension.FolderOrderList;


        #region MoreMenu

        /// <summary>
        /// MoreMenu property.
        /// </summary>
        public ContextMenu MoreMenu
        {
            get { return _MoreMenu; }
            set { if (_MoreMenu != value) { _MoreMenu = value; RaisePropertyChanged(); } }
        }

        //
        private ContextMenu _MoreMenu;


        //
        private void InitializeMoreMenu(FolderPanelModel source)
        {
            var menu = new ContextMenu();
            menu.Items.Add(CreateCommandMenuItem("ページリスト", CommandType.ToggleVisiblePageList, source));
            menu.Items.Add(new Separator());
            menu.Items.Add(CreateListItemStyleMenuItem("一覧表示", PanelListItemStyle.Normal));
            menu.Items.Add(CreateListItemStyleMenuItem("コンテンツ表示", PanelListItemStyle.Content));
            menu.Items.Add(CreateListItemStyleMenuItem("バナー表示", PanelListItemStyle.Banner));
            menu.Items.Add(new Separator());
            menu.Items.Add(CreateRecursiveFlagMenuItem("この場所ではサブフォルダーを読み込む"));

            this.MoreMenu = menu;
        }

        //
        private MenuItem CreateRecursiveFlagMenuItem(string header)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = ToggleFolderRecursive;
            item.SetBinding(MenuItem.IsCheckedProperty, new Binding("FolderCollection.FolderParameter.IsFolderRecursive"));
            return item;
        }

        //
        private MenuItem CreateCommandMenuItem(string header, CommandType command, FolderPanelModel source)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = RoutedCommandTable.Current[command];
            item.CommandParameter = MenuCommandTag.Tag; // コマンドがメニューからであることをパラメータで伝えてみる
            if (CommandTable.Current[command].CreateIsCheckedBinding != null)
            {
                var binding = CommandTable.Current[command].CreateIsCheckedBinding();
                item.SetBinding(MenuItem.IsCheckedProperty, binding);
            }

            return item;
        }

        //
        private MenuItem CreateListItemStyleMenuItem(string header, PanelListItemStyle style)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = SetListItemStyle;
            item.CommandParameter = style;
            var binding = new Binding(nameof(_model.PanelListItemStyle))
            {
                Converter = _PanelListItemStyleToBooleanConverter,
                ConverterParameter = style,
                Source = _model,
            };
            item.SetBinding(MenuItem.IsCheckedProperty, binding);

            return item;
        }


        private PanelListItemStyleToBooleanConverter _PanelListItemStyleToBooleanConverter = new PanelListItemStyleToBooleanConverter();


        /// <summary>
        /// SetListItemStyle command.
        /// </summary>
        public RelayCommand<PanelListItemStyle> SetListItemStyle
        {
            get { return _SetListItemStyle = _SetListItemStyle ?? new RelayCommand<PanelListItemStyle>(SetListItemStyle_Executed); }
        }

        //
        private RelayCommand<PanelListItemStyle> _SetListItemStyle;

        //
        private void SetListItemStyle_Executed(PanelListItemStyle style)
        {
            _model.PanelListItemStyle = style;
        }

        #endregion

        #region DragStart

        private DragStart _dragStart;
        private FolderItem _dragFolderItem;

        //
        private void InitializeDragStart()
        {
            _dragStart = new DragStart();

            // ドラッグ中のファイルロック禁止
            _dragStart.Dragging +=
                (s, e) =>
                {
                    SevenZipArchiverProfile.Current.IsUnlockMode = true;
                    if (_dragFolderItem?.Path == BookOperation.Current.Place) BookOperation.Current.Unlock();
                };

            _dragStart.Dragged +=
                (s, e) => SevenZipArchiverProfile.Current.IsUnlockMode = false;
        }

        //
        public void Drag_MouseDown(object sender, MouseButtonEventArgs e, FolderItem folderItem)
        {
            if (!FileIOProfile.Current.IsEnabled) return;
            _dragFolderItem = folderItem;
            _dragStart.Drag_MouseDown(sender, e, folderItem.GetFileDragData(), DragDropEffects.All);
        }

        //
        public void Drag_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _dragStart.Drag_MouseUp(sender, e);
        }

        //
        public void Drag_MouseMove(object sender, MouseEventArgs e)
        {
            _dragStart.Drag_MouseMove(sender, e);
        }

        #endregion



        /// <summary>
        /// コマンド実行可能状態を更新
        /// </summary>
        private void UpdateCommandCanExecute()
        {
            this.MoveToPrevious.RaiseCanExecuteChanged();
            this.MoveToNext.RaiseCanExecuteChanged();
        }


        /// <summary>
        /// SetHome command.
        /// </summary>
        private RelayCommand _SetHome;
        public RelayCommand SetHome
        {
            get { return _SetHome = _SetHome ?? new RelayCommand(_model.SetHome_Executed); }
        }


        /// <summary>
        /// MoveToHome command.
        /// </summary>
        private RelayCommand _MoveToHome;
        public RelayCommand MoveToHome
        {
            get { return _MoveToHome = _MoveToHome ?? new RelayCommand(_model.MoveToHome_Executed); }
        }


        /// <summary>
        /// MoveTo command.
        /// </summary>
        public RelayCommand<string> MoveTo
        {
            get { return _MoveTo = _MoveTo ?? new RelayCommand<string>(_model.MoveTo_Executed); }
        }

        //
        private RelayCommand<string> _MoveTo;



        /// <summary>
        /// MoveToPrevious command.
        /// </summary>
        private RelayCommand _MoveToPrevious;
        public RelayCommand MoveToPrevious
        {
            get { return _MoveToPrevious = _MoveToPrevious ?? new RelayCommand(_model.MoveToPrevious_Executed, _model.MoveToPrevious_CanExecutre); }
        }


        /// <summary>
        /// MoveToNext command.
        /// </summary>
        private RelayCommand _MoveToNext;
        public RelayCommand MoveToNext
        {
            get { return _MoveToNext = _MoveToNext ?? new RelayCommand(_model.MoveToNext_Executed, _model.MoveToNext_CanExecute); }
        }


        /// <summary>
        /// 履歴取得
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        internal List<KeyValuePair<int, string>> GetHistory(int direction, int size)
        {
            return _model.History.GetHistory(direction, size);
        }


        /// <summary>
        /// MoveToHistory command.
        /// </summary>
        private RelayCommand<KeyValuePair<int, string>> _MoveToHistory;
        public RelayCommand<KeyValuePair<int, string>> MoveToHistory
        {
            get { return _MoveToHistory = _MoveToHistory ?? new RelayCommand<KeyValuePair<int, string>>(_model.MoveToHistory_Executed); }
        }


        /// <summary>
        /// MoveToUp command.
        /// </summary>
        private RelayCommand _MoveToUp;
        public RelayCommand MoveToUp
        {
            get { return _MoveToUp = _MoveToUp ?? new RelayCommand(_model.MoveToParent_Execute, _model.MoveToParent_CanExecute); }
        }

        /// <summary>
        /// Sync command.
        /// 現在開いているフォルダーで更新
        /// </summary>
        private RelayCommand _Sync;
        public RelayCommand Sync
        {
            get { return _Sync = _Sync ?? new RelayCommand(_model.Sync_Executed); }
        }

        /// <summary>
        /// ToggleFolderRecursive command.
        /// </summary>
        public RelayCommand ToggleFolderRecursive
        {
            get { return _ToggleFolderRecursive = _ToggleFolderRecursive ?? new RelayCommand(_model.ToggleFolderRecursive_Executed); }
        }

        //
        private RelayCommand _ToggleFolderRecursive;



        /// <summary>
        /// IsRenaming property.
        /// </summary>
        private bool _isRenaming;
        public bool IsRenaming
        {
            get { return _isRenaming; }
            set { if (_isRenaming != value) { _isRenaming = value; RaisePropertyChanged(); } }
        }


        /// <summary>
        /// ListContent property.
        /// </summary>
        public FolderListBox ListContent
        {
            get { return _listContent; }
            private set { if (_listContent != value) { _listContent = value; RaisePropertyChanged(); } }
        }

        //
        private FolderListBox _listContent;

        //
        public void UpdateListContent()
        {
            Debug.WriteLine("*** Update ListContent ***");
            ListContent = new FolderListBox(this);
        }
    }

}
