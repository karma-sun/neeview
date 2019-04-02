using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// BookSource生成設定
    /// </summary>
    public class BookSourceCreateSetting
    {
        /// <summary>
        /// フォルダー再帰？
        /// </summary>
        public bool IsRecursiveFolder { get; set; }

        /// <summary>
        /// 圧縮ファイルの再帰方針
        /// </summary>
        public ArchiveEntryCollectionMode ArchiveRecursiveMode { get; set; }

        /// <summary>
        /// ページ収集モード
        /// </summary>
        public BookPageCollectMode BookPageCollectMode { get; set; } = BookPageCollectMode.ImageAndBook;

        /// <summary>
        /// ページソート
        /// </summary>
        public PageSortMode SortMode { get; set; }
    }



    public class BookSource : IDisposable
    {
        // 再読み込みを要求
        public event EventHandler DartyBook;

        // この本の場所
        // nullの場合、この本は無効
        public string Address { get; private set; }

        // この本はディレクトリ？
        public bool IsDirectory { get; private set; }


        // サブフォルダー読み込み
        private bool _isRecursiveFolder;
        public bool IsRecursiveFolder
        {
            get { return _isRecursiveFolder; }
            set
            {
                if (_isRecursiveFolder != value)
                {
                    _isRecursiveFolder = value;
                    DartyBook?.Invoke(this, null);
                }
            }
        }


        // この本のアーカイバ
        public ArchiveEntryCollection ArchiveEntryCollection { get; private set; }

        // メディアアーカイバ？
        public bool IsMedia => ArchiveEntryCollection?.Archiver is MediaArchiver;

        // ページマークアーカイバ？
        public bool IsPagemarkFolder => ArchiveEntryCollection?.Archiver is PagemarkArchiver;


        /// <summary>
        /// 読み込まれなかったサブフォルダ数。再帰判定用
        /// </summary>
        public int SubFolderCount { get; private set; }


        public BookPageCollection Pages { get; private set; }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    this.DartyBook = null;
                    Pages.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion


        public string GetArchiverDetail()
        {
            var archiver = ArchiveEntryCollection?.Archiver;
            if (archiver == null)
            {
                return null;
            }

            var inner = archiver.Parent != null ? Properties.Resources.WordInner + " " : "";

            var extension = LoosePath.GetExtension(archiver.EntryName);

            var archiverType = ArchiverManager.Current.GetArchiverType(archiver);
            switch (archiverType)
            {
                case ArchiverType.FolderArchive:
                    return Properties.Resources.ArchiveFormatFolder;
                case ArchiverType.ZipArchiver:
                case ArchiverType.SevenZipArchiver:
                case ArchiverType.SusieArchiver:
                    return inner + Properties.Resources.ArchiveFormatCompressedFile + $"({extension})";
                case ArchiverType.PdfArchiver:
                    return inner + Properties.Resources.ArchiveFormatPdf + $"({extension})";
                case ArchiverType.MediaArchiver:
                    return inner + Properties.Resources.ArchiveFormatMedia + $"({extension})";
                case ArchiverType.PagemarkArchiver:
                    return Properties.Resources.ArchiveFormatPagemark;
                default:
                    return Properties.Resources.ArchiveFormatUnknown;
            }
        }

        public string GetDetail()
        {
            string text = "";
            text += GetArchiverDetail() + "\n";
            text += string.Format(Properties.Resources.BookAddressInfoPage, Pages.Count);
            return text;
        }

        public string GetFolderPlace()
        {
            return ArchiveEntryCollection.GetFolderPlace();
        }


        public class BookFactory
        {
            // 本読み込み
            public async Task<BookSource> CreateAsync(QueryPath address, BookSourceCreateSetting setting, CancellationToken token)
            {
                var book = new BookSource();
                book.IsRecursiveFolder = setting.IsRecursiveFolder;

                // ページ生成
                var archiveEntryCollection = CreateArchiveEntryCollection(address.SimplePath, setting.IsRecursiveFolder, setting.ArchiveRecursiveMode);
                var pages = await CreatePageCollection(archiveEntryCollection, setting.BookPageCollectMode, token);

                // 再帰判定用サブフォルダー数カウント
                if (archiveEntryCollection.Mode != ArchiveEntryCollectionMode.IncludeSubArchives && pages.Where(e => !(e is ArchivePage)).Count() == 0)
                {
                    var entries = await archiveEntryCollection.GetEntriesWhereBookAsync(token);
                    book.SubFolderCount = entries.Count;
                }

                // 事前展開処理
                await PreExtractAsync(pages, token);

                // prefix設定
                SetPagePrefix(pages);

                book.ArchiveEntryCollection = archiveEntryCollection;
                book.Pages = new BookPageCollection(pages, setting.SortMode);
                book.Address = archiveEntryCollection.Path;
                book.IsDirectory = archiveEntryCollection.Archiver is FolderArchive;

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
                        page = new MediaPage(bookPrefix, entry);
                    }
                    else if (entry.Archiver is PdfArchiver)
                    {
                        page = new PdfPage(bookPrefix, entry);
                    }
                    else if (BookProfile.Current.IsEnableAnimatedGif && LoosePath.GetExtension(entry.EntryName) == ".gif")
                    {
                        page = new AnimatedPage(bookPrefix, entry);
                    }
                    else
                    {
                        page = new BitmapPage(bookPrefix, entry);
                    }
                }
                else if (entry.IsBook())
                {
                    page = new ArchivePage(bookPrefix, entry);
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
                                page = new BitmapPage(bookPrefix, entry);
                            }
                            else
                            {
                                page = new FilePage(bookPrefix, entry, FilePageIcon.File);
                            }
                            break;
                        case ArchiverType.FolderArchive:
                            page = new FilePage(bookPrefix, entry, FilePageIcon.Folder);
                            break;
                        default:
                            page = new FilePage(bookPrefix, entry, FilePageIcon.Archive);
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
}
