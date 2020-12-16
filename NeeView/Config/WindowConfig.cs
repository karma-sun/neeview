using NeeLaboratory.ComponentModel;
using NeeView.Windows;
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
        private double _maximizeWindowGapWidth = 8.0;
        private WindowStateEx _state;
        private bool _isCaptionEmulateInFullScreen;
        private bool _mouseActivateAndEat;
        private bool _isAeroSnapPlacementEnabled = true;
        private bool _isAutoHideInNormal = false;
        private bool _isAutoHideInMaximized = true;
        private bool _IsAutoHideInFullScreen = false;


        [PropertyMember("@ParamWindowShapeChromeFrame")]
        public WindowChromeFrame WindowChromeFrame
        {
            get { return _windowChromeFrame; }
            set { SetProperty(ref _windowChromeFrame, value); }
        }

        [PropertyMember("@ParamWindowIsCaptionVisible")]
        public bool IsCaptionVisible
        {
            get { return _isCaptionVisible; }
            set { SetProperty(ref _isCaptionVisible, value); }
        }

        [PropertyMember("@ParamWindowIsTopmost")]
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

        [PropertyRange("@ParamWindowShapeMaximizeWindowGapWidth", 0, 16, TickFrequency = 1, IsEditable = true, Tips = "@ParamWindowShapeMaximizeWindowGapWidthTips"), DefaultValue(8.0)]
        public double MaximizeWindowGapWidth
        {
            get { return _maximizeWindowGapWidth; }
            set { SetProperty(ref _maximizeWindowGapWidth, value); }
        }

        [PropertyMember("@ParamWindowMouseActivateAndEat")]
        public bool MouseActivateAndEat
        {
            get { return _mouseActivateAndEat; }
            set { SetProperty(ref _mouseActivateAndEat, value); }
        }

        /// <summary>
        /// ウィンドウ状態
        /// </summary>
        [PropertyMember("@ParamWindowState")]
        public WindowStateEx State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }

        /// <summary>
        /// エアロスナップのウィンドウ座標を保存
        /// </summary>
        [PropertyMember("@ParamIsRestoreAeroSnapPlacement")]
        public bool IsRestoreAeroSnapPlacement
        {
            get { return _isAeroSnapPlacementEnabled; }
            set { SetProperty(ref _isAeroSnapPlacementEnabled, value); }
        }

        [PropertyMember("@WindowConfig.IsAutoHideInNormal")]
        public bool IsAutoHideInNormal
        {
            get { return _isAutoHideInNormal; }
            set { SetProperty(ref _isAutoHideInNormal, value); }
        }

        [PropertyMember("@WindowConfig.IsAutoHidInMaximized")]
        public bool IsAutoHidInMaximized
        {
            get { return _isAutoHideInMaximized; }
            set { SetProperty(ref _isAutoHideInMaximized, value); }
        }

        [PropertyMember("@WindowConfig.IsAutoHideInFullScreen")]
        public bool IsAutoHideInFullScreen
        {
            get { return _IsAutoHideInFullScreen; }
            set { SetProperty(ref _IsAutoHideInFullScreen, value); }
        }


        #region HiddenParameters

        /// <summary>
        /// フルスクリーンから復帰するウィンドウ状態
        /// </summary>
        [PropertyMapIgnore]
        public WindowStateEx LastState { get; set; }

        /// <summary>
        /// 復元ウィンドウ座標
        /// </summary>
        [PropertyMapIgnore]
        [ObjectMergeReferenceCopy]
        public WindowPlacement WindowPlacement { get; set; }

        #endregion HiddenParameters
    }

}