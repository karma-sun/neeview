using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class PageListPanelAccessor : LayoutPanelAccessor
    {
        private PageListPanel _panel;
        private PageList _model;


        public PageListPanelAccessor() : base(nameof(PageListPanel))
        {
            _panel = (PageListPanel)CustomLayoutPanelManager.Current.GetPanel(nameof(PageListPanel));
            _model = _panel.Presenter.PageList;
        }

        [WordNodeMember]
        public string Path
        {
            get { return _panel.Presenter.PageList.Path; }
        }

        [WordNodeMember(DocumentType = typeof(PanelListItemStyle))]
        public string Style
        {
            get { return _model.PanelListItemStyle.ToString(); }
            set { AppDispatcher.Invoke(() => _model.PanelListItemStyle = (PanelListItemStyle)Enum.Parse(typeof(PanelListItemStyle), value)); }
        }

        [WordNodeMember(DocumentType = typeof(PageNameFormat))]
        public string Format
        {
            get { return AppDispatcher.Invoke(() => _panel.Presenter.PageListView.GetFormat().ToString()); }
            set { AppDispatcher.Invoke(() => _panel.Presenter.PageListView.SetFormat((PageNameFormat)Enum.Parse(typeof(PageNameFormat), value))); }
        }

        [WordNodeMember(DocumentType = typeof(PageSortMode))]
        public string SortMode
        {
            get { return AppDispatcher.Invoke(() => _panel.Presenter.PageListView.GetSortMode().ToString()); }
            set { AppDispatcher.Invoke(() => _panel.Presenter.PageListView.SetSortMode((PageSortMode)Enum.Parse(typeof(PageSortMode), value))); }
        }

        [WordNodeMember]
        public PageAccessor[] Items
        {
            get { return AppDispatcher.Invoke(() => GetItems()); }
        }

        [WordNodeMember]
        public PageAccessor[] SelectedItems
        {
            get { return AppDispatcher.Invoke(() => GetSelectedItems()); }
            set { AppDispatcher.Invoke(() => SetSelectedItems(value)); }
        }


        private PageAccessor[] GetItems()
        {
            return ToStringArray(_panel.Presenter.PageListBox?.GetItems());
        }

        private PageAccessor[] GetSelectedItems()
        {
            return ToStringArray(_panel.Presenter.PageListBox?.GetSelectedItems());
        }

        private void SetSelectedItems(PageAccessor[] selectedItems)
        {
            selectedItems = selectedItems ?? new PageAccessor[] { };
            _panel.Presenter.PageListBox?.SetSelectedItems(selectedItems.Select(e => e.Source));
        }

        private PageAccessor[] ToStringArray(IEnumerable<Page> pages)
        {
            return pages?.Select(e => new PageAccessor(e)).ToArray() ?? new PageAccessor[] { };
        }


        internal WordNode CreateWordNode(string name)
        {
            return WordNodeHelper.CreateClassWordNode(name, this.GetType());
        }
    }
}
