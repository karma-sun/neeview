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
        public List<SusiePlugin> AMPlgunList { get; set; } = new List<SusiePlugin>();
        // 画像プラグインリスト
        public List<SusiePlugin> INPlgunList { get; set; } = new List<SusiePlugin>();

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


        // 対応アーカイブプラグイン取得
        public SusiePlugin GetArchivePlugin(string fileName)
        {
            // 先頭の一部をメモリに読み込む
            var head = new byte[4096]; // バッファに余裕をもたせる
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                fs.Read(head, 0, 2048);
            }

            foreach (var plugin in AMPlgunList)
            {
                try
                {
                    if (plugin.IsSupported(fileName, head))
                    {
                        return plugin;
                    }
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
            SusiePlugin spiDummy;
            return GetPicture(fileName, buff, out spiDummy);
        }

        /// <summary>
        /// 画像取得 (メモリ版)
        /// </summary>
        /// <param name="fileName">フォーマット判定に使用される。ファイルアクセスはされません</param>
        /// <param name="buff">画像データ</param>
        /// <param name="spi">使用されたプラグイン</param>
        /// <returns>BitmapImage</returns>
        public BitmapImage GetPicture(string fileName, byte[] buff, out SusiePlugin spi)
        {
            foreach (var plugin in INPlgunList)
            {
                try
                {
                    var bitmapImage = plugin.GetPicture(fileName, buff);
                    if (bitmapImage != null)
                    {
                        spi = plugin;
                        return bitmapImage;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
            spi = null;
            return null;
        }


        /// <summary>
        /// 画像取得 (ファイル版)
        /// </summary>
        /// <param name="fileName">ファイルパス</param>
        /// <returns>BitmapImage</returns>
        public BitmapImage GetPictureFromFile(string fileName)
        {
            SusiePlugin spiDummy;
            return GetPictureFromFile(fileName, out spiDummy);
        }

        /// <summary>
        /// 画像取得 (ファイル版)
        /// </summary>
        /// <param name="fileName">ファイルパス</param>
        /// <param name="spi">使用されたプラグイン</param>
        /// <returns>BitmapImage</returns>
        public BitmapImage GetPictureFromFile(string fileName, out SusiePlugin spi)
        {
            // 先頭の一部をメモリに読み込む
            var head = new byte[4096];
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                fs.Read(head, 0, 2048);
            }

            foreach (var plugin in INPlgunList)
            {
                try
                {
                    var bitmapImage = plugin.GetPictureFromFile(fileName, head);
                    if (bitmapImage != null)
                    {
                        spi = plugin;
                        return bitmapImage;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
            spi = null;
            return null;
        }
    }
}
