// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using NeeView.Windows.Property;
using PhotoSauce.MagicScaler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public enum ResizeInterpolation
    {
        NearestNeighbor,
        Average,
        Linear,
        Quadratic,
        Hermite,
        Mitchell,
        CatmullRom,
        Cubic,
        CubicSmoother,
        Lanczos, // default.
        Spline36,
    }

    public static class ResizeInterpolationExtensions
    {
        public static List<ResizeInterpolation> ResizeInterpolationList { get; } =
            Enum.GetValues(typeof(ResizeInterpolation)).Cast<ResizeInterpolation>().Where(e => e != ResizeInterpolation.NearestNeighbor).ToList();
        //Enum.GetValues(typeof(ResizeInterpolation)).Cast<ResizeInterpolation>().ToList();
    }


    /// <summary>
    /// 
    /// </summary>
    public class UnsharpMaskProfile : BindableBase
    {
        private int _amount;
        private double _radius;
        private int _threshold;

        /// <summary>
        /// UnsharpAmount property.
        /// 25-200
        /// </summary>
        [PropertyRange(25, 200, Name = "適応量")]
        [DefaultValue(40)]
        public int Amount
        {
            get { return _amount; }
            set { if (_amount != value) { _amount = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// UnsharpRadius property.
        /// 0.3-3.0
        /// </summary>
        [PropertyRange(0.3, 3.0, Name = "半径")]
        [DefaultValue(1.5)]
        public double Radius
        {
            get { return _radius; }
            set { if (_radius != value) { _radius = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// UnsharpThrethold property.
        /// 0-10
        /// </summary>
        [PropertyRange(0, 10, Name = "閾値")]
        [DefaultValue(0)]
        public int Threshold
        {
            get { return _threshold; }
            set { if (_threshold != value) { _threshold = value; RaisePropertyChanged(); } }
        }

        //
        public UnsharpMaskSettings Create()
        {
            return new UnsharpMaskSettings(_amount, _radius, (byte)_threshold);
        }

        //
        public override int GetHashCode()
        {
            return Amount.GetHashCode() ^ Radius.GetHashCode() ^ Threshold.GetHashCode();
        }

        #region Memento
        // インスタンスごと差し替えると問題があるため、Memento形式にする

        //
        public UnsharpMaskProfile CreateMemento()
        {
            var memento = (UnsharpMaskProfile)this.MemberwiseClone();
            return memento;
        }

        //
        public void Restore(UnsharpMaskProfile memento)
        {
            if (memento == null) return;

            this.Amount = memento.Amount;
            this.Radius = memento.Radius;
            this.Threshold = memento.Threshold;
        }

        #endregion
    }


    /// <summary>
    /// 
    /// </summary>
    public class ImageFilter : BindableBase
    {
        public static ImageFilter Current { get; private set; }
        
        #region Properties

        /// <summary>
        /// ResizeInterpolation property.
        /// </summary>
        private ResizeInterpolation _resizeInterpolation;
        public ResizeInterpolation ResizeInterpolation
        {
            get { return _resizeInterpolation; }
            set { if (_resizeInterpolation != value) { _resizeInterpolation = value; RaisePropertyChanged(); } }
        }


        /// <summary>
        /// Sharpen property.
        /// </summary>
        private bool _sharpen;
        public bool Sharpen
        {
            get { return _sharpen; }
            set { if (_sharpen != value) { _sharpen = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// UnsharpMask profile
        /// </summary>
        public UnsharpMaskProfile UnsharpMaskProfile { get; set; }

        #endregion

        #region Constructor

        //
        public ImageFilter()
        {
            Current = this;

            _resizeInterpolation = ResizeInterpolation.Lanczos;

            var setting = new ProcessImageSettings(); // default values.
            _sharpen = setting.Sharpen;
            this.UnsharpMaskProfile = new UnsharpMaskProfile();
            this.UnsharpMaskProfile.Amount = setting.UnsharpMask.Amount;
            this.UnsharpMaskProfile.Radius = setting.UnsharpMask.Radius;
            this.UnsharpMaskProfile.Threshold = setting.UnsharpMask.Threshold;
            this.UnsharpMaskProfile.PropertyChanged += (s, e) => RaisePropertyChanged(nameof(UnsharpMaskProfile));
        }

        #endregion

        #region Methods

        //
        public override int GetHashCode()
        {
            return _resizeInterpolation.GetHashCode() ^ _sharpen.GetHashCode() ^ UnsharpMaskProfile.GetHashCode();
        }

        //
        public ProcessImageSettings CreateProcessImageSetting()
        {
            var setting = new ProcessImageSettings();

            setting.Sharpen = this.Sharpen;
            setting.UnsharpMask = this.UnsharpMaskProfile.Create();

            switch (_resizeInterpolation)
            {
                case ResizeInterpolation.NearestNeighbor:
                    setting.Interpolation = InterpolationSettings.NearestNeighbor;
                    break;
                case ResizeInterpolation.Average:
                    setting.Interpolation = InterpolationSettings.Average;
                    break;
                case ResizeInterpolation.Linear:
                    setting.Interpolation = InterpolationSettings.Linear;
                    break;
                case ResizeInterpolation.Quadratic:
                    setting.Interpolation = InterpolationSettings.Quadratic;
                    //setting.Interpolation = new InterpolationSettings(new PhotoSauce.MagicScaler.Interpolators.QuadraticInterpolator(1.0));
                    break;
                case ResizeInterpolation.Hermite:
                    setting.Interpolation = InterpolationSettings.Hermite;
                    break;
                case ResizeInterpolation.Mitchell:
                    setting.Interpolation = InterpolationSettings.Mitchell;
                    break;
                case ResizeInterpolation.CatmullRom:
                    setting.Interpolation = InterpolationSettings.CatmullRom;
                    break;
                case ResizeInterpolation.Cubic:
                    setting.Interpolation = InterpolationSettings.Cubic;
                    //setting.Interpolation = new InterpolationSettings(new PhotoSauce.MagicScaler.Interpolators.CubicInterpolator(0, 0.5));
                    break;
                case ResizeInterpolation.CubicSmoother:
                    setting.Interpolation = InterpolationSettings.CubicSmoother;
                    break;
                case ResizeInterpolation.Lanczos:
                    setting.Interpolation = InterpolationSettings.Lanczos;
                    //setting.Interpolation = new InterpolationSettings(new PhotoSauce.MagicScaler.Interpolators.LanczosInterpolator(3));
                    break;
                case ResizeInterpolation.Spline36:
                    setting.Interpolation = InterpolationSettings.Spline36;
                    break;
            }

            return setting;
        }

        #endregion

        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public ResizeInterpolation ResizeInterpolation { get; set; }
            [DataMember]
            public bool Sharpen { get; set; }
            [DataMember]
            public UnsharpMaskProfile UnsharpMaskProfile { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.ResizeInterpolation = this.ResizeInterpolation;
            memento.Sharpen = this.Sharpen;
            memento.UnsharpMaskProfile = this.UnsharpMaskProfile.CreateMemento();
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.ResizeInterpolation = memento.ResizeInterpolation;
            this.Sharpen = memento.Sharpen;
            this.UnsharpMaskProfile.Restore(memento.UnsharpMaskProfile);
        }

        #endregion
    }
}
