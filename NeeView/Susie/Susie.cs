// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
    /// for Susie Plugin
    /// </summary>
    public class Susie
    {
        // 書庫プラグインリスト
        public Dictionary<string, SusiePlugin> AMPlgunList { get; private set; } = new Dictionary<string, SusiePlugin>();
        // 画像プラグインリスト
        public Dictionary<string, SusiePlugin> INPlgunList { get; private set; } = new Dictionary<string, SusiePlugin>();

        // すべてのプラグインのEnumerator
        public IEnumerable<SusiePlugin> PluginCollection
        {
            get
            {
                foreach (var plugin in AMPlgunList.Values) yield return plugin;
                foreach (var plugin in INPlgunList.Values) yield return plugin;
            }
        }


        // レジストリに登録されているSusiePluginパスの取得
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

        
        // プラグインロード
        public void Load(IEnumerable<string> spiFiles)
        {
            if (spiFiles == null) return;

            foreach (var fileName in spiFiles)
            {
                var source = SusiePlugin.Create(fileName);
                if (source != null)
                {
                    if (source.ApiVersion == "00IN" && !INPlgunList.ContainsKey(fileName))
                    {
                        INPlgunList.Add(fileName, source);
                    }
                    else if (source.ApiVersion == "00AM" && !AMPlgunList.ContainsKey(fileName))
                    {
                        AMPlgunList.Add(fileName, source);
                    }
                }
            }
        }


        // ロード済プラグイン取得
        public SusiePlugin GetPlugin(string fileName)
        {
            SusiePlugin plugin;
            if (AMPlgunList.TryGetValue(fileName, out plugin)) return plugin;
            if (INPlgunList.TryGetValue(fileName, out plugin)) return plugin;
            return null;
        }
        

        /// <summary>
        /// アーカイブ情報取得
        /// </summary>
        /// <param name="fileName">アーカイブファイル名</param>
        /// <returns>アーカイブ情報</returns>
        public ArchiveEntryCollection GetArchiveInfo(string fileName)
        {
            foreach (var plugin in AMPlgunList.Values)
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


        /// <summary>
        /// 画像取得 (メモリ版)
        /// </summary>
        /// <param name="fileName">フォーマット判定に使用される。ファイルアクセスはされません</param>
        /// <param name="buff">画像データ</param>
        /// <returns>BitmapImage</returns>
        public BitmapImage GetPicture(string fileName, byte[] buff)
        {
            foreach (var plugin in INPlgunList.Values)
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
