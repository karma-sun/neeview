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
    public class Playlist : BindableBase
    {
        private ObservableCollection<PlaylistItem> _items;
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
                return this.Items.FirstOrDefault(e => e.Path == path);
            }
        }

        // TODO: 指数的に重くなるので改善を
        public List<PlaylistItem> Collect(IEnumerable<string> paths)
        {
            if (paths is null) return new List<PlaylistItem>();

            lock(_lock)
            {
                return  paths.Select(e => this.Items.FirstOrDefault(x => x.Path == e)).Where(e => e != null).ToList();
            }
        }

        public List<PlaylistItem> Add(IEnumerable<string> paths)
        {
            var targetItem = Config.Current.Playlist.IsFirstIn ? this.Items.FirstOrDefault() : null;
            return Insert(paths, targetItem);
        }

        public List<PlaylistItem> Insert(IEnumerable<string> paths, PlaylistItem targetItem)
        {
            if (!IsEditable) return null;
            if (paths is null) return null;

            List<PlaylistItem> news = new List<PlaylistItem>();

            lock (_lock)
            {
                var index = targetItem != null ? this.Items.IndexOf(targetItem) : this.Items.Count;

                var pathList = paths.ToList();

                var entries = this.Items.Select(e => e.Path).ToList();
                var entriesA = pathList.Intersect(entries).ToList();
                var entriesB = pathList.Except(entries).Select(e => new PlaylistItem(e)).ToList();

                this.Items = new ObservableCollection<PlaylistItem>(this.Items.Take(index).Concat(entriesB.Concat(this.Items.Skip(index))));

                var already = this.Items.Where(e => entriesA.IndexOf(e.Path) >= 0).ToList();

                news = entriesB.Concat(already).ToList();

                _isDarty = true;
            }

            return news;
        }

        public void Remove(IEnumerable<PlaylistItem> items)
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

                this.Items = new ObservableCollection<PlaylistItem>(itemsB.Take(index).Concat(itemsA.Concat(itemsB.Skip(index))));

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
                item.Name = newName;
                
                _isDarty = true;
            }

            return true;
        }

        public void Open(PlaylistItem item)
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
    }


    public class BookPlaylist
    {
        private Book _book;
        private Playlist _playlist;

        public BookPlaylist(Book book, Playlist playlist)
        {
            _book = book ?? throw new ArgumentNullException(nameof(book));
            _playlist = playlist;
        }

        public bool IsEnabled(Page page)
        {
            if (page is null)
            {
                return false;
            }

            if (_playlist is null || !_playlist.IsEditable)
            {
                return false;
            }

            if (_book.IsMedia || _book.IsPlaylist || _book.IsTemporary)
            {
                return false;
            }

            return true;
        }

        public bool Contains(Page page)
        {
            if (_playlist is null) return false;
            if (page is null) return false;

            return Find(page) != null;
        }

        public PlaylistItem Find(Page page)
        {
            if (_playlist is null) return null;
            if (page is null) return null;

            return _playlist.Find(page.SystemPath);
        }

        public PlaylistItem Add(Page page)
        {
            if (_playlist is null) return null;
            if (page is null) return null;

            return Add(new List<Page> { page })?.FirstOrDefault();
        }

        public List<PlaylistItem> Add(IEnumerable<Page> pages)
        {
            if (_playlist is null) return null;
            if (pages is null) return null;

            return _playlist.Add(pages.Select(e => e.SystemPath).ToList());
        }


        public bool Remove(Page page)
        {
            if (_playlist is null) return false;
            if (page is null) return false;

            return Remove(new List<Page> { page });
        }

        public bool Remove(IEnumerable<Page> pages)
        {
            if (_playlist is null) return false;
            if (pages is null) return false;

            var items = _playlist.Collect(pages.Select(e => e.SystemPath).ToList());
            if (items.Any())
            {
                _playlist.Remove(items);
                return true;
            }
            else
            {
                return false;
            }
        }

        public PlaylistItem Set(Page page, bool isEntry)
        {
            if (_playlist is null) return null;
            if (page is null) return null;

            if (isEntry)
            {
                return Add(page);
            }
            else
            {
                Remove(page);
                return null;
            }
        }

        public PlaylistItem Toggle(Page page)
        {
            if (_playlist is null) return null;
            if (page is null) return null;

            return Set(page, Find(page) is null);
        }

        // TODO: 指数的に重くなるので対策を
        public List<Page> Collect()
        {
            if (_playlist?.Items is null) return new List<Page>();

            var items = _playlist.Items.Where(e => e.Path.StartsWith(_book.Address)).Select(e => e.Path).ToList();
            if (items.Any())
            {
                return _book.Pages.Where(e => items.Contains(e.SystemPath)).ToList();
            }
            else
            {
                return new List<Page>();
            }
        }

    }
}
