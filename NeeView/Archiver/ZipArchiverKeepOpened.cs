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
    /// アーカイバ：標準Zipアーカイバ、アーカイブ保持版
    /// 通常のアーカイバはアクセス毎にインスタンス開放するが、このアーカイバは保持し続ける(ファイルロック)。
    /// 高速化を期待したのだが、結果出ず、未使用です。
    /// </summary>
    public class ZipArchiverKeepOpened : Archiver
    {
        private string _ArchiveFileName;
        public override string FileName => _ArchiveFileName;

        private ZipArchive _Archive;

        //
        public ZipArchiverKeepOpened(string archiveFileName)
        {
            _ArchiveFileName = archiveFileName;
        }


        // エントリーリストを得る
        public override List<ArchiveEntry> GetEntries()
        {
            _Archive = OpenArchive();

            List<ArchiveEntry> entries = new List<ArchiveEntry>();

            foreach (var entry in _Archive.Entries)
            {
                if (entry.Length > 0)
                {
                    entries.Add(new ArchiveEntry()
                    {
                        Path = entry.FullName,
                        UpdateTime = entry.LastWriteTime.UtcDateTime,
                    });
                }
            }

            return entries;
        }


        // アーカイブを開く
        private ZipArchive OpenArchive()
        {
            if (_Archive == null)
            {
                _Archive = ZipFile.OpenRead(_ArchiveFileName);
            }
            return _Archive;
        }


        // アーカイブを閉じる
        private void CloseArchive()
        {
            if (_Archive != null)
            {
                _Archive.Dispose();
                _Archive = null;
            }
        }


        // エントリーのストリームを得る
        public override Stream OpenEntry(string entryName)
        {
            _Archive = OpenArchive();

            ZipArchiveEntry entry = _Archive.GetEntry(entryName);
            if (entry == null) throw new ArgumentException($"アーカイブエントリ {entryName} が見つかりません");

            using (var stream = entry.Open())
            {
                var ms = new MemoryStream();
                stream.CopyTo(ms);
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }
        }

        //
        public override void ExtractToFile(string entryName, string exportFileName)
        {
            _Archive = OpenArchive();

            ZipArchiveEntry entry = _Archive.GetEntry(entryName);
            if (entry == null) throw new ArgumentException($"アーカイブエントリ {entryName} が見つかりません");

            entry.ExtractToFile(exportFileName);
        }


        //
        public override void Dispose()
        {
            CloseArchive();
            base.Dispose();
        }
    }

}
