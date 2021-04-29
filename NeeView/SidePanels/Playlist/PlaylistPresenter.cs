using System;

namespace NeeView
{
    public class PlaylistPresenter
    {
        private PlaylistView _playliseView;
        private PlaylistModel _playlistModel;

        private PlaylistListBox _playlistListBox;
        private PlaylistListBoxViewModel _playlistListBoxViewModel = new PlaylistListBoxViewModel();


        public PlaylistPresenter(PlaylistView playlistView, PlaylistModel playlistModel)
        {
            _playliseView = playlistView;
            _playlistModel = playlistModel;

            _playlistModel.AddPropertyChanged(nameof(PlaylistModel.ListBoxModel),
                (s, e) => UpdateListBox());

            Config.Current.History.AddPropertyChanged(nameof(HistoryConfig.PanelListItemStyle),
                (s, e) => UpdateListBoxContent());

            UpdateListBox();
        }


        public PlaylistView HistoryListView => _playliseView;
        public PlaylistListBox HistoryListBox => _playlistListBox;
        public PlaylistModel HistoryList => _playlistModel;


        private void UpdateListBox()
        {
            _playlistListBoxViewModel.SetModel(_playlistModel.ListBoxModel);
            UpdateListBoxContent();
        }

        private void UpdateListBoxContent()
        {
            if (_playlistListBox != null)
            {
                _playlistListBox.DataContext = null;
            }

            _playlistListBox = new PlaylistListBox(_playlistListBoxViewModel);
            _playliseView.ListBoxContent.Content = _playlistListBox;
        }


        public void Refresh()
        {
            _playlistListBox?.Refresh();
        }

        public void FocusAtOnce()
        {
            _playlistListBox?.FocusAtOnce();
        }
    }
}
