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
    public class SusieExtractArchiver : Archiver
    {
        #region Fields

        private string _temp;
        private SusieArchiver _archiver;
        List<ArchiveEntry> _entries;

        #endregion

        #region Constructors

        public SusieExtractArchiver(string path, ArchiveEntry source, bool isRoot) : base(path, source, isRoot)
        {
            _archiver = new SusieArchiver(path, source, isRoot);
        }

        #endregion

        #region Properties

        public override string ToString() => _archiver.ToString() + " extractor";

        public override bool IsFileSystem { get; } = false;

        #endregion

        #region Methods

        public override List<ArchiveEntry> GetEntriesInner(CancellationToken token)
        {
            if (_disposedValue) throw new ApplicationException("Archive already colosed.");

            //Debug.WriteLine($"SusieExtractArchiver.GetEntry: ${this.Path}");

            Open(token);

            token.ThrowIfCancellationRequested();

            return _entries;
        }

        public override bool IsSupported()
        {
            return _archiver.IsSupported();
        }

        public override string GetFileSystemPath(ArchiveEntry entry)
        {
            if (entry.IsDirectory) throw new InvalidOperationException();

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

        public override void SetRootFlag(bool flag)
        {
            base.SetRootFlag(flag);
            if (_archiver != null)
            {
                _archiver.SetRootFlag(flag);
            }
        }


        private void Open(CancellationToken token)
        {
            if (IsDisposed || _temp != null) return;


            var directory = Temporary.Current.CreateCountedTempFileName("arc", "");
            _temp = directory;

            Directory.CreateDirectory(directory);

            var spi = _archiver.GetPlugin();
            var oldIsCacheEnabled = spi.IsCacheEnabled;

            lock (spi.GlobalLock)
            {
                token.ThrowIfCancellationRequested();

                //Debug.WriteLine($"SusieExtractArchiver.Open: ${this.Path}");

                try
                {
                    spi.IsCacheEnabled = true;

                    _entries = _archiver.GetEntries(token);

                    token.ThrowIfCancellationRequested();

                    foreach (var entry in _entries)
                    {
                        if (!entry.IsDirectory)
                        {
                            var extension = LoosePath.GetExtension(entry.EntryLastName);
                            var tempFileName = LoosePath.Combine(_temp, $"{entry.Id:000000}{extension}");
                            entry.ExtractToFile(tempFileName, false);

                            entry.Archiver = this;
                            entry.Instance = tempFileName;
                        }
                    }
                }
                finally
                {
                    spi.IsCacheEnabled = oldIsCacheEnabled;
                }
            }
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
                    if (_archiver != null)
                    {
                        _archiver.Dispose();
                    }
                }

                Close();
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}
