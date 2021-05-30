using NeeView.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// プレイリスト用フォルダーコレクション
    /// </summary>
    public class PlaylistFolderCollection : FolderCollection
    {
        #region Fields

        private ArchiveEntryCollection _collection;

        #endregion

        #region Constructors

        public PlaylistFolderCollection(QueryPath path, bool isOverlayEnabled) : base(path, isOverlayEnabled)
        {
        }

        public override async Task InitializeItemsAsync(CancellationToken token)
        {
            try
            {
                _collection = new ArchiveEntryCollection(this.Place.SimplePath, ArchiveEntryCollectionMode.CurrentDirectory, ArchiveEntryCollectionMode.CurrentDirectory, ArchiveEntryCollectionOption.None);
            }
            catch
            {
                this.Items = new ObservableCollection<FolderItem>() { _folderItemFactory.CreateFolderItemEmpty() };
                return;
            }

            var entries = await _collection.GetEntriesAsync(token);

            var items = entries
                .Where(e => e.IsBook())
                .Select(e => CreateFolderItem(e, e.Id))
                .Where(e => e != null);

            var list = Sort(items).ToList();

            if (!list.Any())
            {
                list.Add(_folderItemFactory.CreateFolderItemEmpty());
            }

            this.Items = new ObservableCollection<FolderItem>(list);
            BindingOperations.EnableCollectionSynchronization(this.Items, new object());
        }

        #endregion

        #region Properties

        public override FolderOrderClass FolderOrderClass => FolderOrderClass.Full;

        #endregion Properties

        #region Methods

        /// <summary>
        /// フォルダーリスト上での親フォルダーを取得
        /// </summary>
        public override QueryPath GetParentQuery()
        {
            if (Place == null)
            {
                return null;
            }
            else if (_collection == null)
            {
                return new QueryPath(ArchiverManager.Current.GetExistPathName(Place.SimplePath));
            }
            else
            {
                return new QueryPath(_collection.GetFolderPlace());
            }
        }


        private FolderItem CreateFolderItem(ArchiveEntry entry, int id)
        {
            var item = CreateFolderItem(entry);
            if (item != null)
            {
                item.Name = entry.EntryName ?? item.Name;
                item.EntryTime = new DateTime(id);
                item.Attributes |= FolderItemAttribute.PlaylistMember;
            }
            return item;
        }

        private FolderItem CreateFolderItem(ArchiveEntry entry)
        {
            var entity = (ArchiveEntry)entry.Instance;

            if (entity.IsFileSystem)
            {
                return _folderItemFactory.CreateFolderItem(entry.Link);
            }
            else
            {
                return _folderItemFactory.CreateFolderItem(entity, null);
            }
        }

        #endregion
    }
}
