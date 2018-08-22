using SevenZip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace NeeView
{
    /// <summary>
    /// SusieArchiver と SusieExtractArchvier を使い分けるプロキシ
    /// </summary>
    public class SusieArchiverProxy : Archiver
    {
        #region Fields

        private Archiver _archiver;

        #endregion

        #region Constructors

        public SusieArchiverProxy(string path, ArchiveEntry source, bool isRoot) : base(path, source, isRoot)
        {
            var spi = SusieContext.Current.Susie?.GetArchivePlugin(Path, true);
            var isExtract = spi != null ? spi.IsPreExtract : false;

            if (isExtract)
            {
                _archiver = new SusieExtractArchiver(path, source, isRoot);
            }
            else
            {
                _archiver = new SusieArchiver(path, source, isRoot);
            }
        }

        #endregion

        #region Medhods

        public override List<ArchiveEntry> GetEntries(CancellationToken token)
        {
            if (_disposedValue) throw new ApplicationException("Archive already colosed.");
            return _archiver.GetEntries(token);
        }

        public override bool IsSupported()
        {
            return _archiver.IsSupported();
        }

        public override Stream OpenStream(ArchiveEntry entry)
        {
            if (_disposedValue) throw new ApplicationException("Archive already colosed.");
            if (_archiver == null) throw new ApplicationException("Not initialized.");

            return _archiver.OpenStream(entry);
        }

        public override void ExtractToFile(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            if (_disposedValue) throw new ApplicationException("Archive already colosed.");
            if (_archiver == null) throw new ApplicationException("Not initialized.");

            _archiver.ExtractToFile(entry, exportFileName, isOverwrite);
        }

        public override void SetRootFlag(bool flag)
        {
            base.SetRootFlag(flag);
            if (_archiver != null)
            {
                _archiver.SetRootFlag(flag);
            }
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
                        _archiver = null;
                    }
                }
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}
