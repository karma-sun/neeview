using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class ZipArchiver : Archiver
    {
        private string _ArchiveFileName;
        public override string Path => _ArchiveFileName;

        public ZipArchiver(string archiveFileName)
        {
            _ArchiveFileName = archiveFileName;
        }

        // エントリーリストを得る
        public override List<PageFileInfo> GetEntries()
        {
            List<PageFileInfo> entries = new List<PageFileInfo>();

            using (var archiver = ZipFile.OpenRead(_ArchiveFileName))
            {
                foreach (var entry in archiver.Entries)
                {
                    if (entry.Length > 0)
                    {
                        entries.Add(new PageFileInfo()
                        {
                            Path = entry.FullName,
                            UpdateTime = entry.LastWriteTime.UtcDateTime,
                        });
                    }
                }
            }

            return entries;
        }


        // エントリーのストリームを得る
        public override Stream OpenEntry(string entryName)
        {
            using (var archiver = ZipFile.OpenRead(_ArchiveFileName))
            {
                ZipArchiveEntry entry = archiver.GetEntry(entryName);
                if (entry == null) throw new ArgumentException($"アーカイブエントリ {entryName} が見つかりません");

                var ms = new MemoryStream();

                using (var stream = entry.Open())
                {
                    stream.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                }

                return ms;
            }
        }

        public override void ExtractToFile(string entryName, string exportFileName)
        {
            using (var archiver = ZipFile.OpenRead(_ArchiveFileName))
            {
                ZipArchiveEntry entry = archiver.GetEntry(entryName);
                if (entry == null) throw new ArgumentException($"アーカイブエントリ {entryName} が見つかりません");

                entry.ExtractToFile(exportFileName);
            }
        }
    }
}
