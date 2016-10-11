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
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    //
    [DataContract]
    public class BloomEffectUnit : EffectUnit
    {
        private static BloomEffect _effect = new BloomEffect();
        public override Effect Effect => _effect;

        /// <summary>
        /// Property: BaseIntensity
        /// </summary>
        [DataMember]
        [PropertyRange(0, 4)]
        [DefaultValue(1.0)]
        public double BaseIntensity
        {
            get { return _effect.BaseIntensity; }
            set { if (_effect.BaseIntensity != value) { _effect.BaseIntensity = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: BaseSaturation
        /// </summary>
        [DataMember]
        [PropertyRange(0, 4)]
        [DefaultValue(1.0)]
        public double BaseSaturation
        {
            get { return _effect.BaseSaturation; }
            set { if (_effect.BaseSaturation != value) { _effect.BaseSaturation = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: BloomIntensity
        /// </summary>
        [DataMember]
        [PropertyRange(0, 4)]
        [DefaultValue(1.25)]
        public double BloomIntensity
        {
            get { return _effect.BloomIntensity; }
            set { if (_effect.BloomIntensity != value) { _effect.BloomIntensity = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: BloomIntensity
        /// </summary>
        [DataMember]
        [PropertyRange(0, 4)]
        [DefaultValue(1.0)]
        public double BloomSaturation
        {
            get { return _effect.BloomSaturation; }
            set { if (_effect.BloomSaturation != value) { _effect.BloomSaturation = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: Threshold
        /// </summary>
        [DataMember]
        [PropertyRange(0, 1.0)]
        [DefaultValue(0.25)]
        public double Threshold
        {
            get { return _effect.Threshold; }
            set
            {
                var a = value < 0.99 ? value : 0.99;
                if (_effect.Threshold != a) { _effect.Threshold = a; RaiseEffectPropertyChanged(); }
            }
        }
    }

}
