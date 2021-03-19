using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class HistoryPanelAccessor : LayoutPanelAccessor
    {
        private HistoryPanel _panel;
        private HistoryList _model;


        public HistoryPanelAccessor() : base(nameof(HistoryPanel))
        {
            _panel = (HistoryPanel)CustomLayoutPanelManager.Current.GetPanel(nameof(HistoryPanel));
            _model = _panel.Presenter.HistoryList;
        }


        [WordNodeMember(DocumentType = typeof(PanelListItemStyle))]
        public string Style
        {
            get { return _model.PanelListItemStyle.ToString(); }
            set { AppDispatcher.Invoke(() => _model.PanelListItemStyle = (PanelListItemStyle)Enum.Parse(typeof(PanelListItemStyle), value)); }
        }

        [WordNodeMember]
        public HistoryItemAccessor[] Items
        {
            get { return AppDispatcher.Invoke(() => GetItems()); }
        }

        [WordNodeMember]
        public HistoryItemAccessor[] SelectedItems
        {
            get { return AppDispatcher.Invoke(() => GetSelectedItems()); }
            set { AppDispatcher.Invoke(() => SetSelectedItems(value)); }
        }

        private HistoryItemAccessor[] GetItems()
        {
            return ToStringArray(_panel.Presenter.HistoryListBox?.GetItems());
        }

        private HistoryItemAccessor[] GetSelectedItems()
        {
            return ToStringArray(_panel.Presenter.HistoryListBox?.GetSelectedItems());
        }

        private void SetSelectedItems(HistoryItemAccessor[] selectedItems)
        {
            selectedItems = selectedItems ?? new HistoryItemAccessor[] { };
            _panel.Presenter.HistoryListBox?.SetSelectedItems(selectedItems.Select(e => e.Source));
        }

        private HistoryItemAccessor[] ToStringArray(IEnumerable<BookHistory> items)
        {
            return items?.Select(e => new HistoryItemAccessor(e)).ToArray() ?? new HistoryItemAccessor[] { };
        }

        internal WordNode CreateWordNode(string name)
        {
            return WordNodeHelper.CreateClassWordNode(name, this.GetType());
        }
    }
}
