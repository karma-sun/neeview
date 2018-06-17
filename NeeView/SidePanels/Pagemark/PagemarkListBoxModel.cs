using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using NeeView.Windows;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace NeeView
{
    public class PagemarkListBoxModel : BindableBase
    {
        // Fields

        private ObservableCollection<TreeListNode<IPagemarkEntry>> _items;
        private TreeListNode<IPagemarkEntry> _selectedItem;
        private Toast _toast;

        // Constructors

        public PagemarkListBoxModel()
        {
            PagemarkCollection.Current.PagemarkChanged += PagemarkCollection_PagemarkChanged;
        }


        // Events

        public event CollectionChangeEventHandler Changing;
        public event CollectionChangeEventHandler Changed;

        public ObservableCollection<TreeListNode<IPagemarkEntry>> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }

        public TreeListNode<IPagemarkEntry> SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        // TODO: この参照方向はどうなの？
        public bool IsThumbnailVisibled => PagemarkList.Current.IsThumbnailVisibled;
        public PanelListItemStyle PanelListItemStyle => PagemarkList.Current.PanelListItemStyle;


        // Methods

        private void PagemarkCollection_PagemarkChanged(object sender, PagemarkCollectionChangedEventArgs e)
        {
            TreeListNode<IPagemarkEntry> selectedItem = null;

            if (e.Action == NotifyCollectionChangedAction.Remove && SelectedItem == e.Item)
            {
                int selectedIndex = Items.IndexOf(SelectedItem);
                if (selectedIndex >= 0)
                {
                    selectedIndex = selectedIndex < Items.Count - 1 ? selectedIndex + 1 : Items.Count - 1;
                    if (selectedIndex >= 0)
                    {
                        selectedItem = Items[selectedIndex];
                    }
                }
            }

            Refresh(selectedItem);

            if (_toast != null)
            {
                _toast.Cancel();
                _toast = null;
            }
        }

        public void Decide(TreeListNode<IPagemarkEntry> item)
        {
            switch (item.Value)
            {
                case Pagemark pagemark:

                    bool isJumped = BookOperation.Current.JumpPagemarkInPlace(pagemark);
                    if (!isJumped)
                    {
                        var options = pagemark.EntryName != null ? BookLoadOption.IsPage : BookLoadOption.None;
                        BookHub.Current.RequestLoad(pagemark.Place, pagemark.EntryName, options, true);
                    }
                    break;
                case PagemarkFolder folder:
                    if (item.Children.Count > 0)
                    {
                        item.IsExpanded = !item.IsExpanded;
                        Refresh();
                    }
                    break;
            }
        }

        public void Expand(TreeListNode<IPagemarkEntry> item, bool isExpanded)
        {
            if (item.IsExpandEnabled && item.IsExpanded != isExpanded)
            {
                item.IsExpanded = isExpanded;
                Refresh();
            }
        }

        private void Refresh(TreeListNode<IPagemarkEntry> selectedItem = null)
        {
            Changing?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null));
            var collection = PagemarkCollection.Current.Items.GetExpandedCollection();
            Items = new ObservableCollection<TreeListNode<IPagemarkEntry>>(collection);
            if (selectedItem != null)
            {
                SelectedItem = selectedItem;
            }
            Changed?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null));
        }

        public bool Remove(TreeListNode<IPagemarkEntry> item)
        {
            var memento = new TreeListNodeMemento<IPagemarkEntry>(item);

            var isRemoved = PagemarkCollection.Current.Remove(item);
            if (isRemoved)
            {
                if (item.Value is PagemarkFolder)
                {
                    var count = item.Count(e => e.Value is Pagemark);
                    if (count > 0)
                    {
                        _toast = new Toast(string.Format(Properties.Resources.DialogPagemarkFolderDelete, count), Properties.Resources.WordRestore, () => PagemarkCollection.Current.Restore(memento));
                        ToastService.Current.Show(_toast);
                    }
                }
            }

            return isRemoved;
        }


        public void SetSelectedItem(string place, string entryName)
        {
            var node = PagemarkCollection.Current.FindNode(place, entryName);
            if (node == null)
            {
                return;
            }

            if (Items.Contains(node))
            {
                SelectedItem = node;
            }
            else
            {
                for (var parent = node.Parent; parent != null; parent = parent.Parent)
                {
                    parent.IsExpanded = true;
                }
                Refresh();
                SelectedItem = node;
            }
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
            }
        }


        public void Move(DropInfo<TreeListNode<IPagemarkEntry>> dropInfo)
        {
            if (dropInfo == null) return;
            if (dropInfo.DragItem == dropInfo.DropItem) return;

            var item = dropInfo.DragItem;
            var target = dropInfo.DropItem;

            var indexFrom = Items.IndexOf(dropInfo.DragItem);
            var indexTo = Items.IndexOf(dropInfo.DropItem);


            const double margine = 0.25;

            if (target.Value is PagemarkFolder folder)
            {
                if (dropInfo.Position < margine)
                {
                    PagemarkCollection.Current.Move(item, target, -1);
                }
                else if (dropInfo.Position > (1.0 - margine) && !target.IsExpanded)
                {
                    PagemarkCollection.Current.Move(item, target, +1);
                }
                else
                {
                    PagemarkCollection.Current.MoveToChild(item, target);
                }
            }
            else
            {
                if (target.GetNext() == null && dropInfo.Position > (1.0 - margine))
                {
                    PagemarkCollection.Current.Move(item, target, +1);
                }
                else if (indexFrom < indexTo)
                {
                    PagemarkCollection.Current.Move(item, target, +1);
                }
                else
                {
                    PagemarkCollection.Current.Move(item, target, -1);
                }
            }
        }

        internal void NewFolder()
        {
            var node = new TreeListNode<IPagemarkEntry>(new PagemarkFolder() { Name = Properties.Resources.WordNewFolder });
            PagemarkCollection.Current.AddFirst(node);
            SelectedItem = node;
            Changed?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Add, node));
        }


        public void PrevPagemark()
        {
            if (BookHub.Current.IsLoading) return;

            if (!CanMoveSelected(-1))
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.NotifyPagemarkPrevFailed);
                return;
            }

            if (MoveSelected(-1))
            {
                if (SelectedItem?.Value is Pagemark)
                {
                    Decide(SelectedItem);
                }
            }
        }

        public void NextPagemark()
        {
            if (BookHub.Current.IsLoading) return;

            if (!CanMoveSelected(+1))
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.NotifyPagemarkNextFailed);
                return;
            }

            if (MoveSelected(+1))
            {
                if (SelectedItem?.Value is Pagemark)
                {
                    Decide(SelectedItem);
                }
            }
        }

        public bool CanMoveSelected(int direction)
        {
            var pagemarks = Items.Where(e => e.Value is Pagemark);

            if (SelectedItem == null)
            {
                return pagemarks.Count() > 0;
            }
            else
            {
                var index = Items.IndexOf(SelectedItem);
                return direction > 0
                    ? index < Items.IndexOf(pagemarks.Last())
                    : index > Items.IndexOf(pagemarks.First());
            }
        }

        public bool MoveSelected(int direction)
        {
            if (direction == 0) throw new ArgumentOutOfRangeException(nameof(direction));

            if (SelectedItem == null)
            {
                var pagemarks = Items.Where(e => e.Value is Pagemark);
                var node = direction >= 0 ? pagemarks.FirstOrDefault() : pagemarks.LastOrDefault();
                if (node != null)
                {
                    SelectedItem = node;
                    return true;
                }
            }
            else
            {
                var node = GetNeighborPagemark(SelectedItem, direction);
                if (node != null)
                {
                    SelectedItem = node;
                    return true;
                }
            }

            return false;
        }

        private TreeListNode<IPagemarkEntry> GetNeighborPagemark(TreeListNode<IPagemarkEntry> item, int direction)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (direction == 0) throw new ArgumentOutOfRangeException(nameof(direction));

            if (Items == null || Items.Count <= 0)
            {
                return null;
            }

            int index = Items.IndexOf(item);
            if (index < 0)
            {
                return null;
            }

            while (true)
            {
                index = index + direction;
                if (index < 0 || index >= Items.Count)
                {
                    return null;
                }
                var node = Items[index];
                if (node.Value is Pagemark)
                {
                    return node;
                }
            }
        }

    }
}
