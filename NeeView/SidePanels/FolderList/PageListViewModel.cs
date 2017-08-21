// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using NeeView.Windows.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// 
    /// </summary>
    public class PageListViewModel : BindableBase
    {

        public Dictionary<PageNameFormat, string> FormatList { get; } = new Dictionary<PageNameFormat, string>
        {
            [PageNameFormat.None] = "そのまま",
            [PageNameFormat.Smart] = "標準表示",
            [PageNameFormat.NameOnly] = "名前のみ",
        };

        #region Property: Format
        private PageNameFormat _format = PageNameFormat.Smart;
        public PageNameFormat Format
        {
            get { return _format; }
            set { _format = value; RaisePropertyChanged(); }
        }
        #endregion


        public Dictionary<PageSortMode, string> PageSortModeList => PageSortModeExtension.PageSortModeList;

        #region Property: Title
        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; RaisePropertyChanged(); }
        }
        #endregion

        #region Property: PageSortMode
        private PageSortMode _pageSortMode;
        public PageSortMode PageSortMode
        {
            get { return _pageSortMode; }
            set { _pageSortMode = value; BookSetting.Current.SetSortMode(value); }
        }
        #endregion

        #region Property: SelectedItem
        private Page _selectedItem;
        public Page SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; RaisePropertyChanged(); }
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

            this.MoreMenu = menu;
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
        public PageList Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        private PageList _model;


        //
        public PageListViewModel(PageList model)
        {
            _model = model;
            _model.AddPropertyChanged(nameof(_model.PanelListItemStyle), (s, e) => UpdateListBoxContent());
            _model.BookHub.ViewContentsChanged += BookHub_ViewContentsChanged;
            _model.BookOperation.BookChanged += (s, e) => Reflesh();

            InitializeMoreMenu();
            UpdateListBoxContent();

            Reflesh();
        }

        //
        private void BookHub_ViewContentsChanged(object sender, ViewPageCollection e)
        {
            var contents = e?.Collection;
            if (contents == null) return;

            var mainContent = contents.Count > 0 ? (contents.First().PagePart.Position < contents.Last().PagePart.Position ? contents.First() : contents.Last()) : null;
            if (mainContent != null)
            {
                SelectedItem = mainContent.Page;
            }
        }

        //
        private void Reflesh()
        {
            Title = System.IO.Path.GetFileName(_model.BookOperation.Book?.Place);

            _pageSortMode = BookSetting.Current.BookMemento.SortMode;
            RaisePropertyChanged(nameof(PageSortMode));

            App.Current?.Dispatcher.Invoke(() => this.ListBoxContent.FocusSelectedItem());
        }


        //
        public void Jump(Page page)
        {
            _model.BookOperation.JumpPage(page);
        }


        //
        public bool CanRemove(Page page)
        {
            return FileIO.Current.CanRemoveFile(page);
        }

        //
        public async Task Remove(Page page)
        {
            await FileIO.Current.RemoveFile(page);
        }

        // サムネイル要求
        public void RequestThumbnail(int start, int count, int margin, int direction)
        {
            if (_model.PanelListItemStyle.HasThumbnail())
            {
                ThumbnailManager.Current.RequestThumbnail(_model.PageCollection, QueueElementPriority.PageListThumbnail, start, count, margin, direction);
            }
        }


        /// <summary>
        /// ListBoxContent property.
        /// </summary>
        public PageListBox ListBoxContent
        {
            get { return _listBoxContent; }
            set { if (_listBoxContent != value) { _listBoxContent = value; RaisePropertyChanged(); } }
        }

        private PageListBox _listBoxContent;

        private void UpdateListBoxContent()
        {
            Debug.WriteLine("*** PageList ***");
            this.ListBoxContent = new PageListBox(this);
        }

    }
}
