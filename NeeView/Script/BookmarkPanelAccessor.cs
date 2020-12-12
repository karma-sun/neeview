using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class BookmarkPanelAccessor : LayoutPanelAccessor
    {
        private BookmarkPanel _panel;
        private BookmarkFolderList _model;


        public BookmarkPanelAccessor() : base(nameof(BookmarkPanel))
        {
            _panel = (BookmarkPanel)MainLayoutPanelManager.Current.GetPanel(nameof(BookmarkPanel));
            _model = _panel.Presenter.BookmarkFolderList;
        }

        [WordNodeMember]
        public string Path
        {
            get { return _model.Place?.SimplePath; }
            set { AppDispatcher.Invoke(() => _model.RequestPlace(new QueryPath(value), null, FolderSetPlaceOption.UpdateHistory)); }
        }

        [WordNodeMember(DocumentType = typeof(PanelListItemStyle))]
        public string Style
        {
            get { return _model.FolderListConfig.PanelListItemStyle.ToString(); }
            set { AppDispatcher.Invoke(() => _model.FolderListConfig.PanelListItemStyle = (PanelListItemStyle)Enum.Parse(typeof(PanelListItemStyle), value)); }
        }

        [WordNodeMember(DocumentType = typeof(FolderOrder))]
        public string FolderOrder
        {
            get { return _model.GetFolderOrder().ToString(); }
            set { AppDispatcher.Invoke(() => _model.SetFolderOrder((FolderOrder)Enum.Parse(typeof(FolderOrder), value))); }
        }

        [WordNodeMember]
        public BookmarkItemAccessor[] Items
        {
            get { return AppDispatcher.Invoke(() => GetItems()); }
        }

        [WordNodeMember]
        public BookmarkItemAccessor[] SelectedItems
        {
            get { return AppDispatcher.Invoke(() => GetSelectedItems()); }
            set { AppDispatcher.Invoke(() => SetSelectedItems(value)); }
        }

        private BookmarkItemAccessor[] GetItems()
        {
            return ToStringArray(_panel.Presenter.FolderListBox?.GetItems());
        }

        private BookmarkItemAccessor[] GetSelectedItems()
        {
            return ToStringArray(_panel.Presenter.FolderListBox?.GetSelectedItems());
        }

        private void SetSelectedItems(BookmarkItemAccessor[] selectedItems)
        {
            selectedItems = selectedItems ?? new BookmarkItemAccessor[] { };
            _panel.Presenter.FolderListBox?.SetSelectedItems(selectedItems.Select(e => e.Source));
        }

        private BookmarkItemAccessor[] ToStringArray(IEnumerable<FolderItem> items)
        {
            return items?.Select(e => new BookmarkItemAccessor(e)).ToArray() ?? new BookmarkItemAccessor[] { };
        }

        internal WordNode CreateWordNode(string name)
        {
            var node = WordNodeHelper.CreateClassWordNode(name, this.GetType());

            return node;
        }
    }
}
