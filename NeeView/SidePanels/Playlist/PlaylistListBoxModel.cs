using NeeLaboratory.ComponentModel;
using NeeView.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class PlaylistListBoxModel : BindableBase
    {
        private ObservableCollection<PlaylistListBoxItem> _items;
        private string _playlistPath;
        private DelayAction _delaySave;
        private object _lock = new object();
        private bool _isDarty;


        public PlaylistListBoxModel(string path)
        {
            _delaySave = new DelayAction(App.Current.Dispatcher, TimeSpan.FromSeconds(0.2), () => SavePlaylist(true), TimeSpan.FromSeconds(0.5));

            // NOTE: 非同期で読み込む
            var async = LoadAsync(path);
        }

        public string PlaylistPath => _playlistPath;

        public ObservableCollection<PlaylistListBoxItem> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
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


        private async Task LoadAsync(string path)
        {
            if (this.Items != null)
            {
                Save();
            }

            if (string.IsNullOrEmpty(path))
            {
                path = System.IO.Path.Combine(Config.Current.Playlist.PlaylistFolder, "Default.nvplst");
            }

            if (System.IO.File.Exists(path))
            {
                try
                {
                    var playlist = await PlaylistTools.LoadAsync(path);
                    lock (_lock)
                    {
                        this.Items = new ObservableCollection<PlaylistListBoxItem>(playlist.Items.Select(e => new PlaylistListBoxItem(e)));
                        _playlistPath = path;
                        _isDarty = false;
                    }
                }
                catch (Exception ex)
                {
                    ToastService.Current.Show(new Toast(ex.Message, Properties.Resources.Playlist_ErrorDialog_Title, ToastIcon.Error));
                    lock (_lock)
                    {
                        this.Items = null;
                        _playlistPath = null;
                        _isDarty = false;
                    }
                }
            }
            else
            {
                lock (_lock)
                {
                    this.Items = new ObservableCollection<PlaylistListBoxItem>();
                    _playlistPath = path;
                    _isDarty = false;
                }
            }
        }


        public void Save()
        {
            // TODO: 例外処理 ... 保存での例外はそのまま例外としてアプリ停止させる？

            if (this.Items is null) return;
            if (_playlistPath is null) throw new InvalidOperationException();
            if (!_isDarty) return;

            lock (_lock)
            {
                Debug.WriteLine($"Save Playlist: {_playlistPath}");

                var playlist = new Playlist(this.Items.Select(e => e.ToPlaylistItem()));
                playlist.Save(_playlistPath, true);

                _isDarty = false;
            }
        }

        private void SavePlaylist(bool sync)
        {
            Save();

            // TODO: 他のNeeViewとの同期
            //if (sync)
            //{
            //    RemoteCommandService.Current.Send(new RemoteCommand("LoadPlaylist"), RemoteCommandDelivery.All);
            //}
        }


        public string RenamePlaylist(string newName)
        {
            if (this.Items is null) return null;
            if (string.IsNullOrWhiteSpace(newName)) return null;

            Save();

            try
            {
                var newPath = FileIO.CreateUniquePath(Path.Combine(Path.GetDirectoryName(_playlistPath), newName + Path.GetExtension(_playlistPath)));
                var file = new FileInfo(_playlistPath);
                if (file.Exists)
                {
                    file.MoveTo(newPath);
                }
                _playlistPath = newPath;
            }
            catch (Exception ex)
            {
                ToastService.Current.Show(new Toast(ex.Message, Properties.Resources.Playlist_ErrorDialog_Title, ToastIcon.Error));
                return null;
            }

            // TODO: 他のNeeViewとの同期 ... ファイルシステムの名前変更監視？

            return _playlistPath;
        }

        public PlaylistListBoxItem Add(string path)
        {
            return Insert(path, null);
        }

        public PlaylistListBoxItem Insert(string path, PlaylistListBoxItem targetItem)
        {
            if (this.Items is null) return null;
            if (path is null) return null;

            var item = new PlaylistListBoxItem(path);

            lock (_lock)
            {
                if (this.Items.Any(e => e.Path == path)) return null;

                var index = targetItem != null ? this.Items.IndexOf(targetItem) : -1;
                if (index >= 0)
                {
                    this.Items.Insert(index, item);
                }
                else
                {
                    this.Items.Add(item);
                }

                _isDarty = true;
            }

            _delaySave.Request();

            return item;
        }

        public void Remove(PlaylistListBoxItem item)
        {
            if (this.Items is null) return;
            if (item is null) return;

            lock (_lock)
            {
                this.Items.Remove(item);

                _isDarty = true;
            }

            _delaySave.Request();
        }

        public async Task DeleteInvalidItemsAsync(CancellationToken token)
        {
            if (this.Items is null) return;

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
            lock (_lock)
            {
                foreach (var node in unlinked)
                {
                    this.Items.Remove(node);
                }
                _isDarty = true;
            }

            _delaySave.Request();
        }

        public void Move(PlaylistListBoxItem item, PlaylistListBoxItem targetItem)
        {
            if (this.Items is null) return;
            if (item is null) return;
            if (item == targetItem) return;

            lock (_lock)
            {
                var oldIndex = this.Items.IndexOf(item);
                var newIndex = targetItem is null ? this.Items.Count - 1 : this.Items.IndexOf(targetItem);
                this.Items.Move(oldIndex, newIndex);

                _isDarty = true;
            }

            _delaySave.Request();
        }

        public void Sort()
        {
            if (this.Items is null) return;

            lock (_lock)
            {
                var sorted = this.Items.OrderBy(e => Path.GetDirectoryName(e.Path), NaturalSort.Comparer);
                this.Items = new ObservableCollection<PlaylistListBoxItem>(sorted);
                _isDarty = true;
            }

            _delaySave.Request();
        }


        public bool Rename(PlaylistListBoxItem item, string newName)
        {
            if (this.Items is null) return false;
            if (item is null) return false;
            if (item.Name == newName) return false;

            lock (_lock)
            {
                item.Name = newName;

                _isDarty = true;
            }

            _delaySave.Request();

            return true;
        }


        public void Open(PlaylistListBoxItem item)
        {
            if (this.Items is null) return;
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
    }
}
