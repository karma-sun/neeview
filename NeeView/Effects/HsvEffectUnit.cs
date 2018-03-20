using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    //
    [DataContract]
    public class HsvEffectUnit : EffectUnit
    {
        private static HsvEffect s_effect = new HsvEffect();
        public override Effect Effect => s_effect;


        /// <summary>
        /// Property: Hue
        /// </summary>
        [DataMember]
        [PropertyRange("@ParamEffectHue", 0.0, 360.0)]
        [DefaultValue(0.0)]
        public double Hue
        {
            get { return s_effect.Hue; }
            set { if (s_effect.Hue != value) { s_effect.Hue = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: Saturation
        /// </summary>
        [DataMember]
        [PropertyRange("@ParamEffectSaturation", - 1.0, 1.0)]
        [DefaultValue(0.0)]
        public double Saturation
        {
            get { return s_effect.Saturation; }
            set { if (s_effect.Saturation != value) { s_effect.Saturation = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: Value
        /// </summary>
        [DataMember]
        [PropertyRange("@ParamEffectBrightness", - 1.0, 1.0)]
        [DefaultValue(0.0)]
        public double Value
        {
            get { return s_effect.Value; }
            set { if (s_effect.Value != value) { s_effect.Value = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
