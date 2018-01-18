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
    public class BlurEffectUnit : EffectUnit
    {
        private static BlurEffect s_effect = new BlurEffect();
        public override Effect Effect => s_effect;

        /// <summary>
        /// Property: Radius
        /// </summary>
        [DataMember]
        [PropertyRange(0, 100)]
        [DefaultValue(5.0)]
        public double Radius
        {
            get { return s_effect.Radius; }
            set { if (s_effect.Radius != value) { s_effect.Radius = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
