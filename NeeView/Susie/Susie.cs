using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Susie
{
    /// <summary>
    /// 
    /// </summary>
    public class Susie
    {
        public List<SusiePlugin> INPlgunList { get; private set; } = new List<SusiePlugin>();
        public List<SusiePlugin> AMPlgunList { get; private set; } = new List<SusiePlugin>();

        public List<string> SearchPath { get; set; } = new List<string>();

        private bool _Initialized;

        private static bool _SusiePluginInstallPathInitialized;
        private static string _SusiePluginInstallPath;
        public static string GetSusiePluginInstallPath()
        {
            if (!_SusiePluginInstallPathInitialized)
            {
                try
                {
                    RegistryKey regkey = Registry.CurrentUser.OpenSubKey(@"Software\Takechin\Susie\Plug-in", false);
                    _SusiePluginInstallPath = (string)regkey?.GetValue("Path");
                }
                catch
                {
                }
                _SusiePluginInstallPathInitialized = true;
            }

            return _SusiePluginInstallPath;
        }

        public Susie()
        {
        }

        public Susie(params string[] searchPath)
        {
            SearchPath = searchPath.ToList();
        }

        public void Initialize()
        {
            if (_Initialized) throw new InvalidOperationException("already initialized.");

            SearchPath.ForEach(e => Load(e));
            _Initialized = true;
        }

        //
        private void Load(string folder)
        {
            if (string.IsNullOrEmpty(folder)) return;
            if (!Directory.Exists(folder)) return;
            
            foreach (string s in Directory.GetFiles(folder, "*.spi"))
            {
                var source = SusiePlugin.Create(s);
                if (source != null)
                {
                    if (source.ApiVersion == "00IN" && !INPlgunList.Exists(e => e.PluginVersion == source.PluginVersion))
                    {
                        INPlgunList.Add(source);
                    }
                    else if (source.ApiVersion == "00AM" && !AMPlgunList.Exists(e => e.PluginVersion == source.PluginVersion))
                    {
                        AMPlgunList.Add(source);
                    }
                }
            }
        }

        //
        public ArchiveFileInfoCollection GetArchiveInfo(string fileName)
        {
            if (!_Initialized) Initialize();

            foreach (var plugin in AMPlgunList)
            {
                try
                {
                    var archiveInfo = plugin.GetArchiveInfo(fileName);
                    if (archiveInfo != null) return archiveInfo;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
            return null;
        }

        //
        public BitmapImage GetPicture(string fileName, byte[] buff)
        {
            if (!_Initialized) Initialize();

            foreach (var plugin in INPlgunList)
            {
                try
                {
                    var bitmapImage = plugin.GetPicture(fileName, buff);
                    if (bitmapImage != null) return bitmapImage;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
            return null;
        }



    }
}
