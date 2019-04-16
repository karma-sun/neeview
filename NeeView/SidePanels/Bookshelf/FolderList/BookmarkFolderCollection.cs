using NeeView.Collections;
using NeeView.Collections.Generic;
using NeeView.IO;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NeeView
{
    public class BookmarkFolderCollection : FolderCollection, IDisposable
    {
        // Fields

        private TreeListNode<IBookmarkEntry> _bookmarkPlace;

        // Constructors

        public BookmarkFolderCollection(QueryPath path, bool isOverlayEnabled) : base(path, isOverlayEnabled)
        {
        }

        public override async Task InitializeItemsAsync(CancellationToken token)
        {
            await Task.Yield();

            _bookmarkPlace = BookmarkCollection.Current.FindNode(Place.FullPath) ?? new TreeListNode<IBookmarkEntry>();

            var items = _bookmarkPlace.Children
                .Select(e => CreateFolderItem(e))
                .Where(e => e != null)
                .ToList();

            var list = Sort(items).ToList();

            if (!list.Any())
            {
                list.Add(_folderItemFactory.CreateFolderItemEmpty());
            }

            this.Items = new ObservableCollection<FolderItem>(list);
            BindingOperations.EnableCollectionSynchronization(this.Items, new object());

            // 変更監視
            BookmarkCollection.Current.BookmarkChanged += BookmarkCollection_BookmarkChanged;
        }


        // Properties

        public override FolderOrderClass FolderOrderClass => FolderOrderClass.Full;

        public TreeListNode<IBookmarkEntry> BookmarkPlace => _bookmarkPlace;


        // Methods

        private void BookmarkCollection_BookmarkChanged(object sender, BookmarkCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case EntryCollectionChangedAction.Add:
                    if (e.Parent == _bookmarkPlace)
                    {
                        var item = Items.FirstOrDefault(i => e.Item == i.Source);
                        if (item == null)
                        {
                            item = CreateFolderItem(e.Item);
                            AddItem(item);
                        }
                    }
                    break;

                case EntryCollectionChangedAction.Remove:
                    if (e.Parent == _bookmarkPlace)
                    {
                        var item = Items.FirstOrDefault(i => e.Item == i.Source);
                        if (item != null)
                        {
                            DeleteItem(item);
                        }
                    }
                    break;

                case EntryCollectionChangedAction.Rename:
                    if (e.Parent == _bookmarkPlace)
                    {
                        var item = Items.FirstOrDefault(i => e.Item == i.Source);
                        if (item != null)
                        {
                            RenameItem(item, e.Item.Value.Name);
                        }
                    }
                    break;


                case EntryCollectionChangedAction.Move:
                    // nop.
                    break;

                case EntryCollectionChangedAction.Replace:
                case EntryCollectionChangedAction.Reset:
                    // nop. (work at FoderList.)
                    break;
            }
        }


        private FolderItem CreateFolderItem(TreeListNode<IBookmarkEntry> node)
        {
            if (node.Value is BookmarkFolder)
            {
                return CreateFolderItemBookmarkFolder(node);
            }
            else if (node.Value is Bookmark)
            {
                return CreateFolderItemBookmark(node);
            }
            else
            {
                return null;
            }
        }

        private FolderItem CreateFolderItemBookmarkFolder(TreeListNode<IBookmarkEntry> node)
        {
            if (!(node?.Value is BookmarkFolder folder)) return null;

            return new ConstFolderItem(new FolderThumbnail(), _isOverlayEnabled)
            {
                Source = node,
                Type = FolderItemType.Directory,
                Place = Place,
                Name = folder.Name,
                TargetPath = node.CreateQuery(),
                Length = -1,
                Attributes = FolderItemAttribute.Directory | FolderItemAttribute.Bookmark,
                IsReady = true
            };
        }

        private FolderItem CreateFolderItemBookmark(TreeListNode<IBookmarkEntry> node)
        {
            if (!(node?.Value is Bookmark bookmark)) return null;

            // TODO: 書庫内パスの対応
            var archiveEntry = ArchiveEntry.Create(bookmark.Place);

            var item = _folderItemFactory.CreateFolderItem(archiveEntry, null);
            item.Source = node;
            item.Attributes |= FolderItemAttribute.Bookmark;
            item.EntryTime = bookmark.EntryTime;
            return item;
        }

        #region IDisposable Support

        private bool _disposedValue = false;

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    BookmarkCollection.Current.BookmarkChanged -= BookmarkCollection_BookmarkChanged;
                }

                _disposedValue = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
