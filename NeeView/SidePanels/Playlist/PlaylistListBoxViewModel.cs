using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace NeeView
{
    public class PlaylistListBoxViewModel : BindableBase
    {
        private Playlist _model;
        private ObservableCollection<PlaylistItem> _items;
        private Visibility _visibility = Visibility.Hidden;


        public PlaylistListBoxViewModel()
        {
            this.CollectionViewSource = new CollectionViewSource();
            this.CollectionViewSource.Filter += CollectionViewSourceFilter;

            Config.Current.Playlist.AddPropertyChanged(nameof(PlaylistConfig.IsGroupBy),
                (s, e) => UpdateGroupBy());

            Config.Current.Playlist.AddPropertyChanged(nameof(PlaylistConfig.IsCurrentBookFilterEnabled),
                (s, e) => UpdateFilter(true));

            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.IsDecoratePlace),
                (s, e) => UpdateDispPlace());

            Config.Current.Playlist.AddPropertyChanged(nameof(PlaylistConfig.IsFirstIn),
                (s, e) => UpdateIsFirstIn());

            BookOperation.Current.BookChanged +=
                (s, e) => UpdateFilter(false);
        }


        public bool IsThumbnailVisibled => _model.IsThumbnailVisibled;

        public CollectionViewSource CollectionViewSource { get; private set; }



        public ObservableCollection<PlaylistItem> Items
        {
            get { return _items; }
            private set { SetProperty(ref _items, value); }
        }

        private PlaylistItem _selectedItem;

        public PlaylistItem SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }


        public Visibility Visibility
        {
            get { return _visibility; }
            set { _visibility = value; RaisePropertyChanged(); }
        }

        public bool IsEditable
        {
            get { return _model.IsEditable; }
        }

        public bool IsGroupBy
        {
            get { return Config.Current.Playlist.IsGroupBy; }
        }

        public bool IsFirstIn
        {
            get { return Config.Current.Playlist.IsFirstIn; }
            set
            {
                if (Config.Current.Playlist.IsFirstIn != value)
                {
                    Config.Current.Playlist.IsFirstIn = value;
                    UpdateIsFirstIn();
                }
            }
        }

        public bool IsLastIn
        {
            get { return !IsFirstIn; }
            set { IsFirstIn = !value; }
        }

        public string ErrorMessage => _model.ErrorMessage;


        private void UpdateIsFirstIn()
        {
            RaisePropertyChanged(nameof(IsFirstIn));
            RaisePropertyChanged(nameof(IsLastIn));
        }

        public void SetModel(Playlist model)
        {
            // TODO: 購読の解除。今の所Modelのほうが寿命が短いので問題ないが、安全のため。

            _model = model;

            _model.AddPropertyChanged(nameof(_model.Items),
                (s, e) => AppDispatcher.Invoke(() => UpdateItems()));

            _model.AddPropertyChanged(nameof(_model.IsEditable),
                (s, e) => RaisePropertyChanged(nameof(IsEditable)));

            UpdateItems();
        }

        private void CollectionViewSourceFilter(object sender, FilterEventArgs e)
        {
            if (Config.Current.Playlist.IsCurrentBookFilterEnabled && BookOperation.Current.IsValid)
            {
                var item = (PlaylistItem)e.Item;
                var book = BookOperation.Current.Book;
                e.Accepted = book is null || (item.Path.StartsWith(book.Address) && book.Pages.PageMap.ContainsKey(item.Path));
            }
            else
            {
                e.Accepted = true;
            }
        }

        private void UpdateDispPlace()
        {
            if (this.Items is null) return;

            foreach (var item in this.Items)
            {
                item.UpdateDispPlace();
            }

            UpdateGroupBy();
        }

        private void UpdateGroupBy()
        {
            RaisePropertyChanged(nameof(IsGroupBy));

            this.CollectionViewSource.GroupDescriptions.Clear();
            if (Config.Current.Playlist.IsGroupBy)
            {
                this.CollectionViewSource.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PlaylistItem.DispPlace)));
            }
        }

        private void UpdateFilter(bool isForce)
        {
            if (isForce || Config.Current.Playlist.IsCurrentBookFilterEnabled)
            {
                this.CollectionViewSource.View.Refresh();
            }
        }

        private void UpdateItems()
        {
            if (this.Items != _model.Items)
            {
                this.Items = _model.Items;
                this.CollectionViewSource.Source = this.Items;
                UpdateGroupBy();
            }
        }


        public bool IsLRKeyEnabled()
        {
            return Config.Current.Panels.IsLeftRightKeyEnabled || _model.PanelListItemStyle == PanelListItemStyle.Thumbnail;
        }

        private int GetSelectedIndex()
        {
            return this.Items.IndexOf(this.SelectedItem);
        }

        private void SetSelectedIndex(int index)
        {
            if (this.Items.Count > 0)
            {
                index = MathUtility.Clamp(index, 0, this.Items.Count - 1);
                this.SelectedItem = this.Items[index];
            }
        }

        public PlaylistItem AddCurrentPage()
        {
            var path = BookOperation.Current.GetPage()?.EntryFullName;
            if (path is null) return null;

            var targetItem = this.IsFirstIn ? this.Items.FirstOrDefault() : null;
            var result = Insert(new List<string> { path }, targetItem);
            return result?.FirstOrDefault();
        }

        public bool CanMoveUp()
        {
            return _model.CanMoveUp(this.SelectedItem);
        }

        public void MoveUp()
        {
            _model.MoveUp(this.SelectedItem);
        }

        public bool CanMoveDown()
        {
            return _model.CanMoveDown(this.SelectedItem);
        }

        public void MoveDown()
        {
            _model.MoveDown(this.SelectedItem);
        }


        public List<PlaylistItem> Insert(IEnumerable<string> paths, PlaylistItem targetItem)
        {
            if (_model.Items is null) return null;

            this.SelectedItem = null;

            var items = _model.Insert(paths, targetItem);

            this.SelectedItem = items.FirstOrDefault();

            return items;
        }

        public void Remove(IEnumerable<PlaylistItem> items)
        {
            if (_model.Items is null) return;

            var index = GetSelectedIndex();
            this.SelectedItem = null;

            _model.Remove(items);

            SetSelectedIndex(index);
        }

        public void Move(IEnumerable<PlaylistItem> items, PlaylistItem targetItem)
        {
            if (_model.Items is null) return;

            _model.Move(items, targetItem);
        }

        public List<string> CollectAnotherPlaylists()
        {
            return _model.CollectAnotherPlaylists();
        }


        public void MoveToAnotherPlaylist(string path, List<PlaylistItem> items)
        {
            if (_model.Items is null) return;

            _model.MoveToAnotherPlaylist(path, items);
        }


        public bool Rename(PlaylistItem item, string newName)
        {
            return _model.Rename(item, newName);
        }

        public void Open(PlaylistItem item)
        {
            _model.Open(item);
        }

        public bool CanMovePrevious()
        {
            return this.Items != null;
        }

        public bool MovePrevious()
        {
            this.CollectionViewSource.View.MoveCurrentTo(this.SelectedItem);
            this.CollectionViewSource.View.MoveCurrentToPrevious();
            var item = this.CollectionViewSource.View.CurrentItem as PlaylistItem;
            if (item != null)
            {
                this.SelectedItem = item;
                _model.Open(this.SelectedItem);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CanMoveNext()
        {
            return this.Items != null;
        }

        public bool MoveNext()
        {
            this.CollectionViewSource.View.MoveCurrentTo(this.SelectedItem);
            this.CollectionViewSource.View.MoveCurrentToNext();
            var item = this.CollectionViewSource.View.CurrentItem as PlaylistItem;
            if (item != null)
            {
                this.SelectedItem = item;
                _model.Open(this.SelectedItem);
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
