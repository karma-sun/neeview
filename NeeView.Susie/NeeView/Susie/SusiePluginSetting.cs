using System;
using System.Runtime.Serialization;

namespace NeeView.Susie
{
    [DataContract]
    public class SusiePluginSetting
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool IsEnabled { get; set; }

        [DataMember]
        public bool IsCacheEnabled { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsPreExtract { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string UserExtensions { get; set; }

        public SusiePluginInfo ToSusiePluginInfo()
        {
            var info = new SusiePluginInfo();
            info.Name = Name;
            info.IsEnabled = IsEnabled;
            info.IsCacheEnabled = IsCacheEnabled;
            info.UserExtension = new FileExtensionCollection(UserExtensions);
            return info;
        }
    }
}
