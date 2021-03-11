using NeeLaboratory.ComponentModel;
using System.Windows.Controls;

namespace NeeView
{
    public class MainMenuSelector : BindableBase
    {
        private MainMenu _mainMenu;
        private Menu _menuBarMenu;
        private Menu _addressBarMenu;


        public MainMenuSelector(MainMenu mainMenu)
        {
            _mainMenu = mainMenu;

            _mainMenu.AddPropertyChanged(nameof(MainMenu.Menu), (s, e) => Update());
            Config.Current.Window.AddPropertyChanged(nameof(WindowConfig.IsCaptionVisible), (s, e) => Update());
            Config.Current.MenuBar.AddPropertyChanged(nameof(MenuBarConfig.IsAddressBarEnabled), (s, e) => Update());

            Update();
        }


        public Menu MenuBarMenu
        {
            get { return _menuBarMenu; }
            set { SetProperty(ref _menuBarMenu, value); }
        }


        public Menu AddressBarMenu
        {
            get { return _addressBarMenu; }
            set { SetProperty(ref _addressBarMenu, value); }
        }


        private void Update()
        {
            if (_mainMenu.IsHamburgerMenu && Config.Current.Window.IsCaptionVisible && Config.Current.MenuBar.IsAddressBarEnabled)
            {
                MenuBarMenu = null;
                AddressBarMenu = _mainMenu.Menu;
            }
            else
            {
                AddressBarMenu = null;
                MenuBarMenu = _mainMenu.Menu;
            }
        }

    }
}
