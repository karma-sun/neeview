using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;

namespace NeeView
{
    public class HistoryListBoxViewModel : BindableBase
    {
        private HistoryList _model;
        private BookHistory _selectedItem;
        private Visibility _visibility = Visibility.Hidden;
        private bool _isDarty = true;


        public HistoryListBoxViewModel(HistoryList model)
        {
            _model = model;
            _model.AddPropertyChanged(nameof(HistoryList.FilterPath), HistoryList_FilterPathChanged);
            _model.AddPropertyChanged(nameof(HistoryList.Items), (s, e) => RaisePropertyChanged(nameof(Items)));

            BookHub.Current.HistoryChanged += BookHub_HistoryChanged;
            BookHub.Current.HistoryListSync += BookHub_HistoryListSync;
        }


        public event EventHandler SelectedItemChanging;
        public event EventHandler SelectedItemChanged;


        public bool IsThumbnailVisibled => _model.IsThumbnailVisibled;

        public List<BookHistory> Items => _model.Items;

        public BookHistory SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; RaisePropertyChanged(); }
        }

        public Visibility Visibility
        {
            get { return _visibility; }
            set { _visibility = value; RaisePropertyChanged(); }
        }


        private void HistoryList_FilterPathChanged(object sender, PropertyChangedEventArgs e)
        {
            _isDarty = true;
            UpdateItems();
        }

        private void BookHub_HistoryListSync(object sender, BookHubPathEventArgs e)
        {
            SelectedItemChanging?.Invoke(this, null);
            SelectedItem = BookHistoryCollection.Current.Find(e.Path);
            SelectedItemChanged?.Invoke(this, null);

        }

        private void BookHub_HistoryChanged(object sender, BookMementoCollectionChangedArgs e)
        {
            _isDarty = _isDarty || e.HistoryChangedType != BookMementoCollectionChangedType.Update;
            if (_isDarty && Visibility == Visibility.Visible)
            {
                UpdateItems();
            }
        }

        public void UpdateItems()
        {
            if (_isDarty)
            {
                _isDarty = false;

                AppDispatcher.Invoke(() => SelectedItemChanging?.Invoke(this, null));

                var item = SelectedItem;
                _model.UpdateItems();
                SelectedItem = Items.Count > 0 ? item : null;

                AppDispatcher.Invoke(() => SelectedItemChanged?.Invoke(this, null));
            }
        }

        public void Remove(IEnumerable<BookHistory> items)
        {
            if (items == null) return;

            // 位置ずらし
            SelectedItemChanging?.Invoke(this, null);
            SelectedItem = GetNeighbor(SelectedItem, items);
            SelectedItemChanged?.Invoke(this, null);

            BookHistoryCollection.Current.Remove(items.Select(e => e.Path));
        }

        public void Remove(BookHistory item)
        {
            if (item == null) return;

            // 位置ずらし
            SelectedItemChanging?.Invoke(this, null);
            SelectedItem = GetNeighbor(item);
            SelectedItemChanged?.Invoke(this, null);

            // 削除
            BookHistoryCollection.Current.Remove(item.Path);
        }

        // となりを取得
        private BookHistory GetNeighbor(BookHistory item, IEnumerable<BookHistory> excludes = null)
        {
            if (Items == null || Items.Count <= 0) return null;

            int index = Items.IndexOf(item);
            if (index < 0) return Items[0];

            var items = Items.Skip(index).Concat(Items.Take(index));
            if (excludes != null)
            {
                items = items.Where(e => !excludes.Contains(e));
            }

            return items.FirstOrDefault();
        }

        public void Load(string path)
        {
            if (path == null) return;
            BookHub.Current?.RequestLoad(this, path, null, BookLoadOption.KeepHistoryOrder | BookLoadOption.SkipSamePlace | BookLoadOption.IsBook, true);
        }

    }
}
