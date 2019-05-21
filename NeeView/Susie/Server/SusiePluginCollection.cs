using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NeeView.Susie.Server
{
    public class SusiePluginCollection : IDisposable
    {
        public SusiePluginCollection()
        {
        }


        public string PluginFolder { get; private set; }

        /// <summary>
        /// 書庫プラグインリスト
        /// </summary>
        public List<SusiePlugin> AMPluginList { get; private set; } = new List<SusiePlugin>();

        /// <summary>
        /// 画像プラグインリスト
        /// </summary>
        public List<SusiePlugin> INPluginList { get; private set; } = new List<SusiePlugin>();

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

            INPluginList = new List<SusiePlugin>(plugins.Where(e => e.PluginType == SusiePluginType.Image));
            AMPluginList = new List<SusiePlugin>(plugins.Where(e => e.PluginType == SusiePluginType.Archive));
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
            INPluginList = new List<SusiePlugin>(INPluginList.OrderBy(e => e, comparar));
            AMPluginList = new List<SusiePlugin>(AMPluginList.OrderBy(e => e, comparar));
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


        // 名前でプラグイン取得
        public SusiePlugin GetPluginFromName(string name)
        {
            return PluginCollection.FirstOrDefault(e => e.Name == name);
        }

        // 対応アーカイブプラグイン取得
        public SusiePlugin GetArchivePlugin(string fileName, byte[] buff, bool isCheckExtension)
        {
            return GetPlugin(AMPluginList, fileName, buff, isCheckExtension);
        }


        // 対応画像プラグイン取得
        public SusiePlugin GetImagePlugin(string fileName, byte[] buff, bool isCheckExtension)
        {
            return GetPlugin(INPluginList, fileName, buff, isCheckExtension);
        }

        /// <summary>
        /// 対応プラグインを取得
        /// </summary>
        public SusiePlugin GetPlugin(List<SusiePlugin> plugins, string fileName, byte[] buff, bool isCheckExtension)
        {
            plugins = plugins ?? PluginCollection.ToList();
            buff = buff ?? LoadHead(fileName);

            foreach (var plugin in plugins)
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
        /// 画像取得
        /// </summary>
        /// <param name="plugins">仕様プラグイン。nullですべてのプラグインから取得を試みる</param>
        /// <param name="fileName">ファイル名</param>
        /// <param name="buff">ファイル実体。指定されていればこのメモリから画像生成する</param>
        /// <param name="isCheckExtension">拡張子判定を行う？</param>
        public SusieImage GetImage(List<SusiePlugin> plugins, string fileName, byte[] buff, bool isCheckExtension)
        {
            plugins = plugins ?? INPluginList;

            var fromFile = buff is null;
            buff = buff ?? LoadHead(fileName);

            foreach (var plugin in plugins.Where(e => e.IsEnabled))
            {
                try
                {
                    var bitmapImage = fromFile
                        ? plugin.GetPictureFromFile(fileName, buff, isCheckExtension)
                        : plugin.GetPicture(fileName, buff, isCheckExtension);
                    if (bitmapImage != null)
                    {
                        return new SusieImage(plugin.Name, bitmapImage);
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
        /// 判定用にファイル先頭を読み込む
        /// </summary>
        private byte[] LoadHead(string fileName)
        {
            var buff = new byte[2048];
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                fs.Read(buff, 0, 2048);
            }
            return buff;
        }
    }
}
