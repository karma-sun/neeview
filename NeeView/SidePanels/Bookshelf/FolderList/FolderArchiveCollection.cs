using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

namespace NeeView
{

    /// <summary>
    /// 検索コレクション
    /// </summary>
    public class FolderArchiveCollection : FolderCollection, IDisposable
    {
        #region Fields

        private Archiver _archiver;

        #endregion

        #region Constructors

        /// <summary>
        /// constructor
        /// </summary>
        public FolderArchiveCollection(QueryPath path, Archiver archiver, bool isActive) : base(path, isActive)
        {
            _archiver = archiver;

            if (archiver == null)
            {
                this.Items = new ObservableCollection<FolderItem>() { CreateFolderItemEmpty() };
                return;
            }

            var items = archiver.GetArchives()
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
        }

        #endregion

        #region Properties

        public Archiver Archiver => _archiver;

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
            else if (_archiver == null)
            {
                return new QueryPath(ArchiverManager.Current.GetExistPathName(Place.SimplePath));
            }
            else if (_archiver.Parent != null)
            {
                return new QueryPath(_archiver.Parent.FullPath);
            }
            else
            {
                return base.GetParentQuery();
            }
        }

        /// <summary>
        /// アーカイブエントリから項目作成
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public FolderItem CreateFolderItem(ArchiveEntry entry)
        {
            return new FileFolderItem()
            {
                Type = FolderItemType.ArchiveEntry,
                ArchiveEntry = entry,
                Place = new QueryPath(entry.Archiver.FullPath),
                Name = entry.EntryName,
                LastWriteTime = entry.LastWriteTime ?? default(DateTime),
                Length = entry.Length,
                Attributes = FolderItemAttribute.ArchiveEntry,
                IsReady = true
            };
        }

        #endregion

        #region IDisposable Support

        private bool _disposedValue = false; // 重複する呼び出しを検出するには

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _archiver?.Dispose();
                }

                _disposedValue = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
