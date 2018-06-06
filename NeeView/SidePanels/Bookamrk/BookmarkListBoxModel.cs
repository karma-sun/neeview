using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using NeeView.Windows;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    public class BookmarkListBoxModel : BindableBase
    {
        #region Fields

        private ObservableCollection<TreeListNode<IBookmarkEntry>> _items;
        private TreeListNode<IBookmarkEntry> _selectedItem;

        #endregion

        #region Constructors

        public BookmarkListBoxModel()
        {
            BookmarkCollection.Current.BookmarkChanged += BookmarkCollection_BookmarkChanged;
        }

        #endregion

        #region Events

        public event EventHandler Changing;
        public event EventHandler Changed;

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
        }

        public void Decide(TreeListNode<IBookmarkEntry> item)
        {
            if (item.Value is Bookmark bookmark)
            {
                BookHub.Current.RequestLoad(bookmark.Place, null, BookLoadOption.SkipSamePlace | BookLoadOption.IsBook, true);
            }
        }

        private void Refresh()
        {
            Changing?.Invoke(this, null);
            Items = new ObservableCollection<TreeListNode<IBookmarkEntry>>(BookmarkCollection.Current.Items);
            Changed?.Invoke(this, null);
        }

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

        public void Add(string place)
        {
            if (place == null) throw new ArgumentNullException(nameof(place));

            if (Contains(place))
            {
                return;
            }

            if (place.StartsWith(Temporary.TempDirectory))
            {
                // TODO: テンポラリは登録できない通知
                return;
            }

            // TODO: place指定でのunit取得をもっとスマートにできそう
            var unit = BookMementoCollection.Current.GetValid(place) ?? BookMementoCollection.Current.Set(CreateBookMemento(place));
            BookmarkCollection.Current.AddFirst(new Bookmark(unit));
        }

        public bool Remove(TreeListNode<IBookmarkEntry> item)
        {
            int selectedIndex = Items.IndexOf(SelectedItem);

            bool isRemoved = BookmarkCollection.Current.Remove(item);
            if (isRemoved)
            {
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

        // TODO: ここでToggleは漠然としすぎている。もっと上位で判定すべきか
        public bool Toggle(string place)
        {
            if (place == null) return false;

            var node = FindNode(place);
            if (node == null)
            {
                Add(place);
                return true;
            }
            else
            {
                Remove(node);
                return false;
            }
        }

        // 指定したブックの設定作成
        private Book.Memento CreateBookMemento(string place)
        {
            if (place == null) throw new ArgumentNullException();

            var memento = BookHub.Current.CreateBookMemento();
            if (memento == null || memento.Place != place)
            {
                memento = BookSetting.Current.BookMementoDefault.Clone();
                memento.Place = place;
            }
            return memento;
        }

        public void Move(DropInfo<TreeListNode<IBookmarkEntry>> dropInfo)
        {
            if (dropInfo == null) return;
            if (dropInfo.DragItem == dropInfo.DropItem) return;

            var item = dropInfo.DragItem;


            var indexFrom = Items.IndexOf(dropInfo.DragItem);
            var indexTo = Items.IndexOf(dropInfo.DropItem);

            if (indexFrom < indexTo)
            {
                BookmarkCollection.Current.Move(item, dropInfo.DropItem, +1);
            }
            else
            {
                BookmarkCollection.Current.Move(item, dropInfo.DropItem, -1);
            }
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
