using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using NeeView.Windows;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    public class BookmarkListBoxModel : BindableBase
    {
        #region Fields

        private ObservableCollection<TreeListNode<IBookmarkEntry>> _items;
        private TreeListNode<IBookmarkEntry> _selectedItem;
        private Toast _toast;

        #endregion

        #region Constructors

        public BookmarkListBoxModel()
        {
            BookmarkCollection.Current.BookmarkChanged += BookmarkCollection_BookmarkChanged;
        }

        #endregion

        #region Events

        public event CollectionChangeEventHandler Changing;
        public event CollectionChangeEventHandler Changed;

        #endregion

        #region Properties

        public ObservableCollection<TreeListNode<IBookmarkEntry>> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }

        public TreeListNode<IBookmarkEntry> SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }


        // TODO: この参照方向はどうなの？
        public bool IsThumbnailVisibled => BookmarkList.Current.IsThumbnailVisibled;
        public PanelListItemStyle PanelListItemStyle => BookmarkList.Current.PanelListItemStyle;

        #endregion

        #region Methods

        private void BookmarkCollection_BookmarkChanged(object sender, BookmarkCollectionChangedEventArgs e)
        {
            Refresh();

            if (_toast != null)
            {
                _toast.Cancel();
                _toast = null;
            }
        }

        public void Decide(TreeListNode<IBookmarkEntry> item)
        {
            switch (item.Value)
            {
                case Bookmark bookmark:
                    BookHub.Current.RequestLoad(bookmark.Place, null, BookLoadOption.SkipSamePlace | BookLoadOption.IsBook, true);
                    break;
                case BookmarkFolder folder:
                    if (item.Children.Count > 0)
                    {
                        item.IsExpanded = !item.IsExpanded;
                        Refresh();
                    }
                    break;
            }
        }

        public void Expand(TreeListNode<IBookmarkEntry> item, bool isExpanded)
        {
            if (item.IsExpandEnabled && item.IsExpanded != isExpanded)
            {
                item.IsExpanded = isExpanded;
                Refresh();
            }
        }

        private void Refresh()
        {
            Changing?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null));
            var collection = BookmarkCollection.Current.Items.GetExpandedCollection();
            Items = new ObservableCollection<TreeListNode<IBookmarkEntry>>(collection);
            Changed?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null));
        }

        // TODO: Find系はいらない？Collectionと同じ？

        public Bookmark Find(string place)
        {
            if (place == null) return null;

            return BookmarkCollection.Current.Items.Select(e => e.Value).OfType<Bookmark>().FirstOrDefault(e => e.Place == place);
        }

        public BookMementoUnit FindUnit(string place)
        {
            if (place == null) return null;

            return Find(place)?.Unit;
        }

        public TreeListNode<IBookmarkEntry> FindNode(string place)
        {
            if (place == null) return null;

            return BookmarkCollection.Current.Items.FirstOrDefault(e => e.Value is Bookmark bookmark ? bookmark.Place == place : false);
        }

        public bool Contains(string place)
        {
            if (place == null) return false;

            return Find(place) != null;
        }


        public TreeListNode<IBookmarkEntry> Add(string place)
        {
            if (place == null) throw new ArgumentNullException(nameof(place));

            if (Contains(place))
            {
                return null;
            }

            if (place.StartsWith(Temporary.TempDirectory))
            {
                ToastService.Current.Show(new Toast(Properties.Resources.DialogBookmarkError));
                return null;
            }

            var unit = BookMementoCollection.Current.Set(place);
            var node = new TreeListNode<IBookmarkEntry>(new Bookmark(unit));
            BookmarkCollection.Current.AddFirst(node);

            return node;
        }

        public bool Remove(TreeListNode<IBookmarkEntry> item)
        {
            int selectedIndex = Items.IndexOf(SelectedItem);
            var memento = new TreeListNodeMemento<IBookmarkEntry>(item);

            bool isRemoved = BookmarkCollection.Current.Remove(item);
            if (isRemoved)
            {
                if (item.Value is BookmarkFolder)
                {
                    var count = item.Count(e => e.Value is Bookmark);
                    if (count > 0)
                    {
                        _toast = new Toast(string.Format(Properties.Resources.DialogPagemarkFolderDelete, count), Properties.Resources.WordRestore, () => BookmarkCollection.Current.Restore(memento));
                        ToastService.Current.Show(_toast);
                    }
                }

                if (selectedIndex >= 0 && !Items.Contains(SelectedItem))
                {
                    selectedIndex = selectedIndex < Items.Count ? selectedIndex : Items.Count - 1;
                    if (selectedIndex >= 0)
                    {
                        SelectedItem = Items[selectedIndex];
                    }
                }
            }

            return isRemoved;
        }

        //
        public void AddBookmark()
        {
            var place = BookHub.Current.Book?.Place;
            if (place == null)
            {
                return;
            }

            var node = FindNode(place);
            if (node == null)
            {
                node = Add(place);
                if (node == null)
                {
                    return;
                }
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


        // TODO: ここでToggleは漠然としすぎている。もっと上位で判定すべきか
        public bool Toggle(string place)
        {
            if (place == null) return false;

            var node = FindNode(place);
            if (node == null)
            {
                node = Add(place);
                return node != null;
            }
            else
            {
                Remove(node);
                return false;
            }
        }

        public void Move(DropInfo<TreeListNode<IBookmarkEntry>> dropInfo)
        {
            if (dropInfo == null) return;
            if (dropInfo.DragItem == dropInfo.DropItem) return;

            var item = dropInfo.DragItem;
            var target = dropInfo.DropItem;

            var indexFrom = Items.IndexOf(dropInfo.DragItem);
            var indexTo = Items.IndexOf(dropInfo.DropItem);


            const double margine = 0.25;

            if (target.Value is BookmarkFolder folder)
            {
                if (dropInfo.Position < margine)
                {
                    BookmarkCollection.Current.Move(item, target, -1);
                }
                else if (dropInfo.Position > (1.0 - margine) && !target.IsExpanded)
                {
                    BookmarkCollection.Current.Move(item, target, +1);
                }
                else
                {
                    BookmarkCollection.Current.MoveToChild(item, target);
                }
            }
            else
            {
                if (target.GetNext() == null && dropInfo.Position > (1.0 - margine))
                {
                    BookmarkCollection.Current.Move(item, target, +1);
                }
                else if (indexFrom < indexTo)
                {
                    BookmarkCollection.Current.Move(item, target, +1);
                }
                else
                {
                    BookmarkCollection.Current.Move(item, target, -1);
                }
            }
        }

        internal void NewFolder()
        {
            var node = new TreeListNode<IBookmarkEntry>(new BookmarkFolder() { Name = Properties.Resources.WordNewFolder });
            BookmarkCollection.Current.AddFirst(node);
            SelectedItem = node;
            Changed?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Add, node));
        }



        // ブックマークを戻る
        public void PrevBookmark()
        {
            if (BookHub.Current.IsLoading) return;

            if (!CanMoveSelected(-1))
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.NotifyBookmarkPrevFailed);
                return;
            }

            if (MoveSelected(-1))
            {
                if (SelectedItem?.Value is Bookmark bookmark)
                {
                    BookHub.Current.RequestLoad(bookmark.Place, null, BookLoadOption.SkipSamePlace | BookLoadOption.IsBook, true);
                }
            }
        }

        // ブックマークを進む
        public void NextBookmark()
        {
            if (BookHub.Current.IsLoading) return;

            if (!CanMoveSelected(+1))
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.NotifyBookmarkNextFailed);
                return;
            }

            if (MoveSelected(+1))
            {
                if (SelectedItem?.Value is Bookmark bookmark)
                {
                    BookHub.Current.RequestLoad(bookmark.Place, null, BookLoadOption.SkipSamePlace | BookLoadOption.IsBook, true);
                }
            }
        }

        public bool CanMoveSelected(int direction)
        {
            var bookmarks = Items.Where(e => e.Value is Bookmark);

            if (SelectedItem == null)
            {
                return bookmarks.Count() > 0;
            }
            else
            {
                var index = Items.IndexOf(SelectedItem);
                return direction > 0
                    ? index < Items.IndexOf(bookmarks.Last())
                    : index > Items.IndexOf(bookmarks.First());
            }
        }

        public bool MoveSelected(int direction)
        {
            if (direction == 0) throw new ArgumentOutOfRangeException(nameof(direction));

            if (SelectedItem == null)
            {
                var bookmarks = Items.Where(e => e.Value is Bookmark);
                var node = direction >= 0 ? bookmarks.FirstOrDefault() : bookmarks.LastOrDefault();
                if (node != null)
                {
                    SelectedItem = node;
                    return true;
                }
            }
            else
            {
                var node = GetNeighborBookmark(SelectedItem, direction);
                if (node != null)
                {
                    SelectedItem = node;
                    return true;
                }
            }

            return false;
        }

        private TreeListNode<IBookmarkEntry> GetNeighborBookmark(TreeListNode<IBookmarkEntry> item, int direction)
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
                if (node.Value is Bookmark)
                {
                    return node;
                }
            }
        }
    }

    #endregion
}
