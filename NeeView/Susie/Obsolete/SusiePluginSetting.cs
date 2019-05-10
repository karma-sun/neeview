using NeeView.Susie;
using System;
using System.Runtime.Serialization;


namespace NeeView
{
    /// <summary>
    /// プラグイン単位の設定 (Obsolete)
    /// </summary>
    [Obsolete, DataContract]
    public class SusiePluginSetting
    {
        public SusiePluginSetting(bool isEnable, bool isPreExtract)
        {
            this.IsEnabled = isEnable;
            this.IsPreExtract = isPreExtract;
        }

        [DataMember]
        public bool IsEnabled { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsPreExtract { get; set; }


        public SusiePlugin.Memento ToPluginMemento()
        {
            return new SusiePlugin.Memento()
            {
                IsEnabled = IsEnabled,
                IsPreExtract = IsPreExtract,
            };
        }
    }
}
