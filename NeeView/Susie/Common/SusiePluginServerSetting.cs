using NeeLaboratory.ComponentModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView.Susie
{
    [DataContract]
    public class SusiePluginServerSetting
    {
        [DataMember]
        public string PluginFolder { get; set; }

        [DataMember]
        public List<SusiePluginSetting> PluginSettings { get; set; }
    }
}
