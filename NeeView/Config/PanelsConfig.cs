using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Windows;

namespace NeeView
{
    public class PanelsConfig : BindableBase
    {
        private bool _isSideBarEnabled = true;
        private double _opacity = 1.0;
        private bool _isHidePanelInFullscreen = true;
        private string _fontName = SystemFonts.MessageFontFamily.Source;
        private double _fontSize = 15.0;

        /// <summary>
        /// サイドバー表示フラグ 
        /// </summary>
        public bool IsSideBarEnabled
        {
            get { return _isSideBarEnabled; }
            set { SetProperty(ref _isSideBarEnabled, value); }
        }

        /// <summary>
        /// パネルの透明度
        /// </summary>
        [PropertyPercent("@ParamSidePanelOpacity", Tips = "@ParamSidePanelOpacityTips")]
        public double Opacity
        {
            get { return _opacity; }
            set { SetProperty(ref _opacity, value); }
        }

        /// <summary>
        /// フルスクリーン時にパネルを隠す
        /// </summary>
        [PropertyMember("@ParamIsHidePanelInFullscreen")]
        public bool IsHidePanelInFullscreen
        {
            get { return _isHidePanelInFullscreen; }
            set { SetProperty(ref _isHidePanelInFullscreen, value); }
        }

        /// <summary>
        /// パネルでの左右キー操作有効
        /// </summary>
        [PropertyMember("@ParamSidePanelIsLeftRightKeyEnabled", Tips = "@ParamSidePanelIsLeftRightKeyEnabledTips")]
        public bool IsLeftRightKeyEnabled { get; set; } = true;

        /// <summary>
        /// タッチパ操作でのリストバウンド効果
        /// </summary>
        [PropertyMember("@ParamSidePanelIsManipulationBoundaryFeedbackEnabled")]
        public bool IsManipulationBoundaryFeedbackEnabled { get; set; }


#if false
        /// <summary>
        /// フォント名
        /// </summary>
        [PropertyMember("@ParamListItemFontName")]
        public string FontName
        {
            get { return _fontName; }
            set { SetProperty(ref _fontName, string.IsNullOrWhiteSpace(value) ? SystemFonts.MessageFontFamily.Source : value); }
        }

        /// <summary>
        /// フォントサイズ
        /// </summary>
        [PropertyRange("@ParamListItemFontSize", 8, 24, TickFrequency = 0.5, IsEditable = true)]
        public double FontSize
        {
            get { return _fontSize; }
            set { SetProperty(ref _fontSize, Math.Max(1.0, value)); }
        }
#endif
    }
}