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
        public List<SusiePlugin> AMPlgunList { get; private set; } = new List<SusiePlugin>();
        // 画像プラグインリスト
        public List<SusiePlugin> INPlgunList { get; private set; } = new List<SusiePlugin>();

        // すべてのプラグインのEnumerator
        public IEnumerable<SusiePlugin> PluginCollection
        {
            get
            {
                foreach (var plugin in AMPlgunList) yield return plugin;
                foreach (var plugin in INPlgunList) yield return plugin;
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
                    _SusiePluginInstallPath = (string)regkey?.GetValue("Path") ?? "";
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
                    if (source.ApiVersion == "00IN" && !INPlgunList.Any(e => e.FileName == fileName))
                    {
                        INPlgunList.Add(source);
                    }
                    else if (source.ApiVersion == "00AM" && !AMPlgunList.Any(e => e.FileName == fileName))
                    {
                        AMPlgunList.Add(source);
                    }
                    else
                    {
                        Debug.WriteLine("no support SPI (wrong API version): " + Path.GetFileName(fileName));
                    }
                }
                else
                {
                    Debug.WriteLine("no support SPI (Exception): " + Path.GetFileName(fileName));
                }
            }
        }


        // ロード済プラグイン取得
        public SusiePlugin GetPlugin(string fileName)
        {
            return PluginCollection.FirstOrDefault(e => e.FileName == fileName);
        }


        /// <summary>
        /// アーカイブ情報取得
        /// </summary>
        /// <param name="fileName">アーカイブファイル名</param>
        /// <returns>アーカイブ情報</returns>
        public ArchiveEntryCollection GetArchiveInfo(string fileName)
        {
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


        /// <summary>
        /// 画像取得 (メモリ版)
        /// </summary>
        /// <param name="fileName">フォーマット判定に使用される。ファイルアクセスはされません</param>
        /// <param name="buff">画像データ</param>
        /// <returns>BitmapImage</returns>
        public BitmapImage GetPicture(string fileName, byte[] buff)
        {
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
