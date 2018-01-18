// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
    [DataContract]
    public class MonochromeEffectUnit : EffectUnit
    {
        private static MonochromeEffect s_effect = new MonochromeEffect();
        public override Effect Effect => s_effect;

        /// <summary>
        /// Property: Color
        /// </summary>
        [IgnoreDataMember]
        [PropertyMember]
        [DefaultValue(typeof(Color), "#FFFFFFFF")]
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
    }
}
