using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NeeView.Susie
{
    /// <summary>
    /// for Susie Plugin
    /// </summary>
    public class Susie : INotifyPropertyChanged
    {
        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion


        /// <summary>
        /// 書庫プラグインリスト
        /// </summary>
        private ObservableCollection<SusiePlugin> _AMPluginList = new ObservableCollection<SusiePlugin>();
        public ObservableCollection<SusiePlugin> AMPluginList
        {
            get { return _AMPluginList; }
            set { if (_AMPluginList != value) { _AMPluginList = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// 画像プラグインリスト
        /// </summary>
        private ObservableCollection<SusiePlugin> _INPluginList = new ObservableCollection<SusiePlugin>();
        public ObservableCollection<SusiePlugin> INPluginList
        {
            get { return _INPluginList; }
            set { if (_INPluginList != value) { _INPluginList = value; RaisePropertyChanged(); } }
        }



        // すべてのプラグインのEnumerator
        public IEnumerable<SusiePlugin> PluginCollection
        {
            get
            {
                foreach (var plugin in AMPluginList) yield return plugin;
                foreach (var plugin in INPluginList) yield return plugin;
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
        public void Load(IEnumerable<string> spiFiles, bool isPluginCacheEnabled)
        {
            if (spiFiles == null) return;

            // 既存のプラグインから残すものを抽出
            var inPluginList = INPluginList.Where(e => spiFiles.Contains(e.FileName)).ToList();
            var amPluginList = AMPluginList.Where(e => spiFiles.Contains(e.FileName)).ToList();

            // 削除するプラグイン
            var removes = PluginCollection.Where(e => !(inPluginList.Contains(e) || amPluginList.Contains(e)));
            foreach (var spi in removes)
            {
                spi.Dispose();
            }

            // 新しいプラグイン追加
            foreach (var fileName in spiFiles)
            {
                var spi = SusiePlugin.Create(fileName);
                if (spi != null)
                {
                    spi.IsCacheEnabled = isPluginCacheEnabled;
                    if (spi.PluginType == SusiePluginType.Image && !inPluginList.Any(e => e.FileName == fileName))
                    {
                        inPluginList.Add(spi);
                    }
                    else if (spi.PluginType == SusiePluginType.Archive && !amPluginList.Any(e => e.FileName == fileName))
                    {
                        amPluginList.Add(spi);
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

            var comparar = new MyComparer(spiFiles);
            INPluginList = new ObservableCollection<SusiePlugin>(inPluginList.OrderBy(e => e, comparar));
            AMPluginList = new ObservableCollection<SusiePlugin>(amPluginList.OrderBy(e => e, comparar));
        }

        /// <summary>
        /// 予約順にSPIを並び替えるためコンペア 
        /// </summary>
        class MyComparer : IComparer<SusiePlugin>
        {
            private List<string> _order;

            public MyComparer(IEnumerable<string> order)
            {
                _order = order.ToList();
            }

            public int Compare(SusiePlugin spiX, SusiePlugin spiY)
            {
                int indexX = _order.IndexOf(spiX.FileName);
                int indexY = _order.IndexOf(spiY.FileName);
                return indexX - indexY;
            }
        }

        // プラグインキャッシュ設定を変更
        public void SetPluginCahceEnabled(bool isPluginCacheEnabled)
        {
            foreach (var spi in PluginCollection)
            {
                spi.IsCacheEnabled = isPluginCacheEnabled;
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
            foreach (var plugin in AMPluginList)
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
            foreach (var plugin in INPluginList)
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
        /// <returns>Bitmap</returns>
        public byte[] GetPicture(string fileName, byte[] buff, bool isCheckExtension)
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
        /// <returns>Bitmap</returns>
        public byte[] GetPicture(string fileName, byte[] buff, bool isCheckExtension, out SusiePlugin spi)
        {
            foreach (var plugin in INPluginList)
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
        /// <returns>Bitmap</returns>
        public byte[] GetPictureFromFile(string fileName, bool isCheckExtension)
        {
            SusiePlugin spiDummy;
            return GetPictureFromFile(fileName, isCheckExtension, out spiDummy);
        }

        /// <summary>
        /// 画像取得 (ファイル版)
        /// </summary>
        /// <param name="fileName">ファイルパス</param>
        /// <param name="spi">使用されたプラグイン</param>
        /// <returns>Bitmap</returns>
        public byte[] GetPictureFromFile(string fileName, bool isCheckExtension, out SusiePlugin spi)
        {
            // 先頭の一部をメモリに読み込む
            var head = new byte[4096];
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                fs.Read(head, 0, 2048);
            }

            foreach (var plugin in INPluginList)
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
