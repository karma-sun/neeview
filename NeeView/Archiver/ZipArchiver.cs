// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// TODO: 書庫内書庫 ストリームによる多重展開が可能？

namespace NeeView
{
    /// <summary>
    /// アーカイバー：標準Zipアーカイバー
    /// </summary>
    public class ZipArchiver : Archiver
    {
        private bool _isDisposed;

        public override string ToString()
        {
            return ".Net ZipArchiver";
        }

        // コンストラクタ
        public ZipArchiver(string path, ArchiveEntry source) : base(path, source)
        {
        }

        //
        public override bool IsDisposed => _isDisposed;

        // Dispose
        public override void Dispose()
        {
            _isDisposed = true;
            base.Dispose();
        }

        // サポート判定
        public override bool IsSupported()
        {
            return true;
        }


        // エントリーリストを得る
        public override List<ArchiveEntry> GetEntries(CancellationToken token)
        {
            if (_isDisposed) throw new ApplicationException("Archive already colosed.");

            var list = new List<ArchiveEntry>();

            using (var archiver = ZipFile.OpenRead(Path))
            {
                for (int id = 0; id < archiver.Entries.Count; ++id)
                {
                    token.ThrowIfCancellationRequested();

                    var entry = archiver.Entries[id];
                    if (entry.Length > 0)
                    {
                        list.Add(new ArchiveEntry()
                        {
                            Archiver = this,
                            Id = id,
                            Instance = null,
                            EntryName = entry.FullName,
                            Length = entry.Length,
                            LastWriteTime = entry.LastWriteTime.DateTime,
                        });
                    }
                }
            }

            return list;
        }

        // エントリーのストリームを得る
        public override Stream OpenStream(ArchiveEntry entry)
        {
            if (_isDisposed) throw new ApplicationException("Archive already colosed.");

            using (var archiver = ZipFile.OpenRead(Path))
            {
                ZipArchiveEntry archiveEntry = archiver.Entries[entry.Id];
                if (archiveEntry.FullName != entry.EntryName)
                {
                    throw new ApplicationException("ページデータの不整合");
                }

                using (var stream = archiveEntry.Open())
                {
                    var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    return ms;
                }
            }
        }

        //
        public override void ExtractToFile(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            if (_isDisposed) throw new ApplicationException("Archive already colosed.");

            using (var archiver = ZipFile.OpenRead(Path))
            {
                ZipArchiveEntry archiveEntry = archiver.Entries[entry.Id];
                archiveEntry.ExtractToFile(exportFileName, isOverwrite);
            }
        }
    }
}
