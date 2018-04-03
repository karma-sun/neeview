using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
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
        #region Fields

        private string _title;
        private PageSortMode _pageSortMode;
        private Page _selectedItem;
        private List<Page> _viewItems;
        private PageList _model;
        private PageListBox _listBoxContent;

        #endregion

        #region Constructors

        public PageListViewModel(PageList model)
        {
            _model = model;
            _model.AddPropertyChanged(nameof(_model.PanelListItemStyle), (s, e) => UpdateListBoxContent());
            _model.AddPropertyChanged(nameof(_model.PageCollection), PageList_UpdatePageCollection);
            _model.BookHub.ViewContentsChanged += BookHub_ViewContentsChanged;
            _model.BookOperation.BookChanged += (s, e) => Reflesh();

            _viewItems = new List<Page>();

            InitializeMoreMenu();
            UpdateListBoxContent();

            Reflesh();
        }

        #endregion

        #region Events

        public event EventHandler<ViewItemsChangedEventArgs> ViewItemsChanged;

        #endregion

        #region Properties

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

        public Page SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; RaisePropertyChanged(); }
        }

        public List<Page> ViewItems
        {
            get { return _viewItems; }
            set
            {
                if (_viewItems.SequenceEqual(value)) return;

                var removes = _viewItems.Where(e => !value.Contains(e));
                var direction = removes.Any() && value.Any() ? removes.First().Index < value.First().Index ? +1 : -1 : 0;

                _viewItems = value;

                ViewItemsChanged?.Invoke(this, new ViewItemsChangedEventArgs(_viewItems, direction));
            }
        }

        public PageList Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        public PageListBox ListBoxContent
        {
            get { return _listBoxContent; }
            set { if (_listBoxContent != value) { _listBoxContent = value; RaisePropertyChanged(); } }
        }

        // ページリスト切り替え直後はListBoxに反映されない。
        // 反映されたらこのフラグをクリアする。
        public bool IsPageCollectionDarty { get; set; }

        #endregion

        #region MoreMenu

        // fields

        private ContextMenu _moreMenu;
        private PanelListItemStyleToBooleanConverter _panelListItemStyleToBooleanConverter = new PanelListItemStyleToBooleanConverter();

        // properties

        public ContextMenu MoreMenu
        {
            get { return _moreMenu; }
            set { if (_moreMenu != value) { _moreMenu = value; RaisePropertyChanged(); } }
        }

        // methods

        private void InitializeMoreMenu()
        {
            var menu = new ContextMenu();
            menu.Items.Add(CreateListItemStyleMenuItem(Properties.Resources.WordStyleList, PanelListItemStyle.Normal));
            menu.Items.Add(CreateListItemStyleMenuItem(Properties.Resources.WordStyleContent, PanelListItemStyle.Content));
            menu.Items.Add(CreateListItemStyleMenuItem(Properties.Resources.WordStyleBanner, PanelListItemStyle.Banner));

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

        // commands

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

        #region Methods

        private void PageList_UpdatePageCollection(object sender, PropertyChangedEventArgs e)
        {
            IsPageCollectionDarty = true;
        }

        //
        private void BookHub_ViewContentsChanged(object sender, ViewPageCollectionChangedEventArgs e)
        {
            var contents = e?.ViewPageCollection?.Collection;
            if (contents == null) return;

            var mainContent = contents.Count > 0 ? (contents.First().PagePart.Position < contents.Last().PagePart.Position ? contents.First() : contents.Last()) : null;
            if (mainContent != null)
            {
                SelectedItem = mainContent.Page;
            }

            this.ViewItems = contents.Where(i => i != null).Select(i => i.Page).OrderBy(i => i.Index).ToList();
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
            return FileIO.Current.CanRemovePage(page);
        }

        //
        public async Task Remove(Page page)
        {
            await FileIO.Current.RemovePageAsync(page);
        }

        //
        private void UpdateListBoxContent()
        {
            this.ListBoxContent = new PageListBox(this);
        }

        #endregion
    }
}
