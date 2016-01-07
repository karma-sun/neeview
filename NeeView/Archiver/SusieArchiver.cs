using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class SusieArchiver : Archiver
    {
        private string _ArchiveFileName;
        public override string Path => _ArchiveFileName;

        Dictionary<string, Susie.ArchiveFileInfo> _ArchiveFileInfoDictionary;

        public SusieArchiver(string archiveFileName)
        {
            _ArchiveFileName = archiveFileName;
        }

        // エントリーリストを得る
        public override List<PageFileInfo> GetEntries()
        {
            var infoCollection = ModelContext.Susie.GetArchiveInfo(_ArchiveFileName);

            if (infoCollection == null) throw new NotSupportedException();

            _ArchiveFileInfoDictionary = new Dictionary<string, Susie.ArchiveFileInfo>();
            List<PageFileInfo> entries = new List<PageFileInfo>();
            foreach (var entry in infoCollection)
            {
                try
                {
                    string name = (entry.Path.TrimEnd('\\', '/') + "\\" + entry.FileName).TrimStart('\\', '/');

                    entries.Add(new PageFileInfo()
                    {
                        Path = name, //  entry.FileName,
                        UpdateTime = entry.TimeStamp,
                    });

                    _ArchiveFileInfoDictionary.Add(name, entry);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

            }

            return entries;
        }

        private object _Lock = new object();

        // エントリーのストリームを得る
        public override Stream OpenEntry(string entryName)
        {
            lock (_Lock)
            {
                var info = _ArchiveFileInfoDictionary[entryName];
                byte[] buffer = info.Load();
                return new MemoryStream(buffer, 0, buffer.Length, false, true);
            }
        }

        public override void ExtractToFile(string entryName, string extractFileName)
        {
            var info = _ArchiveFileInfoDictionary[entryName];

            string tempDirectory = Temporary.CreateTempFileName("Susie");
            Directory.CreateDirectory(tempDirectory);

            info.ExtractToFolder(tempDirectory);

            var files = Directory.GetFiles(tempDirectory);
            File.Move(files[0], extractFileName);
            Directory.Delete(tempDirectory, true);
        }
    }

}
