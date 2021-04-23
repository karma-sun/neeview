using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;

namespace NeeView
{
    public class PlaylistListBoxViewModel : BindableBase
    {
        private PlaylistListBoxModel _model;
        private ObservableCollection<PlaylistListBoxItem> _items;
        private PlaylistListBoxItem _selectedItem;
        private Visibility _visibility = Visibility.Hidden;


        public PlaylistListBoxViewModel(PlaylistListBoxModel model)
        {
            _model = model;

            _model.AddPropertyChanged(nameof(_model.Items),
                (s, e) => this.Items = _model.Items);

            this.Items = _model.Items;
        }


        public event EventHandler SelectedItemChanging;
        public event EventHandler SelectedItemChanged;


        public bool IsThumbnailVisibled => _model.IsThumbnailVisibled;


        public ObservableCollection<PlaylistListBoxItem> Items
        {
            get { return _items; }
            private set
            {
                if (_items != value)
                {
                    if (_items != null)
                    {
                        _items.CollectionChanged -= ItemsCollectionChanged;
                    }
                    _items = value;
                    if (_items != null)
                    {
                        _items.CollectionChanged += ItemsCollectionChanged;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        public PlaylistListBoxItem SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; RaisePropertyChanged(); }
        }

        public Visibility Visibility
        {
            get { return _visibility; }
            set { _visibility = value; RaisePropertyChanged(); }
        }


        private void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    SelectedItem = e.NewItems[0] as PlaylistListBoxItem;
                    break;
            }
        }

        public bool IsLRKeyEnabled()
        {
            return Config.Current.Panels.IsLeftRightKeyEnabled || _model.PanelListItemStyle == PanelListItemStyle.Thumbnail;
        }


        public void Add(IEnumerable<string> paths)
        {
            Insert(paths, null);
        }

        public void Insert(IEnumerable<string> paths, PlaylistListBoxItem targetItem)
        {
            SelectedItemChanging?.Invoke(this, null);

            PlaylistListBoxItem selectedItem = null;

            foreach (var path in paths)
            {
                selectedItem = _model.Insert(path, targetItem) ?? selectedItem;
            }

            this.SelectedItem = selectedItem ?? this.SelectedItem;
            SelectedItemChanged?.Invoke(this, null);
        }

        public void Remove(IEnumerable<PlaylistListBoxItem> items)
        {
            SelectedItemChanging?.Invoke(this, null);

            var index = _model.Items.IndexOf(_selectedItem);

            foreach (var item in items)
            {
                _model.Remove(item);
            }

            if (_model.Items.Count > 0)
            {
                index = MathUtility.Clamp(index, 0, _model.Items.Count - 1);
                this.SelectedItem = _model.Items[index];
            }

            SelectedItemChanged?.Invoke(this, null);
        }

        public void Move(IEnumerable<PlaylistListBoxItem> items, PlaylistListBoxItem targetItem)
        {
            SelectedItemChanging?.Invoke(this, null);

            foreach (var item in items)
            {
                _model.Move(item, targetItem);
            }

            SelectedItemChanged?.Invoke(this, null);
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
