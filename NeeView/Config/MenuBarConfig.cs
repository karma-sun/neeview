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
        private bool _isHideMenuInAutoHideMode = true;


        [JsonIgnore]
        [PropertyMapReadOnly]
        [PropertyMember]
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }


        // メニューを自動的に隠す
        [PropertyMember]
        public bool IsHideMenu
        {
            get { return _isHideMenu; }
            set { SetProperty(ref _isHideMenu, value); }
        }

        // メニューを自動的に隠す(自動非表示モード)
        [PropertyMember]
        public bool IsHideMenuInAutoHideMode
        {
            get { return _isHideMenuInAutoHideMode; }
            set { SetProperty(ref _isHideMenuInAutoHideMode, value); }
        }

        // アドレスバーON/OFF
        [PropertyMember]
        public bool IsAddressBarEnabled
        {
            get { return _isAddressBarEnabled; }
            set { SetProperty(ref _isAddressBarEnabled, value); }
        }

        /// <summary>
        /// ハンバーガーメニューにする
        /// </summary>
        [PropertyMember]
        public bool IsHamburgerMenu
        {
            get { return _isHamburgerMenu; }
            set { SetProperty(ref _isHamburgerMenu, value); }
        }
    }
}


