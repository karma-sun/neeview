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
        private bool _isPageBackgroundChecker;


        [PropertyMember("@ParamBackgroundType")]
        public BackgroundType BackgroundType
        {
            get { return _backgroundType; }
            set { SetProperty(ref _backgroundType, value); }
        }

        [PropertyMember("@ParamCustomBackground", Tips = "@ParamCustomBackgroundTips")]
        [PropertyMapLabel("@WordCustomBackground")]
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

        // ページ背景は格子模様
        [PropertyMember("@ParamIsPageBackgroundChecker", Tips = "@ParamIsPageBackgroundCheckerTips")]
        public bool IsPageBackgroundChecker
        {
            get { return _isPageBackgroundChecker; }
            set { SetProperty(ref _isPageBackgroundChecker, value); }
        }

    }
}