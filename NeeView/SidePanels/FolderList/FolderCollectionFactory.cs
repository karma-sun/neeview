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
        public async Task<FolderCollection> CreateFolderCollectionAsync(string place, CancellationToken token)
        {
            if (place == null || Directory.Exists(place))
            {
                return await CreateEntryFolderCollectionAsync(place, token);
            }
            else
            {
                return await CreateArchiveFolderCollectionAsync(place, token);
            }
        }

        // 検索コレクション作成
        public async Task<FolderCollection> CreateSearchFolderCollectionAsync(string place, string keyword, CancellationToken token)
        {
            if (SearchEngine == null) throw new InvalidOperationException("SearchEngine not initialized.");

            try
            {
                var result = await SearchEngine.SearchAsync(place, keyword);
                token.ThrowIfCancellationRequested();

                var collection = CreateSearchCollection(place, result);
                return collection;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
        }

        // 通常フォルダーコレクション作成
        private async Task<FolderCollection> CreateEntryFolderCollectionAsync(string place, CancellationToken token)
        {
            var collection = await Task.Run(() => CreateEntryCollection(place));
            token.ThrowIfCancellationRequested();

            return collection;
        }

        // アーカイブフォルダーコレクション作成
        public async Task<FolderCollection> CreateArchiveFolderCollectionAsync(string place, CancellationToken token)
        {
            try
            {
                using (var entry = await ArchiveFileSystem.CreateArchiveEntry(place, token))
                {
                    var collection = CreateArchiveCollection(place, await ArchiverManager.Current.CreateArchiverAsync(entry, false, false, token));
                    token.ThrowIfCancellationRequested();
                    return collection;
                }
            }
            catch (Exception ex)
            {
                // アーカイブパスが展開できない場合、実在パスでの展開を行う
                Debug.WriteLine($"Cannot open: {ex.Message}");
                place = ArchiveFileSystem.GetExistDirectoryName(place);
                return await CreateEntryFolderCollectionAsync(place, token);
            }
        }

        /// <summary>
        /// FolderCollection 作成
        /// </summary>
        private FolderCollection CreateEntryCollection(string place)
        {
            try
            {
                return new FolderEntryCollection(place);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                // 救済措置。取得に失敗した時はカレントディレクトリに移動
                return new FolderEntryCollection(Environment.CurrentDirectory);
            }
        }


        /// <summary>
        /// FolderCollection作成(書庫内アーカイブリスト)
        /// </summary>
        public FolderCollection CreateArchiveCollection(string place, Archiver archiver)
        {
            return new FolderArchiveCollection(place, archiver);
        }

        /// <summary>
        /// FolderCollection作成(検索結果)
        /// </summary>
        private FolderCollection CreateSearchCollection(string place, NeeLaboratory.IO.Search.SearchResultWatcher searchResult)
        {
            return new FolderSearchCollection(place, searchResult);
        }

        #endregion
    }
}
