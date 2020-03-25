using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;

namespace NeeView
{
    public class Config : BindableBase
    {
        public static Config Current { get; } = new Config();


        public SystemConfig System { get; set; } = new SystemConfig();

        public StartUpConfig StartUp { get; set; } = new StartUpConfig();

        public PerformanceConfig Performance { get; set; } = new PerformanceConfig();

        public ImageConfig Image { get; set; } = new ImageConfig();

        public ArchiveConfig Archive { get; set; } = new ArchiveConfig();

        public SusieConfig Susie { get; set; } = new SusieConfig();

        public HistoryConfig History { get; set; } = new HistoryConfig();

        public BookmarkConfig Bookmark { get; set; } = new BookmarkConfig();

        public PagemarkConfig Pagemark { get; set; } = new PagemarkConfig();

        public WindowConfig Window { get; set; } = new WindowConfig();

        public LayoutConfig Layout { get; set; } = new LayoutConfig();

        public ThumbnailConfig Thumbnail { get; set; } = new ThumbnailConfig();

        public SlideShowConfig SlideShow { get; set; } = new SlideShowConfig();

        public EffectConfig Effect { get; set; } = new EffectConfig();

        public ImageCustomSizeConfig ImageCustomSize { get; set; } = new ImageCustomSizeConfig();

        public ImageDotKeepConfig ImageDotKeep { get; set; } = new ImageDotKeepConfig();

        public ImageGridConfig ImageGrid { get; set; } = new ImageGridConfig();

        public ImageResizeFilterConfig ImageResizeFilter { get; set; } = new ImageResizeFilterConfig();

        public ViewConfig View { get; set; } = new ViewConfig();

        public MouseConfig Mouse { get; set; } = new MouseConfig();

        public TouchConfig Touch { get; set; } = new TouchConfig();

        public LoupeConfig Loupe { get; set; } = new LoupeConfig();

        public CommandConfig Command { get; set; } = new CommandConfig();

        public ScriptConfig Script { get; set; } = new ScriptConfig();
    }

    public class MouseConfig : BindableBase
    {
        private bool _isGestureEnabled = true;
        private bool _isDragEnabled = true;
        private double _gestureMinimumDistance = 30.0;
        private LongButtonDownMode _longButtonDownMode = LongButtonDownMode.Loupe;
        private bool _isCursorHideEnabled = true;
        private double _cursorHideTime = 2.0;

        // マウスジェスチャー有効
        [PropertyMember("@ParamMouseIsGestureEnabled")]
        public bool IsGestureEnabled
        {
            get { return _isGestureEnabled; }
            set { SetProperty(ref _isGestureEnabled, value); }
        }

        // マウスドラッグ有効
        [PropertyMember("@ParamMouseIsDragEnabled")]
        public bool IsDragEnabled
        {
            get { return _isDragEnabled; }
            set { SetProperty(ref _isDragEnabled, value); }
        }

        // ドラッグ開始距離
        [PropertyRange("@ParamMouseMinimumDragDistance", 1.0, 200.0, TickFrequency = 1.0, IsEditable = true, Tips = "@ParamMouseMinimumDragDistanceTips")]
        public double MinimumDragDistance { get; set; } = 5.0;


        [PropertyRange("@ParamMouseGestureMinimumDistance", 5.0, 200.0, TickFrequency = 1.0, IsEditable = true, Tips = "@ParamMouseGestureMinimumDistanceTips")]
        public double GestureMinimumDistance
        {
            get { return _gestureMinimumDistance; }
            set { _gestureMinimumDistance = Math.Max(value, SystemParameters.MinimumHorizontalDragDistance); }
        }

        [PropertyMember("@ParamMouseLongButtonDownMode")]
        public LongButtonDownMode LongButtonDownMode
        {
            get { return _longButtonDownMode; }
            set { _longButtonDownMode = value; RaisePropertyChanged(); }
        }

        [PropertyMember("@ParamMouseLongButtonMask")]
        public LongButtonMask LongButtonMask { get; set; }

        [PropertyRange("@ParamMouseLongButtonDownTime", 0.1, 2.0, TickFrequency = 0.1, Tips = "@ParamMouseLongButtonDownTimeTips")]
        public double LongButtonDownTime { get; set; } = 1.0;

        [PropertyRange("@ParamMouseLongButtonRepeatTime", 0.01, 1.0, TickFrequency = 0.01, Tips = "@ParamMouseLongButtonRepeatTimeTips")]
        public double LongButtonRepeatTime { get; set; } = 0.1;

        /// <summary>
        /// カーソルの自動非表示
        /// </summary>
        [PropertyMember("@ParamIsCursorHideEnabled")]
        public bool IsCursorHideEnabled
        {
            get { return _isCursorHideEnabled; }
            set { SetProperty(ref _isCursorHideEnabled, value); }
        }

        [PropertyRange("@ParameterCursorHideTime", 1.0, 10.0, TickFrequency = 0.2, IsEditable = true)]
        public double CursorHideTime
        {
            get => _cursorHideTime;
            set => SetProperty(ref _cursorHideTime, Math.Max(1.0, value));
        }

        [PropertyMember("@ParameterIsCursorHideReleaseAction")]
        public bool IsCursorHideReleaseAction { get; set; } = true;

        [PropertyRange("@ParameterCursorHideReleaseDistance", 0.0, 1000.0, TickFrequency = 1.0, IsEditable = true)]
        public double CursorHideReleaseDistance { get; set; } = 5.0;
    }

    public class TouchConfig : BindableBase
    {
        private bool _isEnabled = true;
        private double _gestureMinimumDistance = 16.0;

        [PropertyMember("@ParamTouchIsEnabled", Tips = "@ParamTouchIsEnabledTips")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { if (_isEnabled != value) { _isEnabled = value; RaisePropertyChanged(); } }
        }

        /// ドラッグアクション
        [PropertyMember("@ParamTouchDragAction")]
        public TouchAction DragAction { get; set; } = TouchAction.Gesture;

        /// 長押しドラッグアクション
        [PropertyMember("@ParamTouchHoldAction")]
        public TouchAction HoldAction { get; set; } = TouchAction.Drag;


        [PropertyMember("@ParamTouchIsAngleEnabled")]
        public bool IsAngleEnabled { get; set; } = true;

        [PropertyMember("@ParamTouchIsScaleEnabled")]
        public bool IsScaleEnabled { get; set; } = true;


        [PropertyMember("@ParamTouchGestureMinimumDistance", Tips = "@ParamTouchGestureMinimumDistanceTips")]
        public double GestureMinimumDistance
        {
            get { return _gestureMinimumDistance; }
            set { _gestureMinimumDistance = value; }
        }

        [PropertyMember("@ParamTouchMinimumManipulationRadius", Tips = "@ParamTouchMinimumManipulationRadiusTips")]
        public double MinimumManipulationRadius { get; set; } = 80.0;

        [PropertyMember("@ParamTouchMinimumManipulationDistance", Tips = "@ParamTouchMinimumManipulationDistanceTips")]
        public double MinimumManipulationDistance { get; set; } = 30.0;

    }

    public class LoupeConfig : BindableBase
    {
        private double _defaultScale = 2.0;
        private bool _IsLoupeCenter;
        private double _minimumScale = 2.0;
        private double _maximumScale = 10.0;
        private double _scaleStep = 1.0;
        private bool _isResetByRestart = false;
        private bool _isResetByPageChanged = true;

        [PropertyMember("@ParamLoupeIsLoupeCenter")]
        public bool IsLoupeCenter
        {
            get { return _IsLoupeCenter; }
            set { if (_IsLoupeCenter != value) { _IsLoupeCenter = value; RaisePropertyChanged(); } }
        }

        [PropertyRange("@ParamLoupeMinimumScale", 1, 20, TickFrequency = 1.0, IsEditable = true)]
        public double MinimumScale
        {
            get { return _minimumScale; }
            set
            {
                if (_minimumScale != value)
                {
                    _minimumScale = value;
                    RaisePropertyChanged();
                }
            }
        }

        [PropertyRange("@ParamLoupeMaximumScale", 1, 20, TickFrequency = 1.0, IsEditable = true)]
        public double MaximumScale
        {
            get { return _maximumScale; }
            set
            {
                if (_maximumScale != value)
                {
                    _maximumScale = value;
                    RaisePropertyChanged();
                }
            }
        }

        [PropertyRange("@ParamLoupeDefaultScale", 1, 20, TickFrequency = 1.0, IsEditable = true)]
        public double DefaultScale
        {
            get { return _defaultScale; }
            set { SetProperty(ref _defaultScale, value); }
        }

        [PropertyRange("@ParamLoupeScaleStep", 0.1, 5.0, TickFrequency = 0.1, IsEditable = true)]
        public double ScaleStep
        {
            get { return _scaleStep; }
            set { if (_scaleStep != value) { _scaleStep = Math.Max(value, 0.0); RaisePropertyChanged(); } }
        }

        [PropertyMember("@ParamLoupeIsResetByRestart", Tips = "@ParamLoupeIsResetByRestartTips")]
        public bool IsResetByRestart
        {
            get { return _isResetByRestart; }
            set { if (_isResetByRestart != value) { _isResetByRestart = value; RaisePropertyChanged(); } }
        }

        [PropertyMember("@ParamLoupeIsResetByPageChanged")]
        public bool IsResetByPageChanged
        {
            get { return _isResetByPageChanged; }
            set { if (_isResetByPageChanged != value) { _isResetByPageChanged = value; RaisePropertyChanged(); } }
        }

        [PropertyMember("@ParamLoupeIsWheelScalingEnabled", Tips = "@ParamLoupeIsWheelScalingEnabledTips")]
        public bool IsWheelScalingEnabled { get; set; } = true;

        [PropertyRange("@ParamLoupeSpeed", 0.0, 10.0, TickFrequency = 0.1, Format = "×{0:0.0}")]
        public double Speed { get; set; } = 1.0;

        [PropertyMember("@ParamLoupeIsEscapeKeyEnabled")]
        public bool IsEscapeKeyEnabled { get; set; } = true;
    }

}