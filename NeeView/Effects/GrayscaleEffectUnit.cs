// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeLaboratory.Property;
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
    public class GrayscaleEffectUnit : EffectUnit
    {
        private static GrayscaleEffect s_effect = new GrayscaleEffect();
        public override Effect Effect => s_effect;

        /// <summary>
        /// Property: Radius
        /// </summary>
        [DataMember]
        [PropertyRange(0, 1)]
        [DefaultValue(0.5)]
        public double DesaturationFactor
        {
            get { return s_effect.DesaturationFactor; }
            set { if (s_effect.DesaturationFactor != value) { s_effect.DesaturationFactor = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
