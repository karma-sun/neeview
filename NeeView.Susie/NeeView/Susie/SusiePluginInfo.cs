using NeeLaboratory.Collections.Specialized;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView.Susie
{
    [DataContract]
    public class SusiePluginInfo
    {
        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string ApiVersion { get; set; }

        [DataMember]
        public string PluginVersion { get; set; }

        [DataMember]
        public SusiePluginType PluginType { get; set; }

        [DataMember]
        public string DetailText { get; set; }

        [DataMember]
        public bool HasConfigurationDlg { get; set; }

        [DataMember]
        public bool IsEnabled { get; set; }

        [DataMember]
        public bool IsCacheEnabled { get; set; }

        [DataMember]
        public bool IsPreExtract { get; set; }

        [DataMember]
        public FileExtensionCollection DefaultExtension { get; set; }

        [DataMember]
        public FileExtensionCollection UserExtension { get; set; }

        public FileExtensionCollection Extensions => UserExtension ?? DefaultExtension;

        public SusiePluginSetting ToSusiePluginSetting()
        {
            var setting = new SusiePluginSetting();
            setting.Name = Name;
            setting.IsEnabled = IsEnabled;
            setting.IsCacheEnabled = IsCacheEnabled;
            setting.IsPreExtract = IsPreExtract;
            setting.UserExtensions = UserExtension?.ToOneLine();
            return setting;
        }
    }
}
