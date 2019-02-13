using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    [Serializable]
    public class BookAddressException : Exception
    {
        public BookAddressException(string message) : base(message)
        {
        }

        public BookAddressException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// アーカイブパスに対応したブックアドレス
    /// </summary>
    public class BookAddress : IDisposable
    {
        #region Fields

        private ArchiveEntry _archiveEntry;

        #endregion

        #region Properties

        /// <summary>
        /// ブックのアーカイバ
        /// </summary>
        public Archiver Archiver { get; private set; }

        /// <summary>
        /// 開始ページ名
        /// </summary>
        public string EntryName { get; set; }

        /// <summary>
        /// ブックの場所
        /// </summary>
        public string Place => Archiver.SystemPath;

        /// <summary>
        /// ページを含めたアーカイブパス
        /// </summary>
        public string FullPath => LoosePath.Combine(Place, EntryName);

        #endregion

        #region Methods

        /// <summary>
        /// 初期化(必須)。
        /// アーカイブ展開等を含むため、非同期処理。
        /// </summary>
        /// <param name="path">入力パス</param>
        /// <param name="entryName">開始ページ名</param>
        /// <param name="isArchiveRecursive">アーカイブ自動展開</param>
        /// <param name="token">キャンセルトークン</param>
        /// <returns></returns>
        public async Task InitializeAsync(string path, string entryName, BookLoadOption option, bool isArchiveRecursive, CancellationToken token)
        {
            var query = new QueryPath(path);

            if (query.Scheme == QueryScheme.Pagemark)
            {
                this.Archiver = new PagemarkArchiver(path, null, true);
                this.EntryName = entryName;
                return;
            }

            if (query.Scheme == QueryScheme.Bookmark)
            {
                var node = BookmarkCollection.Current.FindNode(path);
                switch (node.Value)
                {
                    case Bookmark bookmark:
                        path = bookmark.Place;
                        break;
                    case BookmarkFolder folder:
                        throw new BookAddressException(string.Format(Properties.Resources.NotifyCannotOpenBookmarkFolder, path));
                }
            }

            _archiveEntry = await ArchiveFileSystem.CreateArchiveEntry(path, token);

            if (entryName != null)
            {
                Debug.Assert(!option.HasFlag(BookLoadOption.IsBook));
                this.Archiver = await ArchiverManager.Current.CreateArchiverAsync(_archiveEntry, true, false, token);
                this.EntryName = entryName;
            }
            else if (Directory.Exists(path) || ArchiverManager.Current.IsSupported(_archiveEntry.SystemPath))
            {
                Debug.Assert(!option.HasFlag(BookLoadOption.IsPage));
                this.Archiver = await ArchiverManager.Current.CreateArchiverAsync(_archiveEntry, true, false, token);
                this.EntryName = null;
            }
            else if (_archiveEntry.Archiver != null)
            {
                if (isArchiveRecursive)
                {
                    this.Archiver = _archiveEntry.RootArchiver;
                    this.EntryName = _archiveEntry.EntryFullName;
                }
                else
                {
                    this.Archiver = _archiveEntry.Archiver;
                    this.EntryName = _archiveEntry.EntryName;

                    // このアーカイブをROOTとする
                    this.Archiver.SetRootFlag(true);
                }
            }
            else
            {
                if (option.HasFlag(BookLoadOption.IsBook))
                {
                    this.Archiver = new FolderArchive(_archiveEntry.SystemPath, null, true);
                    this.EntryName = null;
                }
                else
                {
                    this.Archiver = new FolderArchive(Path.GetDirectoryName(_archiveEntry.SystemPath), null, true);
                    this.EntryName = Path.GetFileName(_archiveEntry.EntryName);
                }
            }
        }

        /// <summary>
        /// 使用しているアーカイバを破棄
        /// </summary>
        private void Terminate()
        {
            this.Archiver?.Dispose();
            _archiveEntry?.Dispose();
        }

        #region IDisposable Support
        private bool _disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // マネージ状態を破棄します (マネージ オブジェクト)。
                    Terminate();
                }

                _disposedValue = true;
            }
        }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
        }
        #endregion

        #endregion
    }

}
