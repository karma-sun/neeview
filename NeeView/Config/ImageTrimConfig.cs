using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class ImageTrimConfig : BindableBase
    {
        private const double _maxRate = 0.9;

        private bool _isEnabled;
        private double _top;
        private double _bottom;
        private double _left;
        private double _right;


        [PropertyMember("@ParamImageTrimIsEnabled", IsVisible = false)]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }


        [PropertyPercent("@ParamImageTrimLeft", 0.0, _maxRate)]
        public double Left
        {
            get { return _left; }
            set
            {
                if (SetProperty(ref _left, MathUtility.Clamp(value, 0.0, _maxRate)))
                {
                    if (_left + _right > _maxRate)
                    {
                        _right = _maxRate - _left;
                        RaisePropertyChanged(nameof(Right));
                    }
                }
            }
        }

        [PropertyPercent("@ParamImageTrimRight", 0.0, _maxRate)]
        public double Right
        {
            get { return _right; }
            set
            {
                if (SetProperty(ref _right, MathUtility.Clamp(value, 0.0, _maxRate)))
                {
                    if (_left + _right > _maxRate)
                    {
                        _left = _maxRate - _right;
                        RaisePropertyChanged(nameof(Left));
                    }
                }
            }
        }


        [PropertyPercent("@ParamImageTrimTop", 0.0, _maxRate)]
        public double Top
        {
            get { return _top; }
            set
            {
                if (SetProperty(ref _top, MathUtility.Clamp(value, 0.0, _maxRate)))
                {
                    if (_top + _bottom > _maxRate)
                    {
                        _bottom = _maxRate - _top;
                        RaisePropertyChanged(nameof(Bottom));
                    }
                }
            }
        }

        [PropertyPercent("@ParamImageTrimBottom", 0.0, _maxRate)]
        public double Bottom
        {
            get { return _bottom; }
            set
            {
                if (SetProperty(ref _bottom, MathUtility.Clamp(value, 0.0, _maxRate)))
                {
                    if (_top + _bottom > _maxRate)
                    {
                        _top = _maxRate - _bottom;
                        RaisePropertyChanged(nameof(Top));
                    }
                }
            }
        }


    }
}