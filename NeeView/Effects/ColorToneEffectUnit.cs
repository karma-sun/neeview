// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using Microsoft.Expression.Media.Effects;
using NeeLaboratory.Property;
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
    public class ColorToneEffectUnit : EffectUnit
    {
        private static ColorToneEffect s_effect = new ColorToneEffect();
        public override Effect Effect => s_effect;

        /// <summary>
        /// Property: DarkColor
        /// </summary>
        [IgnoreDataMember]
        [PropertyMember()]
        [DefaultValue(typeof(Color), "#FF338000")]
        public Color DarkColor
        {
            get { return s_effect.DarkColor; }
            set { if (s_effect.DarkColor != value) { s_effect.DarkColor = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// for serializer
        /// </summary>
        [DataMember]
        public string DarkColorCode
        {
            get { return DarkColor.ToString(); }
            set { DarkColor = (Color)ColorConverter.ConvertFromString(value); }
        }

        /// <summary>
        /// Property: LightColor
        /// </summary>
        [IgnoreDataMember]
        [PropertyMember()]
        [DefaultValue(typeof(Color), "#FFFFE580")]
        public Color LightColor
        {
            get { return s_effect.LightColor; }
            set { if (s_effect.LightColor != value) { s_effect.LightColor = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// for serializer
        /// </summary>
        [DataMember]
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
            get { return s_effect.ToneAmount; }
            set { if (s_effect.ToneAmount != value) { s_effect.ToneAmount = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: Desaturation
        /// </summary>
        [DataMember]
        [PropertyRange(0, 1)]
        [DefaultValue(0.5)]
        public double Desaturation
        {
            get { return s_effect.Desaturation; }
            set { if (s_effect.Desaturation != value) { s_effect.Desaturation = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
