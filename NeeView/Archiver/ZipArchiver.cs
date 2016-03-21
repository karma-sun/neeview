// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
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
        public override Dictionary<string, ArchiveEntry> GetEntries()
        {
            Entries.Clear();

            using (var archiver = ZipFile.OpenRead(_ArchiveFileName))
            {
                foreach (var entry in archiver.Entries)
                {
                    if (entry.Length > 0)
                    {
                        Entries.Add(entry.FullName, new ArchiveEntry()
                        {
                            FileName = entry.FullName,
                            FileSize = entry.Length,
                            LastWriteTime = entry.LastWriteTime.Date,
                        });
                    }
                }
            }

            return Entries;
        }



        // エントリーのストリームを得る
        public override Stream OpenEntry(string entryName)
        {
            using (var archiver = ZipFile.OpenRead(_ArchiveFileName))
            {
                ZipArchiveEntry entry = archiver.GetEntry(entryName);
                if (entry == null) throw new ArgumentException($"アーカイブエントリ {entryName} が見つかりません");

                using (var stream = entry.Open())
                {
                    var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    return ms;
                }
            }
        }


        //
        public override void ExtractToFile(string entryName, string exportFileName, bool isOverwrite)
        {
            using (var archiver = ZipFile.OpenRead(_ArchiveFileName))
            {
                ZipArchiveEntry entry = archiver.GetEntry(entryName);
                if (entry == null) throw new ArgumentException($"アーカイブエントリ {entryName} が見つかりません");

                entry.ExtractToFile(exportFileName, isOverwrite);
            }
        }
    }
}
