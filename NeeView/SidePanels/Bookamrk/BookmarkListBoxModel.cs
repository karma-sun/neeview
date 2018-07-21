using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    public class BookmarkListBoxModel : BindableBase
    {
        // Fields

        private TreeListNode<IBookmarkEntry> _selectedItem;
        private Toast _toast;


        // Constructors

        public BookmarkListBoxModel()
        {
            BookmarkCollection.Current.BookmarkChanged += BookmarkCollection_BookmarkChanged;
        }


        // Events

        public event CollectionChangeEventHandler Changed;
        public event EventHandler SelectedItemChanged;


        // Properties

        public BookmarkList BookmarkList => BookmarkList.Current;

        public BookmarkCollection BookmarkCollection => BookmarkCollection.Current;

        public TreeListNode<IBookmarkEntry> SelectedItem
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


        // Methods

        private void BookmarkCollection_BookmarkChanged(object sender, BookmarkCollectionChangedEventArgs e)
        {
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
            }
        }

        public void Expand(TreeListNode<IBookmarkEntry> item, bool isExpanded)
        {
            if (item.CanExpand && item.IsExpanded != isExpanded)
            {
                item.IsExpanded = isExpanded;
            }
        }

        public bool Remove(TreeListNode<IBookmarkEntry> item)
        {
            var next = item.Next ?? item.Previous ?? item.Parent;

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

                if (next != null)
                {
                    SelectedItem = next;
                }
            }

            return isRemoved;
        }

        #region Boomark Special

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

            if (node != null)
            {
                SelectedItem = node;

                SelectedItemChanged?.Invoke(this, null);
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

        #endregion

        public void Move(DropInfo<TreeListNode<IBookmarkEntry>> dropInfo)
        {
            if (dropInfo == null) return;
            if (dropInfo.Data == dropInfo.DropTarget) return;

            var item = dropInfo.Data;
            var target = dropInfo.DropTarget;

            const double margin = 0.33;

            if (target.Value is BookmarkFolder folder)
            {
                if (dropInfo.Position < margin)
                {
                    BookmarkCollection.Current.Move(item, target, -1);
                }
                else if (dropInfo.Position > (1.0 - margin) && !target.IsExpanded)
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
                if (target.Next == null && dropInfo.Position > (1.0 - margin))
                {
                    BookmarkCollection.Current.Move(item, target, +1);
                }
                else if (item.CompareOrder(item, target))
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
            var ignoreNames = BookmarkCollection.Current.Items.Children.Where(e => e.Value is BookmarkFolder).Select(e => e.Value.Name);
            var name = BookmarkCollection.Current.GetValidateFolderName(ignoreNames, Properties.Resources.WordNewFolder, Properties.Resources.WordNewFolder);
            var node = new TreeListNode<IBookmarkEntry>(new BookmarkFolder() { Name = name });
            BookmarkCollection.Current.AddFirst(node);
            SelectedItem = node;
            Changed?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Add, node));
        }



        // ブックマークを戻る
        public void PrevBookmark()
        {
            if (BookHub.Current.IsLoading) return;

            var node = GetNeighborBookmark(SelectedItem, -1);
            if (node != null)
            {
                SelectedItem = node;
                if (node.Value is Bookmark bookmark)
                {
                    BookHub.Current.RequestLoad(bookmark.Place, null, BookLoadOption.SkipSamePlace | BookLoadOption.IsBook, true);
                }
                SelectedItemChanged?.Invoke(this, null);
            }
            else
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.NotifyBookmarkPrevFailed);
            }
        }

        // ブックマークを進む
        public void NextBookmark()
        {
            if (BookHub.Current.IsLoading) return;

            var node = GetNeighborBookmark(SelectedItem, +1);
            if (node != null)
            {
                SelectedItem = node;
                if (node.Value is Bookmark bookmark)
                {
                    BookHub.Current.RequestLoad(bookmark.Place, null, BookLoadOption.SkipSamePlace | BookLoadOption.IsBook, true);
                }
                SelectedItemChanged?.Invoke(this, null);
            }
            else
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.NotifyBookmarkNextFailed);
            }
        }

        private TreeListNode<IBookmarkEntry> GetNeighborBookmark(TreeListNode<IBookmarkEntry> item, int direction)
        {
            if (direction == 0) throw new ArgumentOutOfRangeException(nameof(direction));

            if (item == null)
            {
                var bookmarks = BookmarkCollection.Current.Items.GetExpandedCollection().Where(e => e.Value is Bookmark).ToList();
                return direction >= 0 ? bookmarks.FirstOrDefault() : bookmarks.LastOrDefault();
            }
            else
            {
                var bookmarks = BookmarkCollection.Current.Items.GetExpandedCollection().Where(e => e.Value is Bookmark || e == item).ToList();
                if (bookmarks.Count <= 0)
                {
                    return null;
                }
                int index = bookmarks.IndexOf(item);
                if (index < 0)
                {
                    return null;
                }
                return bookmarks.ElementAtOrDefault(index + direction);
            }
        }

        public int IndexOfSelectedItem()
        {
            return IndexOfExpanded(SelectedItem);
        }

        public int IndexOfExpanded(TreeListNode<IBookmarkEntry> item)
        {
            if (item == null)
            {
                return -1;
            }

            return BookmarkCollection.Current.Items.GetExpandedCollection().IndexOf(item);
        }
    }
}
