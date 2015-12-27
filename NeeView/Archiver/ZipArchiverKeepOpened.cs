using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class ZipArchiverKeepOpened : Archiver
    {
        private string _ArchiveFileName;
        public override string Path => _ArchiveFileName;

        private ZipArchive _Archive;
        private object _Lock = new object();

        public ZipArchiverKeepOpened(string archiveFileName)
        {
            _ArchiveFileName = archiveFileName;
        }

        // エントリーリストを得る
        public override List<PageFileInfo> GetEntries()
        {
            _Archive = OpenArchive();

            List<PageFileInfo> entries = new List<PageFileInfo>();

            foreach (var entry in _Archive.Entries)
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

            var ms = new MemoryStream();

            lock (_Lock)
            {
                using (var stream = entry.Open())
                {
                    stream.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                }
            }

            return ms;
        }

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
