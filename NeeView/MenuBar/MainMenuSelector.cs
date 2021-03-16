using NeeLaboratory.ComponentModel;
using System.Windows.Controls;

namespace NeeView
{
    public class MainMenuSelector : BindableBase
    {
        private MainMenu _mainMenu;
        private MainMenuPlacement _mainMenuPlacement;
        private Menu _menuBarMenu;
        private Menu _addressBarMenu;


        public MainMenuSelector(MainMenu mainMenu)
        {
            _mainMenu = mainMenu;

            _mainMenu.AddPropertyChanged(nameof(MainMenu.Menu),
                (s, e) => Update());

            Config.Current.Window.AddPropertyChanged(nameof(WindowConfig.IsCaptionVisible),
                (s, e) => UpdatePlacement());

            Config.Current.MenuBar.AddPropertyChanged(nameof(MenuBarConfig.IsHamburgerMenu),
                (s, e) => UpdatePlacement());

            Config.Current.MenuBar.AddPropertyChanged(nameof(MenuBarConfig.IsAddressBarEnabled),
                (s, e) => UpdatePlacement());

            UpdatePlacement();
        }


        public MainMenuPlacement MainMenuPlacement
        {
            get { return _mainMenuPlacement; }
            private set
            {
                if (SetProperty(ref _mainMenuPlacement, value))
                {
                    _mainMenu.Update();
                }
            }
        }

        public Menu MenuBarMenu
        {
            get { return _menuBarMenu; }
            private set { SetProperty(ref _menuBarMenu, value); }
        }


        public Menu AddressBarMenu
        {
            get { return _addressBarMenu; }
            private set { SetProperty(ref _addressBarMenu, value); }
        }


        private void UpdatePlacement()
        {
            if (Config.Current.MenuBar.IsHamburgerMenu && Config.Current.Window.IsCaptionVisible && Config.Current.MenuBar.IsAddressBarEnabled)
            {
                MainMenuPlacement = MainMenuPlacement.AddressBar;
            }
            else
            {
                MainMenuPlacement = MainMenuPlacement.MenuBar;
            }
        }

        private void Update()
        {
            switch (_mainMenuPlacement)
            {
                default:
                    MenuBarMenu = null;
                    AddressBarMenu = null;
                    break;

                case MainMenuPlacement.MenuBar:
                    AddressBarMenu = null;
                    MenuBarMenu = _mainMenu.Menu;
                    break;

                case MainMenuPlacement.AddressBar:
                    MenuBarMenu = null;
                    AddressBarMenu = _mainMenu.Menu;
                    break;
            }
        }

    }


    public enum MainMenuPlacement
    {
        None,
        MenuBar,
        AddressBar,
    }

}
