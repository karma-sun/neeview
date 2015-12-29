using SevenZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class SevenZipArchiver : Archiver
    {
        static SevenZipArchiver()
        {
            SevenZipExtractor.SetLibraryPath("7z.dll");
            //var features = SevenZip.SevenZipExtractor.CurrentLibraryFeatures;
            //Console.WriteLine(((uint)features).ToString("X6"));
        }

        private string _ArchiveFileName;
        public override string Path => _ArchiveFileName;

        private static object _Lock = new object();

        public SevenZipArchiver(string archiveFileName)
        {
            _ArchiveFileName = archiveFileName;
        }

        // エントリーリストを得る
        public override List<PageFileInfo> GetEntries()
        {
            List<PageFileInfo> entries = new List<PageFileInfo>();

            lock (_Lock)
            {
                using (var archive = new SevenZipExtractor(_ArchiveFileName))
                {
                    foreach (var entry in archive.ArchiveFileData)
                    {
                        if (!entry.IsDirectory)
                        {
                            entries.Add(new PageFileInfo()
                            {
                                Path = entry.FileName,
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
            finally
            {
                archive?.Dispose();
            }
        }


        public override void ExtractToFile(string entryName, string exportFileName)
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
