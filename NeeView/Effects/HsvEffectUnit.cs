// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
        private static HsvEffect s_effect = new HsvEffect();
        public override Effect Effect => s_effect;


        /// <summary>
        /// Property: Hue
        /// </summary>
        [DataMember]
        [PropertyRange(0.0, 360.0, Name ="色相")]
        [DefaultValue(0.0)]
        public double Hue
        {
            get { return s_effect.Hue; }
            set { if (s_effect.Hue != value) { s_effect.Hue = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: Saturation
        /// </summary>
        [DataMember]
        [PropertyRange(-1.0, 1.0, Name ="彩度")]
        [DefaultValue(0.0)]
        public double Saturation
        {
            get { return s_effect.Saturation; }
            set { if (s_effect.Saturation != value) { s_effect.Saturation = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: Value
        /// </summary>
        [DataMember]
        [PropertyRange(-1.0, 1.0, Name ="明度")]
        [DefaultValue(0.0)]
        public double Value
        {
            get { return s_effect.Value; }
            set { if (s_effect.Value != value) { s_effect.Value = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
