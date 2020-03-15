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
        private bool _isOverlayEnabled;

        public FolderCollectionFactory(FolderSearchEngine searchEngine, bool isOverlayEnabled)
        {
            SearchEngine = searchEngine;
            _isOverlayEnabled = isOverlayEnabled;
        }


        #region Properties

        public FolderSearchEngine SearchEngine { get; set; }

        #endregion

        #region Methods

        // フォルダーコレクション作成
        public async Task<FolderCollection> CreateFolderCollectionAsync(QueryPath path, bool isActive, CancellationToken token)
        {
            if (path.Scheme == QueryScheme.Root)
            {
                return await CreateRootFolderCollectionAsync(path, isActive, token);
            }
            else if (path.Scheme == QueryScheme.QuickAccess)
            {
                return await CreateQuickAccessFolderCollectionAsync(path, isActive, token);
            }
            else if (path.Scheme == QueryScheme.Bookmark)
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
                else if (PlaylistArchive.IsSupportExtension(path.Path))
                {
                    return await CreatePlaylistFolderCollectionAsync(path, isActive, token);
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

                var collection = new FolderSearchCollection(path, result, isActive, _isOverlayEnabled);
                await collection.InitializeItemsAsync(token);
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
            FolderCollection collection;
            try
            {
                collection = new FolderEntryCollection(path, isActive, _isOverlayEnabled);
                await collection.InitializeItemsAsync(token);
            }
            catch (Exception ex)
            {
                // NOTE: 救済措置。取得に失敗した時はカレントディレクトリに移動
                Debug.WriteLine($"Cannot open: {ex.Message}");
                collection = new FolderEntryCollection(new QueryPath(System.Environment.CurrentDirectory), isActive, _isOverlayEnabled);
                await collection.InitializeItemsAsync(token);
            }

            token.ThrowIfCancellationRequested();

            return collection;
        }

        // ブックマークフォルダーコレクション作成
        private async Task<FolderCollection> CreateBookmarkFolderCollectionAsync(QueryPath path, bool isActive, CancellationToken token)
        {
            var collection = new BookmarkFolderCollection(path, _isOverlayEnabled);
            await collection.InitializeItemsAsync(token);

            token.ThrowIfCancellationRequested();

            return collection;
        }

        // プレイリストフォルダーコレクション作成
        public async Task<FolderCollection> CreatePlaylistFolderCollectionAsync(QueryPath path, bool isActive, CancellationToken token)
        {
            try
            {
                var collection = new PlaylistFolderCollection(path, _isOverlayEnabled);
                await collection.InitializeItemsAsync(token);
                token.ThrowIfCancellationRequested();
                return collection;
            }
            catch (Exception ex)
            {
                // NOTE: 展開できない場合、実在パスでの展開を行う
                Debug.WriteLine($"Cannot open: {ex.Message}");
                var place = ArchiveEntryUtility.GetExistDirectoryName(path.SimplePath);
                return await CreateEntryFolderCollectionAsync(new QueryPath(place), isActive, token);
            }
        }

        // アーカイブフォルダーコレクション作成
        public async Task<FolderCollection> CreateArchiveFolderCollectionAsync(QueryPath path, bool isActive, CancellationToken token)
        {
            try
            {
                var collection = new FolderArchiveCollection(path, BookHub.Current.ArchiveRecursiveMode, isActive, _isOverlayEnabled);
                await collection.InitializeItemsAsync(token);
                token.ThrowIfCancellationRequested();
                return collection;
            }
            catch (Exception ex)
            {
                // NOTE: アーカイブパスが展開できない場合、実在パスでの展開を行う
                Debug.WriteLine($"Cannot open: {ex.Message}");
                var place = ArchiveEntryUtility.GetExistDirectoryName(path.SimplePath);
                return await CreateEntryFolderCollectionAsync(new QueryPath(place), isActive, token);
            }
        }

        /// <summary>
        /// Rootコレクション作成
        /// </summary>
        private async Task<FolderCollection> CreateRootFolderCollectionAsync(QueryPath path, bool isActive, CancellationToken token)
        {
            var collection = new RootFolderCollection(path, _isOverlayEnabled);
            await collection.InitializeItemsAsync(token);
            return collection;
        }

        /// <summary>
        /// クイックアクセスコレクション作成
        /// </summary>
        private async Task<FolderCollection> CreateQuickAccessFolderCollectionAsync(QueryPath path, bool isActive, CancellationToken token)
        {
            var collection = new QuickAccessFolderCollection(_isOverlayEnabled);
            await collection.InitializeItemsAsync(token);
            return collection;
        }

        #endregion
    }
}
