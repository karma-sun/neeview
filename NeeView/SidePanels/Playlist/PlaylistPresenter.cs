using System;

namespace NeeView
{
    public class PlaylistPresenter
    {
        private PlaylistView _playliseView;
        private PlaylisHub _playlistHub;

        private PlaylistListBox _playlistListBox;
        private PlaylistListBoxViewModel _playlistListBoxViewModel = new PlaylistListBoxViewModel();


        public PlaylistPresenter(PlaylistView playlistView, PlaylisHub playlistModel)
        {
            _playliseView = playlistView;
            _playlistHub = playlistModel;

            _playlistHub.AddPropertyChanged(nameof(PlaylistHub.Playlist),
                (s, e) => UpdateListBox());

            Config.Current.Playlist.AddPropertyChanged(nameof(PlaylistConfig.PanelListItemStyle),
                (s, e) => UpdateListBoxContent());

            UpdateListBox();
        }


        public PlaylistView PlaylistView => _playliseView;
        public PlaylistListBox PlaylistListBox=> _playlistListBox;
        public PlaylisHub PlaylistHub => _playlistHub;


        private void UpdateListBox()
        {
            _playlistListBoxViewModel.SetModel(_playlistHub.Playlist);
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
