using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class SlideShowConfig : BindableBase
    {
        private double _slideShowInterval = 5.0;
        private bool _isCancelSlideByMouseMove = true;
        private bool _isSlideShowByLoop = true;

        /// <summary>
        /// スライドショーの表示間隔(秒)
        /// </summary>
        [PropertyMember]
        public double SlideShowInterval
        {
            get { return _slideShowInterval; }
            set { SetProperty(ref _slideShowInterval, value); }
        }

        /// <summary>
        /// カーソルでスライドを止める.
        /// </summary>
        [PropertyMember]
        public bool IsCancelSlideByMouseMove
        {
            get { return _isCancelSlideByMouseMove; }
            set { SetProperty(ref _isCancelSlideByMouseMove, value); }
        }

        /// <summary>
        /// ループ再生フラグ
        /// </summary>
        [PropertyMember]
        public bool IsSlideShowByLoop
        {
            get { return _isSlideShowByLoop; }
            set { SetProperty(ref _isSlideShowByLoop ,value); } 
        }

    }
}