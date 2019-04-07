using NeeView.Collections.Generic;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class BookSourceFactory
    {
        // 本読み込み
        public async Task<BookSource> CreateAsync(QueryPath address, BookCreateSetting setting, CancellationToken token)
        {
            // ページ生成
            var archiveEntryCollection = CreateArchiveEntryCollection(address.SimplePath, setting.IsRecursiveFolder, setting.ArchiveRecursiveMode);
            var pages = await CreatePageCollection(archiveEntryCollection, setting.BookPageCollectMode, token);

            // 再帰判定用サブフォルダー数カウント
            int subFolderCount = 0;
            if (archiveEntryCollection.Mode != ArchiveEntryCollectionMode.IncludeSubArchives && pages.Where(e => e.PageType == PageType.File).Count() == 0)
            {
                var entries = await archiveEntryCollection.GetEntriesWhereBookAsync(token);
                subFolderCount = entries.Count;
            }

            // 事前展開処理
            await PreExtractAsync(pages, token);

            // prefix設定
            SetPagePrefix(pages);

            var book = new BookSource(archiveEntryCollection, new BookPageCollection(pages, setting.SortMode));
            book.SubFolderCount = subFolderCount;

            return book;
        }


        private ArchiveEntryCollection CreateArchiveEntryCollection(string place, bool isRecursived, ArchiveEntryCollectionMode archiveRecursiveMode)
        {
            var collectMode = isRecursived ? ArchiveEntryCollectionMode.IncludeSubArchives : ArchiveEntryCollectionMode.CurrentDirectory;
            var collectModeIfArchive = isRecursived ? ArchiveEntryCollectionMode.IncludeSubArchives : archiveRecursiveMode;
            var collectOption = ArchiveEntryCollectionOption.None;
            return new ArchiveEntryCollection(place, collectMode, collectModeIfArchive, collectOption);
        }

        /// <summary>
        /// ページ生成
        /// </summary>
        private async Task<List<Page>> CreatePageCollection(ArchiveEntryCollection archiveEntryCollection, BookPageCollectMode bookPageCollectMode, CancellationToken token)
        {
            List<ArchiveEntry> entries;
            switch (bookPageCollectMode)
            {
                case BookPageCollectMode.Image:
                    entries = await archiveEntryCollection.GetEntriesWhereImageAsync(token);
                    break;
                case BookPageCollectMode.ImageAndBook:
                    entries = await archiveEntryCollection.GetEntriesWhereImageAndArchiveAsync(token);
                    break;
                case BookPageCollectMode.All:
                default:
                    entries = await archiveEntryCollection.GetEntriesWherePageAllAsync(token);
                    break;
            }

            var bookPrefix = LoosePath.TrimDirectoryEnd(archiveEntryCollection.Path);
            return entries.Select(e => CreatePage(bookPrefix, e)).ToList();
        }

        /// <summary>
        /// ページ作成
        /// </summary>
        /// <param name="entry">ファイルエントリ</param>
        /// <returns></returns>
        private Page CreatePage(string bookPrefix, ArchiveEntry entry)
        {
            Page page;

            if (entry.IsImage())
            {
                if (entry.Archiver is MediaArchiver)
                {
                    page = new Page(bookPrefix, new MediaContent(entry));
                }
                else if (entry.Archiver is PdfArchiver)
                {
                    page = new Page(bookPrefix, new PdfContetnt(entry));
                }
                else if (BookProfile.Current.IsEnableAnimatedGif && LoosePath.GetExtension(entry.EntryName) == ".gif")
                {
                    page = new Page(bookPrefix, new AnimatedContent(entry));
                }
                else
                {
                    page = new Page(bookPrefix, new BitmapContent(entry));
                }
            }
            else if (entry.IsBook())
            {
                page = new Page(bookPrefix, new ArchiveContent(entry));
                page.Thumbnail.IsCacheEnabled = true;
            }
            else
            {
                var type = entry.IsDirectory ? ArchiverType.FolderArchive : ArchiverManager.Current.GetSupportedType(entry.EntryName);
                switch (type)
                {
                    case ArchiverType.None:
                        if (BookProfile.Current.IsAllFileAnImage)
                        {
                            entry.IsIgnoreFileExtension = true;
                            page = new Page(bookPrefix, new BitmapContent(entry));
                        }
                        else
                        {
                            page = new Page(bookPrefix, new FileContent(entry, FilePageIcon.File, null));
                        }
                        break;
                    case ArchiverType.FolderArchive:
                        page = new Page(bookPrefix, new FileContent(entry, FilePageIcon.Folder, null));
                        break;
                    default:
                        page = new Page(bookPrefix, new FileContent(entry, FilePageIcon.Archive, null));
                        break;
                }
            }

            return page;
        }


        /// <summary>
        /// PageのPrefix設定
        /// </summary>
        private void SetPagePrefix(List<Page> pages)
        {
            // TODO: ページ生成と同時に行うべき?
            var prefix = GetPagesPrefix(pages);
            foreach (var page in pages)
            {
                page.Prefix = prefix;
            }
        }

        // 名前の最長一致文字列取得
        private string GetPagesPrefix(List<Page> pages)
        {
            if (pages == null || pages.Count == 0) return "";

            string s = pages[0].EntryFullName;
            foreach (var page in pages)
            {
                s = GetStartsWith(s, page.EntryFullName);
                if (string.IsNullOrEmpty(s)) break;
            }

            // １ディレクトリだけの場合に表示が消えないようにする
            if (pages.Count == 1)
            {
                s = s.TrimEnd('\\', '/');
            }

            // 最初の区切り記号
            for (int i = s.Length - 1; i >= 0; --i)
            {
                if (s[i] == '\\' || s[i] == '/')
                {
                    return s.Substring(0, i + 1);
                }
            }

            // ヘッダとして認識できなかった
            return "";
        }

        //
        private string GetStartsWith(string s0, string s1)
        {
            if (s0 == null || s1 == null) return "";

            if (s0.Length > s1.Length)
            {
                var temp = s0;
                s0 = s1;
                s1 = temp;
            }

            for (int i = 0; i < s0.Length; ++i)
            {
                char a0 = s0[i];
                char a1 = s1[i];
                if (s0[i] != s1[i])
                {
                    return i > 0 ? s0.Substring(0, i) : "";
                }
            }

            return s0;
        }


        // 事前展開(仮)
        // TODO: 事前展開の非同期化。ページアクセスをトリガーにする
        private async Task PreExtractAsync(List<Page> pages, CancellationToken token)
        {
            var archivers = pages
                .Select(e => e.Entry.Archiver)
                .Distinct()
                .Where(e => e != null && !e.IsFileSystem)
                .ToList();

            foreach (var archiver in archivers)
            {
                if (archiver.CanPreExtract())
                {
                    Debug.WriteLine($"PreExtract: EXTRACT {archiver.EntryName}");
                    await archiver.PreExtractAsync(token);
                }
            }
        }
    }

}
