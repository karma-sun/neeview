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
    public class ColorSelectEffectUnit : EffectUnit
    {
        private static ColorSelectEffect _effect = new ColorSelectEffect();
        public override Effect GetEffect() => _effect;


        /// <summary>
        /// Property: Hue
        /// </summary>
        [DataMember]
        [PropertyRange("@ParamEffectHue", 0.0, 360.0)]
        [DefaultValue(15.0)]
        public double Hue
        {
            get { return _effect.Hue; }
            set { if (_effect.Hue != value) { _effect.Hue = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: Range
        /// </summary>
        [DataMember]
        [PropertyRange("@ParamEffectRange", 0.0, 1.0)]
        [DefaultValue(0.1)]
        public double Range
        {
            get { return _effect.Range; }
            set { if (_effect.Range != value) { _effect.Range = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: Curve
        /// </summary>
        [DataMember]
        [PropertyRange("@ParamEffectCurve", 0.0, 0.2)]
        [DefaultValue(0.1)]
        public double Curve
        {
            get { return _effect.Curve; }
            set { if (_effect.Curve != value) { _effect.Curve = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
