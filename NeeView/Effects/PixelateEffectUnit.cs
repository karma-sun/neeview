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
    // https://msdn.microsoft.com/ja-jp/library/microsoft.expression.media.effects(v=expression.40).aspx
    // v EmbossedEffect 
    // v MagnifyEffect 
    // v RippleEffect 
    // SwirlEffect 

    //
    [DataContract]
    public class PixelateEffectUnit : EffectUnit
    {
        private static PixelateEffect _effect = new PixelateEffect();
        public override Effect Effect => _effect;

        /// <summary>
        /// Property: Pixelation
        /// </summary>
        [DataMember]
        [PropertyRange(0, 1)]
        [DefaultValue(0.75)]
        public double Pixelation
        {
            get { return _effect.Pixelation; }
            set { if (_effect.Pixelation != value) { _effect.Pixelation = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
