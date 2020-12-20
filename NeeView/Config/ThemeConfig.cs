using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class ThemeConfig : BindableBase
    {
        private PanelColor _panelColor = PanelColor.Dark;
        private PanelColor _menuColor = PanelColor.Light;


        /// <summary>
        /// テーマカラー：パネル
        /// </summary>
        [PropertyMember]
        public PanelColor PanelColor
        {
            get { return _panelColor; }
            set { SetProperty(ref _panelColor, value); }
        }

        /// <summary>
        /// テーマカラー：メニュー
        /// </summary>
        [PropertyMember]
        public PanelColor MenuColor
        {
            get { return _menuColor; }
            set { SetProperty(ref _menuColor, value); }
        }

    }
}