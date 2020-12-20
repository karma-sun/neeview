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
        private static HsvEffect _effect = new HsvEffect();
        public override Effect GetEffect() => _effect;


        /// <summary>
        /// Property: Hue
        /// </summary>
        [DataMember]
        [PropertyRange(0.0, 360.0)]
        [DefaultValue(0.0)]
        public double Hue
        {
            get { return _effect.Hue; }
            set { if (_effect.Hue != value) { _effect.Hue = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: Saturation
        /// </summary>
        [DataMember]
        [PropertyRange(-1.0, 1.0)]
        [DefaultValue(0.0)]
        public double Saturation
        {
            get { return _effect.Saturation; }
            set { if (_effect.Saturation != value) { _effect.Saturation = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: Value
        /// </summary>
        [DataMember]
        [PropertyRange(-1.0, 1.0)]
        [DefaultValue(0.0)]
        public double Value
        {
            get { return _effect.Value; }
            set { if (_effect.Value != value) { _effect.Value = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
