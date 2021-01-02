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
    public class BookAddress
    {
        #region Properties

        /// <summary>
        /// 開始ページ名
        /// </summary>
        public string EntryName { get; set; }

        /// <summary>
        /// ブックのアドレス
        /// </summary>
        public QueryPath Address { get; set; }

        /// <summary>
        /// ブックのあるフォルダー
        /// </summary>
        public QueryPath Place { get; set; }

        /// <summary>
        /// ページを含めたアーカイブパス
        /// </summary>
        public string SystemPath => LoosePath.Combine(Address.SimplePath, EntryName);

        /// <summary>
        /// ソースアドレス。ショートカットファイルとか
        /// </summary>
        public QueryPath SourceAddress { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// BookAddress生成
        /// </summary>
        ///  <param name="query">入力パス</param>
        /// <param name="entryName">開始ページ名</param>
        /// <param name="option"></param>
        /// <param name="token">キャンセルトークン</param>
        /// <returns>生成したインスタンス</returns>
        public static async Task<BookAddress> CreateAsync(QueryPath query, QueryPath sourceQuery, string entryName, ArchiveEntryCollectionMode mode, BookLoadOption option, CancellationToken token)
        {
            var address = new BookAddress();
            await address.ConstructAsync(query, sourceQuery, entryName, mode, option, token);
            return address;
        }

        /// <summary>
        /// 初期化。
        /// アーカイブ展開等を含むため、非同期処理。
        /// </summary>
        private async Task ConstructAsync(QueryPath query, QueryPath sourceQuery, string entryName, ArchiveEntryCollectionMode mode, BookLoadOption option, CancellationToken token)
        {
            this.SourceAddress = sourceQuery ?? query;

            // ページマークはそのまま
            if (query.Scheme == QueryScheme.Pagemark)
            {
                this.Address = new QueryPath(QueryScheme.Pagemark);
                this.EntryName = entryName;
                this.Place = new QueryPath(QueryScheme.Root);
                return;
            }

            // ブックマークは実体のパスへ
            if (query.Scheme == QueryScheme.Bookmark)
            {
                var node = BookmarkCollection.Current.FindNode(query);
                switch (node.Value)
                {
                    case Bookmark bookmark:
                        query = new QueryPath(bookmark.Path);
                        break;
                    case BookmarkFolder folder:
                        throw new BookAddressException(string.Format(Properties.Resources.Notice_CannotOpenBookmarkFolder, query.SimplePath));
                }
            }

            // アーカイブエントリを取得
            var entry = await ArchiveEntryUtility.CreateAsync(query.SimplePath, token);

            // ページ名が指定されているなら入力そのまま
            if (entryName != null)
            {
                Debug.Assert(!option.HasFlag(BookLoadOption.IsBook));
                this.Address = query;
                this.EntryName = entryName;
            }
            // パスはブック
            else if (entry.IsBook() || option.HasFlag(BookLoadOption.IsBook))
            {
                Debug.Assert(!option.HasFlag(BookLoadOption.IsPage));
                this.Address = query;
                this.EntryName = null;
            }
            // パスはページ
            else
            {
                this.Address = query.GetParent();
                this.EntryName = query.FileName;
                entry = await ArchiveEntryUtility.CreateAsync(Address.SimplePath, token);
            }

            this.Place = GetPlace(entry, mode);
            Debug.Assert(this.Place != null);
        }

        /// <summary>
        /// エントリのあるフォルダーの場所を取得
        /// </summary>
        private QueryPath GetPlace(ArchiveEntry entry, ArchiveEntryCollectionMode mode)
        {
            if (entry == null)
            {
                return new QueryPath(QueryScheme.Root);
            }

            if (entry.IsFileSystem)
            {
                return new QueryPath(entry.SystemPath).GetParent();
            }
            else
            {
                if (mode == ArchiveEntryCollectionMode.IncludeSubArchives)
                {
                    return new QueryPath(entry.Archiver.RootArchiver.SystemPath).GetParent();
                }
                else if (mode == ArchiveEntryCollectionMode.IncludeSubDirectories)
                {
                    if (entry.IsArchive())
                    {
                        return new QueryPath(entry.Archiver.SystemPath);
                    }
                    else if (entry.Archiver.Parent != null)
                    {
                        return new QueryPath(entry.Archiver.Parent.SystemPath);
                    }
                    else
                    {
                        return new QueryPath(entry.Archiver.SystemPath).GetParent();
                    }
                }
                else
                {
                    return new QueryPath(entry.SystemPath).GetParent();
                }
            }
        }

        #endregion
    }

}
