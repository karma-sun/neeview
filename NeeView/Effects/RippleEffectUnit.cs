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
        private static RippleEffect _effect = new RippleEffect();
        public override Effect Effect => _effect;

        /// <summary>
        /// Property: Center
        /// </summary>
        [DataMember]
        [PropertyMember]
        [DefaultValue(typeof(Point), "0.5,0.5")]
        public Point Center
        {
            get { return _effect.Center; }
            set { if (_effect.Center != value) { _effect.Center = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: Frequency
        /// </summary>
        [DataMember]
        [PropertyRange(0, 100)]
        [DefaultValue(40)]
        public double Frequency
        {
            get { return _effect.Frequency; }
            set { if (_effect.Frequency != value) { _effect.Frequency = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: Magnitude
        /// </summary>
        [DataMember]
        [PropertyRange(0, 1)]
        [DefaultValue(0.1)]
        public double Magnitude
        {
            get { return _effect.Magnitude; }
            set { if (_effect.Magnitude != value) { _effect.Magnitude = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: Phase
        /// </summary>
        [DataMember]
        [PropertyRange(0, 100)]
        [DefaultValue(10)]
        public double Phase
        {
            get { return _effect.Phase; }
            set { if (_effect.Phase != value) { _effect.Phase = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
