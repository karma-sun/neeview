﻿using NeeView.Windows.Property;
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
        private static BlurEffect _effect = new BlurEffect();
        public override Effect GetEffect() => _effect;

        /// <summary>
        /// Property: Radius
        /// </summary>
        [DataMember]
        [PropertyRange(0, 100)]
        [DefaultValue(5.0)]
        public double Radius
        {
            get { return _effect.Radius; }
            set { if (_effect.Radius != value) { _effect.Radius = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
