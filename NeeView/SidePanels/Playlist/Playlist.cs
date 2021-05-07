using NeeLaboratory;
using NeeLaboratory.Collection;
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
    public class Playlist : BindableBase
    {
        private ObservableCollection<PlaylistItem> _items;
        private MultiMap<string, PlaylistItem> _itemsMap = new MultiMap<string, PlaylistItem>();
        private string _playlistPath;
        private object _lock = new object();
        private bool _isDarty;
        private DateTime _lastWriteTime;
        private bool _isEditable;


        public Playlist()
        {
        }

        public Playlist(string path, PlaylistSource playlistFile)
        {
            this.Path = path;
            this.Items = new ObservableCollection<PlaylistItem>(playlistFile.Items.Select(e => new PlaylistItem(e)));
            this.IsEditable = true;
        }



        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event EventHandler<PlaylistItemRenamedEventArgs> ItemRenamed;


        public string Path
        {
            get { return _playlistPath; }
            set { SetProperty(ref _playlistPath, value); }
        }

        public DateTime LastWriteTime
        {
            get { return _lastWriteTime; }
            set { SetProperty(ref _lastWriteTime, value); }
        }

        public bool IsEditable
        {
            get { return _isEditable && this.Items != null; }
            set { SetProperty(ref _isEditable, value); }
        }

        public bool IsDarty
        {
            get { return _isDarty; }
            set
            {
                if (_isDarty != value)
                {
                    lock (_lock)
                    {
                        _isDarty = value;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<PlaylistItem> Items
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
                    _itemsMap = _items.ToMultiMap(x => x.Path, x => x);

                    if (_items != null)
                    {
                        _items.CollectionChanged += OnCollectionChanged;
                    }

                    RaisePropertyChanged();
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
            }
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



        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var oldItems = e.OldItems?.Cast<PlaylistItem>();
            var newItems = e.NewItems?.Cast<PlaylistItem>();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    _itemsMap = _items.ToMultiMap(x => x.Path, x => x);
                    break;

                case NotifyCollectionChangedAction.Add:
                    foreach (PlaylistItem item in newItems)
                    {
                        _itemsMap.Add(item.Path, item);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (PlaylistItem item in oldItems)
                    {
                        _itemsMap.Remove(item.Path, item);
                    };
                    break;

                case NotifyCollectionChangedAction.Move:
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (PlaylistItem item in oldItems.Except(newItems))
                    {
                        _itemsMap.Remove(item.Path, item);
                    }
                    foreach (PlaylistItem item in newItems.Except(oldItems))
                    {
                        _itemsMap.Add(item.Path, item);
                    }
                    break;

                default:
                    throw new NotSupportedException();
            }

            Debug.Assert(this.Items.Count == _itemsMap.Count);

            CollectionChanged?.Invoke(this, e);
        }

        public PlaylistSource CreatePlaylistSource()
        {
            lock (_lock)
            {
                return new PlaylistSource(this.Items.Select(e => e.ToPlaylistItem()));
            }
        }

        public PlaylistItem Find(string path)
        {
            if (this.Items is null) return null;

            lock (_lock)
            {
                if (_itemsMap.TryGetValue(path, out var item))
                {
                    return item;
                }
            }

            return null;
        }

        public List<PlaylistItem> Collect(IEnumerable<string> paths)
        {
            if (paths is null) return new List<PlaylistItem>();

            lock (_lock)
            {
                return paths.Select(e => _itemsMap.TryGetValue(e, out var item) ? item : null)
                    .Where(e => e != null)
                    .ToList();
            }
        }

        public List<PlaylistItem> Add(IEnumerable<string> paths)
        {
            var targetItem = Config.Current.Playlist.IsFirstIn ? this.Items.FirstOrDefault() : null;
            return Insert(paths, targetItem);
        }

        public PlaylistItem Insert(string path, PlaylistItem targetItem)
        {
            if (!IsEditable) return null;
            if (path is null) return null;

            var item = Find(path);
            if (item != null)
            {
                return item;
            }

            var index = targetItem != null ? this.Items.IndexOf(targetItem) : this.Items.Count;
            if (index < 0) return null;
            item = new PlaylistItem(path);
            this.Items.Insert(index, item);

            _isDarty = true;

            return item;
        }

        public List<PlaylistItem> Insert(IEnumerable<string> paths, PlaylistItem targetItem)
        {
            if (!IsEditable) return null;
            if (paths is null && !paths.Any()) return null;

            if (paths.Count() == 1)
            {
                var result = Insert(paths.First(), targetItem);
                return result is null ? null : new List<PlaylistItem> { result };
            }

            List<PlaylistItem> news = new List<PlaylistItem>();

            lock (_lock)
            {
                var oldCount = this.Items.Count;

                var index = targetItem != null ? this.Items.IndexOf(targetItem) : this.Items.Count;

                var pathList = paths.ToList();

                var entries = this.Items.Select(e => e.Path).ToList();
                var keepEntries = pathList.Intersect(entries).ToList();
                var newEntries = pathList.Except(entries).Select(e => new PlaylistItem(e)).ToList();

                this.Items = new ObservableCollection<PlaylistItem>(this.Items.Take(index).Concat(newEntries.Concat(this.Items.Skip(index))));
                Debug.Assert(this.Items.Count == oldCount + newEntries.Count);

                var already = Collect(keepEntries);
                news = newEntries.Concat(already).ToList();

                _isDarty = true;
            }

            return news;
        }

        public void Remove(PlaylistItem item)
        {
            if (!IsEditable) return;
            if (item is null) return;

            lock (_lock)
            {
                this.Items.Remove(item);

                _isDarty = true;
            }
        }

        public void Remove(IEnumerable<PlaylistItem> items)
        {
            if (!IsEditable) return;
            if (items is null && !items.Any()) return;

            if (items.Count() == 1)
            {
                Remove(items.First());
            }

            lock (_lock)
            {
                this.Items = new ObservableCollection<PlaylistItem>(this.Items.Except(items));

                _isDarty = true;
            }
        }

        public async Task DeleteInvalidItemsAsync(CancellationToken token)
        {
            if (!IsEditable) return;

            // 削除項目収集
            var unlinked = new List<PlaylistItem>();
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

        public void Move(PlaylistItem item, PlaylistItem targetItem)
        {
            if (!IsEditable) return;
            if (item is null) return;
            if (item == targetItem) return;

            lock (_lock)
            {
                var oldIndex = this.Items.IndexOf(item);
                if (oldIndex < 0) return;
                var newIndex = targetItem is null ? this.Items.Count - 1 : this.Items.IndexOf(targetItem);
                this.Items.Move(oldIndex, newIndex);

                _isDarty = true;
            }
        }

        public void Move(IEnumerable<PlaylistItem> items, PlaylistItem targetItem)
        {
            if (!IsEditable) return;
            if (items is null || !items.Any()) return;
            if (items.Contains(targetItem)) return;

            if (items.Count() == 1)
            {
                Move(items.First(), targetItem);
                return;
            }

            lock (_lock)
            {
                var oldCount = this.Items.Count;

                var itemsA = items
                    .Select(e => (value: e, index: this.Items.IndexOf(e)))
                    .Where(e => e.index >= 0)
                    .OrderBy(e => e.index)
                    .Select(e => e.value)
                    .ToList();

                var itemsB = this.Items.Except(itemsA).ToList();

                var isMoveDown = targetItem is null || this.Items.IndexOf(itemsA.First()) < this.Items.IndexOf(targetItem);
                var index = targetItem is null ? itemsB.Count : itemsB.IndexOf(targetItem) + (isMoveDown ? 1 : 0);

                this.Items = new ObservableCollection<PlaylistItem>(itemsB.Take(index).Concat(itemsA.Concat(itemsB.Skip(index))));
                Debug.Assert(this.Items.Count == oldCount);

                _isDarty = true;
            }
        }

        public void Sort()
        {
            if (!IsEditable) return;

            lock (_lock)
            {
                var sorted = this.Items.OrderBy(e => e.Path, NaturalSort.Comparer);
                this.Items = new ObservableCollection<PlaylistItem>(sorted);

                _isDarty = true;
            }
        }

        public bool Rename(PlaylistItem item, string newName)
        {
            if (!IsEditable) return false;
            if (item is null) return false;
            if (item.Name == newName) return false;

            lock (_lock)
            {
                var oldName = item.Name;
                item.Name = newName;
                ItemRenamed?.Invoke(this, new PlaylistItemRenamedEventArgs(item, oldName));

                _isDarty = true;
            }

            return true;
        }

        public void Open(PlaylistItem item)
        {
            if (item is null) return;

            // try jump in current book.
            var isSuccess = BookOperation.Current.JumpPageWithPath(this, item.Path);
            if (isSuccess)
            {
                return;
            }

            // try open page at new book.
            var options = BookLoadOption.None;
            BookHub.Current.RequestLoad(this, item.Path, null, options, true);
        }

        public bool CanMoveUp(PlaylistItem item)
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

        public void MoveUp(PlaylistItem item)
        {
            if (!CanMoveUp(item)) return;

            var index = this.Items.IndexOf(item);
            var target = Config.Current.Playlist.IsGroupBy ? this.Items.Take(index).LastOrDefault(e => e.Place == item.Place) : this.Items[index - 1];
            if (target is null) return;

            Move(item, target);
        }

        public bool CanMoveDown(PlaylistItem item)
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

        public void MoveDown(PlaylistItem item)
        {
            if (!CanMoveDown(item)) return;

            var index = this.Items.IndexOf(item);

            var target = Config.Current.Playlist.IsGroupBy ? this.Items.Skip(index + 1).FirstOrDefault(e => e.Place == item.Place) : this.Items[index + 1];
            if (target is null) return;

            Move(item, target);
        }


        #region Save

        private SimpleDelayAction _delaySave = new SimpleDelayAction();
        private SemaphoreSlim _saveSemaphore = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _cancellationTokenSource;

        public void DelaySave(Action savedCallback)
        {
            _delaySave.Request(() => Save(savedCallback), TimeSpan.FromSeconds(1.0));
        }

        public void Flush()
        {
            _delaySave.Flush();

            // NOTE: 確実に処理の終了を待つ
            _saveSemaphore.Wait();
            _saveSemaphore.Release();
        }

        public void CancelSave()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
        }

        public void Save(Action savedCallback, bool isForce = false)
        {
            if (!this.IsEditable) return;
            if (this.Path is null) return;

            PlaylistSource source;
            lock (_lock)
            {
                if (!_isDarty && !isForce) return;
                source = CreatePlaylistSource();
                _isDarty = false;
            }

            // 非同期保存
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(async () => await SaveAsync(this.Path, source, savedCallback, _cancellationTokenSource.Token));
        }

        private async Task SaveAsync(string path, PlaylistSource source, Action SavedCallback, CancellationToken token)
        {
            await _saveSemaphore.WaitAsync();
            try
            {
                if (!this.IsEditable) return;
                token.ThrowIfCancellationRequested();

                await RetryAction.RetryActionAsync(() =>
                {
                    this.LastWriteTime = DateTime.Now;
                    source.Save(path, true);
                }
                , 3, 1000, token);

                SavedCallback?.Invoke();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                if (this.IsEditable)
                {
                    this.IsEditable = false; // 以後編集不可
                    ToastService.Current.Show(new Toast(ex.Message, Properties.Resources.Playlist_FailedToSave, ToastIcon.Error));
                }
            }
            finally
            {
                _saveSemaphore.Release();
            }
        }

        #endregion Save

        #region Load

        public static Playlist Load(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return new Playlist();
            }

            var file = new FileInfo(path);

            if (file.Exists)
            {
                var playlistFile = LoadPlaylist(path);
                if (playlistFile != null)
                {
                    var playlist = new Playlist(path, playlistFile);
                    playlist.IsEditable = !file.Attributes.HasFlag(FileAttributes.ReadOnly);
                    return playlist;
                }
                else
                {
                    return new Playlist();
                }
            }
            else
            {
                var playlist = new Playlist(path, new PlaylistSource());
                playlist.IsDarty = true;
                return playlist;
            }
        }

        private static PlaylistSource LoadPlaylist(string path)
        {
            try
            {
                return PlaylistSourceTools.Load(path);
            }
            catch (Exception ex)
            {
                ToastService.Current.Show(new Toast(ex.Message, Properties.Resources.Playlist_FailedToLoad, ToastIcon.Error));
                return null;
            }
        }

        #endregion Load

        #region Move to another playlist

        public List<string> CollectAnotherPlaylists()
        {
            return PlaylistHub.GetPlaylistFiles()
                .Select(e => e.FullName)
                .Where(e => e != _playlistPath)
                .ToList();
        }

        public void MoveToAnotherPlaylist(string path, IEnumerable<PlaylistItem> items)
        {
            if (path is null) return;
            if (items is null || !items.Any()) return;
            if (path == _playlistPath) return;

            var playlist = Load(path);
            if (!playlist.IsEditable) return;

            var newItems = playlist.Add(items.Select(e => e.Path).ToArray());

            var map = items.Where(e => e.IsNameChanged).ToDictionary(e => e.Path, e => e);
            foreach (var item in newItems)
            {
                if (map.TryGetValue(item.Path, out var mapItem))
                {
                    item.Name = mapItem.Name;
                }
            }

            playlist.Save(() => AppDispatcher.Invoke(() => Remove(items)));
        }

        #endregion Move to another playlist
    }
}
