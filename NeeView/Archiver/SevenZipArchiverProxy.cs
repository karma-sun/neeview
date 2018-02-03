// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using SevenZip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace NeeView
{
    /// <summary>
    /// SevenZipArchiver と ZevenZipExtractArchvier を使い分けるプロキシ
    /// </summary>
    public class SevenZipArchiverProxy : Archiver
    {
        #region Fields

        private ArchiveEntry _source;
        private Archiver _archiver;
        private bool _isAll;

        private bool _isDisposed;

        #endregion

        #region Constructors

        public SevenZipArchiverProxy(string path, ArchiveEntry source, bool isRoot, bool isAll) : base(path, source, isRoot)
        {
            SevenZipArchiver.InitializeLibrary();
            _source = source;
            _isAll = isAll;
        }

        #endregion

        #region Properties

        public override bool IsDisposed => _isDisposed;

        #endregion

        #region Medhods

        //
        public override List<ArchiveEntry> GetEntries(CancellationToken token)
        {
            if (_isDisposed) throw new ApplicationException("Archive already colosed.");

            if (_archiver != null)
            {
                return _archiver.GetEntries(token);
            }

            var profile = SevenZipArchiverProfile.Current;
            var fileInfo = new FileInfo(this.Path);
            bool isExtract = _isAll && fileInfo.Length / (1024 * 1024) < profile.PreExtractSolidSize && (profile.IsPreExtract || IsSolid());

            if (isExtract)
            {
                ////Debug.WriteLine($"Pre extract: {this.Path}");
                try
                {
                    _archiver = new SevenZipExtractArchiver(this.Path, _source, this.RootFlag);
                    return _archiver.GetEntries(token);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{ex.Message}\n-> Change to SevenZipArchiver.");

                    if (_archiver != null)
                    {
                        _archiver.Dispose();
                        _archiver = null;
                    }
                }
            }

            _archiver = new SevenZipArchiver(this.Path, _source, this.RootFlag);
            return _archiver.GetEntries(token);
        }

        //
        private bool IsSolid()
        {
            using (var extractor = new SevenZipExtractor(this.Path))
            {
                return extractor.IsSolid;
            }
        }

        public override bool IsSupported()
        {
            return true;
        }

        //
        public override Stream OpenStream(ArchiveEntry entry)
        {
            if (_isDisposed) throw new ApplicationException("Archive already colosed.");
            if (_archiver == null) throw new ApplicationException("Not initialized.");

            return _archiver.OpenStream(entry);
        }

        //
        public override void ExtractToFile(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            if (_isDisposed) throw new ApplicationException("Archive already colosed.");
            if (_archiver == null) throw new ApplicationException("Not initialized.");

            _archiver.ExtractToFile(entry, exportFileName, isOverwrite);
        }

        //
        public override void SetRootFlag(bool flag)
        {
            base.SetRootFlag(flag);
            if (_archiver != null)
            {
                _archiver.SetRootFlag(flag);
            }
        }

        //
        public override void Dispose()
        {
            _isDisposed = true;

            _archiver?.Dispose();
            _archiver = null;

            base.Dispose();
        }

        #endregion
    }
}
