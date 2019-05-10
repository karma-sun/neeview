using System.Collections.Generic;

namespace NeeView.Susie
{
    public class SusiePluginServerSetting
    {
        public string PluginFolder { get; set; }
        public List<SusiePluginSetting> PluginSettings { get; set; }
        public bool IsPluginCacheEnabled { get; set; }
    }
}
