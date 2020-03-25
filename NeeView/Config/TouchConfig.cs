using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class TouchConfig : BindableBase
    {
        private bool _isEnabled = true;


        [PropertyMember("@ParamTouchIsEnabled", Tips = "@ParamTouchIsEnabledTips")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
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
        public double GestureMinimumDistance { get; set; } = 16.0;

        [PropertyMember("@ParamTouchMinimumManipulationRadius", Tips = "@ParamTouchMinimumManipulationRadiusTips")]
        public double MinimumManipulationRadius { get; set; } = 80.0;

        [PropertyMember("@ParamTouchMinimumManipulationDistance", Tips = "@ParamTouchMinimumManipulationDistanceTips")]
        public double MinimumManipulationDistance { get; set; } = 30.0;

    }

}