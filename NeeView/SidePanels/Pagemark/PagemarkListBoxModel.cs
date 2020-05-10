using NeeLaboratory.ComponentModel;
using NeeView.Collections;
using NeeView.Collections.Generic;
using NeeView.Windows;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace NeeView
{
    public class PagemarkListBoxModel : BindableBase
    {
        // Fields

        private TreeListNode<IPagemarkEntry> _selectedItem;
        private Toast _toast;
        private PagemarkList _pagemarkList;

        // Constructors

        public PagemarkListBoxModel(PagemarkList pagemarkList)
        {
            _pagemarkList = pagemarkList;

            PagemarkCollection.Current.PagemarkChanged += PagemarkCollection_PagemarkChanged;
            BookOperation.Current.BookChanged += (s, e) => UpdateItems();

            UpdateItems();
        }


        // Events

        public event EventHandler SelectedItemChanged;

        // Properties

        public PagemarkList PagemarkList => _pagemarkList;

        public PagemarkCollection PagemarkCollection => PagemarkCollection.Current;


        private ObservableCollection<TreeListNode<IPagemarkEntry>> _items;
        public ObservableCollection<TreeListNode<IPagemarkEntry>> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }


        public TreeListNode<IPagemarkEntry> SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem != null && _selectedItem != value)
                {
                    _selectedItem.IsSelected = false;
                }

                if (SetProperty(ref _selectedItem, value))
                {
                    if (_selectedItem != null)
                    {
                        _selectedItem.IsSelected = true;
                    }
                }
            }
        }


        private string _placeDispString;
        public string PlaceDispString
        {
            get { return _placeDispString; }
            set { SetProperty(ref _placeDispString, value); }
        }


        // Methods

        private void PagemarkCollection_PagemarkChanged(object sender, PagemarkCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case EntryCollectionChangedAction.Replace:
                case EntryCollectionChangedAction.Reset:
                    UpdateItems();
                    break;
                case EntryCollectionChangedAction.Add:
                case EntryCollectionChangedAction.Remove:
                    if (e.Item.Value is PagemarkFolder folder && folder.Path == BookOperation.Current.Address)
                    {
                        UpdateItems();
                    }
                    break;
            }


            if (_toast != null)
            {
                _toast.Cancel();
                _toast = null;
            }
        }

        public void UpdateItems()
        {
            if (_pagemarkList.IsCurrentBook)
            {
                PlaceDispString = LoosePath.GetFileName(BookOperation.Current.Address);
                var node = PagemarkCollection.Items.Children.FirstOrDefault(e => e.Value is PagemarkFolder folder && folder.Path == BookOperation.Current.Address);
                if (node != null)
                {
                    Items = node.Children;
                }
                else
                {
                    Items = null;
                }
            }
            else
            {
                PlaceDispString = Properties.Resources.WordAllPagemark;
                Items = PagemarkCollection.Items.Children;
            }
        }

        public void Decide(TreeListNode<IPagemarkEntry> item, bool allowChangeBook)
        {
            if (item.Value is Pagemark pagemark)
            {
                bool isJumped = BookOperation.Current.JumpPagemarkInPlace(pagemark);
                if (!isJumped && allowChangeBook)
                {
                    var options = pagemark.EntryName != null ? BookLoadOption.IsPage : BookLoadOption.None;
                    BookHub.Current.RequestLoad(pagemark.Path, pagemark.EntryName, options, true);
                }
            }
        }

        public void Expand(TreeListNode<IPagemarkEntry> item, bool isExpanded)
        {
            if (item.CanExpand && item.IsExpanded != isExpanded)
            {
                item.IsExpanded = isExpanded;
            }
        }


        public bool Remove(TreeListNode<IPagemarkEntry> item)
        {
            var next = item.Next ?? item.Previous ?? item.Parent;

            var memento = new TreeListNodeMemento<IPagemarkEntry>(item);

            var isRemoved = PagemarkCollection.Current.Remove(item);
            if (isRemoved)
            {
                if (item.Value is PagemarkFolder)
                {
                    var count = item.Count(e => e.Value is Pagemark);
                    if (count > 0)
                    {
                        _toast = new Toast(string.Format(Properties.Resources.DialogPagemarkFolderDelete, count), null, ToastIcon.Information, Properties.Resources.WordRestore, () => PagemarkCollection.Current.Restore(memento));
                        ToastService.Current.Show(_toast);
                    }
                }

                if (next != null)
                {
                    SelectedItem = next;
                }
            }

            return isRemoved;
        }

        #region Pagemark Special

        public void SetSelectedItem(string place, string entryName)
        {
            var node = PagemarkCollection.Current.FindNode(place, entryName);
            if (node == null)
            {
                return;
            }

            SelectedItem = node;
            SelectedItemChanged?.Invoke(this, null);
        }


        /// <summary>
        /// 指定のマーカーに移動。存在しなければ移動しない
        /// </summary>
        public void Jump(string place, string entryName)
        {
            var node = PagemarkCollection.Current.FindNode(place, entryName);
            if (node != null)
            {
                SelectedItem = node;
                SelectedItemChanged?.Invoke(this, null);
            }
        }

        #endregion


        public bool Rename(TreeListNode<IPagemarkEntry> item, string newName)
        {
            if (item == null) return false;

            if (item.Value is Pagemark pagemark)
            {
                PagemarkCollection.Current.RenameDispName(item, newName);
                PagemarkCollection.Current.SortOne(item);
                return true;
            }

            return false;
        }


        // ページマークを戻る
        public void PrevPagemark()
        {
            if (BookHub.Current.IsLoading) return;

            var node = GetNeighborPagemark(SelectedItem, -1);
            if (node != null)
            {
                SelectedItem = node;

                if (node.Value is Pagemark)
                {
                    Decide(node, true);
                }

                SelectedItemChanged?.Invoke(this, null);
            }
            else
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.NotifyPagemarkPrevFailed);
            }
        }

        // ページマークを進む
        public void NextPagemark()
        {
            if (BookHub.Current.IsLoading) return;

            var node = GetNeighborPagemark(SelectedItem, +1);
            if (node != null)
            {
                SelectedItem = node;

                if (node.Value is Pagemark)
                {
                    Decide(node, true);
                }

                SelectedItemChanged?.Invoke(this, null);
            }
            else
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.NotifyPagemarkNextFailed);
            }
        }

        private TreeListNode<IPagemarkEntry> GetNeighborPagemark(TreeListNode<IPagemarkEntry> item, int direction)
        {
            if (direction == 0) throw new ArgumentOutOfRangeException(nameof(direction));

            if (item == null)
            {
                var pagemarks = PagemarkCollection.Current.Items.GetExpandedCollection().Where(e => e.Value is Pagemark).ToList();
                return direction >= 0 ? pagemarks.FirstOrDefault() : pagemarks.LastOrDefault();
            }
            else
            {
                var pagemarks = PagemarkCollection.Current.Items.GetExpandedCollection().Where(e => e.Value is Pagemark || e == item).ToList();
                if (pagemarks.Count <= 0)
                {
                    return null;
                }
                int index = pagemarks.IndexOf(item);
                if (index < 0)
                {
                    return null;
                }
                return pagemarks.ElementAtOrDefault(index + direction);
            }
        }

        public int IndexOfSelectedItem()
        {
            return IndexOfExpanded(SelectedItem);
        }

        public int IndexOfExpanded(TreeListNode<IPagemarkEntry> item)
        {
            if (item == null)
            {
                return -1;
            }

            return PagemarkCollection.Current.Items.GetExpandedCollection().IndexOf(item);
        }
    }
}
