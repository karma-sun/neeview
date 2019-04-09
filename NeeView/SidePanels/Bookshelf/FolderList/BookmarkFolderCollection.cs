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

        public BookmarkFolderCollection(QueryPath path, bool isOverlayEnabled) : base(path, false, isOverlayEnabled)
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
                list.Add(CreateFolderItemEmpty());
            }

            this.Items = new ObservableCollection<FolderItem>(list);
            BindingOperations.EnableCollectionSynchronization(this.Items, new object());

            // 変更監視
            BookmarkCollection.Current.BookmarkChanged += BookmarkCollection_BookmarkChanged;
        }


        // Properties

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
                            CreateItem(item);
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


        public FolderItem CreateFolderItem(TreeListNode<IBookmarkEntry> node)
        {
            var scheme = QueryScheme.Bookmark.ToSchemeString();

            switch (node.Value)
            {
                case BookmarkFolder folder:
                    return new ConstFolderItem(new FolderThumbnail(), _isOverlayEnabled)
                    {
                        Source = node,
                        Type = FolderItemType.Directory,
                        Place = Place,
                        Name = folder.Name,
                        Length = -1,
                        Attributes = FolderItemAttribute.Directory | FolderItemAttribute.Bookmark,
                        IsReady = true
                    };

                case Bookmark bookmark:

                    var archiveEntry = ArchiveEntry.Create(bookmark.Place);

                    return new FileFolderItem(_isOverlayEnabled)
                    {
                        Source = node,
                        Type = FolderItemType.File,
                        Place = Place,
                        TargetPath = new QueryPath(bookmark.Place),
                        IsDirectoryTarget = bookmark.Unit.Memento.IsDirectorty,
                        Name = bookmark.Name,
                        ArchiveEntry = archiveEntry,
                        LastWriteTime = archiveEntry.LastWriteTime,
                        EntryTime = bookmark.EntryTime,
                        Length = archiveEntry.Length,
                        Attributes = FolderItemAttribute.Bookmark,
                        IsReady = true
                    };

                default:
                    return null;
            }
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
