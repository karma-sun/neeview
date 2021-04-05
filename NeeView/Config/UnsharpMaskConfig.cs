using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using PhotoSauce.MagicScaler;
using System;
using System.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// UnsharpMask setting for resize filter (PhotoSauce.MagicScaler)
    /// </summary>
    public class UnsharpMaskConfig : BindableBase, ICloneable
    {
        private int _amount;
        private double _radius;
        private int _threshold;

        /// <summary>
        /// UnsharpAmount property.
        /// 25-200
        /// </summary>
        [PropertyRange(25, 200)]
        [DefaultValue(40)]
        public int Amount
        {
            get { return _amount; }
            set { SetProperty(ref _amount, value); }
        }

        /// <summary>
        /// UnsharpRadius property.
        /// 0.3-3.0
        /// </summary>
        [PropertyRange(0.3, 3.0, TickFrequency = 0.05)]
        [DefaultValue(1.5)]
        public double Radius
        {
            get { return _radius; }
            set { SetProperty(ref _radius, value); }
        }

        /// <summary>
        /// UnsharpThrethold property.
        /// 0-10
        /// </summary>
        [PropertyRange(0, 10)]
        [DefaultValue(0)]
        public int Threshold
        {
            get { return _threshold; }
            set { SetProperty(ref _threshold, value); }
        }


        public UnsharpMaskSettings CreateUnsharpMaskSetting()
        {
            return new UnsharpMaskSettings(_amount, _radius, (byte)_threshold);
        }

        public override int GetHashCode()
        {
            return Amount.GetHashCode() ^ Radius.GetHashCode() ^ Threshold.GetHashCode();
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

}