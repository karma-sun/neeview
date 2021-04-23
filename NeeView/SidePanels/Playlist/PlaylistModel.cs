using Microsoft.Win32;
using NeeLaboratory.ComponentModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
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
        private int _listBoxModelLockCount;
        private CancellationTokenSource _deleteInvalidItemsCancellationToken;


        private PlaylistModel()
        {
            Config.Current.Playlist.AddPropertyChanged(nameof(PlaylistConfig.CurrentPlaylist),
                (s, e) => RaisePropertyChanged(nameof(SelectedItem)));

            this.AddPropertyChanged(nameof(SelectedItem),
                (s, e) => SelectedItemChanged());

            UpdateListBoxModel();
        }


        public string DefaultPlaylist => Path.Combine(Config.Current.Playlist.PlaylistFolder, "Default.nvpls");
        public string NewPlaylist => Path.Combine(Config.Current.Playlist.PlaylistFolder, "NewPlaylist.nvpls");

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
            if (SelectedItem != null && !PlaylistCollection.Contains(SelectedItem))
            {
                UpdatePlaylistCollection();
            }

            UpdateListBoxModel();
        }


        // TODO: 非同期化
        private void UpdatePlaylistCollection()
        {
            try
            {
                _listBoxModelLockCount++;
                var selectedItem = this.SelectedItem;

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

                if (!list.Contains(DefaultPlaylist))
                {
                    list.Add(DefaultPlaylist);
                }

                if (this.SelectedItem != null && !list.Any(e => e == this.SelectedItem))
                {
                    list.Add(this.SelectedItem);
                }

                var groups = list.GroupBy(e => Path.GetDirectoryName(e) == Config.Current.Playlist.PlaylistFolder);
                var normals = groups.FirstOrDefault(e => e.Key == true)?.OrderBy(e => e != DefaultPlaylist);
                var externals = groups.FirstOrDefault(e => e.Key == false)?.OrderBy(e => e);

                var items = new List<object>();
                items.AddRange(normals);
                if (externals != null)
                {
                    items.Add(new Separator());
                    items.AddRange(externals);
                }

                this.PlaylistCollection = items;
                this.SelectedItem = selectedItem;
            }
            finally
            {
                _listBoxModelLockCount--;
            }
        }

        private void UpdateListBoxModel()
        {
            if (this.ListBoxModel is null || _listBoxModelLockCount <= 0 && this.ListBoxModel?.PlaylistPath != this.SelectedItem)
            {
                this.ListBoxModel?.Flush();
                this.ListBoxModel = new PlaylistListBoxModel(this.SelectedItem);
            }
        }

        public void AddPlaylist()
        {
            this.ListBoxModel.Add(BookOperation.Current.GetPage()?.SystemPath);
        }

        public void Flush()
        {
            this.ListBoxModel?.Flush();
        }


        public void CreateNew()
        {
            var newPath = FileIO.CreateUniquePath(NewPlaylist);
            SelectedItem = newPath;
        }

        public void Open()
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "NeeView Playlist|*.nvpls|All|*.*";

            if (dialog.ShowDialog(App.Current.MainWindow) == true)
            {
                SelectedItem = dialog.FileName;
            }
        }

        public bool CanDelete()
        {
            return SelectedItem != null && SelectedItem != DefaultPlaylist;
        }

        public async Task DeleteAsync()
        {
            if (!CanDelete()) return;

            bool isSuccessed = await FileIO.Current.RemoveFileAsync(SelectedItem, Properties.Resources.Playlist_DeleteDialog_Title, null);
            if (isSuccessed)
            {
                SelectedItem = DefaultPlaylist;
                UpdatePlaylistCollection();
            }
        }

        public bool CanRename()
        {
            return true;
        }

        public bool Rename(string newName)
        {
            if (!CanRename()) return false;

            var newPath = this.ListBoxModel?.RenamePlaylist(newName);
            if (newPath != null)
            {
                SelectedItem = newPath;
                return true;
            }

            return false;
        }


        public void OpenAsBook()
        {
            _listBoxModel.Flush();
            BookHub.Current.RequestLoad(this, SelectedItem, null, BookLoadOption.IsBook, true);
        }

        public async Task DeleteInvalidItemsAsync()
        {
            _deleteInvalidItemsCancellationToken?.Cancel();
            _deleteInvalidItemsCancellationToken = new CancellationTokenSource();
            await _listBoxModel.DeleteInvalidItemsAsync(_deleteInvalidItemsCancellationToken.Token);
        }

        public void SortItems()
        {
            _listBoxModel.Sort();
        }
    }
}
