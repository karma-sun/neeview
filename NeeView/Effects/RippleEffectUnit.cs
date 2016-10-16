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
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    //
    [DataContract]
    public class RippleEffectUnit : EffectUnit
    {
        private static RippleEffect s_effect = new RippleEffect();
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
        /// Property: Frequency
        /// </summary>
        [DataMember]
        [PropertyRange(0, 100)]
        [DefaultValue(40)]
        public double Frequency
        {
            get { return s_effect.Frequency; }
            set { if (s_effect.Frequency != value) { s_effect.Frequency = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: Magnitude
        /// </summary>
        [DataMember]
        [PropertyRange(0, 1)]
        [DefaultValue(0.1)]
        public double Magnitude
        {
            get { return s_effect.Magnitude; }
            set { if (s_effect.Magnitude != value) { s_effect.Magnitude = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: Phase
        /// </summary>
        [DataMember]
        [PropertyRange(0, 100)]
        [DefaultValue(10)]
        public double Phase
        {
            get { return s_effect.Phase; }
            set { if (s_effect.Phase != value) { s_effect.Phase = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
