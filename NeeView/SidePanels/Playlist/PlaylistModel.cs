using Microsoft.Win32;
using NeeLaboratory.ComponentModel;
using NeeView.IO;
using NeeView.Threading;
using System;
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
        private bool _isListBoxModeDarty;


        private PlaylistModel()
        {
            InitializeFileWatcher();

            Config.Current.Playlist.AddPropertyChanged(nameof(PlaylistConfig.CurrentPlaylist),
                (s, e) => RaisePropertyChanged(nameof(SelectedItem)));

            Config.Current.Playlist.AddPropertyChanged(nameof(PlaylistConfig.IsCurrentBookFilterEnabled),
                (s, e) => RaisePropertyChanged(nameof(FilterMessage)));

            BookOperation.Current.BookChanged +=
                (s, e) => RaisePropertyChanged(nameof(FilterMessage));

            this.AddPropertyChanged(nameof(SelectedItem),
                (s, e) => SelectedItemChanged());

            UpdateListBoxModel();
        }


        public event EventHandler PlaylistItemsStateChanged;


        public string DefaultPlaylist => Config.Current.Playlist.DefaultPlaylist;
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
            get 
            { return Config.Current.Playlist.CurrentPlaylist; }
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
            set
            {
                if (_listBoxModel != value)
                {
                    if (_listBoxModel != null)
                    {
                        _listBoxModel.ItemsStateChanged -= ListBoxModel_ItemsStateChanged;
                    }

                    _listBoxModel = value;

                    if (_listBoxModel != null)
                    {
                        _listBoxModel.ItemsStateChanged += ListBoxModel_ItemsStateChanged;
                    }

                    RaisePropertyChanged();
                }
            }
        }


        public string FilterMessage
        {
            get { return Config.Current.Playlist.IsCurrentBookFilterEnabled ? LoosePath.GetFileName(BookOperation.Current.Address) : null; }
        }




        private void ListBoxModel_ItemsStateChanged(object sender, EventArgs e)
        {
            PlaylistItemsStateChanged?.Invoke(this, null);
        }

        private void SelectedItemChanged()
        {
            if (!this.PlaylistCollection.Contains(SelectedItem))
            {
                UpdatePlaylistCollection();
            }

            UpdateListBoxModel();
        }


        public void UpdatePlaylistCollection()
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
                var normals = groups.FirstOrDefault(e => e.Key == true)?.OrderBy(e => e != DefaultPlaylist).ThenBy(e => e, NaturalSort.Comparer);
                var externals = groups.FirstOrDefault(e => e.Key == false)?.OrderBy(e => e, NaturalSort.Comparer);

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
            if (_listBoxModelLockCount <= 0 && (this.ListBoxModel is null || _isListBoxModeDarty || this.ListBoxModel?.PlaylistPath != this.SelectedItem))
            {
                if (!_isListBoxModeDarty && this.ListBoxModel != null)
                {
                    this.ListBoxModel.Flush();
                }

                this.ListBoxModel = new PlaylistListBoxModel(this.SelectedItem);
                _isListBoxModeDarty = false;

                StartFileWatch(this.SelectedItem);
            }
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
                //UpdatePlaylistCollection();
            }
        }

        public bool CanRename()
        {
            return true;
        }

        public bool Rename(string newName)
        {
            if (!CanRename()) return false;

            this.ListBoxModel?.Save();

            try
            {
                var newPath = FileIO.CreateUniquePath(Path.Combine(Path.GetDirectoryName(SelectedItem), newName + Path.GetExtension(SelectedItem)));
                var file = new FileInfo(SelectedItem);
                if (file.Exists)
                {
                    file.MoveTo(newPath);
                }
                else
                {
                    SelectedItem = newPath;
                }
                return true;
            }
            catch (Exception ex)
            {
                ToastService.Current.Show(new Toast(ex.Message, Properties.Resources.Playlist_ErrorDialog_Title, ToastIcon.Error));
                return false;
            }
        }


        public void OpenAsBook()
        {
            _listBoxModel?.Flush();
            BookHub.Current.RequestLoad(this, SelectedItem, null, BookLoadOption.IsBook, true);
        }


        #region ListBoxModel Controls

        public async Task DeleteInvalidItemsAsync()
        {
            _deleteInvalidItemsCancellationToken?.Cancel();
            _deleteInvalidItemsCancellationToken = new CancellationTokenSource();
            await _listBoxModel?.DeleteInvalidItemsAsync(_deleteInvalidItemsCancellationToken.Token);
        }

        public void SortItems()
        {
            _listBoxModel?.Sort();
        }

        #endregion


        #region FileSystemWatcher

        private SingleFileWatcher _watcher;
        private SimpleDelayAction _delayReloadAction;

        private void InitializeFileWatcher()
        {
            _watcher = new SingleFileWatcher(SingleFileWaterOptions.FollowRename);
            _watcher.Changed += Watcher_Changed;
            _watcher.Deleted += Watcher_Deleted;
            _watcher.Renamed += Watcher_Renamed;

            _delayReloadAction = new SimpleDelayAction();
        }

        private void StartFileWatch(string path)
        {
            _delayReloadAction.Cancel();
            _watcher.Start(path);
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (string.Compare(SelectedItem, e.FullPath, StringComparison.OrdinalIgnoreCase) != 0) return;

            Debug.WriteLine($"## Watcher.Changed: {e.FullPath}");

            if (_listBoxModel.LastWriteTime.AddSeconds(5.0) < DateTime.Now)
            {
                _delayReloadAction.Request(async () => await _listBoxModel.ReloadAsync(), TimeSpan.FromSeconds(1.0));
            }
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if (string.Compare(SelectedItem, e.FullPath, StringComparison.OrdinalIgnoreCase) != 0) return;

            Debug.WriteLine($"## Watcher.Deleted: {e.FullPath}");

            SelectedItem = DefaultPlaylist;
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (string.Compare(SelectedItem, e.OldFullPath, StringComparison.OrdinalIgnoreCase) != 0) return;

            Debug.WriteLine($"## Watcher.Renamed: {e.OldFullPath} -> {e.FullPath}");

            if (_listBoxModel.PlaylistPath != e.OldFullPath) return;

            _listBoxModel.PlaylistPath = e.FullPath;
            SelectedItem = e.FullPath;
        }

        #endregion FileSystemWatcher
    }
}
