using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView.Susie.Client
{
    public class SusiePluginClient : IRemoteSusiePlugin, IDisposable
    {
        private SusiePluginCollection _pluginCollection;

        public SusiePluginClient()
        {

        }

        [Obsolete]
        public SusiePluginCollection PluginCollection => _pluginCollection;


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


        public void ExtracArchiveEntrytToFolder(string pluginName, string fileName, uint position, string extractFolder)
        {
            throw new System.NotImplementedException();
        }

        public List<SusieArchiveEntry> GetArchiveEntry(string pluginName, string fileName)
        {
            throw new System.NotImplementedException();
        }

        public SusiePluginInfo GetArchivePlugin(string fileName, byte[] buff, bool isCheckExtension)
        {
            throw new System.NotImplementedException();
        }

        public SusiePluginInfo GetImagePlugin(string fileName, byte[] buff, bool isCheckExtension)
        {
            // TODO: buff==nullのときの処理。ヘッダ2KBを読み込む
            var plugin = _pluginCollection.GetImagePlugin(fileName, buff, isCheckExtension);
            return plugin.ToSusiePluginInfo();
        }

        public SusiePicture GetPicture(string fileName, byte[] buff, bool isCheckExtension)
        {
            if (buff != null)
            {
                var result = new SusiePicture();
                result.BitmapData = _pluginCollection.GetPicture(fileName, buff, isCheckExtension, out var susiePlugin);
                result.PluginName = susiePlugin.ToString();
                return result;
            }
            else
            {
                var result = new SusiePicture();
                result.BitmapData = _pluginCollection.GetPictureFromFile(fileName, isCheckExtension, out var susiePlugin);
                result.PluginName = susiePlugin.ToString();
                return result;
            }
        }

        public List<SusiePluginInfo> GetPlugins(List<string> pluginNames)
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
            setting.IsPluginCacheEnabled = _pluginCollection.IsPluginCacheEnabled;
            setting.PluginSettings = _pluginCollection.StorePlugins()
                .Select(e => e.Value.ToSusiePluginSetting(LoosePath.GetFileName(e.Key)))
                .ToList();

            return setting;
        }

        public byte[] LoadArchiveEntry(string pluginName, string fileName, uint position)
        {
            throw new System.NotImplementedException();
        }

        public void SetPluginCahceEnabled(bool isCacheEnabled)
        {
            throw new System.NotImplementedException();
        }

        public void SetPluginFolder(string pluginFolder)
        {
            throw new System.NotImplementedException();
        }

        public void SetPlugins(List<SusiePluginSetting> settings)
        {
            throw new System.NotImplementedException();
        }

        public void SetServerSetting(SusiePluginServerSetting setting)
        {
            _pluginCollection = new SusiePluginCollection(false, setting.IsPluginCacheEnabled);
            _pluginCollection.Initialize(setting.PluginFolder);
            _pluginCollection.RestorePlugins(setting.PluginSettings.ToDictionary(e => LoosePath.Combine(setting.PluginFolder, e.Name), e => ToPluginMemento(e)));

#pragma warning disable CS0612
            SusiePlugin.Memento ToPluginMemento(SusiePluginSetting element)
            {
                var memento = new SusiePlugin.Memento();
                memento.IsEnabled = element.IsEnabled;
                memento.IsPreExtract = element.IsPreExtract;
                memento.UserExtensions = element.UserExtensions;
                return memento;
            }
#pragma warning restore CS0612
        }

        public void ShowConfigulationDlg(string pluginName, int hwnd)
        {
            var plugin = _pluginCollection.GetPluginFromName(pluginName);
            plugin.OpenConfigulationDialog(new IntPtr(hwnd));
        }
    }
}
