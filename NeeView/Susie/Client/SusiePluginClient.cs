using System.Collections.Generic;

namespace NeeView.Susie.Client
{
    public class SusiePluginClient : IRemoteSusiePlugin
    {
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
            throw new System.NotImplementedException();
        }

        public SusiePicture GetPicture(string fileName, byte[] buff, bool isCheckExtension)
        {
            throw new System.NotImplementedException();
        }

        public List<SusiePluginInfo> GetPlugins(List<string> pluginNames)
        {
            throw new System.NotImplementedException();
        }

        public SusiePluginServerSetting GetServerSetting()
        {
            throw new System.NotImplementedException();
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
            throw new System.NotImplementedException();
        }

        public void ShowConfigulationDlg(string pluginName, int hWnd)
        {
            throw new System.NotImplementedException();
        }
    }
}
