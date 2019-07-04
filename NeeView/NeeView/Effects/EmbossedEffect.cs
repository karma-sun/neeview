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
    public class EmbossedEffectUnit : EffectUnit
    {
        private static EmbossedEffect s_effect = new EmbossedEffect();
        public override Effect Effect => s_effect;

        /// <summary>
        /// Property: Color
        /// </summary>
        [IgnoreDataMember]
        [PropertyMember]
        [DefaultValue(typeof(Color), "#FF808080")]
        public Color Color
        {
            get { return s_effect.Color; }
            set { if (s_effect.Color != value) { s_effect.Color = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// for serializer
        /// </summary>
        [DataMember]
        public string ColorCode
        {
            get { return Color.ToString(); }
            set { Color = (Color)ColorConverter.ConvertFromString(value); }
        }

        /// <summary>
        /// Property: Amount
        /// </summary>
        [DataMember]
        [PropertyRange(-5, 5)]
        [DefaultValue(3)]
        public double Amount
        {
            get { return s_effect.Amount; }
            set { if (s_effect.Amount != value) { s_effect.Amount = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: Height
        /// </summary>
        [DataMember]
        [PropertyRange(0, 5)]
        [DefaultValue(1)]
        public double Height
        {
            get { return s_effect.Height * 1000.0; }
            set { var a = value * 0.001; if (s_effect.Height != a) { s_effect.Height = a; RaiseEffectPropertyChanged(); } }
        }
    }
}
