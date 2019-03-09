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

        public SusieArchiverProxy(string path, ArchiveEntry source) : base(path, source)
        {
            var spi = SusieContext.Current.Susie?.GetArchivePlugin(Path, true);
            var isExtract = spi != null ? spi.IsPreExtract : false;

            if (isExtract)
            {
                _archiver = new SusieExtractArchiver(path, source);
            }
            else
            {
                _archiver = new SusieArchiver(path, source);
            }
        }

        #endregion

        #region Medhods

        public override List<ArchiveEntry> GetEntriesInner(CancellationToken token)
        {
            return _archiver.GetEntries(token);
        }

        public override bool IsSupported()
        {
            return _archiver.IsSupported();
        }

        public override Stream OpenStream(ArchiveEntry entry)
        {
            if (_archiver == null) throw new ApplicationException("Not initialized.");

            return _archiver.OpenStream(entry);
        }

        public override void ExtractToFile(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            if (_archiver == null) throw new ApplicationException("Not initialized.");

            _archiver.ExtractToFile(entry, exportFileName, isOverwrite);
        }

        #endregion
    }
}
