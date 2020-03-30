using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class TouchConfig : BindableBase
    {
        private bool _isEnabled = true;
        private TouchAction _dragAction = TouchAction.Gesture;
        private TouchAction _holdAction = TouchAction.Drag;
        private bool _isAngleEnabled = true;
        private bool _isScaleEnabled = true;
        private double _gestureMinimumDistance = 16.0;
        private double _minimumManipulationRadius = 80.0;
        private double _minimumManipulationDistance = 30.0;


        [PropertyMember("@ParamTouchIsEnabled", Tips = "@ParamTouchIsEnabledTips")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        /// ドラッグアクション
        [PropertyMember("@ParamTouchDragAction")]
        public TouchAction DragAction
        {
            get { return _dragAction; }
            set { SetProperty(ref _dragAction, value); }
        }

        /// 長押しドラッグアクション
        [PropertyMember("@ParamTouchHoldAction")]
        public TouchAction HoldAction
        {
            get { return _holdAction; }
            set { SetProperty(ref _holdAction, value); }
        }

        [PropertyMember("@ParamTouchIsAngleEnabled")]
        public bool IsAngleEnabled
        {
            get { return _isAngleEnabled; }
            set { SetProperty(ref _isAngleEnabled, value); }
        }

        [PropertyMember("@ParamTouchIsScaleEnabled")]
        public bool IsScaleEnabled
        {
            get { return _isScaleEnabled; }
            set { SetProperty(ref _isScaleEnabled, value); }
        }

        [PropertyMember("@ParamTouchGestureMinimumDistance", Tips = "@ParamTouchGestureMinimumDistanceTips")]
        public double GestureMinimumDistance
        {
            get { return _gestureMinimumDistance; }
            set { SetProperty(ref _gestureMinimumDistance, value); }
        }

        [PropertyMember("@ParamTouchMinimumManipulationRadius", Tips = "@ParamTouchMinimumManipulationRadiusTips")]
        public double MinimumManipulationRadius
        {
            get { return _minimumManipulationRadius; }
            set { SetProperty(ref _minimumManipulationRadius, value); }
        }

        [PropertyMember("@ParamTouchMinimumManipulationDistance", Tips = "@ParamTouchMinimumManipulationDistanceTips")]
        public double MinimumManipulationDistance
        {
            get { return _minimumManipulationDistance; }
            set { SetProperty(ref _minimumManipulationDistance, value); }
        }
    }

}