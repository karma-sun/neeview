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
        private PlaylistListBoxModel _model;
        private ObservableCollection<PlaylistListBoxItem> _items;
        private Visibility _visibility = Visibility.Hidden;


        public PlaylistListBoxViewModel()
        {
            this.CollectionViewSource = new CollectionViewSource();
            this.CollectionViewSource.Filter += CollectioonViewSourceFilter;

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



        public ObservableCollection<PlaylistListBoxItem> Items
        {
            get { return _items; }
            private set { SetProperty(ref _items, value); }
        }

        private PlaylistListBoxItem _selectedItem;

        public PlaylistListBoxItem SelectedItem
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


        private void UpdateIsFirstIn()
        {
            RaisePropertyChanged(nameof(IsFirstIn));
            RaisePropertyChanged(nameof(IsLastIn));
        }

        public void SetModel(PlaylistListBoxModel model)
        {
            // TODO: 購読の解除。今の所Modelのほうが寿命が短いので問題ないが、安全のため。

            _model = model;

            _model.AddPropertyChanged(nameof(_model.Items),
                (s, e) => UpdateItems());

            _model.AddPropertyChanged(nameof(_model.IsEditable),
                (s, e) => RaisePropertyChanged(nameof(IsEditable)));

            UpdateItems();
        }

        private void CollectioonViewSourceFilter(object sender, FilterEventArgs e)
        {
            if (Config.Current.Playlist.IsCurrentBookFilterEnabled && BookOperation.Current.IsValid)
            {
                var item = (PlaylistListBoxItem)e.Item;
                e.Accepted = BookOperation.Current.Book.Pages.Any(x => x.SystemPath == item.Path);
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
                this.CollectionViewSource.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PlaylistListBoxItem.DispPlace)));
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

        public PlaylistListBoxItem AddCurrentPage()
        {
            var path = BookOperation.Current.GetPage()?.SystemPath;
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


        public List<PlaylistListBoxItem> Insert(IEnumerable<string> paths, PlaylistListBoxItem targetItem)
        {
            if (_model.Items is null) return null;

            this.SelectedItem = null;

            var items = _model.Insert(paths, targetItem);

            this.SelectedItem = items.FirstOrDefault();

            return items;
        }

        public void Remove(IEnumerable<PlaylistListBoxItem> items)
        {
            if (_model.Items is null) return;

            var index = GetSelectedIndex();
            this.SelectedItem = null;

            _model.Remove(items);

            SetSelectedIndex(index);
        }

        public void Move(IEnumerable<PlaylistListBoxItem> items, PlaylistListBoxItem targetItem)
        {
            if (_model.Items is null) return;

            _model.Move(items, targetItem);
        }

        public bool Rename(PlaylistListBoxItem item, string newName)
        {
            return _model.Rename(item, newName);
        }

        public void Open(PlaylistListBoxItem item)
        {
            _model.Open(item);
        }
    }
}
