using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class PagemarPanelAccessor : LayoutPanelAccessor
    {
        private PagemarkPanel _panel;
        private PagemarkList _model;


        public PagemarPanelAccessor() : base(nameof(PagemarkPanel))
        {
            _panel = (PagemarkPanel)MainLayoutPanelManager.Current.GetPanel(nameof(PagemarkPanel));
            _model = _panel.PagemarkListView.PagemarkList;
        }

        [WordNodeMember]
        public string Style
        {
            get { return _model.PanelListItemStyle.ToString(); }
            set { AppDispatcher.Invoke(() => _model.PanelListItemStyle = (PanelListItemStyle)Enum.Parse(typeof(PanelListItemStyle), value)); }
        }

        [WordNodeMember]
        public PagemarkItemAccessor[] Items
        {
            get { return AppDispatcher.Invoke(() => GetExpandedItems()); }
        }

        [WordNodeMember]
        public PagemarkItemAccessor[] SelectedItems
        {
            get { return AppDispatcher.Invoke(() => GetSelectedItems()); }
            set { AppDispatcher.Invoke(() => SetSelectedItems(value)); }
        }


        private PagemarkItemAccessor[] GetExpandedItems()
        {
            return ToAccessorArray(_panel.PagemarkListView?.PagemarkListBox?.GetExpandedItems());
        }

        private PagemarkItemAccessor[] GetSelectedItems()
        {
            return ToAccessorArray(_panel.PagemarkListView?.PagemarkListBox?.GetSelectedItems());
        }

        private void SetSelectedItems(PagemarkItemAccessor[] selectedItems)
        {
            selectedItems = selectedItems ?? new PagemarkItemAccessor[] { };
            _panel.PagemarkListView?.PagemarkListBox?.SetSelectedItems(selectedItems.Select(e => e.Source));
        }

        private PagemarkItemAccessor[] ToAccessorArray(IEnumerable<TreeListNode<IPagemarkEntry>> items)
        {
            return items?.Select(e => new PagemarkItemAccessor(e)).ToArray() ?? new PagemarkItemAccessor[] { };
        }

        internal WordNode CreateWordNode(string name)
        {
            return WordNodeHelper.CreateClassWordNode(name, this.GetType());
        }
    }
}
