using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class LayoutConfig : BindableBase
    {
        public ThemeConfig Theme { get; set; } = new ThemeConfig();

        public WindowLayoutConfig Window { get; set; } = new WindowLayoutConfig();
    }


    public class ThemeConfig : BindableBase
    {
        private PanelColor _panelColor = PanelColor.Dark;
        private PanelColor _menuColor = PanelColor.Light;

        /// <summary>
        /// テーマカラー：パネル
        /// </summary>
        [PropertyMember("@ParamPanelColor")]
        public PanelColor PanelColor
        {
            get { return _panelColor; }
            set { SetProperty(ref _panelColor, value); }
        }

        /// <summary>
        /// テーマカラー：メニュー
        /// </summary>
        [PropertyMember("@ParamMenuColor")]
        public PanelColor MenuColor
        {
            get { return _menuColor; }
            set { SetProperty(ref _menuColor, value); }
        }
    }

}