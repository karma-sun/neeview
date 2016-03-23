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

        private string _ArchiveFileName;
        public override string FileName => _ArchiveFileName;

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

            lock (_Lock)
            {
                using (var archive = new SevenZipExtractor(_ArchiveFileName))
                {
                    foreach (var entry in archive.ArchiveFileData)
                    {
                        try
                        {
                            if (!entry.IsDirectory)
                            {
                                Entries.Add(entry.FileName, new ArchiveEntry()
                                {
                                    FileName = entry.FileName,
                                    FileSize = (long)entry.Size,
                                    LastWriteTime = entry.LastWriteTime,
                                });
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e.Message);
                        }
                    }
                }
            }

            return Entries;
        }


        // エントリーのストリームを得る
        public override Stream OpenEntry(string entryName)
        {
            SevenZipExtractor archive = null;
            Exception exception = null;

            for (int retryCount = 0; retryCount < 2; ++retryCount) // retry
            {
                try
                {
                    lock (_Lock)
                    {
                        archive = new SevenZipExtractor(_ArchiveFileName);
                    }

                    var ms = new MemoryStream();
                    archive.ExtractFile(entryName, ms);
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
        public override void ExtractToFile(string entryName, string exportFileName, bool isOverwrite)
        {
            SevenZipExtractor archive = null;

            try
            {
                lock (_Lock)
                {
                    archive = new SevenZipExtractor(_ArchiveFileName);
                }

                using (Stream fs = new FileStream(exportFileName, FileMode.Create, FileAccess.Write))
                {
                    archive.ExtractFile(entryName, fs);
                }
            }
            finally
            {
                archive?.Dispose();
            }
        }
    }

}
