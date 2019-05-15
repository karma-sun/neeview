using NeeLaboratory.ComponentModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView.Susie
{
    [DataContract]
    public class SusiePluginServerSetting
    {
        [DataMember, DefaultValue(true)]
        public bool IsPluginCacheEnabled { get; set; } = true;

        [DataMember]
        public string PluginFolder { get; set; }

        [DataMember]
        public List<SusiePluginSetting> PluginSettings { get; set; }


        [OnDeserializing]
        private void Deserializing(StreamingContext c)
        {
            this.InitializePropertyDefaultValues();
        }
    }
}
