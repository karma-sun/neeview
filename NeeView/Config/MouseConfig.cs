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
        private double _minimumDragDistance = 5.0;
        private LongButtonMask _longButtonMask;
        private double _longButtonDownTime = 1.0;
        private double _longButtonRepeatTime = 0.1;
        private bool _isCursorHideReleaseAction = true;
        private double _cursorHideReleaseDistance = 5.0;
        private bool _isHoverScroll;


        // マウスジェスチャー有効
        [PropertyMember]
        public bool IsGestureEnabled
        {
            get { return _isGestureEnabled; }
            set { SetProperty(ref _isGestureEnabled, value); }
        }

        // マウスドラッグ有効
        [PropertyMember]
        public bool IsDragEnabled
        {
            get { return _isDragEnabled; }
            set { SetProperty(ref _isDragEnabled, value); }
        }

        // ドラッグ開始距離
        [PropertyRange(1.0, 200.0, TickFrequency = 1.0, IsEditable = true)]
        public double MinimumDragDistance
        {
            get { return _minimumDragDistance; }
            set { SetProperty(ref _minimumDragDistance, value); }
        }

        [PropertyRange(5.0, 200.0, TickFrequency = 1.0, IsEditable = true)]
        public double GestureMinimumDistance
        {
            get { return _gestureMinimumDistance; }
            set { SetProperty(ref _gestureMinimumDistance, Math.Max(value, SystemParameters.MinimumHorizontalDragDistance)); }
        }

        [PropertyMember]
        public LongButtonDownMode LongButtonDownMode
        {
            get { return _longButtonDownMode; }
            set { SetProperty(ref _longButtonDownMode, value); }
        }

        [PropertyMember]
        public LongButtonMask LongButtonMask
        {
            get { return _longButtonMask; }
            set { SetProperty(ref _longButtonMask, value); }
        }

        [PropertyRange(0.1, 2.0, TickFrequency = 0.1)]
        public double LongButtonDownTime
        {
            get { return _longButtonDownTime; }
            set { SetProperty(ref _longButtonDownTime, value); }
        }

        [PropertyRange(0.01, 1.0, TickFrequency = 0.01)]
        public double LongButtonRepeatTime
        {
            get { return _longButtonRepeatTime; }
            set { SetProperty(ref _longButtonRepeatTime, value); }
        }

        /// <summary>
        /// カーソルの自動非表示
        /// </summary>
        [PropertyMember]
        public bool IsCursorHideEnabled
        {
            get { return _isCursorHideEnabled; }
            set { SetProperty(ref _isCursorHideEnabled, value); }
        }

        [PropertyRange(1.0, 10.0, TickFrequency = 0.2, IsEditable = true)]
        public double CursorHideTime
        {
            get { return _cursorHideTime; }
            set { SetProperty(ref _cursorHideTime, Math.Max(1.0, value)); }
        }

        [PropertyMember]
        public bool IsCursorHideReleaseAction
        {
            get { return _isCursorHideReleaseAction; }
            set { SetProperty(ref _isCursorHideReleaseAction, value); }
        }

        [PropertyRange(0.0, 1000.0, TickFrequency = 1.0, IsEditable = true)]
        public double CursorHideReleaseDistance
        {
            get { return _cursorHideReleaseDistance; }
            set { SetProperty(ref _cursorHideReleaseDistance, value); }
        }

        [PropertyMember]
        public bool IsHoverScroll
        {
            get { return _isHoverScroll; }
            set { SetProperty(ref _isHoverScroll, value); }
        }

    }

}