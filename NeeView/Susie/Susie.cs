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

        //public List<string> SearchPath { get; set; } = new List<string>();

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


        /*
        public void Initialize()
        {
            if (_Initialized) throw new InvalidOperationException("already initialized.");

            SearchPath.ForEach(e => Load(e));
            _Initialized = true;
        }
        */

        public void Initialize(Dictionary<string, bool> spiFiles)
        {
            if (_Initialized) throw new InvalidOperationException("already initialized.");

            Load(spiFiles);
            _Initialized = true;
        }


        //
        private void Load(string folder)
        {
            if (string.IsNullOrEmpty(folder)) return;
            if (!Directory.Exists(folder)) return;

            foreach (string s in Directory.GetFiles(folder, "*.spi"))
            {
                Load(s, true);
            }
        }

        //
        private void Load(Dictionary<string, bool> spiFiles)
        {
            if (spiFiles == null) return;

            foreach (var pair in spiFiles)
            {
                Load(pair.Key, pair.Value);
            }
        }

        //
        private void Load(string fileName, bool isEnable)
        {
            var source = SusiePlugin.Create(fileName);
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

                source.IsEnable = isEnable;
            }
        }

        //
        public ArchiveFileInfoCollection GetArchiveInfo(string fileName)
        {
            if (!_Initialized) Initialize(null);

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
            if (!_Initialized) Initialize(null);

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
