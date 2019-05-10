using NeeView;
using NeeView.Susie;

namespace NeeView.Susie
{
    public class SusiePluginInfo
    {
        public string FileName { get; set; }
        public string Name { get; set; }
        public string ApiVersion { get; set; }
        public string PluginVersion { get; set; }
        public SusiePluginType PluginType { get; set; }
        public string DetailText { get; set; }
        public bool HasConfigurationDlg { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsPreExtract { get; set; }
        public FileTypeCollection DefaultExtension { get; set; }
        public FileTypeCollection UserExtension { get; set; }

        public FileTypeCollection Extensions => UserExtension ?? DefaultExtension;
    }
}
