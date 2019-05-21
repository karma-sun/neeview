using NeeView.Susie.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NeeView.Susie.Client
{
    public class SusiePluginClient : IRemoteSusiePlugin, IDisposable
    {
        private SusiePluginServer _server;

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
                    _server?.Dispose();
                    _server = null;
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
            _server = new SusiePluginServer();
            _server.Initialize(pluginFolder, settings);
        }

        public void ExtracArchiveEntrytToFolder(string pluginName, string fileName, int position, string extractFolder)
        {
            _server.ExtracArchiveEntrytToFolder(pluginName, fileName, position, extractFolder);
        }

        public List<SusieArchiveEntry> GetArchiveEntries(string pluginName, string fileName)
        {
            return _server.GetArchiveEntries(pluginName, fileName);
        }

        public SusiePluginInfo GetArchivePlugin(string fileName, byte[] buff, bool isCheckExtension)
        {
            return _server.GetArchivePlugin(fileName, buff, isCheckExtension);
        }

        public SusiePluginInfo GetImagePlugin(string fileName, byte[] buff, bool isCheckExtension)
        {
            return _server.GetImagePlugin(fileName, buff, isCheckExtension);
        }

        public SusieImage GetImage(string pluginName, string fileName, byte[] buff, bool isCheckExtension)
        {
            return _server.GetImage(pluginName, fileName, buff, isCheckExtension);
        }

        public List<SusiePluginInfo> GetPlugin(List<string> pluginNames)
        {
            return _server.GetPlugin(pluginNames);
        }


        public byte[] ExtractArchiveEntry(string pluginName, string fileName, int position)
        {
            return ExtractArchiveEntry(pluginName, fileName, position);
        }


        public void SetPlugin(List<SusiePluginSetting> settings)
        {
            _server.SetPlugin(settings);
        }

        public void SetPluginOrder(List<string> order)
        {
            _server.SetPluginOrder(order);
        }


        public void ShowConfigulationDlg(string pluginName, int hwnd)
        {
            _server.ShowConfigulationDlg(pluginName, hwnd);
        }
    }
}
