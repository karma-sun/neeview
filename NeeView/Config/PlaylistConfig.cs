using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class PlaylistConfig : BindableBase, IHasPanelListItemStyle
    {
        private PanelListItemStyle _panelListItemStyle;
        private bool _isGroupBy;
        private bool _isCurrentBookFilterEnabled;
        private bool _isFirstIn;

        [JsonInclude, JsonPropertyName(nameof(PlaylistFolder))]
        public string _playlistFolder;

        [JsonInclude, JsonPropertyName(nameof(CurrentPlaylist))]
        public string _currentPlaylist;


        [PropertyMember]
        public PanelListItemStyle PanelListItemStyle
        {
            get { return _panelListItemStyle; }
            set { SetProperty(ref _panelListItemStyle, value); }
        }

        [JsonIgnore]
        [PropertyPath(FileDialogType = Windows.Controls.FileDialogType.Directory)]
        public string PlaylistFolder
        {
            get { return _playlistFolder ?? SaveData.DefaultPlaylistsFolder; }
            set { SetProperty(ref _playlistFolder, (string.IsNullOrWhiteSpace(value) || value.Trim() == SaveData.DefaultPlaylistsFolder) ? null : value.Trim()); }
        }

        [JsonIgnore]
        public string DefaultPlaylist => System.IO.Path.Combine(PlaylistFolder, "Default.nvpls");

        [JsonIgnore]
        [PropertyPath(FileDialogType = FileDialogType.SaveFile, Filter = "NeeView Playlist|*.nvpls")]
        public string CurrentPlaylist
        {
            get { return _currentPlaylist ?? DefaultPlaylist; }
            set { SetProperty(ref _currentPlaylist, (string.IsNullOrWhiteSpace(value) || value.Trim() == DefaultPlaylist) ? null : value.Trim()); }
        }

        public bool IsGroupBy
        {
            get { return _isGroupBy; }
            set { SetProperty(ref _isGroupBy, value); }
        }

        public bool IsCurrentBookFilterEnabled
        {
            get { return _isCurrentBookFilterEnabled; }
            set { SetProperty(ref _isCurrentBookFilterEnabled, value); }
        }

        public bool IsFirstIn
        {
            get { return _isFirstIn; }
            set { SetProperty(ref _isFirstIn, value); }
        }

    }
}

