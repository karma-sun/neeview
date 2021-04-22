using NeeLaboratory.ComponentModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NeeView
{
    public class PlaylistModel : BindableBase
    {
        static PlaylistModel() => Current = new PlaylistModel();
        public static PlaylistModel Current { get; }


        private List<object> _playlistCollection;
        private PlaylistListBoxModel _listBoxModel;


        private PlaylistModel()
        {
            Config.Current.Playlist.AddPropertyChanged(nameof(PlaylistConfig.CurrentPlaylist),
                (s, e) => RaisePropertyChanged(nameof(SelectedItem)));

            this.AddPropertyChanged(nameof(SelectedItem),
                (s, e) => SelectedItemChanged());

            UpdateListBoxModel();
        }


        public List<object> PlaylistCollection
        {
            get
            {
                if (_playlistCollection is null)
                {
                    UpdatePlaylistCollection();
                }
                return _playlistCollection;
            }
            set { SetProperty(ref _playlistCollection, value); }
        }

        public string SelectedItem
        {
            get { return Config.Current.Playlist.CurrentPlaylist; }
            set
            {
                if (Config.Current.Playlist.CurrentPlaylist != value)
                {
                    Config.Current.Playlist.CurrentPlaylist = value;
                }
            }
        }

        public PlaylistListBoxModel ListBoxModel
        {
            get { return _listBoxModel; }
            set { SetProperty(ref _listBoxModel, value); }
        }


        private void SelectedItemChanged()
        {
            if (!PlaylistCollection.Contains(SelectedItem))
            {
                UpdatePlaylistCollection();
            }

            UpdateListBoxModel();
        }

        // TODO: 非同期化
        private void UpdatePlaylistCollection()
        {
            var list = new List<string>();

            try
            {
                var folder = System.IO.Path.GetFullPath(Config.Current.Playlist.PlaylistFolder);
                var playlists = Directory.GetFiles(folder, "*.nvpls");
                list = playlists.ToList();
            }
            catch (DirectoryNotFoundException)
            {
            }

            if (this.SelectedItem != null && !list.Any(e => e == this.SelectedItem))
            {
                list.Add(this.SelectedItem);
            }

            list.Sort();


            var groups = list.GroupBy(e => Path.GetDirectoryName(e) == Config.Current.Playlist.PlaylistFolder);
            var items = new List<object>();
            var normals = groups.FirstOrDefault(e => e.Key == true);
            var externals = groups.FirstOrDefault(e => e.Key == false);
            if (normals != null)
            {
                items.AddRange(normals);
                if (externals != null)
                {
                    items.Add(new Separator());
                    items.AddRange(externals);
                }
            }
            else
            {
                items.AddRange(externals);
            }

            this.PlaylistCollection = items;
        }

        private void UpdateListBoxModel()
        {
            this.ListBoxModel = new PlaylistListBoxModel(this.SelectedItem);
        }

        public void AddPlaylist()
        {
            this.ListBoxModel.Add(BookOperation.Current.GetPage()?.SystemPath);
        }

        public void Flush()
        {
            this.ListBoxModel?.Flush();
        }
    }
}
