using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class PlaylistListBoxModel : BindableBase
    {
        private ObservableCollection<PlaylistListBoxItem> _items;
        private string _playlistPath;
        private SimpleDelayAction _delaySave = new SimpleDelayAction();
        private object _lock = new object();
        private bool _isDarty;
        private DateTime _lastWriteTime;
        private SemaphoreSlim _saveSemaphore = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _cancellationTokenSource;


        public PlaylistListBoxModel(string path)
        {
            PlaylistPath = path;

            // NOTE: 非同期で読み込む
            var async = LoadAsync(path);
        }


        public event EventHandler ItemsStateChanged;


        public string PlaylistPath
        {
            get { return _playlistPath; }
            set { SetProperty(ref _playlistPath, value); }
        }

        ////public bool IsFileBusy { get; private set; }

        public DateTime LastWriteTime => _lastWriteTime;

        public ObservableCollection<PlaylistListBoxItem> Items
        {
            get { return _items; }
            set
            {
                if (_items != value)
                {
                    if (_items != null)
                    {
                        _items.CollectionChanged += OnCollectionChanged;
                    }

                    _items = value;

                    if (_items != null)
                    {
                        _items.CollectionChanged += OnCollectionChanged;
                    }

                    RaisePropertyChanged();
                    ItemsStateChanged?.Invoke(this, null);
                }
            }
        }


        private bool _isEditable;
        public bool IsEditable
        {
            get { return _isEditable && this.Items != null; }
            set { SetProperty(ref _isEditable, value); }
        }


        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ItemsStateChanged?.Invoke(this, null);
        }


        public bool IsThumbnailVisibled
        {
            get
            {
                switch (Config.Current.Playlist.PanelListItemStyle)
                {
                    default:
                        return false;
                    case PanelListItemStyle.Content:
                        return Config.Current.Panels.ContentItemProfile.ImageWidth > 0.0;
                    case PanelListItemStyle.Banner:
                        return Config.Current.Panels.BannerItemProfile.ImageWidth > 0.0;
                }
            }
        }

        public PanelListItemStyle PanelListItemStyle
        {
            get => Config.Current.Playlist.PanelListItemStyle;
            set => Config.Current.Playlist.PanelListItemStyle = value;
        }


        public void Flush()
        {
            _delaySave.Flush();
        }

        public async Task ReloadAsync()
        {
            if (_playlistPath is null) return;

            var playlist = await LoadPlaylistAsync(_playlistPath);
            if (playlist != null)
            {
                SetPlaylist(_playlistPath, playlist);
            }
        }

        private async Task LoadAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                ResetPlaylist();
                return;
            }

            var file = new FileInfo(path);

            if (file.Exists)
            {
                var playlist = await LoadPlaylistAsync(path);
                if (playlist != null)
                {
                    SetPlaylist(path, playlist);
                    IsEditable = !file.Attributes.HasFlag(FileAttributes.ReadOnly);
                }
                else
                {
                    ResetPlaylist();
                }
            }
            else
            {
                SetPlaylist(path, new Playlist());
                IsEditable = true;
                Save();
            }
        }

        private async Task<Playlist> LoadPlaylistAsync(string path)
        {
            try
            {
                return await PlaylistTools.LoadAsync(path);
            }
            catch (Exception ex)
            {
                ToastService.Current.Show(new Toast(ex.Message, Properties.Resources.Playlist_FailedToLoad, ToastIcon.Error));
                return null;
            }
        }

        private void SetPlaylist(string path, Playlist playlist)
        {
            if (path is null) throw new ArgumentNullException(nameof(path));
            if (playlist is null) throw new ArgumentNullException(nameof(playlist));

            lock (_lock)
            {
                this.Items = new ObservableCollection<PlaylistListBoxItem>(playlist.Items.Select(e => new PlaylistListBoxItem(e)));
                this.PlaylistPath = path;
                _isDarty = false;
            }
        }

        private void ResetPlaylist()
        {
            lock (_lock)
            {
                this.Items = null;
                PlaylistPath = null;
                IsEditable = false;
                _isDarty = false;
            }
        }


        public void RequestSave()
        {
            _delaySave.Request(() => Save(), TimeSpan.FromSeconds(0.5));
        }

        public void Save()
        {
            if (!IsEditable) return;
            if (_playlistPath is null) return;
            if (!_isDarty) return;

            Playlist playlist;

            lock (_lock)
            {
                playlist = new Playlist(this.Items.Select(e => e.ToPlaylistItem()));
                _isDarty = false;
            }

            // 非同期保存
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var async = SaveAsync(_playlistPath, playlist, _cancellationTokenSource.Token);
        }

        private async Task SaveAsync(string path, Playlist playlis, CancellationToken token)
        {
            await _saveSemaphore.WaitAsync();
            try
            {
                if (!IsEditable) return;
                token.ThrowIfCancellationRequested();

                await RetryAction.RetryActionAsync(() =>
                {
                    _lastWriteTime = DateTime.Now;
                    playlis.Save(path, true);
                }
                , 3, 1000, token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                if (IsEditable)
                {
                    IsEditable = false; // 以後編集不可
                    ToastService.Current.Show(new Toast(ex.Message, Properties.Resources.Playlist_FailedToSave, ToastIcon.Error));
                }
            }
            finally
            {
                _saveSemaphore.Release();
            }
        }



        public PlaylistListBoxItem Find(string path)
        {
            if (this.Items is null) return null;

            lock (_lock)
            {
                return this.Items.FirstOrDefault(e => e.Path == path);
            }
        }



        public List<PlaylistListBoxItem> Insert(IEnumerable<string> paths, PlaylistListBoxItem targetItem)
        {
            if (!IsEditable) return null;
            if (paths is null) return null;

            List<PlaylistListBoxItem> news = new List<PlaylistListBoxItem>();

            lock (_lock)
            {
                var index = targetItem != null ? this.Items.IndexOf(targetItem) : this.Items.Count;

                var pathList = paths.ToList();

                var entries = this.Items.Select(e => e.Path).ToList();
                var entriesA = pathList.Intersect(entries).ToList();
                var entriesB = pathList.Except(entries).Select(e => new PlaylistListBoxItem(e)).ToList();

                this.Items = new ObservableCollection<PlaylistListBoxItem>(this.Items.Take(index).Concat(entriesB.Concat(this.Items.Skip(index))));

                var already = this.Items.Where(e => entriesA.IndexOf(e.Path) >= 0).ToList();

                news = entriesB.Concat(already).ToList();

                _isDarty = true;
            }

            RequestSave();

            return news;
        }

        public void Remove(IEnumerable<PlaylistListBoxItem> items)
        {
            if (!IsEditable) return;
            if (items is null) return;

            lock (_lock)
            {
                foreach (var item in items)
                {
                    this.Items.Remove(item);
                }

                _isDarty = true;
            }

            RequestSave();
        }

        public async Task DeleteInvalidItemsAsync(CancellationToken token)
        {
            if (!IsEditable) return;

            // 削除項目収集
            var unlinked = new List<PlaylistListBoxItem>();
            foreach (var node in this.Items)
            {
                if (!await ArchiveEntryUtility.ExistsAsync(node.Path, token))
                {
                    unlinked.Add(node);
                }
            }

            // 削除実行
            Remove(unlinked);
            ToastService.Current.Show(new Toast(string.Format(Properties.Resources.Playlist_DeleteItemsMessage, unlinked.Count)));
        }

        public void Move(PlaylistListBoxItem item, PlaylistListBoxItem targetItem)
        {
            if (!IsEditable) return;
            if (item is null) return;
            if (item == targetItem) return;

            lock (_lock)
            {
                var oldIndex = this.Items.IndexOf(item);
                var newIndex = targetItem is null ? this.Items.Count - 1 : this.Items.IndexOf(targetItem);
                this.Items.Move(oldIndex, newIndex);

                _isDarty = true;
            }

            RequestSave();
        }

        public void Move(IEnumerable<PlaylistListBoxItem> items, PlaylistListBoxItem targetItem)
        {
            if (!IsEditable) return;
            if (items is null || !items.Any()) return;
            if (items.Contains(targetItem)) return;

            lock (_lock)
            {
                var itemsA = items
                    .Select(e => (value: e, index: this.Items.IndexOf(e)))
                    .Where(e => e.index >= 0)
                    .OrderBy(e => e.index)
                    .Select(e => e.value)
                    .ToList();

                var itemsB = this.Items.Except(itemsA).ToList();

                var isMoveDown = targetItem is null || this.Items.IndexOf(itemsA.First()) < this.Items.IndexOf(targetItem);
                var index = targetItem is null ? itemsB.Count : itemsB.IndexOf(targetItem) + (isMoveDown ? 1 : 0);

                this.Items = new ObservableCollection<PlaylistListBoxItem>(itemsB.Take(index).Concat(itemsA.Concat(itemsB.Skip(index))));

                _isDarty = true;
            }

            RequestSave();
        }

        public void Sort()
        {
            if (!IsEditable) return;

            lock (_lock)
            {
                var sorted = this.Items.OrderBy(e => e.Path, NaturalSort.Comparer);
                this.Items = new ObservableCollection<PlaylistListBoxItem>(sorted);
                _isDarty = true;
            }

            RequestSave();
        }

        public bool Rename(PlaylistListBoxItem item, string newName)
        {
            if (!IsEditable) return false;
            if (item is null) return false;
            if (item.Name == newName) return false;

            lock (_lock)
            {
                item.Name = newName;

                _isDarty = true;
            }

            RequestSave();

            return true;
        }


        public void Open(PlaylistListBoxItem item)
        {
            if (item is null) return;

            // try jump in current book.
            var isSuccess = BookOperation.Current.JumpPageWithSystemPath(this, item.Path);
            if (isSuccess)
            {
                return;
            }

            // try open page at new book.
            var options = BookLoadOption.None;
            BookHub.Current.RequestLoad(this, item.Path, null, options, true);
        }


        public bool CanMoveUp(PlaylistListBoxItem item)
        {
            if (!IsEditable) return false;
            if (item is null) return false;

            var index = this.Items.IndexOf(item);
            if (index <= 0) return false;

            if (Config.Current.Playlist.IsGroupBy)
            {
                return this.Items.Take(index).Any(e => e.Place == item.Place);
            }

            return true;
        }

        public void MoveUp(PlaylistListBoxItem item)
        {
            if (!CanMoveUp(item)) return;

            var index = this.Items.IndexOf(item);
            var target = Config.Current.Playlist.IsGroupBy ? this.Items.Take(index).LastOrDefault(e => e.Place == item.Place) : this.Items[index - 1];
            if (target is null) return;

            Move(item, target);
        }

        public bool CanMoveDown(PlaylistListBoxItem item)
        {
            if (!IsEditable) return false;
            if (item is null) return false;

            var index = this.Items.IndexOf(item);
            if (index < 0) return false;
            if (index >= this.Items.Count - 1) return false;

            if (Config.Current.Playlist.IsGroupBy)
            {
                return this.Items.Skip(index + 1).Any(e => e.Place == item.Place);
            }

            return true;
        }

        public void MoveDown(PlaylistListBoxItem item)
        {
            if (!CanMoveDown(item)) return;

            var index = this.Items.IndexOf(item);

            var target = Config.Current.Playlist.IsGroupBy ? this.Items.Skip(index + 1).FirstOrDefault(e => e.Place == item.Place) : this.Items[index + 1];
            if (target is null) return;

            Move(item, target);
        }

    }


}
