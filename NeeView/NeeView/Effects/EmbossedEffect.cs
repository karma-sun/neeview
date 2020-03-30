using Microsoft.Expression.Media.Effects;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    //
    [DataContract]
    public class EmbossedEffectUnit : EffectUnit
    {
        private static EmbossedEffect _effect = new EmbossedEffect();

        public override Effect GetEffect() => _effect;

        /// <summary>
        /// Property: Color
        /// </summary>
        [IgnoreDataMember]
        [PropertyMember]
        [DefaultValue(typeof(Color), "#FF808080")]
        public Color Color
        {
            get { return _effect.Color; }
            set { if (_effect.Color != value) { _effect.Color = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// for serializer
        /// </summary>
        [DataMember]
        [JsonIgnore]
        [PropertyMapIgnore]
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
            get { return _effect.Amount; }
            set { if (_effect.Amount != value) { _effect.Amount = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: Height
        /// </summary>
        [DataMember]
        [PropertyRange(0, 5)]
        [DefaultValue(1)]
        public double Height
        {
            get { return _effect.Height * 1000.0; }
            set { var a = value * 0.001; if (_effect.Height != a) { _effect.Height = a; RaiseEffectPropertyChanged(); } }
        }
    }
}
