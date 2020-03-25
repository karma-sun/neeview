﻿using NeeLaboratory.ComponentModel;
using PhotoSauce.MagicScaler;

namespace NeeView
{
    /// <summary>
    /// Resize filter (PhotoSauce.MagicScaler)
    /// </summary>
    public class ImageResizeFilterConfig : BindableBase
    {
        private bool _isResizeFilterEnabled = false;
        private ResizeInterpolation _resizeInterpolation = ResizeInterpolation.Lanczos;
        private bool _sharpen;

        public ImageResizeFilterConfig()
        {
            var setting = new ProcessImageSettings(); // default values.
            _sharpen = setting.Sharpen;
            this.UnsharpMask = new UnsharpMaskConfig();
            this.UnsharpMask.Amount = setting.UnsharpMask.Amount;
            this.UnsharpMask.Radius = setting.UnsharpMask.Radius;
            this.UnsharpMask.Threshold = setting.UnsharpMask.Threshold;
            this.UnsharpMask.PropertyChanged += (s, e) => RaisePropertyChanged(nameof(UnsharpMask));
        }

        public bool IsEnabled
        {
            get { return _isResizeFilterEnabled; }
            set { if (_isResizeFilterEnabled != value) { _isResizeFilterEnabled = value; RaisePropertyChanged(); } }
        }

        public ResizeInterpolation ResizeInterpolation
        {
            get { return _resizeInterpolation; }
            set { if (_resizeInterpolation != value) { _resizeInterpolation = value; RaisePropertyChanged(); } }
        }

        public bool Sharpen
        {
            get { return _sharpen; }
            set { if (_sharpen != value) { _sharpen = value; RaisePropertyChanged(); } }
        }

        public UnsharpMaskConfig UnsharpMask { get; set; }


        public override int GetHashCode()
        {
            return _resizeInterpolation.GetHashCode() ^ _sharpen.GetHashCode() ^ UnsharpMask.GetHashCode();
        }

        public ProcessImageSettings CreateProcessImageSetting()
        {
            var setting = new ProcessImageSettings();

            setting.Sharpen = this.Sharpen;
            setting.UnsharpMask = this.UnsharpMask.CreateUnsharpMaskSetting();

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
    }
}