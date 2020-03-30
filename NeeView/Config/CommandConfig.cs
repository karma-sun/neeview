using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class CommandConfig : BindableBase
    {
        private bool _isAccessKeyEnabled = true;
        private bool _isReversePageMove = true;
        private bool _isReversePageMoveWheel;

        [PropertyMember("@ParamIsAccessKeyEnabled", Tips = "@ParamIsAccessKeyEnabledTips")]
        public bool IsAccessKeyEnabled
        {
            get { return _isAccessKeyEnabled; }
            set { SetProperty(ref _isAccessKeyEnabled, value); }
        }

        [PropertyMember("@ParamCommandIsReversePageMove", Tips = "@ParamCommandIsReversePageMoveTips")]
        public bool IsReversePageMove
        {
            get { return _isReversePageMove; }
            set { SetProperty(ref _isReversePageMove, value); }
        }

        [PropertyMember("@ParamCommandIsReversePageMoveWheel", Tips = "@ParamCommandIsReversePageMoveWheelTips")]
        public bool IsReversePageMoveWheel
        {
            get { return _isReversePageMoveWheel; }
            set { SetProperty(ref _isReversePageMoveWheel, value); }
        }
    }
}