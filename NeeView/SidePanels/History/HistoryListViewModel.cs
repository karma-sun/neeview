// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections.ObjectModel;
using NeeView.Windows.Input;
using System.Runtime.Serialization;
using System.Windows.Input;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// 
    /// </summary>
    public class HistoryListViewModel : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion


        // 項目変更イベント。フォーカス保存用
        public class SelectedItemChangeEventArgs
        {
            public bool IsFocused { get; set; }
        }
        public event EventHandler<SelectedItemChangeEventArgs> SelectedItemChanging;
        public event EventHandler<SelectedItemChangeEventArgs> SelectedItemChanged;

        #region Property: Items
        private ObservableCollection<BookMementoUnit> _items;
        public ObservableCollection<BookMementoUnit> Items
        {
            get { return _items; }
            set { _items = value; RaisePropertyChanged(); }
        }
        #endregion


        #region Property: SelectedItem
        private BookMementoUnit _selectedItem;
        public BookMementoUnit SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; RaisePropertyChanged(); }
        }
        #endregion


        #region Property: Visibility
        private Visibility _visibility = Visibility.Hidden;
        public Visibility Visibility
        {
            get { return _visibility; }
            set { _visibility = value; RaisePropertyChanged(); }
        }
        #endregion


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
        private void InitializeMoreMenu()
        {
            var menu = new ContextMenu();
            menu.Items.Add(CreateListItemStyleMenuItem("一覧表示", PanelListItemStyle.Normal));
            menu.Items.Add(CreateListItemStyleMenuItem("コンテンツ表示", PanelListItemStyle.Content));
            menu.Items.Add(CreateListItemStyleMenuItem("バナー表示", PanelListItemStyle.Banner));
            menu.Items.Add(new Separator());
            menu.Items.Add(CreateCommandMenuItem("無効な履歴を削除", RemoveUnlinkedCommand));
            menu.Items.Add(CreateCommandMenuItem("すべての履歴を削除", RemoveAllCommand));
            this.MoreMenu = menu;
        }

        //
        private MenuItem CreateCommandMenuItem(string header, ICommand command)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = command;
            return item;
        }


        //
        private MenuItem CreateCommandMenuItem(string header, CommandType command, object source)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = RoutedCommandTable.Current[command];
            item.CommandParameter = MenuCommandTag.Tag; // コマンドがメニューからであることをパラメータで伝えてみる
            if (CommandTable.Current[command].CreateIsCheckedBinding != null)
            {
                var binding = CommandTable.Current[command].CreateIsCheckedBinding();
                binding.Source = source;
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
                Source = _model
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


        private bool _isDarty;

        /// <summary>
        /// Model property.
        /// </summary>
        public HistoryList Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        //
        private HistoryList _model;

        //
        private BookHub _bookHub;


        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="model"></param>
        public HistoryListViewModel(HistoryList model)
        {
            _model = model;
            _bookHub = _model.BookHub;

            _isDarty = true;
            //if (isVisible) UpdateItems();
            UpdateItems();

            _bookHub.HistoryChanged += BookHub_HistoryChanged;
            _bookHub.HistoryListSync += BookHub_HistoryListSync;

            InitializeMoreMenu();
        }

        //
        private void BookHub_HistoryListSync(object sender, string e)
        {
            var args = new SelectedItemChangeEventArgs();
            SelectedItemChanging?.Invoke(this, args);
            SelectedItem = ModelContext.BookHistory.Find(e);
            SelectedItemChanged?.Invoke(this, args);
        }

        //
        private void BookHub_HistoryChanged(object sender, BookMementoCollectionChangedArgs e)
        {
            _isDarty = _isDarty || e.HistoryChangedType != BookMementoCollectionChangedType.Update;
            if (_isDarty && Visibility == Visibility.Visible)
            {
                UpdateItems();
            }
        }

        //
        public void UpdateItems()
        {
            if (_isDarty)
            {
                _isDarty = false;

                var args = new SelectedItemChangeEventArgs();
                App.Current.Dispatcher.Invoke(() => SelectedItemChanging?.Invoke(this, args));

                var item = SelectedItem;
                Items = new ObservableCollection<BookMementoUnit>(ModelContext.BookHistory.Items);
                SelectedItem = Items.Count > 0 ? item : null;

                App.Current.Dispatcher.Invoke(() => SelectedItemChanged?.Invoke(this, args));
            }
        }

        //
        public void Load(string path)
        {
            if (path == null) return;
            _bookHub?.RequestLoad(path, null, BookLoadOption.KeepHistoryOrder | BookLoadOption.SkipSamePlace, true);
        }


        // となりを取得
        public BookMementoUnit GetNeighbor(BookMementoUnit item)
        {
            if (Items == null || Items.Count <= 0) return null;

            int index = Items.IndexOf(item);
            if (index < 0) return Items[0];

            if (index + 1 < Items.Count)
            {
                return Items[index + 1];
            }
            else if (index > 0)
            {
                return Items[index - 1];
            }
            else
            {
                return item;
            }
        }

        //
        public void Remove(BookMementoUnit item)
        {
            if (item == null) return;

            // 位置ずらし
            var args = new SelectedItemChangeEventArgs();
            SelectedItemChanging?.Invoke(this, args);
            SelectedItem = GetNeighbor(item);
            SelectedItemChanged?.Invoke(this, args);

            // 削除
            ModelContext.BookHistory.Remove(item.Memento.Place);
        }

        // サムネイル要求
        public void RequestThumbnail(int start, int count, int margin, int direction)
        {
            if (_model.PanelListItemStyle.HasThumbnail())
            {
                ThumbnailManager.Current.RequestThumbnail(Items, QueueElementPriority.HistoryThumbnail, start, count, margin, direction);
            }
        }

        /// <summary>
        /// RemoveAllCommand command
        /// </summary>
        public RelayCommand RemoveAllCommand
        {
            get { return _removeAllCommand = _removeAllCommand ?? new RelayCommand(RemoveAll_Executed); }
        }

        private RelayCommand _removeAllCommand;

        private void RemoveAll_Executed()
        {
            if (ModelContext.BookHistory.Items.Any())
            {
                var dialog = new MessageDialog($"すべての履歴を削除します。よろしいですか？", "履歴を削除します");
                dialog.Commands.Add(UICommands.Remove);
                dialog.Commands.Add(UICommands.Cancel);
                var answer = dialog.ShowDialog();
                if (answer != UICommands.Remove) return;
            }

            ModelContext.BookHistory.RemoveAll();
        }


        /// <summary>
        /// RemoveUnlinkedCommand command.
        /// </summary>
        public RelayCommand RemoveUnlinkedCommand
        {
            get { return _removeUnlinkedCommand = _removeUnlinkedCommand ?? new RelayCommand(RemoveUnlinkedCommand_Executed); }
        }

        //
        private RelayCommand _removeUnlinkedCommand;

        //
        private void RemoveUnlinkedCommand_Executed()
        {
            ModelContext.BookHistory.RemoveUnlinked();
        }

    }
}
