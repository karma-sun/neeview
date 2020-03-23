using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class MenuBarConfig : BindableBase
    {

        private bool _isHamburgerMenu;

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


