using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class SlideShowConfig : BindableBase
    {
        private double _SlideShowInterval = 5.0;
        private bool _IsCancelSlideByMouseMove = true;
        private bool _IsSlideShowByLoop = true;

        /// <summary>
        /// スライドショーの表示間隔(秒)
        /// </summary>
        [PropertyMember("@ParamSlideShowInterval")]
        public double SlideShowInterval
        {
            get { return _SlideShowInterval; }
            set { if (_SlideShowInterval != value) { _SlideShowInterval = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// カーソルでスライドを止める.
        /// </summary>
        [PropertyMember("@ParamIsCancelSlideByMouseMove", Tips = "@ParamIsCancelSlideByMouseMoveTips")]
        public bool IsCancelSlideByMouseMove
        {
            get { return _IsCancelSlideByMouseMove; }
            set { if (_IsCancelSlideByMouseMove != value) { _IsCancelSlideByMouseMove = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// ループ再生フラグ
        /// </summary>
        [PropertyMember("@ParamIsSlideShowByLoop", Tips = "@ParamIsSlideShowByLoopTips")]
        public bool IsSlideShowByLoop
        {
            get { return _IsSlideShowByLoop; }
            set { if (_IsSlideShowByLoop != value) { _IsSlideShowByLoop = value; RaisePropertyChanged(); } }
        }

    }
}