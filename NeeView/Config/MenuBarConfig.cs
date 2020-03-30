using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class MenuBarConfig : BindableBase
    {
        private bool _isHideMenu;
        private bool _isVisibleAddressBar = true;
        private bool _isHamburgerMenu;


        // メニューを自動的に隠す
        [PropertyMember("@ParamMenuBarIsAutoHide")]
        public bool IsHideMenu
        {
            get { return _isHideMenu; }
            set { SetProperty(ref _isHideMenu, value); }
        }

        // アドレスバーON/OFF
        [PropertyMember("@ParamMenuBarIsVisibleAddressBar")]
        public bool IsVisibleAddressBar
        {
            get { return _isVisibleAddressBar; }
            set { SetProperty(ref _isVisibleAddressBar, value); }
        }

        /// <summary>
        /// ハンバーガーメニューにする
        /// </summary>
        [PropertyMember("@ParamIsHamburgerMenu")]
        public bool IsHamburgerMenu
        {
            get { return _isHamburgerMenu; }
            set { SetProperty(ref _isHamburgerMenu, value); }
        }
    }
}


