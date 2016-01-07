using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Susie
{
    /// <summary>
    /// 
    /// </summary>
    public class SusiePlugin
    {
        public bool IsEnable { get; set; }

        public string FileName { get; private set; }

        public string ApiVersion { get; private set; }
        public string PluginVersion { get; private set; }

        public bool HasConfigurationDlg { get; private set; }

        public class SupportFileType
        {
            public string Extension; // ファイルの種類の拡張子
            public string Note; // ファイルの種類の情報
        }
        public List<SupportFileType> SupportFileTypeList { get; private set; }

        public List<string> Extensions { get; private set; } 


        public static SusiePlugin Create(string fileName)
        {
            var spis = new SusiePlugin();
            return spis.Initialize(fileName) ? spis : null;
        }

        public bool Initialize(string fileName)
        {
            if (FileName != null) throw new InvalidOperationException();

            try
            {
                using (var api = SusiePluginApi.Create(fileName))
                {
                    ApiVersion = api.GetPluginInfo(0);
                    PluginVersion = api.GetPluginInfo(1);

                    SupportFileTypeList = new List<SupportFileType>();
                    while (true)
                    {
                        int index = SupportFileTypeList.Count() * 2 + 2;
                        var fileType = new SupportFileType()
                        {
                            Extension = api.GetPluginInfo(index + 0),
                            Note = api.GetPluginInfo(index + 1)
                        };
                        if (fileType.Extension == null || fileType.Note == null) break;
                        SupportFileTypeList.Add(fileType);
                    }

                    HasConfigurationDlg = api.IsExistFunction("ConfigurationDlg");
                }

                FileName = fileName;

                // create extensions
                Extensions = new List<string>(); 
                foreach (var supportType in this.SupportFileTypeList)
                {
                    foreach (var filter in supportType.Extension.Split(';'))
                    {
                        Extensions.Add(filter.TrimStart('*').ToLower());
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }


        //
        public SusiePluginApi Open()
        {
            if (FileName == null) throw new InvalidOperationException();
            return SusiePluginApi.Create(FileName);
        }


        //
        public int AboutDlg(Window parent)
        {
            if (FileName == null) throw new InvalidOperationException();

            using (var api = Open())
            {
                IntPtr hwnd = new WindowInteropHelper(parent).Handle;
                return api.ConfigurationDlg(hwnd, 0);
            }
        }

        //
        public int ConfigurationDlg(Window parent)
        {
            if (FileName == null) throw new InvalidOperationException();

            using (var api = Open())
            {
                IntPtr hwnd = new WindowInteropHelper(parent).Handle;
                return api.ConfigurationDlg(hwnd, 1);
            }
        }

        public ArchiveFileInfoCollection GetArchiveInfo(string fileName)
        {
            if (FileName == null) throw new InvalidOperationException();
            if (!IsEnable) return null;

            // サポート拡張子チェック
            if (!Extensions.Contains(GetExtension(fileName))) return null;

            using (var api = Open())
            {
                string shortPath = Win32Api.GetShortPathName(fileName);
                if (!api.IsSupported(shortPath)) return null;
                return new ArchiveFileInfoCollection(this, fileName, api.GetArchiveInfo(shortPath));
            }
        }


        public BitmapImage GetPicture(string fileName, byte[] buff)
        {
            if (FileName == null) throw new InvalidOperationException();
            if (!IsEnable) return null;

            // サポート拡張子チェック
            if (!Extensions.Contains(GetExtension(fileName))) return null;

            // SPI
            using (var api = Open())
            {
                if (!api.IsSupported(fileName, buff)) return null;
                return api.GetPicture(buff);
            }
        }

        private static string GetExtension(string s)
        {
            return "." + s.Split('.').Last().ToLower();
        }

    }


    /// <summary>
    /// アーカイブ内ファイル情報 リスト
    /// </summary>
    public class ArchiveFileInfoCollection : List<ArchiveFileInfo>
    {
        public ArchiveFileInfoCollection(SusiePlugin spi, string archiveFileName, List<ArchiveFileInfoRaw> entries)
        {
            string shortPath = Win32Api.GetShortPathName(archiveFileName);
            foreach (var entry in entries)
            {
                this.Add(new ArchiveFileInfo(spi, shortPath, entry));
            }
        }
    }


    /// <summary>
    /// アーカイブ内ファイル情報
    /// </summary>
    public class ArchiveFileInfo
    {
        private SusiePlugin _Spi;
        ArchiveFileInfoRaw _Info; // Raw情報

        public string ArchiveShortFileName { get; private set; }

        public string Path => _Info.path;
        public string FileName => _Info.filename;
        public uint FileSize => _Info.filesize;
        public DateTime TimeStamp => Time_T2DateTime(_Info.timestamp);

        public ArchiveFileInfo(SusiePlugin spi, string archiveFileName, ArchiveFileInfoRaw info)
        {
            _Spi = spi;
            ArchiveShortFileName = archiveFileName;
            _Info = info;
        }

        // メモリ上に解凍
        public byte[] Load()
        {
            using (var api = _Spi.Open())
            {
                return api.GetFile(ArchiveShortFileName, _Info);
            }
        }

        // ファイルに出力
        public void ExtractToFolder(string extractFolder)
        {
            using (var api = _Spi.Open())
            {
                int ret = api.GetFile(ArchiveShortFileName, _Info, extractFolder);
                if (ret != 0) throw new IOException("抽出に失敗しました");
            }
        }

        private static DateTime Time_T2DateTime(uint time_t)
        {
            long win32FileTime = 10000000 * (long)time_t + 116444736000000000;
            return DateTime.FromFileTimeUtc(win32FileTime);
        }
    }


}
