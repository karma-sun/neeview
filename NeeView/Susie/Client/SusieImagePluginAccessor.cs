using NeeView.Susie;
using NeeView.Susie.Client;

namespace NeeView
{
    /// <summary>
    /// 画像プラグインアクセサ
    /// </summary>
    public class SusieImagePluginAccessor
    {
        private readonly SusiePluginClient _client;

        public SusieImagePluginAccessor(SusiePluginClient client, SusiePluginInfo plugin)
        {
            _client = client;
            Plugin = plugin;
        }

        public SusiePluginInfo Plugin { get; }


        public SusieImage GetPicture(string fileName, byte[] buff, bool isCheckExtension)
        {
            return _client.GetImage(Plugin?.Name, fileName, buff, isCheckExtension);
        }
    }
}
