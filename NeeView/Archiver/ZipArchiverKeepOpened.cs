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
    /// アーカイバ：標準Zipアーカイバ、アーカイブ保持版
    /// 通常のアーカイバはアクセス毎にインスタンス開放するが、このアーカイバは保持し続ける(ファイルロック)。
    /// 高速化を期待したのだが、結果出ず、未使用です。
    /// </summary>
    public class ZipArchiverKeepOpened : Archiver
    {
        public override string ToString()
        {
            return ".Net ZipArchiver";
        }

        private string _ArchiveFileName;
        public override string FileName => _ArchiveFileName;

        private ZipArchive _Archive;

        // コンストラクタ
        public ZipArchiverKeepOpened(string archiveFileName)
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
            _Archive = OpenArchive();

            Entries.Clear();

            foreach (var entry in _Archive.Entries)
            {
                try
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
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            return Entries;
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
        public override void ExtractToFile(string entryName, string exportFileName, bool isOverwrite)
        {
            _Archive = OpenArchive();

            ZipArchiveEntry entry = _Archive.GetEntry(entryName);
            if (entry == null) throw new ArgumentException($"アーカイブエントリ {entryName} が見つかりません");

            entry.ExtractToFile(exportFileName, isOverwrite);
        }


        //
        public override void Dispose()
        {
            CloseArchive();
            base.Dispose();
        }
    }

}
