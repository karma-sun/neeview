using NeeLaboratory.ComponentModel;
using NeeView.Windows;
using NeeView.Windows.Property;

namespace NeeView
{
    public class MainViewConfig : BindableBase
    {
        private bool _isFloating;
        private bool _isTopmost;
        private bool _isHideTitleBar;


        [PropertyMember]
        public bool IsFloating
        {
            get { return _isFloating; }
            set { SetProperty(ref _isFloating, value); }
        }

        [PropertyMember]
        public bool IsTopmost
        {
            get { return _isTopmost; }
            set { SetProperty(ref _isTopmost, value); }
        }

        [PropertyMember]
        public bool IsHideTitleBar
        {
            get { return _isHideTitleBar; }
            set { SetProperty(ref _isHideTitleBar, value); }
        }


        /// <summary>
        /// 復元ウィンドウ座標
        /// </summary>
        [PropertyMapIgnore]
        [ObjectMergeReferenceCopy]
        public WindowPlacement WindowPlacement { get; set; } = WindowPlacement.None;
    }
}


