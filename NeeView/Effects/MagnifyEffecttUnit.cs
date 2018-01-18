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
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    // https://msdn.microsoft.com/ja-jp/library/microsoft.expression.media.effects(v=expression.40).aspx
    // v EmbossedEffect 
    // c MagnifyEffect 
    // RippleEffect 
    // SwirlEffect 

    //
    [DataContract]
    public class MagnifyEffectUnit : EffectUnit
    {
        private static MagnifyEffect s_effect = new MagnifyEffect();
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
        /// Property: Amount
        /// </summary>
        [DataMember]
        [PropertyRange(0, 1)]
        [DefaultValue(0.5)]
        public double Amount
        {
            get { return s_effect.Amount; }
            set { if (s_effect.Amount != value) { s_effect.Amount = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: InnerRadius 
        /// </summary>
        [DataMember]
        [PropertyRange(0, 1)]
        [DefaultValue(0.2)]
        public double InnerRadius
        {
            get { return s_effect.InnerRadius; }
            set { if (s_effect.InnerRadius != value) { s_effect.InnerRadius = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: OuterRadius 
        /// </summary>
        [DataMember]
        [PropertyRange(0, 1)]
        [DefaultValue(0.4)]
        public double OuterRadius
        {
            get { return s_effect.OuterRadius; }
            set { if (s_effect.OuterRadius != value) { s_effect.OuterRadius = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
