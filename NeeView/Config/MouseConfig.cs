using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Windows;

namespace NeeView
{
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
            set { SetProperty(ref _longButtonDownMode, value); }
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

}