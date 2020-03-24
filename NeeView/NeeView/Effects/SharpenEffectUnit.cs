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
    public class SharpenEffectUnit : EffectUnit
    {
        private static SharpenEffect _effect = new SharpenEffect();
        public override Effect GetEffect() => _effect;

        [DataMember]
        [PropertyRange(0, 4)]
        [DefaultValue(2.0)]
        public double Amount
        {
            get { return _effect.Amount; }
            set { if (_effect.Amount != value) { _effect.Amount = value; RaiseEffectPropertyChanged(); } }
        }

        [DataMember]
        [PropertyRange(0, 2.0)]
        [DefaultValue(0.5)]
        public double Height
        {
            get { return _effect.Height * 1000; }
            set { var a = value * 0.001; if (_effect.Height != a) { _effect.Height = a; RaiseEffectPropertyChanged(); } }
        }
    }
}
