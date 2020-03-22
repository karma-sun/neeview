using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Windows.Media;

namespace NeeView
{
    public class LayoutConfig : BindableBase
    {
        public ThemeConfig Theme { get; set; } = new ThemeConfig();

        public BackgroundConfig Background { get; set; } = new BackgroundConfig();

        public AutoHideConfig AutoHide { get; set; } = new AutoHideConfig();

        public SidePanelsConfig SidePanels { get; set; } = new SidePanelsConfig();

        public SliderConfig Slider { get; set; } = new SliderConfig();
    }


    public class SidePanelsConfig : BindableBase
    {
        private double _opacity = 1.0;


        [PropertyPercent("@ParamSidePanelOpacity", Tips = "@ParamSidePanelOpacityTips")]
        public double Opacity
        {
            get { return _opacity; }
            set { SetProperty(ref _opacity, value); }
        }
    }

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

    public class BackgroundConfig : BindableBase
    {
        private BackgroundType _backgroundType = BackgroundType.Black;
        private BrushSource _customBackground = new BrushSource();
        private Color _pageBackgroundColor = Colors.Transparent;


        public BackgroundType BackgroundType
        {
            get { return _backgroundType; }
            set { SetProperty(ref _backgroundType, value); }
        }

        [PropertyMember("@ParamCustomBackground", Tips = "@ParamCustomBackgroundTips")]
        public BrushSource CustomBackground
        {
            get { return _customBackground; }
            set { SetProperty(ref _customBackground, value ?? new BrushSource()); }
        }

        // ページの背景色。透過画像用
        [PropertyMember("@ParamPageBackgroundColor")]
        public Color PageBackgroundColor
        {
            get { return _pageBackgroundColor; }
            set { SetProperty(ref _pageBackgroundColor, value); }
        }
    }
}