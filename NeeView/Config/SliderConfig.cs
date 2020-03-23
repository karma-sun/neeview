using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class SliderConfig : BindableBase
    {
        private double _sliderOpacity = 1.0;


        // スライダー透明度
        [PropertyPercent("@ParamSliderOpacity", Tips = "@ParamSliderOpacityTips")]
        public double Opacity
        {
            get { return _sliderOpacity; }
            set { SetProperty(ref _sliderOpacity, value); }
        }
    }
}