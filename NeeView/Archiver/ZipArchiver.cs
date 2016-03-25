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
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイバ：標準Zipアーカイバ
    /// </summary>
    public class ZipArchiver : Archiver
    {
        public override string ToString()
        {
            return ".Net ZipArchiver";
        }

        private string _ArchiveFileName;
        public override string FileName => _ArchiveFileName;


        // コンストラクタ
        public ZipArchiver(string archiveFileName)
        {
            _ArchiveFileName = archiveFileName;
        }

        // サポート判定
        public override bool IsSupported()
        {
            return true;
        }

        // エントリーリストを得る
        public override List<ArchiveEntry> GetEntries()
        {
            var list = new List<ArchiveEntry>();

            using (var archiver = ZipFile.OpenRead(_ArchiveFileName))
            {
                foreach (var zipEntry in archiver.Entries)
                {
                    if (zipEntry.Length > 0)
                    {
                        list.Add(new ArchiveEntry()
                        {
                            Archiver = this,
                            Id = list.Count,
                            Instance = null,
                            FileName = zipEntry.FullName,
                            FileSize = zipEntry.Length,
                            LastWriteTime = zipEntry.LastWriteTime.Date,
                        });
                    }
                }
            }

            return list;
        }

        // エントリーのストリームを得る
        public override Stream OpenStream(ArchiveEntry entry)
        {
            using (var archiver = ZipFile.OpenRead(_ArchiveFileName))
            {
                ZipArchiveEntry zipEntry = archiver.Entries[entry.Id];

                using (var stream = zipEntry.Open())
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
            using (var archiver = ZipFile.OpenRead(_ArchiveFileName))
            {
                ZipArchiveEntry zipEntry = archiver.Entries[entry.Id];
                zipEntry.ExtractToFile(exportFileName, isOverwrite);
            }
        }
    }
}
