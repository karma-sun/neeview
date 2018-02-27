using Microsoft.Expression.Media.Effects;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    //
    [DataContract]
    public class BloomEffectUnit : EffectUnit
    {
        private static BloomEffect s_effect = new BloomEffect();
        public override Effect Effect => s_effect;

        /// <summary>
        /// Property: BaseIntensity
        /// </summary>
        [DataMember]
        [PropertyRange(0, 4)]
        [DefaultValue(1.0)]
        public double BaseIntensity
        {
            get { return s_effect.BaseIntensity; }
            set { if (s_effect.BaseIntensity != value) { s_effect.BaseIntensity = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: BaseSaturation
        /// </summary>
        [DataMember]
        [PropertyRange(0, 4)]
        [DefaultValue(1.0)]
        public double BaseSaturation
        {
            get { return s_effect.BaseSaturation; }
            set { if (s_effect.BaseSaturation != value) { s_effect.BaseSaturation = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: BloomIntensity
        /// </summary>
        [DataMember]
        [PropertyRange(0, 4)]
        [DefaultValue(1.25)]
        public double BloomIntensity
        {
            get { return s_effect.BloomIntensity; }
            set { if (s_effect.BloomIntensity != value) { s_effect.BloomIntensity = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: BloomIntensity
        /// </summary>
        [DataMember]
        [PropertyRange(0, 4)]
        [DefaultValue(1.0)]
        public double BloomSaturation
        {
            get { return s_effect.BloomSaturation; }
            set { if (s_effect.BloomSaturation != value) { s_effect.BloomSaturation = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: Threshold
        /// </summary>
        [DataMember]
        [PropertyRange(0, 1.0)]
        [DefaultValue(0.25)]
        public double Threshold
        {
            get { return s_effect.Threshold; }
            set
            {
                var a = value < 0.99 ? value : 0.99;
                if (s_effect.Threshold != a) { s_effect.Threshold = a; RaiseEffectPropertyChanged(); }
            }
        }
    }
}
