using System;

namespace NeeView
{
    public class BookSource : IDisposable
    {
        public BookSource(ArchiveEntryCollection archiveEntryCollection, BookPageCollection pages)
        {
            ArchiveEntryCollection = archiveEntryCollection;
            Pages = pages;

            _isRecursiveFolder = ArchiveEntryCollection.Mode == ArchiveEntryCollectionMode.IncludeSubArchives;
        }

        // 再読み込みを要求
        public event EventHandler DartyBook;

        // この本のアーカイバ
        public ArchiveEntryCollection ArchiveEntryCollection { get; private set; }

        // この本の場所
        public string Address => this.ArchiveEntryCollection.Path;

        // この本はディレクトリ？
        public bool IsDirectory => this.ArchiveEntryCollection.Archiver is FolderArchive;

        // メディアアーカイバ？
        public bool IsMedia => ArchiveEntryCollection?.Archiver is MediaArchiver;

        // ページマークアーカイバ？
        public bool IsPagemarkFolder => ArchiveEntryCollection?.Archiver is PagemarkArchiver;

        /// <summary>
        /// 読み込まれなかったサブフォルダ数。再帰判定用
        /// </summary>
        public int SubFolderCount { get; set; }

        // この本を構成するページ
        public BookPageCollection Pages { get; private set; }


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
    }

}
