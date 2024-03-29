﻿using Microsoft.Expression.Media.Effects;
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
    [DataContract]
    public class MonochromeEffectUnit : EffectUnit
    {
        private static MonochromeEffect _effect = new MonochromeEffect();
        public override Effect GetEffect() => _effect;

        /// <summary>
        /// Property: Color
        /// </summary>
        [IgnoreDataMember]
        [PropertyMember]
        [DefaultValue(typeof(Color), "#FFFFFFFF")]
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
    }
}
