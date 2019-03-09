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
    public class SevenZipExtractArchiver : Archiver, IDisposable
    {
        #region Fields

        private ArchiveEntry _source;
        private string _temp;

        #endregion

        #region Constructors

        public SevenZipExtractArchiver(string path, ArchiveEntry source) : base(path, source)
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

        public override List<ArchiveEntry> GetEntriesInner(CancellationToken token)
        {
            Open(token);

            token.ThrowIfCancellationRequested();

            var list = new List<ArchiveEntry>();
            var directories = new List<ArchiveEntry>();

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
                        directories.Add(archiveEntry);
                    }
                }

                // ディレクトリエントリを追加
                list.AddRange(CreateDirectoryEntries(list.Concat(directories)));
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
            if (entry.Id < 0) throw new ApplicationException("Cannot open this entry: " + entry.EntryName);

            return new FileStream(GetFileSystemPath(entry), FileMode.Open, FileAccess.Read);
        }

        public override void ExtractToFile(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            if (entry.Id < 0) throw new ApplicationException("Cannot open this entry: " + entry.EntryName);

            File.Copy(GetFileSystemPath(entry), exportFileName, isOverwrite);
        }

        private void Open(CancellationToken token)
        {
            if (_temp != null) return;

            var directory = Temporary.Current.CreateCountedTempFileName("arc", "");

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
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }

                Close();

                _disposedValue = true;
            }
        }

        ~SevenZipExtractArchiver()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
