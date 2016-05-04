// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using SevenZip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TODO: 読み込み速度改善。特にrar。オープン持続フラグ？

namespace NeeView
{
    /// <summary>
    /// アーカイバ：7z.dll
    /// </summary>
    public class SevenZipArchiver : Archiver
    {
        public override string ToString()
        {
            return "7zip.dll";
        }

        private static object _Lock = new object();

        //
        static SevenZipArchiver()
        {
            SevenZipExtractor.SetLibraryPath("7z.dll");
            //var features = SevenZip.SevenZipExtractor.CurrentLibraryFeatures;
            //Console.WriteLine(((uint)features).ToString("X6"));
        }


        // コンストラクタ
        public SevenZipArchiver(string archiveFileName)
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

            lock (_Lock)
            {
                using (var archive = new SevenZipExtractor(FileName))
                {
                    for (int id = 0; id < archive.ArchiveFileData.Count; ++id)
                    {
                        var entry = archive.ArchiveFileData[id];
                        if (!entry.IsDirectory)
                        {
                            list.Add(new ArchiveEntry()
                            {
                                Archiver = this,
                                Id = id,
                                EntryName = entry.FileName,
                                FileSize = (long)entry.Size,
                                LastWriteTime = entry.LastWriteTime,
                            });
                        }
                    }
                }
            }

            return list;
        }


        // エントリーのストリームを得る
        public override Stream OpenStream(ArchiveEntry entry)
        {
            SevenZipExtractor archive = null;
            Exception exception = null;

            for (int retryCount = 0; retryCount < 2; ++retryCount) // retry .. あまり効いてない？
            {
                try
                {
                    lock (_Lock)
                    {
                        archive = new SevenZipExtractor(FileName);
                    }

                    var archiveEntry = archive.ArchiveFileData[entry.Id];
                    if (archiveEntry.FileName != entry.EntryName)
                    {
                        throw new ApplicationException("ページデータの不整合");
                    }

                    var ms = new MemoryStream();
                    archive.ExtractFile(entry.Id, ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    return ms;
                }
                catch (Exception e)
                {
                    exception = e;
                }
                finally
                {
                    archive?.Dispose();
                }
            }

            throw new ApplicationException("ストリーム作成に失敗しました", exception);
        }


        //
        public override void ExtractToFile(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            SevenZipExtractor archive = null;

            try
            {
                lock (_Lock)
                {
                    archive = new SevenZipExtractor(FileName);
                }

                using (Stream fs = new FileStream(exportFileName, FileMode.Create, FileAccess.Write))
                {
                    archive.ExtractFile(entry.Id, fs);
                }
            }
            finally
            {
                archive?.Dispose();
            }
        }
    }

}
