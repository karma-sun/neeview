// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php


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

        private Archiver _archvier;

        #endregion

        #region Constructors

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="place"></param>
        public FolderArchiveCollection(string place, Archiver archiver) : base(place)
        {
            _archvier = archiver;

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

        #region Methods

        /// <summary>
        /// フォルダーリスト上での親フォルダーを取得
        /// </summary>
        /// <returns></returns>
        public string GetParentPlace()
        {
            if (_archvier == null)
            {
                return null;
            }
            else if (_archvier.Parent != null)
            {
                return _archvier.Parent.FullPath;
            }
            else 
            {
                return LoosePath.GetDirectoryName(this.Place);
            }
        }

        /// <summary>
        /// アーカイブエントリから項目作成
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public FolderItem CreateFolderItem(ArchiveEntry entry)
        {
            return new FolderItem()
            {
                Type = FolderItemType.ArchiveEntry,
                ArchiveEntry = entry,
                Path = entry.EntryFullName,
                TargetPath = entry.FullPath,
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
                    // nop.
                }

                _disposedValue = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
