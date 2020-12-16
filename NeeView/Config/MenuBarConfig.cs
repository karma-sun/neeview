using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class MenuBarConfig : BindableBase
    {
        private bool _isVisible;
        private bool _isHideMenu;
        private bool _isAddressBarEnabled = true;
        private bool _isHamburgerMenu;
        private bool _isHideMenuInFullscreen = true;


        [JsonIgnore]
        [PropertyMapReadOnly]
        [PropertyMember("@WordIsPanelVisible")]
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }


        // メニューを自動的に隠す
        [PropertyMember("@ParamMenuBarIsAutoHide")]
        public bool IsHideMenu
        {
            get { return _isHideMenu; }
            set { SetProperty(ref _isHideMenu, value); }
        }

        // メニューを自動的に隠す(フルスクリーン)
        [PropertyMember("@MenuBarConfig.IsHideMenuInFullscreen")]
        public bool IsHideMenuInFullscreen
        {
            get { return _isHideMenuInFullscreen; }
            set { SetProperty(ref _isHideMenuInFullscreen, value); }
        }

        // アドレスバーON/OFF
        [PropertyMember("@ParamMenuBarIsVisibleAddressBar")]
        public bool IsAddressBarEnabled
        {
            get { return _isAddressBarEnabled; }
            set { SetProperty(ref _isAddressBarEnabled, value); }
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


