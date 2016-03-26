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

        // コンストラクタ
        public ZipArchiver(string archiveFileName)
        {
            FileName = archiveFileName;
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

            using (var archiver = ZipFile.OpenRead(FileName))
            {
                for (int id = 0; id < archiver.Entries.Count; ++id)
                {
                    var entry = archiver.Entries[id];
                    if (entry.Length > 0)
                    {
                        list.Add(new ArchiveEntry()
                        {
                            Archiver = this,
                            Id = id,
                            Instance = null,
                            EntryName = entry.FullName,
                            FileSize = entry.Length,
                            LastWriteTime = entry.LastWriteTime.Date,
                        });
                    }
                }
            }

            return list;
        }

        // エントリーのストリームを得る
        public override Stream OpenStream(ArchiveEntry entry)
        {
            using (var archiver = ZipFile.OpenRead(FileName))
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
            using (var archiver = ZipFile.OpenRead(FileName))
            {
                ZipArchiveEntry archiveEntry = archiver.Entries[entry.Id];
                archiveEntry.ExtractToFile(exportFileName, isOverwrite);
            }
        }
    }
}
