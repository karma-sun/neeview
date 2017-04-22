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
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    //
    [DataContract]
    public class SharpenEffectUnit : EffectUnit
    {
        private static SharpenEffect s_effect = new SharpenEffect();
        public override Effect Effect => s_effect;

        [DataMember]
        [PropertyRange(0, 4)]
        [DefaultValue(2.0)]
        public double Amount
        {
            get { return s_effect.Amount; }
            set { if (s_effect.Amount != value) { s_effect.Amount = value; RaiseEffectPropertyChanged(); } }
        }

        [DataMember]
        [PropertyRange(0, 2.0)]
        [DefaultValue(0.5)]
        public double Height
        {
            get { return s_effect.Height * 1000; }
            set { var a = value * 0.001; if (s_effect.Height != a) { s_effect.Height = a; RaiseEffectPropertyChanged(); } }
        }
    }
}
