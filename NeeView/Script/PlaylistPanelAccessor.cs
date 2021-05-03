using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{


    public class PlaylistPanelAccessor : LayoutPanelAccessor
    {
        private PlaylistPanel _panel;
        private PlaylistHub _model;


        public PlaylistPanelAccessor() : base(nameof(PlaylistPanel))
        {
            _panel = (PlaylistPanel)CustomLayoutPanelManager.Current.GetPanel(nameof(PlaylistPanel));
            _model = _panel.Presenter.PlaylistHub;
        }

        [WordNodeMember]
        public string Path
        {
            get { return _model.SelectedItem; }
            set { AppDispatcher.Invoke(() => _model.SelectedItem = value); }
        }

        [WordNodeMember(DocumentType = typeof(PanelListItemStyle))]
        public string Style
        {
            get { return Config.Current.Playlist.PanelListItemStyle.ToString(); }
            set { AppDispatcher.Invoke(() => Config.Current.Playlist.PanelListItemStyle = (PanelListItemStyle)Enum.Parse(typeof(PanelListItemStyle), value)); }
        }

        [WordNodeMember]
        public PlaylistItemAccessor[] Items
        {
            get { return AppDispatcher.Invoke(() => GetItems()); }
        }

        [WordNodeMember]
        public PlaylistItemAccessor[] SelectedItems
        {
            get { return AppDispatcher.Invoke(() => GetSelectedItems()); }
            set { AppDispatcher.Invoke(() => SetSelectedItems(value)); }
        }

        private PlaylistItemAccessor[] GetItems()
        {
            return ToStringArray(_panel.Presenter.PlaylistListBox?.GetItems());
        }

        private PlaylistItemAccessor[] GetSelectedItems()
        {
            return ToStringArray(_panel.Presenter.PlaylistListBox?.GetSelectedItems());
        }

        private void SetSelectedItems(PlaylistItemAccessor[] selectedItems)
        {
            selectedItems = selectedItems ?? new PlaylistItemAccessor[] { };
            _panel.Presenter.PlaylistListBox?.SetSelectedItems(selectedItems.Select(e => e.Source));
        }

        private PlaylistItemAccessor[] ToStringArray(IEnumerable<PlaylistItem> items)
        {
            return items?.Select(e => new PlaylistItemAccessor(e)).ToArray() ?? new PlaylistItemAccessor[] { };
        }

        internal WordNode CreateWordNode(string name)
        {
            var node = WordNodeHelper.CreateClassWordNode(name, this.GetType());

            return node;
        }
    }

}
