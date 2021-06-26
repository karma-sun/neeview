using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class CommandConfig : BindableBase
    {
        private bool _isAccessKeyEnabled = true;
        private bool _isReversePageMove = true;
        private bool _isReversePageMoveWheel;
        private bool _isHorizontalWheelLimitedOnce = true;

        [PropertyMember]
        public bool IsAccessKeyEnabled
        {
            get { return _isAccessKeyEnabled; }
            set { SetProperty(ref _isAccessKeyEnabled, value); }
        }

        [PropertyMember]
        public bool IsReversePageMove
        {
            get { return _isReversePageMove; }
            set { SetProperty(ref _isReversePageMove, value); }
        }

        [PropertyMember]
        public bool IsReversePageMoveWheel
        {
            get { return _isReversePageMoveWheel; }
            set { SetProperty(ref _isReversePageMoveWheel, value); }
        }

        [PropertyMember]
        public bool IsHorizontalWheelLimitedOnce
        {
            get { return _isHorizontalWheelLimitedOnce; }
            set { SetProperty(ref _isHorizontalWheelLimitedOnce, value); }
        }
    }
}