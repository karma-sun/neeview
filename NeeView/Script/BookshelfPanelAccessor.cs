using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NeeView
{
    public class BookshelfPanelAccessor : LayoutPanelAccessor
    {
        private FolderPanel _panel;
        private BookshelfFolderList _model;

        public BookshelfPanelAccessor() : base(nameof(FolderPanel))
        {
            _panel = (FolderPanel)MainLayoutPanelManager.Current.GetPanel(nameof(FolderPanel));
            _model = _panel.Presenter.FolderList;
        }


        [WordNodeMember]
        public string Path
        {
            get { return _model.Place.SimplePath; }
            set { AppDispatcher.Invoke(() => _model.RequestPlace(new QueryPath(value), null, FolderSetPlaceOption.UpdateHistory)); }
        }

        [WordNodeMember]
        public string SearchWord
        {
            get { return AppDispatcher.Invoke(() => _panel.Presenter.FolderListView.GetSearchBoxText()); }
            set { AppDispatcher.Invoke(() => _panel.Presenter.FolderListView.SetSearchBoxText(value)); }
        }

        [WordNodeMember]
        public string Style
        {
            get { return _model.FolderListConfig.PanelListItemStyle.ToString(); }
            set { AppDispatcher.Invoke(() => _model.FolderListConfig.PanelListItemStyle = (PanelListItemStyle)Enum.Parse(typeof(PanelListItemStyle), value)); }
        }

        [WordNodeMember]
        public string FolderOrder
        {
            get { return _model.GetFolderOrder().ToString(); }
            set { AppDispatcher.Invoke(() => _model.SetFolderOrder((FolderOrder)Enum.Parse(typeof(FolderOrder), value))); }
        }

        [WordNodeMember]
        public BookshelfItemAccessor[] Items
        {
            get { return AppDispatcher.Invoke(() => GetItems()); }
        }

        [WordNodeMember]
        public BookshelfItemAccessor[] SelectedItems
        {
            get { return AppDispatcher.Invoke(() => GetSelectedItems()); }
            set { AppDispatcher.Invoke(() => SetSelectedItems(value)); }
        }

        private BookshelfItemAccessor[] GetItems()
        {
            return ToStringArray(_panel.Presenter.FolderListBox?.GetItems());
        }

        private BookshelfItemAccessor[] GetSelectedItems()
        {
            return ToStringArray(_panel.Presenter.FolderListBox?.GetSelectedItems());
        }

        private void SetSelectedItems(BookshelfItemAccessor[] selectedItems)
        {
            selectedItems = selectedItems ?? new BookshelfItemAccessor[] { };

            var listBox = _panel.Presenter.FolderListBox;
            listBox?.SetSelectedItems(selectedItems.Select(e => e.Source));
        }

        private BookshelfItemAccessor[] ToStringArray(IEnumerable<FolderItem> items)
        {
            return items?.Select(e => new BookshelfItemAccessor(e)).ToArray() ?? new BookshelfItemAccessor[] { };
        }

        internal WordNode CreateWordNode(string name)
        {
            return WordNodeHelper.CreateClassWordNode(name, this.GetType());
        }
    }

}