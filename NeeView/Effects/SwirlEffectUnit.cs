// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using Microsoft.Expression.Media.Effects;
using NeeLaboratory.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    // 渦巻き
    [DataContract]
    public class SwirlEffectUnit : EffectUnit
    {
        private static SwirlEffect s_effect = new SwirlEffect();
        public override Effect Effect => s_effect;

        /// <summary>
        /// Property: Center
        /// </summary>
        [DataMember]
        [PropertyMember]
        [DefaultValue(typeof(Point), "0.5,0.5")]
        public Point Center
        {
            get { return s_effect.Center; }
            set { if (s_effect.Center != value) { s_effect.Center = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: TwistAmount
        /// </summary>
        [DataMember]
        [PropertyRange(-50, 50)]
        [DefaultValue(10)]
        public double TwistAmount
        {
            get { return s_effect.TwistAmount; }
            set { if (s_effect.TwistAmount != value) { s_effect.TwistAmount = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
