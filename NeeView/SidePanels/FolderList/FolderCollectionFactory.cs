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
        public async Task<FolderCollection> CreateFolderCollectionAsync(string place, bool isActive, CancellationToken token)
        {
            if (place != null && place.StartsWith(Bookmark.Scheme))
            {
                return await CreateBookmarkFolderCollectionAsync(place, isActive, token);
            }
            else if (place == null || Directory.Exists(place))
            {
                return await CreateEntryFolderCollectionAsync(place, isActive, token);
            }
            else
            {
                return await CreateArchiveFolderCollectionAsync(place, isActive, token);
            }
        }

        // 検索コレクション作成
        public async Task<FolderCollection> CreateSearchFolderCollectionAsync(string place, string keyword, bool isActive, CancellationToken token)
        {
            if (SearchEngine == null) throw new InvalidOperationException("SearchEngine not initialized.");

            try
            {
                var result = await SearchEngine.SearchAsync(place, keyword);
                token.ThrowIfCancellationRequested();

                var collection = CreateSearchCollection(place, result, isActive);
                return collection;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
        }

        // 通常フォルダーコレクション作成
        private async Task<FolderCollection> CreateEntryFolderCollectionAsync(string place, bool isActive, CancellationToken token)
        {
            var collection = await Task.Run(() => CreateEntryCollection(place, isActive));
            token.ThrowIfCancellationRequested();

            return collection;
        }

        // ブックマークフォルダーコレクション作成
        private async Task<FolderCollection> CreateBookmarkFolderCollectionAsync(string place, bool isActive, CancellationToken token)
        {
            var collection = await Task.Run(() => CreateBookmarkFolderCollection(place, isActive));
            token.ThrowIfCancellationRequested();

            return collection;
        }

        // アーカイブフォルダーコレクション作成
        public async Task<FolderCollection> CreateArchiveFolderCollectionAsync(string place, bool isActive, CancellationToken token)
        {
            try
            {
                using (var entry = await ArchiveFileSystem.CreateArchiveEntry(place, token))
                {
                    var collection = CreateArchiveCollection(place, await ArchiverManager.Current.CreateArchiverAsync(entry, false, false, token), isActive);
                    token.ThrowIfCancellationRequested();
                    return collection;
                }
            }
            catch (Exception ex)
            {
                // アーカイブパスが展開できない場合、実在パスでの展開を行う
                Debug.WriteLine($"Cannot open: {ex.Message}");
                place = ArchiveFileSystem.GetExistDirectoryName(place);
                return await CreateEntryFolderCollectionAsync(place, isActive, token);
            }
        }

        /// <summary>
        /// FolderCollection 作成
        /// </summary>
        private FolderCollection CreateEntryCollection(string place, bool isActive)
        {
            try
            {
                return new FolderEntryCollection(place, isActive);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                // 救済措置。取得に失敗した時はカレントディレクトリに移動
                return new FolderEntryCollection(Environment.CurrentDirectory, isActive);
            }
        }


        /// <summary>
        /// FolderCollection作成(書庫内アーカイブリスト)
        /// </summary>
        public FolderCollection CreateArchiveCollection(string place, Archiver archiver, bool isActive)
        {
            return new FolderArchiveCollection(place, archiver, isActive);
        }

        /// <summary>
        /// FolderCollection作成(検索結果)
        /// </summary>
        private FolderCollection CreateSearchCollection(string place, NeeLaboratory.IO.Search.SearchResultWatcher searchResult, bool isActive)
        {
            return new FolderSearchCollection(place, searchResult, isActive);
        }

        /// <summary>
        /// FolderCollecion作成(ブックマーク)
        /// </summary>
        private FolderCollection CreateBookmarkFolderCollection(string place, bool isActive)
        {
            return new BookmarkFolderCollection(place);
        }

        #endregion
    }
}
