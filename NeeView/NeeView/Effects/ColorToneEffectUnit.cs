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
    public class ColorToneEffectUnit : EffectUnit
    {
        private static ColorToneEffect _effect = new ColorToneEffect();
        public override Effect GetEffect() => _effect;

        /// <summary>
        /// Property: DarkColor
        /// </summary>
        [IgnoreDataMember]
        [PropertyMember]
        [DefaultValue(typeof(Color), "#FF338000")]
        public Color DarkColor
        {
            get { return _effect.DarkColor; }
            set { if (_effect.DarkColor != value) { _effect.DarkColor = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// for serializer
        /// </summary>
        [DataMember]
        [JsonIgnore]
        [PropertyMapIgnore]
        public string DarkColorCode
        {
            get { return DarkColor.ToString(); }
            set { DarkColor = (Color)ColorConverter.ConvertFromString(value); }
        }

        /// <summary>
        /// Property: LightColor
        /// </summary>
        [IgnoreDataMember]
        [PropertyMember]
        [DefaultValue(typeof(Color), "#FFFFE580")]
        public Color LightColor
        {
            get { return _effect.LightColor; }
            set { if (_effect.LightColor != value) { _effect.LightColor = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// for serializer
        /// </summary>
        [DataMember]
        [JsonIgnore]
        [PropertyMapIgnore]
        public string LightColorCode
        {
            get { return LightColor.ToString(); }
            set { LightColor = (Color)ColorConverter.ConvertFromString(value); }
        }

        /// <summary>
        /// Property: ToneAmount
        /// </summary>
        [DataMember]
        [PropertyRange(0, 1)]
        [DefaultValue(0.5)]
        public double ToneAmount
        {
            get { return _effect.ToneAmount; }
            set { if (_effect.ToneAmount != value) { _effect.ToneAmount = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: Desaturation
        /// </summary>
        [DataMember]
        [PropertyRange(0, 1)]
        [DefaultValue(0.5)]
        public double Desaturation
        {
            get { return _effect.Desaturation; }
            set { if (_effect.Desaturation != value) { _effect.Desaturation = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
