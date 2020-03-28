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
            await Task.Run(() => InitializeItems(token));
        }

        public void InitializeItems(CancellationToken token)
        {
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
                            if (e.Item.Value is BookmarkFolder bookmarkFlder)
                            {
                                RenameItem(item, e.Item.CreateQuery());
                            }
                            else if (e.Item.Value is Bookmark bookmark)
                            {
                                RenameItem(item, new QueryPath(bookmark.Path));
                            }
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

        private FileSystemInfo GetFileSystemInfo(string path)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(path);
                if (directoryInfo.Exists)
                {
                    return directoryInfo;
                }
                var fileInfo = new FileInfo(path);
                if (fileInfo.Exists)
                {
                    return fileInfo;
                }
            }
            catch
            {
                // アーカイブパス等、ファイル名に使用できない文字が含まれている場合がある
            }
            return null;
        }

        private FolderItem CreateFolderItemBookmark(TreeListNode<IBookmarkEntry> node)
        {
            if (!(node?.Value is Bookmark bookmark)) return null;

            var item = new FileFolderItem(_isOverlayEnabled)
            {
                Source = node,
                Type = FolderItemType.File,
                Place = Place,
                Name = bookmark.Name,
                TargetPath = new QueryPath(bookmark.Path),
                Attributes = FolderItemAttribute.Bookmark,
                EntryTime = bookmark.EntryTime,
                IsReady = true
            };

            switch (GetFileSystemInfo(bookmark.Path))
            {
                case DirectoryInfo directoryInfo:
                    item.Length = -1;
                    item.LastWriteTime = directoryInfo.LastWriteTime;
                    break;
                case FileInfo fileInfo:
                    item.Length = fileInfo.Length;
                    item.LastWriteTime = fileInfo.LastWriteTime;
                    break;
            }

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
