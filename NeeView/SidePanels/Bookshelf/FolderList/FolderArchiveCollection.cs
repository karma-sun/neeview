using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Data;

namespace NeeView
{

    /// <summary>
    /// 検索コレクション
    /// </summary>
    public class FolderArchiveCollection : FolderCollection
    {
        #region Fields

        private Archiver _archiver;

        #endregion

        #region Constructors

        /// <summary>
        /// constructor
        /// </summary>
        public FolderArchiveCollection(QueryPath path, Archiver archiver, bool isActive, bool isOverlayEnabled) : base(path, isActive, isOverlayEnabled)
        {
            _archiver = archiver;

            if (archiver == null)
            {
                this.Items = new ObservableCollection<FolderItem>() { CreateFolderItemEmpty() };
                return;
            }

            // TODO: 重い処理はコンストラクタではなく必要になったらそこで処理させるようにする
            var items = archiver.GetArchives(CancellationToken.None)
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
                return new QueryPath(_archiver.Parent.SystemPath);
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
            return new FileFolderItem(_isOverlayEnabled)
            {
                Type = FolderItemType.ArchiveEntry,
                ArchiveEntry = entry,
                Place = new QueryPath(entry.Archiver.SystemPath),
                Name = entry.EntryName,
                LastWriteTime = entry.LastWriteTime,
                Length = entry.Length,
                Attributes = FolderItemAttribute.ArchiveEntry,
                IsReady = true
            };
        }

        #endregion
    }
}
