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
    public class LevelEffectUnit : EffectUnit
    {
        private static LevelEffect s_effect = new LevelEffect();
        public override Effect Effect => s_effect;


        /// <summary>
        /// Property: Black
        /// </summary>
        [DataMember]
        [PropertyRange(0, 1, Name = "Shadow")]
        [DefaultValue(0.0)]
        public double Black
        {
            get { return s_effect.Black; }
            set { if (s_effect.Black != value) { s_effect.Black = value; RaiseEffectPropertyChanged(); } }
        }


        /// <summary>
        /// Property: White
        /// </summary>
        [DataMember]
        [PropertyRange(0, 1, Name = "Highlight")]
        [DefaultValue(1.0)]
        public double White
        {
            get { return s_effect.White; }
            set { if (s_effect.White != value) { s_effect.White = value; RaiseEffectPropertyChanged(); } }
        }


        /// <summary>
        /// Property: Center
        /// </summary>
        [DataMember]
        [PropertyRange(0, 1)]
        [DefaultValue(0.5)]
        public double Center
        {
            get { return s_effect.Center; }
            set { if (s_effect.Center != value) { s_effect.Center = value; RaiseEffectPropertyChanged(); } }
        }


        /// <summary>
        /// Property: Hue
        /// </summary>
        [DataMember]
        [PropertyRange(0, 1)]
        [DefaultValue(0.0)]
        public double Hue
        {
            get { return s_effect.Hue; }
            set { if (s_effect.Hue != value) { s_effect.Hue = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
