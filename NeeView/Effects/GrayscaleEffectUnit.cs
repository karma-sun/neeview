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
        private static GrayscaleEffect _effect = new GrayscaleEffect();
        public override Effect Effect => _effect;

        /// <summary>
        /// Property: Radius
        /// </summary>
        [DataMember]
        [PropertyRange(0, 1)]
        [DefaultValue(0.5)]
        public double DesaturationFactor
        {
            get { return _effect.DesaturationFactor; }
            set { if (_effect.DesaturationFactor != value) { _effect.DesaturationFactor = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
