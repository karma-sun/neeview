using NeeLaboratory.ComponentModel;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView.Susie
{
    /// <summary>
    /// プラグイン設定(Obsolete)
    /// </summary>
    [Obsolete]
    public class SusiePlugin
    {
        [DataContract]
        public class Memento
        {
            [DataMember, DefaultValue(true)]
            public bool IsEnabled { get; set; } = true;

            [DataMember(EmitDefaultValue = false)]
            public bool IsPreExtract { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string UserExtensions { get; set; }


            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            public SusiePluginSetting ToSusiePluginSetting(string name, bool isCacheEnabled)
            {
                var setting = new SusiePluginSetting();
                setting.Name = name;
                setting.IsEnabled = this.IsEnabled;
                setting.IsCacheEnabled = isCacheEnabled;
                setting.IsPreExtract = this.IsPreExtract;
                setting.UserExtensions = this.UserExtensions;
                return setting;
            }
        }
    }
}
