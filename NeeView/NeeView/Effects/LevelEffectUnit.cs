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
    public class LevelEffectUnit : EffectUnit
    {
        private static LevelEffect _effect = new LevelEffect();

        public override Effect GetEffect() => _effect;


        /// <summary>
        /// Property: Black
        /// </summary>
        [DataMember]
        [PropertyRange("Black", 0, 1, Title = "Input")]
        [DefaultValue(0.0)]
        public double Black
        {
            get { return _effect.Black; }
            set { if (_effect.Black != value) { _effect.Black = value; RaiseEffectPropertyChanged(); } }
        }


        /// <summary>
        /// Property: White
        /// </summary>
        [DataMember]
        [PropertyRange("White", 0, 1)]
        [DefaultValue(1.0)]
        public double White
        {
            get { return _effect.White; }
            set { if (_effect.White != value) { _effect.White = value; RaiseEffectPropertyChanged(); } }
        }


        /// <summary>
        /// Property: Center
        /// </summary>
        [DataMember]
        [PropertyRange(0.1, 0.9)]
        [DefaultValue(0.5)]
        public double Center
        {
            get { return _effect.Center; }
            set { if (_effect.Center != value) { _effect.Center = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: Minimum
        /// </summary>
        [DataMember]
        [PropertyRange("Min", 0, 1, Title = "Output")]
        [DefaultValue(0.0)]
        public double Minimum
        {
            get { return _effect.Minimum; }
            set { if (_effect.Minimum != value) { _effect.Minimum = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: Center
        /// </summary>
        [DataMember]
        [PropertyRange("Max", 0, 1)]
        [DefaultValue(1.0)]
        public double Maximum
        {
            get { return _effect.Maximum; }
            set { if (_effect.Maximum != value) { _effect.Maximum = value; RaiseEffectPropertyChanged(); } }
        }

    }
}
