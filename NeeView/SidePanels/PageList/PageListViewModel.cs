using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView
{
    public class ViewItemsChangedEventArgs : EventArgs
    {
        public ViewItemsChangedEventArgs(List<Page> pages, int direction)
        {
            this.ViewItems = pages;
            this.Direction = direction;
        }

        public List<Page> ViewItems { get; set; }
        public int Direction { get; set; }
    }

    public class PageListViewModel : BindableBase
    {
        private string _title;
        private PageSortMode _pageSortMode;
        private PageList _model;
        private PageListBox _listBoxContent;
        

        public PageListViewModel(PageList model)
        {
            _model = model;
            _model.CollectionChanging += PageList_CollectionChanging;
            _model.CollectionChanged += PageList_CollectionChanged;
            _model.AddPropertyChanged(nameof(_model.PanelListItemStyle), (s, e) => UpdateListBoxContent());

            InitializeMoreMenu();
            UpdateListBoxContent();
        }


        public event EventHandler CollectionChanging;
        public event EventHandler CollectionChanged;


        public Dictionary<PageNameFormat, string> FormatList { get; } = AliasNameExtensions.GetAliasNameDictionary<PageNameFormat>();

        public Dictionary<PageSortMode, string> PageSortModeList { get; } = AliasNameExtensions.GetAliasNameDictionary<PageSortMode>();

        public string Title
        {
            get { return _title; }
            set { _title = value; RaisePropertyChanged(); }
        }

        public PageSortMode PageSortMode
        {
            get { return _pageSortMode; }
            set { _pageSortMode = value; BookSetting.Current.SetSortMode(value); }
        }

        public PageList Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        public PageListBox ListBoxView
        {
            get { return _listBoxContent; }
            set { if (_listBoxContent != value) { _listBoxContent = value; RaisePropertyChanged(); } }
        }


        #region MoreMenu

        private ContextMenu _moreMenu;
        private PanelListItemStyleToBooleanConverter _panelListItemStyleToBooleanConverter = new PanelListItemStyleToBooleanConverter();

        public ContextMenu MoreMenu
        {
            get { return _moreMenu; }
            set { if (_moreMenu != value) { _moreMenu = value; RaisePropertyChanged(); } }
        }

        private void InitializeMoreMenu()
        {
            var menu = new ContextMenu();
            menu.Items.Add(CreateListItemStyleMenuItem(Properties.Resources.WordStyleList, PanelListItemStyle.Normal));
            menu.Items.Add(CreateListItemStyleMenuItem(Properties.Resources.WordStyleContent, PanelListItemStyle.Content));
            menu.Items.Add(CreateListItemStyleMenuItem(Properties.Resources.WordStyleBanner, PanelListItemStyle.Banner));
            menu.Items.Add(CreateListItemStyleMenuItem(Properties.Resources.WordStyleThumbnail, PanelListItemStyle.Thumbnail));

            this.MoreMenu = menu;
        }

        private MenuItem CreateListItemStyleMenuItem(string header, PanelListItemStyle style)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = SetListItemStyle;
            item.CommandParameter = style;
            var binding = new Binding(nameof(_model.PanelListItemStyle))
            {
                Converter = _panelListItemStyleToBooleanConverter,
                ConverterParameter = style,
                Source = _model
            };
            item.SetBinding(MenuItem.IsCheckedProperty, binding);

            return item;
        }

        private RelayCommand<PanelListItemStyle> _setListItemStyle;
        public RelayCommand<PanelListItemStyle> SetListItemStyle
        {
            get { return _setListItemStyle = _setListItemStyle ?? new RelayCommand<PanelListItemStyle>(SetListItemStyle_Executed); }
        }

        private void SetListItemStyle_Executed(PanelListItemStyle style)
        {
            _model.PanelListItemStyle = style;
        }

        #endregion


        private void PageList_CollectionChanging(object sender, EventArgs e)
        {
            CollectionChanging?.Invoke(this, null);
        }

        private void PageList_CollectionChanged(object sender, EventArgs e)
        {
            UpdateListBoxContent();

            CollectionChanged?.Invoke(this, null);
        }

        private void RefreshPageSortMode()
        {
            _pageSortMode = BookSetting.Current.BookMemento.SortMode;
            RaisePropertyChanged(nameof(PageSortMode));
        }

        private void UpdateListBoxContent()
        {
            AppDispatcher.Invoke(() =>
            {
                var vm = new PageListBoxViewModel(_model.ListBoxModel);
                this.ListBoxView = new PageListBox(vm);

                RefreshPageSortMode();
            });
        }
    }
}
