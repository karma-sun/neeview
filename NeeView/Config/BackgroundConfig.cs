using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Windows.Media;

namespace NeeView
{
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