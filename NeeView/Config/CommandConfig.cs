using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class CommandConfig : BindableBase
    {
        private bool _isReversePageMove = true;
        private bool _isReversePageMoveWheel;

        [PropertyMember("@ParamIsAccessKeyEnabled", Tips = "@ParamIsAccessKeyEnabledTips")]
        public bool IsAccessKeyEnabled { get; set; } = true;

        [PropertyMember("@ParamCommandIsReversePageMove", Tips = "@ParamCommandIsReversePageMoveTips")]
        public bool IsReversePageMove
        {
            get { return _isReversePageMove; }
            set { if (_isReversePageMove != value) { _isReversePageMove = value; RaisePropertyChanged(); } }
        }

        [PropertyMember("@ParamCommandIsReversePageMoveWheel", Tips = "@ParamCommandIsReversePageMoveWheelTips")]
        public bool IsReversePageMoveWheel
        {
            get { return _isReversePageMoveWheel; }
            set { if (_isReversePageMoveWheel != value) { _isReversePageMoveWheel = value; RaisePropertyChanged(); } }
        }
    }
}