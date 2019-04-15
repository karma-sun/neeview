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

        public PlaylistFolderCollection(QueryPath path, bool isActive, bool isOverlayEnabled) : base(path, isActive, isOverlayEnabled)
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
                this.Items = new ObservableCollection<FolderItem>() { CreateFolderItemEmpty(Place) };
                return;
            }

            var entries = await _collection.GetEntriesAsync(token);

            var items = entries
                .Select(e => CreateFolderItem(Place, e, e.Id))
                .Where(e => e != null);

                var list = Sort(items).ToList();

            if (!list.Any())
            {
                list.Add(CreateFolderItemEmpty(Place));
            }

            this.Items = new ObservableCollection<FolderItem>(list);
            BindingOperations.EnableCollectionSynchronization(this.Items, new object());
        }

        #endregion

        #region Properties

        public override FolderOrderClass FolderOrderClass => FolderOrderClass.Full;

        #endregion Properties

        #region Methods

        private FolderItem CreateFolderItem(QueryPath parent, ArchiveEntry entry, int id)
        {
            var item = CreateFolderItem(parent, entry);
            if (item != null)
            {
                item.EntryTime = new DateTime(id);
                item.Attributes |= FolderItemAttribute.PlaylistMember;
            }
            return item;
        }

        private FolderItem CreateFolderItem(QueryPath parent, ArchiveEntry entry)
        {
            var ie = (ArchiveEntry)entry.Instance;

            if (ie.IsFileSystem)
            {
                return CreateFolderItemFile(parent, entry);
            }
            else
            {
                return CreateFolderItemArchive(parent, entry);
            }
        }

        private FolderItem CreateFolderItemFile(QueryPath parent, ArchiveEntry entry)
        {
            var directoryInfo = new DirectoryInfo(entry.Link);
            if (directoryInfo.Exists)
            {
                return CreateFolderItem(parent, directoryInfo);
            }
            var fileInfo = new FileInfo(entry.Link);
            if (fileInfo.Exists)
            {
                if (FileShortcut.IsShortcut(fileInfo.FullName))
                {
                    var shortcut = new FileShortcut(fileInfo);
                    if (shortcut.IsValid)
                    {
                        if ((shortcut.Target.Attributes & FileAttributes.Directory) != 0)
                        {
                            return CreateFolderItem(parent, shortcut);
                        }
                        if (ArchiverManager.Current.IsSupported(shortcut.TargetPath))
                        {
                            return CreateFolderItem(parent, shortcut);
                        }
                    }
                }
                if (ArchiverManager.Current.IsSupported(fileInfo.FullName))
                {
                    return CreateFolderItem(parent, fileInfo);
                }
            }
            return null;
        }

        public FolderItem CreateFolderItemArchive(QueryPath parent, ArchiveEntry entry)
        {
            var ie = (ArchiveEntry)entry.Instance;

            return new FileFolderItem(_isOverlayEnabled)
            {
                Type = FolderItemType.ArchiveEntry,
                ArchiveEntry = entry,
                Place = parent,
                Name = entry.EntryName,
                TargetPath = new QueryPath(entry.SystemPath),
                LastWriteTime = entry.LastWriteTime,
                Length = entry.Length,
                Attributes = FolderItemAttribute.ArchiveEntry,
                IsReady = true
            };
        }

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

#endregion
    }
}
