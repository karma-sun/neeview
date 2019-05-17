using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NeeView.Susie.Client
{
    public class SusiePluginClient : IRemoteSusiePlugin, IDisposable
    {
        private SusiePluginCollection _pluginCollection;

        public SusiePluginClient()
        {

        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _pluginCollection?.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion


        public void ExtracArchiveEntrytToFolder(string pluginName, string fileName, int position, string extractFolder)
        {
            var plugin = _pluginCollection.AMPluginList.FirstOrDefault(e => e.Name == pluginName);
            if (plugin == null) throw new SusieIOException($"Cannot find plugin: {pluginName}");

            plugin.ExtracArchiveEntrytToFolder(fileName, position, extractFolder);
        }

        public List<SusieArchiveEntry> GetArchiveEntry(string pluginName, string fileName)
        {
            var plugin = _pluginCollection.AMPluginList.FirstOrDefault(e => e.Name == pluginName);
            if (plugin == null) throw new SusieIOException($"Cannot find plugin: {pluginName}");

            var collection = plugin.GetArchiveInfo(fileName);
            return collection.Select(e => e.ToSusieArchiveEntry()).ToList();
        }

        public SusiePluginInfo GetArchivePlugin(string fileName, byte[] buff, bool isCheckExtension)
        {
            // buff==nullのときの処理。ヘッダ2KBを読み込む
            if (buff == null)
            {
                buff = new byte[4096];
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    fs.Read(buff, 0, 2048);
                }
            }

            var plugin = _pluginCollection.GetArchivePlugin(fileName, buff, isCheckExtension);
            return plugin.ToSusiePluginInfo();
        }

        public SusiePluginInfo GetImagePlugin(string fileName, byte[] buff, bool isCheckExtension)
        {
            // buff==nullのときの処理。ヘッダ2KBを読み込む
            if (buff == null)
            {
                buff = new byte[4096];
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    fs.Read(buff, 0, 2048);
                }
            }

            var plugin = _pluginCollection.GetImagePlugin(fileName, buff, isCheckExtension);
            return plugin.ToSusiePluginInfo();
        }

        public SusieImage GetImage(string pluginName, string fileName, byte[] buff, bool isCheckExtension)
        {
            // TODO: pluginName
            if (buff != null)
            {
                var result = new SusieImage();
                result.BitmapData = _pluginCollection.GetPicture(fileName, buff, isCheckExtension, out var susiePlugin);
                result.PluginName = susiePlugin.ToString();
                return result;
            }
            else
            {
                var result = new SusieImage();
                result.BitmapData = _pluginCollection.GetPictureFromFile(fileName, isCheckExtension, out var susiePlugin);
                result.PluginName = susiePlugin.ToString();
                return result;
            }
        }

        public List<SusiePluginInfo> GetPlugin(List<string> pluginNames)
        {
            var plugins = pluginNames != null
                ? pluginNames.Select(e => _pluginCollection.PluginCollection.FirstOrDefault(x => x.Name == e))
                : _pluginCollection.PluginCollection;

            return plugins.Select(e => e.ToSusiePluginInfo()).ToList();
        }

        public SusiePluginServerSetting GetServerSetting()
        {
            if (_pluginCollection == null) return null;

            var setting = new SusiePluginServerSetting();
            setting.PluginFolder = _pluginCollection.PluginFolder;
            setting.PluginSettings = _pluginCollection.PluginCollection
                .Select(e => e.ToSusiePluginSetting())
                .ToList();

            return setting;
        }

        public byte[] ExtractArchiveEntry(string pluginName, string fileName, int position)
        {
            var plugin = _pluginCollection.AMPluginList.FirstOrDefault(e => e.Name == pluginName);
            if (plugin == null) throw new SusieIOException($"Cannot find plugin: {pluginName}");

            return plugin.LoadArchiveEntry(fileName, position);
        }

        public void SetPluginCahceEnabled(bool isCacheEnabled)
        {
            throw new System.NotImplementedException();
        }

        public void SetPluginFolder(string pluginFolder)
        {
            throw new System.NotImplementedException();
        }

        public void SetPlugin(List<SusiePluginSetting> settings)
        {
            if (settings == null) return;
            _pluginCollection.SetPluginSetting(settings);
        }

        public void SetPluginOrder(List<string> order)
        {
            _pluginCollection.SortPlugins(order);
        }


        public void SetServerSetting(SusiePluginServerSetting setting)
        {
            _pluginCollection = new SusiePluginCollection();
            _pluginCollection.Initialize(setting.PluginFolder);
            _pluginCollection.SetPluginSetting(setting.PluginSettings);
            _pluginCollection.SortPlugins(setting.PluginSettings.Select(e => e.Name).ToList());
        }

        public void ShowConfigulationDlg(string pluginName, int hwnd)
        {
            var plugin = _pluginCollection.GetPluginFromName(pluginName);
            plugin.OpenConfigulationDialog(new IntPtr(hwnd));
        }
    }
}
