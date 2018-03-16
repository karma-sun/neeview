using SevenZip;
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
    /// <summary>
    /// 事前にテンポラリフォルダーに展開してアクセスするアーカイバー
    /// </summary>
    public class SevenZipExtractArchiver : Archiver
    {
        #region Fields

        private ArchiveEntry _source;
        private string _temp;

        #endregion

        #region Constructors

        public SevenZipExtractArchiver(string path, ArchiveEntry source, bool isRoot) : base(path, source, isRoot)
        {
            SevenZipArchiver.InitializeLibrary();
            _source = source ?? new ArchiveEntry(path);
        }

        #endregion

        #region Properties

        public override string ToString() => "7-Zip extractor";

        public override bool IsFileSystem { get; } = false;

        #endregion

        #region Methods

        public override List<ArchiveEntry> GetEntries(CancellationToken token)
        {
            if (_disposedValue) throw new ApplicationException("Archive already colosed.");

            Open(token);

            token.ThrowIfCancellationRequested();

            var list = new List<ArchiveEntry>();
            var directoryEntries = new List<ArchiveEntry>();

            using (var extractor = new SevenZipExtractor(this.Path))
            {
                foreach (var entry in extractor.ArchiveFileData)
                {
                    token.ThrowIfCancellationRequested();

                    var archiveEntry = new ArchiveEntry()
                    {
                        Archiver = this,
                        Id = entry.Index,
                        RawEntryName = entry.FileName,
                        Length = (long)entry.Size,
                        LastWriteTime = entry.LastWriteTime,
                        Instance = System.IO.Path.Combine(_temp, entry.GetTempFileName())
                    };

                    if (!entry.IsDirectory)
                    {
                        list.Add(archiveEntry);
                    }
                    else
                    {
                        archiveEntry.Length = -1;
                        archiveEntry.Instance = null;
                        directoryEntries.Add(archiveEntry);
                    }
                }

                // 空ディレクトリー追加
                if (BookProfile.Current.IsEnableNoSupportFile)
                {
                    list.AddDirectoryEntries(directoryEntries);
                }
            }

            return list;
        }

        public override bool IsSupported()
        {
            return true;
        }

        public override string GetFileSystemPath(ArchiveEntry entry)
        {
            return (string)entry.Instance;
        }

        public override Stream OpenStream(ArchiveEntry entry)
        {
            if (_disposedValue) throw new ApplicationException("Archive already colosed.");

            return new FileStream(GetFileSystemPath(entry), FileMode.Open, FileAccess.Read);
        }

        public override void ExtractToFile(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            if (_disposedValue) throw new ApplicationException("Archive already colosed.");

            File.Copy(GetFileSystemPath(entry), exportFileName, isOverwrite);
        }

        private void Open(CancellationToken token)
        {
            if (IsDisposed || _temp != null) return;

            var directory = Temporary.CreateCountedTempFileName("arc", "");

            using (var extractor = new SevenZipExtractor(this.Path))
            {
                extractor.ExtractArchiveTemp(directory);
            }

            _temp = directory;
        }

        private void Close()
        {
            if (_temp == null) return;

            try
            {
                if (Directory.Exists(_temp))
                {
                    Directory.Delete(_temp, true);
                }
            }
            catch
            {
                // nop.
            }

            _temp = null;
        }

        #endregion

        #region IDisposable Support

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }

                Close();
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}
