using NeeView.Susie.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NeeView.Susie.Server
{
    public class SusiePluginServer : IRemoteSusiePlugin, IDisposable
    {
        private SusiePluginCollection _pluginCollection;

        public SusiePluginServer()
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


        public void Initialize(string pluginFolder, List<SusiePluginSetting> settings)
        {
            _pluginCollection = new SusiePluginCollection();
            _pluginCollection.Initialize(pluginFolder);
            _pluginCollection.SetPluginSetting(settings);
            _pluginCollection.SortPlugins(settings.Select(e => e.Name).ToList());
        }


        public void ExtractArchiveEntryToFolder(string pluginName, string fileName, int position, string extractFolder)
        {
            var plugin = _pluginCollection.AMPluginList.FirstOrDefault(e => e.Name == pluginName);
            if (plugin == null) throw new SusieException($"Cannot find plugin", pluginName);

            plugin.ExtracArchiveEntrytToFolder(fileName, position, extractFolder);
        }

        public List<SusieArchiveEntry> GetArchiveEntries(string pluginName, string fileName)
        {
            var plugin = _pluginCollection.AMPluginList.FirstOrDefault(e => e.Name == pluginName);
            if (plugin == null) throw new SusieException($"Cannot find plugin", pluginName);

            var collection = plugin.GetArchiveEntryCollection(fileName);
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
            var plugins = _pluginCollection.INPluginList;
            if (pluginName != null)
            {
                var plugin = _pluginCollection.INPluginList.FirstOrDefault(e => e.Name == pluginName);
                if (plugin == null) throw new SusieException($"Cannot find plugin", pluginName);
                plugins = new List<Server.SusiePlugin>() { plugin };
            }

            return _pluginCollection.GetImage(plugins, fileName, buff, isCheckExtension);
        }

        public List<SusiePluginInfo> GetPlugin(List<string> pluginNames)
        {
            var plugins = pluginNames != null
                ? pluginNames.Select(e => _pluginCollection.GetPluginFromName(e))
                : _pluginCollection.PluginCollection;

            return plugins.Select(e => e.ToSusiePluginInfo()).ToList();
        }


        public byte[] ExtractArchiveEntry(string pluginName, string fileName, int position)
        {
            var plugin = _pluginCollection.AMPluginList.FirstOrDefault(e => e.Name == pluginName);
            if (plugin == null) throw new SusieException("Cannot find plugin", pluginName);

            return plugin.LoadArchiveEntry(fileName, position);
        }

        public void SetPlugin(List<SusiePluginSetting> settings)
        {
            if (settings == null || !settings.Any()) return;
            _pluginCollection.SetPluginSetting(settings);
        }

        public void SetPluginOrder(List<string> order)
        {
            if (order == null || !order.Any()) return;
            _pluginCollection.SortPlugins(order);
        }


        public void ShowConfigulationDlg(string pluginName, int hwnd)
        {
            var plugin = _pluginCollection.GetPluginFromName(pluginName);
            plugin.OpenConfigulationDialog(new IntPtr(hwnd));
        }
    }
}
