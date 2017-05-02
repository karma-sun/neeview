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

namespace NeeView
{
    /// <summary>
    /// 
    /// </summary>
    public class HistoryControlViewModel : INotifyPropertyChanged
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


        public BookHub BookHub { get; private set; }


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
            //menu.Items.Add(CreateCommandMenuItem("", CommandType.ToggleVisiblePageList, vm));
            //menu.Items.Add(new Separator());
            menu.Items.Add(CreateListItemStyleMenuItem("一覧表示", PanelListItemStyle.Normal));
            menu.Items.Add(CreateListItemStyleMenuItem("コンテンツ表示", PanelListItemStyle.Content));
            menu.Items.Add(CreateListItemStyleMenuItem("バナー表示", PanelListItemStyle.Banner));

            this.MoreMenu = menu;
        }

        //
        private MenuItem CreateCommandMenuItem(string header, CommandType command, object source)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = ModelContext.BookCommands[command];
            item.CommandParameter = MenuCommandTag.Tag; // コマンドがメニューからであることをパラメータで伝えてみる
            if (ModelContext.CommandTable[command].CreateIsCheckedBinding != null)
            {
                var binding = ModelContext.CommandTable[command].CreateIsCheckedBinding();
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
            var binding = new Binding(nameof(PanelListItemStyle))
            {
                Converter = _PanelListItemStyleToBooleanConverter,
                ConverterParameter = style
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
            this.PanelListItemStyle = style;
        }


        /// <summary>
        /// PanelListItemStyle property.
        /// TODO: 保存されるものなのでモデル的なクラスでの実装が望ましい
        /// </summary>
        public PanelListItemStyle PanelListItemStyle
        {
            get { return _PanelListItemStyle; }
            set
            {
                if (_PanelListItemStyle != value)
                {
                    _PanelListItemStyle = value;
                    ////this.FolderListView?.SetPanelListItemStyle(_PanelListItemStyle);
                    RaisePropertyChanged();
                }
            }
        }

        //
        private PanelListItemStyle _PanelListItemStyle;



        #endregion


        private bool _isDarty;

        //
        public void Initialize(BookHub bookHub, bool isVisible)
        {
            BookHub = bookHub;

            _isDarty = true;
            if (isVisible) UpdateItems();

            BookHub.HistoryChanged += BookHub_HistoryChanged;
            BookHub.HistoryListSync += BookHub_HistoryListSync;

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
            BookHub?.RequestLoad(path, null, BookLoadOption.KeepHistoryOrder | BookLoadOption.SkipSamePlace, true);
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
            if (PanelListItemStyle.HasThumbnail())
            {
                ThumbnailManager.Current.RequestThumbnail(Items, QueueElementPriority.HistoryThumbnail, start, count, margin, direction);
            }
        }

        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public PanelListItemStyle PanelListItemStyle { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.PanelListItemStyle = this.PanelListItemStyle;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.PanelListItemStyle = memento.PanelListItemStyle;
        }
        #endregion
    }
}
