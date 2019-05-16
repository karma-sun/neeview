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
    public class SusiePluginCollection : INotifyPropertyChanged, IDisposable
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


        private ObservableCollection<SusiePlugin> _AMPluginList = new ObservableCollection<SusiePlugin>();
        private ObservableCollection<SusiePlugin> _INPluginList = new ObservableCollection<SusiePlugin>();


        public SusiePluginCollection()
        {
        }


        public string PluginFolder { get; set; }

        /// <summary>
        /// 書庫プラグインリスト
        /// </summary>
        public ObservableCollection<SusiePlugin> AMPluginList
        {
            get { return _AMPluginList; }
            set { if (_AMPluginList != value) { _AMPluginList = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// 画像プラグインリスト
        /// </summary>
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


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach (var plugin in PluginCollection)
                    {
                        plugin.Dispose();
                    }
                    INPluginList.Clear();
                    AMPluginList.Clear();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        public void Initialize(string spiFolder)
        {
            PluginFolder = spiFolder;

            // TODO: この判定はおかしい。ちゃんと例外にして処理したほうがよさそう。
            if (string.IsNullOrWhiteSpace(spiFolder)) return;
            if (!Directory.Exists(spiFolder)) return;


            var spiFiles = Directory.GetFiles(spiFolder, "*.spi");

            var plugins = new List<SusiePlugin>();
            foreach (var fileName in spiFiles)
            {
                var spi = SusiePlugin.Create(fileName);
                if (spi != null)
                {
                    if (spi.PluginType == SusiePluginType.None)
                    {
                        Debug.WriteLine("no support SPI (wrong API version): " + Path.GetFileName(fileName));
                        spi.Dispose();
                    }
                    else
                    {
                        plugins.Add(spi);
                    }
                }
                else
                {
                    Debug.WriteLine("no support SPI (Exception): " + Path.GetFileName(fileName));
                }
            }

            INPluginList = new ObservableCollection<SusiePlugin>(plugins.Where(e => e.PluginType == SusiePluginType.Image));
            AMPluginList = new ObservableCollection<SusiePlugin>(plugins.Where(e => e.PluginType == SusiePluginType.Archive));
        }


        public void SetPluginSetting(List<SusiePluginSetting> settings)
        {
            if (settings == null) return;

            foreach (var plugin in PluginCollection)
            {
                var setting = settings.FirstOrDefault(e => e.Name == plugin.Name);
                if (setting != null)
                {
                    plugin.Restore(setting);
                }
            }
        }

        public void SortPlugins(List<string> orders)
        {
            if (orders == null) return;

            var comparar = new PluginOrderComparer(orders);
            INPluginList = new ObservableCollection<SusiePlugin>(INPluginList.OrderBy(e => e, comparar));
            AMPluginList = new ObservableCollection<SusiePlugin>(AMPluginList.OrderBy(e => e, comparar));
        }


        /// <summary>
        /// 予約順にSPIを並び替えるためのコンペア 
        /// </summary>
        class PluginOrderComparer : IComparer<SusiePlugin>
        {
            private List<string> _order;

            public PluginOrderComparer(List<string> order)
            {
                _order = order;
            }

            public int Compare(SusiePlugin spiX, SusiePlugin spiY)
            {
                int indexX = _order.IndexOf(spiX.Name);
                int indexY = _order.IndexOf(spiY.Name);
                return indexX - indexY;
            }
        }

        // ロード済プラグイン取得
        public SusiePlugin GetPlugin(string fileName)
        {
            return PluginCollection.FirstOrDefault(e => e.FileName == fileName);
        }

        // ロード済プラグイン取得
        public SusiePlugin GetPluginFromName(string name)
        {
            return PluginCollection.FirstOrDefault(e => e.Name == name);
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
