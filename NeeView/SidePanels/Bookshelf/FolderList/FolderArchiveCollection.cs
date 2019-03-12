using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NeeView
{

    /// <summary>
    /// アーカイブフォルダコレクション
    /// </summary>
    public class FolderArchiveCollection : FolderCollection
    {
        #region Fields

        private ArchiveEntryCollectionMode _mode;
        private ArchiveEntryCollection _collection;

        #endregion

        #region Constructors

        public FolderArchiveCollection(QueryPath path, ArchiveEntryCollectionMode mode, bool isActive, bool isOverlayEnabled) : base(path, isActive, isOverlayEnabled)
        {
            _mode = mode;
        }

        public override async Task InitializeItemsAsync(CancellationToken token)
        {
            try
            {
                _collection = new ArchiveEntryCollection(this.Place.SimplePath, ArchiveEntryCollectionMode.CurrentDirectory, _mode, ArchiveEntryCollectionOption.None);
            }
            catch
            {
                this.Items = new ObservableCollection<FolderItem>() { CreateFolderItemEmpty() };
                return;
            }

            List<ArchiveEntry> entries;
            switch (_mode)
            {
                case ArchiveEntryCollectionMode.CurrentDirectory:
                    entries = await _collection.GetEntriesWhereBookAsync(token);
                    break;
                case ArchiveEntryCollectionMode.IncludeSubDirectories:
                    entries = await _collection.GetEntriesWhereSubArchivesAsync(token);
                    break;
                default:
                    this.Items = new ObservableCollection<FolderItem>() { CreateFolderItemEmpty() };
                    return;
            }

            var items = entries
                .Select(e => CreateFolderItem(e, _collection.Path))
                .Where(e => e != null)
                .ToList();

            var list = Sort(items).ToList();

            if (!list.Any())
            {
                list.Add(CreateFolderItemEmpty());
            }

            this.Items = new ObservableCollection<FolderItem>(list);
            BindingOperations.EnableCollectionSynchronization(this.Items, new object());
        }

        #endregion


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

        /// <summary>
        /// アーカイブエントリから項目作成
        /// </summary>
        public FolderItem CreateFolderItem(ArchiveEntry entry, string prefix)
        {
            return new FileFolderItem(_isOverlayEnabled)
            {
                Type = FolderItemType.ArchiveEntry,
                ArchiveEntry = entry,
                Place = new QueryPath(prefix),
                Name = entry.SystemPath.Substring(prefix.Length).TrimStart(LoosePath.Separator),
                LastWriteTime = entry.LastWriteTime,
                Length = entry.Length,
                Attributes = FolderItemAttribute.ArchiveEntry,
                IsReady = true
            };
        }

        #endregion
    }
}
