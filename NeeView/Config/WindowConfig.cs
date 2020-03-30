using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows;

namespace NeeView
{
    public class WindowConfig : BindableBase
    {
        private WindowChromeFrame _windowChromeFrame = WindowChromeFrame.WindowFrame;
        private bool _isCaptionVisible = true;
        private bool _isTopmost = false;
        private bool _isFullScreenWithTaskBar;
        private double _maximizeWindowGapWidth = 8.0;
        private WindowStateEx _state;
        private bool _isCaptionEmulateInFullScreen;


        [PropertyMember("@ParamWindowShapeChromeFrame")]
        public WindowChromeFrame WindowChromeFrame
        {
            get { return _windowChromeFrame; }
            set { SetProperty(ref _windowChromeFrame, value); }
        }

        public bool IsCaptionVisible
        {
            get { return _isCaptionVisible; }
            set { SetProperty(ref _isCaptionVisible, value); }
        }

        public bool IsTopmost
        {
            get { return _isTopmost; }
            set { SetProperty(ref _isTopmost, value); }
        }

        [PropertyMember("@ParamIsCaptionEmulateInFullScreen", Tips = "@ParamIsCaptionEmulateInFullScreenTips")]
        public bool IsCaptionEmulateInFullScreen
        {
            get { return _isCaptionEmulateInFullScreen; }
            set { SetProperty(ref _isCaptionEmulateInFullScreen, value); }
        }

        [PropertyMember("@ParamWindowShapeIsFullScreenWithTaskBar")]
        public bool IsFullScreenWithTaskBar
        {
            get { return _isFullScreenWithTaskBar; }
            set { SetProperty(ref _isFullScreenWithTaskBar, value); }
        }

        [PropertyRange("@ParamWindowShapeMaximizeWindowGapWidth", 0, 16, TickFrequency = 1, IsEditable = true, Tips = "@ParamWindowShapeMaximizeWindowGapWidthTips"), DefaultValue(8.0)]
        public double MaximizeWindowGapWidth
        {
            get { return _maximizeWindowGapWidth; }
            set { SetProperty(ref _maximizeWindowGapWidth, value); }
        }


        /// <summary>
        /// ウィンドウ状態
        /// </summary>
        public WindowStateEx State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }

        #region HiddenParameters

        /// <summary>
        /// フルスクリーンから復帰するウィンドウ状態
        /// </summary>
        [PropertyMapIgnore]
        public WindowStateEx LastState { get; set; }

        [PropertyMapIgnore]
        public WINDOWPLACEMENT Placement { get; set; }

        [PropertyMapIgnore]
        public double Width { get; set; } = 640.0;
        
        [PropertyMapIgnore]
        public double Height { get; set; } = 480.0;

        #endregion HiddenParameters
    }

}