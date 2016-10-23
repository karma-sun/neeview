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
    public class ColorSelectEffectUnit : EffectUnit
    {
        private static ColorSelectEffect s_effect = new ColorSelectEffect();
        public override Effect Effect => s_effect;


        /// <summary>
        /// Property: Hue
        /// </summary>
        [DataMember]
        [PropertyRange(0.0, 360.0, Name ="色相")]
        [DefaultValue(15.0)]
        public double Hue
        {
            get { return s_effect.Hue; }
            set { if (s_effect.Hue != value) { s_effect.Hue = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: Range
        /// </summary>
        [DataMember]
        [PropertyRange(0.0, 1.0, Name ="範囲")]
        [DefaultValue(0.1)]
        public double Range
        {
            get { return s_effect.Range; }
            set { if (s_effect.Range != value) { s_effect.Range = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: Curve
        /// </summary>
        [DataMember]
        [PropertyRange(0.0, 0.2, Name ="境界", Tips ="色の境界のぼかし")]
        [DefaultValue(0.1)]
        public double Curve
        {
            get { return s_effect.Curve; }
            set { if (s_effect.Curve != value) { s_effect.Curve = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
