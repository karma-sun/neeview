using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class FolderCollectionFactory
    {
        public static FolderCollectionFactory Current { get; } = new FolderCollectionFactory();

        #region Properties

        public FolderSearchEngine SearchEngine { get; set; }

        #endregion

        #region Methods

        // フォルダーコレクション作成
        public async Task<FolderCollection> CreateFolderCollectionAsync(QueryPath path, bool isActive, CancellationToken token)
        {
            if (path.Scheme == QueryScheme.Bookmark)
            {
                return await CreateBookmarkFolderCollectionAsync(path, isActive, token);
            }
            else if (path.Scheme == QueryScheme.File)
            {
                if (path.Search != null)
                {
                    return await CreateSearchFolderCollectionAsync(path, isActive, token);
                }
                else if (path.Path == null || Directory.Exists(path.Path))
                {
                    return await CreateEntryFolderCollectionAsync(path, isActive, token);
                }
                else
                {
                    return await CreateArchiveFolderCollectionAsync(path, isActive, token);
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        // 検索コレクション作成
        public async Task<FolderCollection> CreateSearchFolderCollectionAsync(QueryPath path, bool isActive, CancellationToken token)
        {
            if (SearchEngine == null) throw new InvalidOperationException("SearchEngine not initialized.");

            try
            {
                var result = await SearchEngine.SearchAsync(path.SimplePath, path.Search);
                token.ThrowIfCancellationRequested();

                var collection = CreateSearchCollection(path, result, isActive);
                return collection;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
        }

        // 通常フォルダーコレクション作成
        private async Task<FolderCollection> CreateEntryFolderCollectionAsync(QueryPath path, bool isActive, CancellationToken token)
        {
            var collection = await Task.Run(() => CreateEntryCollection(path, isActive));
            token.ThrowIfCancellationRequested();

            return collection;
        }

        // ブックマークフォルダーコレクション作成
        private async Task<FolderCollection> CreateBookmarkFolderCollectionAsync(QueryPath path, bool isActive, CancellationToken token)
        {
            var collection = await Task.Run(() => CreateBookmarkFolderCollection(path, isActive));
            token.ThrowIfCancellationRequested();

            return collection;
        }

        // アーカイブフォルダーコレクション作成
        public async Task<FolderCollection> CreateArchiveFolderCollectionAsync(QueryPath path, bool isActive, CancellationToken token)
        {
            try
            {
                using (var entry = await ArchiveFileSystem.CreateArchiveEntry(path.SimplePath, token))
                {
                    var collection = CreateArchiveCollection(path, await ArchiverManager.Current.CreateArchiverAsync(entry, false, false, token), isActive);
                    token.ThrowIfCancellationRequested();
                    return collection;
                }
            }
            catch (Exception ex)
            {
                // アーカイブパスが展開できない場合、実在パスでの展開を行う
                Debug.WriteLine($"Cannot open: {ex.Message}");
                var place = ArchiveFileSystem.GetExistDirectoryName(path.SimplePath);
                return await CreateEntryFolderCollectionAsync(new QueryPath(place), isActive, token);
            }
        }

        /// <summary>
        /// FolderCollection 作成
        /// </summary>
        private FolderCollection CreateEntryCollection(QueryPath path, bool isActive)
        {
            try
            {
                return new FolderEntryCollection(path, isActive);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                // NOTE: 救済措置。取得に失敗した時はカレントディレクトリに移動
                return new FolderEntryCollection(new QueryPath(Environment.CurrentDirectory), isActive);
            }
        }


        /// <summary>
        /// FolderCollection作成(書庫内アーカイブリスト)
        /// </summary>
        public FolderCollection CreateArchiveCollection(QueryPath path, Archiver archiver, bool isActive)
        {
            return new FolderArchiveCollection(path, archiver, isActive);
        }

        /// <summary>
        /// FolderCollection作成(検索結果)
        /// </summary>
        private FolderCollection CreateSearchCollection(QueryPath path, NeeLaboratory.IO.Search.SearchResultWatcher searchResult, bool isActive)
        {
            return new FolderSearchCollection(path, searchResult, isActive);
        }

        /// <summary>
        /// FolderCollecion作成(ブックマーク)
        /// </summary>
        private FolderCollection CreateBookmarkFolderCollection(QueryPath path, bool isActive)
        {
            return new BookmarkFolderCollection(path);
        }

        #endregion
    }
}
