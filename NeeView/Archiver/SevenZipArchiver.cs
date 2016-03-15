// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using SevenZip;
using System;
using System.Collections.Generic;
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


        //
        public SevenZipArchiver(string archiveFileName)
        {
            _ArchiveFileName = archiveFileName;
        }


        // エントリーリストを得る
        public override List<ArchiveEntry> GetEntries()
        {
            List<ArchiveEntry> entries = new List<ArchiveEntry>();

            lock (_Lock)
            {
                using (var archive = new SevenZipExtractor(_ArchiveFileName))
                {
                    foreach (var entry in archive.ArchiveFileData)
                    {
                        if (!entry.IsDirectory)
                        {
                            entries.Add(new ArchiveEntry()
                            {
                                FileName = entry.FileName,
                                UpdateTime = entry.LastWriteTime,
                            });
                        }
                    }
                }
            }

            return entries;
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
