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
        private static bool s_susiePluginInstallPathInitialized;
        private static string s_susiePluginInstallPath;
        public static string GetSusiePluginInstallPath()
        {
            if (!s_susiePluginInstallPathInitialized)
            {
                try
                {
                    RegistryKey regkey = Registry.CurrentUser.OpenSubKey(@"Software\Takechin\Susie\Plug-in", false);
                    s_susiePluginInstallPath = (string)regkey?.GetValue("Path") ?? "";
                }
                catch
                {
                }
                s_susiePluginInstallPathInitialized = true;
            }

            return s_susiePluginInstallPath;
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
        public SusiePlugin GetArchivePlugin(string fileName, bool isCheckExtension)
        {
            // 先頭の一部をメモリに読み込む
            var head = new byte[4096]; // バッファに余裕をもたせる
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                fs.Read(head, 0, 2048);
            }

            return GetArchivePlugin(fileName, head, isCheckExtension);
        }


        // 対応アーカイブプラグイン取得(メモリ版)
        public SusiePlugin GetArchivePlugin(string fileName, byte[] buff, bool isCheckExtension)
        {
            foreach (var plugin in AMPlgunList)
            {
                try
                {
                    if (plugin.IsSupported(fileName, buff, isCheckExtension))
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




        // 対応画像プラグイン取得
        public SusiePlugin GetImagePlugin(string fileName, bool isCheckExtension)
        {
            // 先頭の一部をメモリに読み込む
            var head = new byte[4096]; // バッファに余裕をもたせる
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                fs.Read(head, 0, 2048);
            }

            return GetImagePlugin(fileName, head, isCheckExtension);
        }


        // 対応画像プラグイン取得(メモリ版)
        public SusiePlugin GetImagePlugin(string fileName, byte[] buff, bool isCheckExtension)
        {
            foreach (var plugin in INPlgunList)
            {
                try
                {
                    if (plugin.IsSupported(fileName, buff, isCheckExtension))
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
        public BitmapImage GetPicture(string fileName, byte[] buff, bool isCheckExtension)
        {
            SusiePlugin spiDummy;
            return GetPicture(fileName, buff, isCheckExtension, out spiDummy);
        }

        /// <summary>
        /// 画像取得 (メモリ版)
        /// </summary>
        /// <param name="fileName">フォーマット判定に使用される。ファイルアクセスはされません</param>
        /// <param name="buff">画像データ</param>
        /// <param name="spi">使用されたプラグイン</param>
        /// <returns>BitmapImage</returns>
        public BitmapImage GetPicture(string fileName, byte[] buff, bool isCheckExtension, out SusiePlugin spi)
        {
            foreach (var plugin in INPlgunList)
            {
                try
                {
                    var bitmapImage = plugin.GetPicture(fileName, buff, isCheckExtension);
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
        public BitmapImage GetPictureFromFile(string fileName, bool isCheckExtension)
        {
            SusiePlugin spiDummy;
            return GetPictureFromFile(fileName, isCheckExtension, out spiDummy);
        }

        /// <summary>
        /// 画像取得 (ファイル版)
        /// </summary>
        /// <param name="fileName">ファイルパス</param>
        /// <param name="spi">使用されたプラグイン</param>
        /// <returns>BitmapImage</returns>
        public BitmapImage GetPictureFromFile(string fileName, bool isCheckExtension, out SusiePlugin spi)
        {
            // 先頭の一部をメモリに読み込む
            var head = new byte[4096];
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                fs.Read(head, 0, 2048);
            }

            foreach (var plugin in INPlgunList)
            {
                try
                {
                    var bitmapImage = plugin.GetPictureFromFile(fileName, head, isCheckExtension);
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
