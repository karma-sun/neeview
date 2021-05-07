using Microsoft.Win32;
using NeeLaboratory.ComponentModel;
using NeeView.IO;
using NeeView.Threading;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    public class PlaylistHub : BindableBase
    {
        static PlaylistHub() => Current = new PlaylistHub();
        public static PlaylistHub Current { get; }

        private List<object> _playlistCollection;
        private Playlist _playlist;
        private int _playlistLockCount;
        private CancellationTokenSource _deleteInvalidItemsCancellationToken;
        private bool _isPlaylistDarty;


        private PlaylistHub()
        {
            if (SelectedItem != Config.Current.Playlist.DefaultPlaylist)
            {
                if (!File.Exists(SelectedItem))
                {
                    SelectedItem = Config.Current.Playlist.DefaultPlaylist;
                }
            }

            InitializeFileWatcher();

            Config.Current.Playlist.AddPropertyChanged(nameof(PlaylistConfig.CurrentPlaylist),
                (s, e) => RaisePropertyChanged(nameof(SelectedItem)));

            Config.Current.Playlist.AddPropertyChanged(nameof(PlaylistConfig.IsCurrentBookFilterEnabled),
                (s, e) => RaisePropertyChanged(nameof(FilterMessage)));

            BookOperation.Current.BookChanged +=
                (s, e) => RaisePropertyChanged(nameof(FilterMessage));

            // NOTE: 応急処置
            BookOperation.Current.LinkPlaylistHub(this);

            this.AddPropertyChanged(nameof(SelectedItem),
                (s, e) => SelectedItemChanged());

            UpdatePlaylist();
        }


        public event NotifyCollectionChangedEventHandler PlaylistCollectionChanged;


        public string DefaultPlaylist => Config.Current.Playlist.DefaultPlaylist;
        public string NewPlaylist => Path.Combine(Config.Current.Playlist.PlaylistFolder, "NewPlaylist.nvpls");

        public List<object> PlaylistFiles
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

        public Playlist Playlist
        {
            get { return _playlist; }
            set
            {
                if (_playlist != value)
                {
                    if (_playlist != null)
                    {
                        _playlist.CollectionChanged -= Playlist_CollectionChanged;
                        _playlist.ItemRenamed -= Playlist_ItemRenamed;
                    }

                    _playlist = value;

                    if (_playlist != null)
                    {
                        _playlist.CollectionChanged += Playlist_CollectionChanged;
                        _playlist.ItemRenamed += Playlist_ItemRenamed;
                    }

                    RaisePropertyChanged();
                    PlaylistCollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
            }
        }


        public string FilterMessage
        {
            get { return Config.Current.Playlist.IsCurrentBookFilterEnabled ? LoosePath.GetFileName(BookOperation.Current.Address) : null; }
        }


        private void Playlist_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.Playlist?.DelaySave(StartPlaylistWatch);
            PlaylistCollectionChanged?.Invoke(this, e);
        }

        private void Playlist_ItemRenamed(object sender, PlaylistItemRenamedEventArgs e)
        {
            this.Playlist?.DelaySave(StartPlaylistWatch);
        }

        private void SelectedItemChanged()
        {
            if (!this.PlaylistFiles.Contains(SelectedItem))
            {
                UpdatePlaylistCollection();
            }

            UpdatePlaylist();
        }

        public static List<FileInfo> GetPlaylistFiles()
        {
            var directory = new DirectoryInfo(System.IO.Path.GetFullPath(Config.Current.Playlist.PlaylistFolder));
            if (directory.Exists)
            {
                return directory.GetFiles("*.nvpls").ToList();
            }
            return new List<FileInfo>();
        }

        public void UpdatePlaylistCollection()
        {
            try
            {
                _playlistLockCount++;
                var selectedItem = this.SelectedItem;

                var list = GetPlaylistFiles().Select(e => e.FullName).ToList();

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

                this.PlaylistFiles = items;
                this.SelectedItem = selectedItem;
            }
            finally
            {
                _playlistLockCount--;
            }
        }

        private void UpdatePlaylist()
        {
            if (_playlistLockCount <= 0 && (this.Playlist is null || _isPlaylistDarty || this.Playlist?.Path != this.SelectedItem))
            {
                if (!_isPlaylistDarty && this.Playlist != null)
                {
                    this.Playlist.Flush();
                }

                LoadPlaylist();

                StartPlaylistWatch();

                // NOTE: ファイルが存在しない場合、ここで保存
                this.Playlist.DelaySave(StartPlaylistWatch);
            }
        }

        private void LoadPlaylist()
        {
            this.Playlist = Playlist.Load(this.SelectedItem);
            _isPlaylistDarty = false;
        }

        private void ReloadPlaylist()
        {
            if (this.SelectedItem == this.Playlist?.Path)
            {
                LoadPlaylist();
            }
        }


        private void StartPlaylistWatch()
        {
            StartFileWatch(this.SelectedItem);
        }


        public void Flush()
        {
            this.Playlist?.Flush();
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
            }
        }

        public bool CanRename()
        {
            return this.Playlist?.IsEditable == true;
        }

        public bool Rename(string newName)
        {
            if (!CanRename()) return false;

            this.Playlist?.Flush();

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
            _playlist?.Flush();
            BookHub.Current.RequestLoad(this, SelectedItem, null, BookLoadOption.IsBook, true);
        }


        #region Playlist Controls

        public async Task DeleteInvalidItemsAsync()
        {
            _deleteInvalidItemsCancellationToken?.Cancel();
            _deleteInvalidItemsCancellationToken = new CancellationTokenSource();
            await _playlist?.DeleteInvalidItemsAsync(_deleteInvalidItemsCancellationToken.Token);
        }

        public void SortItems()
        {
            _playlist?.Sort();
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

        private void StartFileWatch(string path, bool isForce = false)
        {
            _delayReloadAction.Cancel();
            _watcher.Start(path);
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (string.Compare(SelectedItem, e.FullPath, StringComparison.OrdinalIgnoreCase) != 0) return;

            Debug.WriteLine($"## Watcher.Changed: {e.FullPath}");

            if (_playlist.LastWriteTime.AddSeconds(5.0) < DateTime.Now)
            {
                _delayReloadAction.Request(() => ReloadPlaylist(), TimeSpan.FromSeconds(1.0));
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

            if (_playlist.Path != e.OldFullPath) return;

            _playlist.Path = e.FullPath;
            SelectedItem = e.FullPath;
        }

        #endregion FileSystemWatcher
    }
}
