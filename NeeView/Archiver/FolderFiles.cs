using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class FolderFiles : Archiver
    {
        private string _FolderFileName;
        public override string Path => _FolderFileName;

        public FolderFiles(string folderFileName)
        {
            _FolderFileName = folderFileName;
        }

        public override List<PageFileInfo> GetEntries()
        {
            int prefixLen = _FolderFileName.Length;
            var entries = new List<PageFileInfo>();
            foreach (var path in Directory.GetFiles(_FolderFileName))
            {
                entries.Add(new PageFileInfo()
                {
                    Path = path.Substring(prefixLen).TrimStart('\\', '/'),
                    UpdateTime = File.GetLastWriteTime(path),
                });
            }
            foreach (var path in Directory.GetDirectories(_FolderFileName))
            {
                entries.Add(new PageFileInfo()
                {
                    Path = path.Substring(prefixLen).TrimStart('\\', '/') + "\\",
                    UpdateTime = File.GetLastWriteTime(path),
                });
            }

            return entries;
        }

        public override Stream OpenEntry(string entryName)
        {
            return new FileStream(System.IO.Path.Combine(_FolderFileName, entryName), FileMode.Open, FileAccess.Read);
        }

        public string GetFullPath(string entryName)
        {
            return System.IO.Path.Combine(_FolderFileName, entryName);
        }

        public override void ExtractToFile(string entryName, string exportFileName)
        {
            throw new NotImplementedException();
        }
    }


}
