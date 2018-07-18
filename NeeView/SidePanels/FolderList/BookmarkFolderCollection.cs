﻿using NeeView.Collections.Generic;
using NeeView.IO;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows.Data;

namespace NeeView
{
    public class BookmarkFolderItem : FolderItem
    {
        private TreeListNode<IBookmarkEntry> _source;

        public BookmarkFolderItem(TreeListNode<IBookmarkEntry> source)
        {
            _source = source;
        }

        public override string Name
        {
            get { return _source.Value.Name; }
            set { }
        }
    }

    public class BookmarkFolderCollection : FolderCollection, IDisposable
    {
        // Fields

        private TreeListNode<IBookmarkEntry> _parent;

        // Constructors

        public BookmarkFolderCollection(string place) : base(place, false)
        {
            _parent = BookmarkCollection.Current.FindNode(place) ?? new TreeListNode<IBookmarkEntry>();

            var items = _parent.Children
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
        // ...

        // Methods

        private void BookmarkCollection_BookmarkChanged(object sender, BookmarkCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case EntryCollectionChangedAction.Add:
                    if (e.Parent == _parent)
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
                    if (e.Parent == _parent)
                    {
                        var item = Items.FirstOrDefault(i => e.Item == i.Source);
                        if (item != null)
                        {
                            DeleteItem(item);
                        }
                    }
                    break;

                case EntryCollectionChangedAction.Rename:
                    if (e.Parent == _parent)
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
                    FolderList.Current.RequestPlace(Bookmark.Scheme + "\\", null, FolderSetPlaceOption.IsUpdateHistory | FolderSetPlaceOption.ResetKeyword | FolderSetPlaceOption.Refresh);
                    break;
            }
        }


        /// <summary>
        /// 親の場所を取得
        /// </summary>
        public override string GetParentPlace()
        {
            if (Place == null || Place.TrimEnd(LoosePath.Separator) == Bookmark.Scheme)
            {
                return Bookmark.Scheme + "\\";
            }

            return LoosePath.GetDirectoryName(Place);
        }


        public FolderItem CreateFolderItem(TreeListNode<IBookmarkEntry> node)
        {
            switch (node.Value)
            {
                case BookmarkFolder folder:
                    return new FolderItem()
                    {
                        Source = node,
                        Type = FolderItemType.Directory,
                        Place = LoosePath.GetDirectoryName(node.CreatePath(Bookmark.Scheme)),
                        Name = folder.Name,
                        Length = -1,
                        Attributes = FolderItemAttribute.Directory | FolderItemAttribute.Bookmark,
                        IsReady = true
                    };

                case Bookmark bookmark:

                    var archiveEntry = new ArchiveEntry(bookmark.Place);

                    return new FolderItem()
                    {
                        Source = node,
                        Type = FolderItemType.File,
                        Place = LoosePath.GetDirectoryName(node.CreatePath(Bookmark.Scheme)),
                        TargetPath = bookmark.Place,
                        Name = bookmark.Name,
                        ArchiveEntry = archiveEntry,
                        LastWriteTime = archiveEntry.LastWriteTime ?? default,
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