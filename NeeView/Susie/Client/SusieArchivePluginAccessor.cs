using NeeView.Susie;
using System.Collections.Generic;
using NeeView.Susie.Client;

namespace NeeView
{
    /// <summary>
    /// 書庫プラグインアクセサ
    /// </summary>
    public class SusieArchivePluginAccessor
    {
        private readonly SusiePluginClient _client;

        public SusieArchivePluginAccessor(SusiePluginClient client, SusiePluginInfo plugin)
        {
            _client = client;
            Plugin = plugin;
        }

        public SusiePluginInfo Plugin { get; }


        public List<SusieArchiveEntry> GetArchiveEntries(string fileName)
        {
            return _client.GetArchiveEntries(Plugin?.Name, fileName);
        }

        public byte[] ExtractArchiveEntry(string fileName, int position)
        {
            return _client.ExtractArchiveEntry(Plugin?.Name, fileName, position);
        }


        public void ExtracArchiveEntrytToFolder(string fileName, int position, string extractFolder)
        {
            _client.ExtractArchiveEntryToFolder(Plugin?.Name, fileName, position, extractFolder);
        }

    }
}
