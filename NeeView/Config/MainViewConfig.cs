using NeeLaboratory.ComponentModel;
using NeeView.Windows;
using NeeView.Windows.Property;

namespace NeeView
{
    public class MainViewConfig : BindableBase
    {
        private bool _isFloating;
        private bool _isTopmost;
        private bool _isAutoHide;


        [PropertyMember("@ParamMainViewIsFloating")]
        public bool IsFloating
        {
            get { return _isFloating; }
            set { SetProperty(ref _isFloating, value); }
        }

        [PropertyMember("@ParamMainViewIsTopmost")]
        public bool IsTopmost
        {
            get { return _isTopmost; }
            set { SetProperty(ref _isTopmost, value); }
        }

        [PropertyMember("@ParamMainViewIsAutoHide")]
        public bool IsAutoHide
        {
            get { return _isAutoHide; }
            set { SetProperty(ref _isAutoHide, value); }
        }


        /// <summary>
        /// 復元ウィンドウ座標
        /// </summary>
        [PropertyMapIgnore]
        [ObjectMergeReferenceCopy]
        public WindowPlacement WindowPlacement { get; set; } = WindowPlacement.None;
    }
}


