using Microsoft.Expression.Media.Effects;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    // https://msdn.microsoft.com/ja-jp/library/microsoft.expression.media.effects(v=expression.40).aspx
    // v EmbossedEffect 
    // c MagnifyEffect 
    // RippleEffect 
    // SwirlEffect 

    //
    [DataContract]
    public class MagnifyEffectUnit : EffectUnit
    {
        private static MagnifyEffect _effect = new MagnifyEffect();
        public override Effect GetEffect() => _effect;

        /// <summary>
        /// Property: Center
        /// </summary>
        [DataMember]
        [PropertyMember]
        [DefaultValue(typeof(Point), "0.5,0.5")]
        public Point Center
        {
            get { return _effect.Center; }
            set { if (_effect.Center != value) { _effect.Center = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: Amount
        /// </summary>
        [DataMember]
        [PropertyRange(0, 1)]
        [DefaultValue(0.5)]
        public double Amount
        {
            get { return _effect.Amount; }
            set { if (_effect.Amount != value) { _effect.Amount = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: InnerRadius 
        /// </summary>
        [DataMember]
        [PropertyRange(0, 1)]
        [DefaultValue(0.2)]
        public double InnerRadius
        {
            get { return _effect.InnerRadius; }
            set { if (_effect.InnerRadius != value) { _effect.InnerRadius = value; RaiseEffectPropertyChanged(); } }
        }

        /// <summary>
        /// Property: OuterRadius 
        /// </summary>
        [DataMember]
        [PropertyRange(0, 1)]
        [DefaultValue(0.4)]
        public double OuterRadius
        {
            get { return _effect.OuterRadius; }
            set { if (_effect.OuterRadius != value) { _effect.OuterRadius = value; RaiseEffectPropertyChanged(); } }
        }
    }
}
