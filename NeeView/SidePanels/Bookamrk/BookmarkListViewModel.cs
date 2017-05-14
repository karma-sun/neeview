// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using NeeView.Windows.Input;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// 
    /// </summary>
    public class BookmarkListViewModel : BindableBase
    {
        public BookHub BookHub { get; private set; }

        public BookmarkCollection Bookmark => ModelContext.Bookmarks;


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
            menu.Items.Add(CreateCommandMenuItem("無効なブックマークを削除", RemoveUnlinkedCommand));
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

        /// <summary>
        /// Model property.
        /// </summary>
        public BookmarkList Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        //
        private BookmarkList _model;


        //
        public BookmarkListViewModel(BookmarkList model)
        {
            _model = model;
            _model.AddPropertyChanged(nameof(_model.PanelListItemStyle), (s, e) => UpdateListBoxContent());

            BookHub = _model.BookHub;

            InitializeMoreMenu();

            UpdateListBoxContent();
        }

        //
        public void Load(string path)
        {
            BookHub?.RequestLoad(path, null, BookLoadOption.SkipSamePlace, true);
        }


        public void Remove(BookMementoUnitNode item)
        {
            if (item == null) return;

            this.ListBoxContent.StoreFocus();
            Bookmark.SelectedItem = Bookmark.GetNeighbor(item);
            this.ListBoxContent.RestoreFocus();

            ModelContext.Bookmarks.Remove(item.Value.Memento.Place);
        }

        // サムネイル要求
        public void RequestThumbnail(int start, int count, int margin, int direction)
        {
            if (Bookmark == null) return;
            if (_model.PanelListItemStyle.HasThumbnail())
            {
                ThumbnailManager.Current.RequestThumbnail(Bookmark.Items, QueueElementPriority.BookmarkThumbnail, start, count, margin, direction);
            }
        }


        /// <summary>
        /// 無効なブックマークを削除するコマンド
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
            ModelContext.Bookmarks.RemoveUnlinked();
        }

        /// <summary>
        /// ListBoxContent property.
        /// </summary>
        public BookmarkListBox ListBoxContent
        {
            get { return _listBoxContent; }
            set { if (_listBoxContent != value) { _listBoxContent = value; RaisePropertyChanged(); } }
        }

        private BookmarkListBox _listBoxContent;

        private void UpdateListBoxContent()
        {
            Debug.WriteLine("*** Bookmark Update ***");
            this.ListBoxContent = new BookmarkListBox(this);
        }
    }
}
