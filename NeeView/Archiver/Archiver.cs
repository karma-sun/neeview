using SevenZip;
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
    public enum ArchiverType
    {
        None,

        FolderFiles,
        ZipArchiver,
        ZipArchiverKeepOpened, // 未使用
        SevenZipArchiver,
        SusieArchiver,

        DefaultArchiver = ZipArchiver
    }

    public class ArchiverManager
    {
        Dictionary<ArchiverType, string[]> _SupprtedFileTypes = new Dictionary<ArchiverType, string[]>()
        {
            [ArchiverType.ZipArchiver] = new string[] { ".zip" },
            [ArchiverType.SevenZipArchiver] = new string[] { ".7z", ".rar", ".lzh" },
            [ArchiverType.SusieArchiver] = new string[] { }
        };

        Dictionary<ArchiverType, List<ArchiverType>> _OrderList = new Dictionary<ArchiverType, List<ArchiverType>>()
        {
            [ArchiverType.DefaultArchiver] = new List<ArchiverType>()
            {
                ArchiverType.ZipArchiver,
                ArchiverType.SevenZipArchiver,
                ArchiverType.SusieArchiver
            },
            [ArchiverType.SusieArchiver] = new List<ArchiverType>()
            {
                ArchiverType.SusieArchiver,
                ArchiverType.ZipArchiver,
                ArchiverType.SevenZipArchiver,
            },
        };

        public ArchiverType OrderType { set; get; } = ArchiverType.DefaultArchiver;

        public bool IsSupported(string fileName)
        {
            return GetSupportedType(fileName) != ArchiverType.None;
        }

        public ArchiverType GetSupportedType(string fileName)
        {
            if (fileName.Last() == '\\') // &&  Directory.Exists(fileName))
            {
                return ArchiverType.FolderFiles;
            }

            string ext = LoosePath.GetExtension(fileName);

            foreach (var type in _OrderList[OrderType])
            {
                if (_SupprtedFileTypes[type].Contains(ext))
                {
                    return type;
                }
            }
            return ArchiverType.None;
        }

        public void UpdateSusieSupprtedFileTypes(Susie.Susie susie)
        {
            var list = new List<string>();
            foreach (var plugin in susie.AMPlgunList)
            {
                if (plugin.IsEnable)
                {
                    list.AddRange(plugin.Extensions);
                    /*
                    foreach (var supportType in plugin.SupportFileTypeList)
                    {
                        foreach (var filter in supportType.Extension.Split(';'))
                        {
                            list.Add(filter.TrimStart('*').ToLower());
                        }
                    }
                    */
                }
            }
            _SupprtedFileTypes[ArchiverType.SusieArchiver] = list.Distinct().ToArray();
        }

        public Archiver CreateArchiver(ArchiverType type, string path)
        {
            switch (type)
            {
                case ArchiverType.FolderFiles:
                    return new FolderFiles(path);
                case ArchiverType.ZipArchiver:
                    return new ZipArchiver(path);
                case ArchiverType.ZipArchiverKeepOpened:
                    return new ZipArchiverKeepOpened(path);
                case ArchiverType.SevenZipArchiver:
                    return new SevenZipArchiver(path);
                case ArchiverType.SusieArchiver:
                    return new SusieArchiver(path);
                default:
                    throw new ArgumentException("no support ArchvierType.", nameof(type));
            }
        }

        public Archiver CreateArchiver(string path)
        {
            if (Directory.Exists(path))
            {
                return CreateArchiver(ArchiverType.FolderFiles, path);
            }

            return CreateArchiver(GetSupportedType(path), path);
        }
    }


    public class PageFileInfo
    {
        public string Path { get; set; }
        public DateTime UpdateTime { get; set; }
    }

    public abstract class Archiver : IDisposable
    {
        public abstract string Path { get; }
        public abstract List<PageFileInfo> GetEntries();
        public abstract Stream OpenEntry(string entryName);
        public abstract void ExtractToFile(string entryName, string exportFileName);

        public List<IDisposable> TrashBox { get; private set; } = new List<IDisposable>();

        public virtual void Dispose()
        {
            TrashBox.ForEach(e => e.Dispose());
            TrashBox.Clear();
        }
    }

}
