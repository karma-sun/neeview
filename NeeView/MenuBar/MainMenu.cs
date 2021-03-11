using NeeLaboratory.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView
{
    public class MainMenu : BindableBase
    {
        static MainMenu() => Current = new MainMenu();
        public static MainMenu Current { get; }


        private MenuTree _mainMenuSource;
        private Menu _menu;

#if DEBUG
        private DebugMenu _debugMenu = new DebugMenu();
#endif


        private MainMenu()
        {
            _mainMenuSource = MenuTree.CreateDefault();

            Config.Current.MenuBar.AddPropertyChanged(nameof(MenuBarConfig.IsHamburgerMenu),
                (s, e) => Update());

            RoutedCommandTable.Current.Changed +=
                (s, e) => UpdateInputGestureText();

            Update();
        }


        public MenuTree MenuSource => _mainMenuSource;

        public Menu Menu
        {
            get { return _menu; }
            set { SetProperty(ref _menu, value); }
        }

        public bool IsHamburgerMenu { get; private set; }


        private void Update()
        {
            var style = new Style(typeof(MenuItem));
            var dataTrigger = new DataTrigger() { Binding = new Binding(nameof(WindowEnvironment.IsHighContrast)) { Source = WindowEnvironment.Current }, Value = false };
            dataTrigger.Setters.Add(new Setter(MenuItem.ForegroundProperty, SystemColors.ControlTextBrush));
            style.Triggers.Add(dataTrigger);

            this.IsHamburgerMenu = Config.Current.MenuBar.IsHamburgerMenu;
            this.Menu = CreateMainMenu(this.IsHamburgerMenu, style);
            this.Menu.UpdateInputGestureText();

            BindingOperations.SetBinding(Menu, Menu.BackgroundProperty, new Binding(nameof(System.Windows.Controls.Menu.Background)) { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ContentControl), 1) });
            BindingOperations.SetBinding(Menu, Menu.ForegroundProperty, new Binding(nameof(System.Windows.Controls.Menu.Foreground)) { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ContentControl), 1) });
        }

        public void UpdateInputGestureText()
        {
            Menu?.UpdateInputGestureText();
        }

        private Menu CreateMainMenu(bool isHamburgerMenu, Style style)
        {
            var items = _mainMenuSource.CreateMenuItems();
#if DEBUG
            items.Add(_debugMenu.CreateDevMenuItem());
#endif

            var menu = new Menu();
            if (isHamburgerMenu)
            {
                IsHamburgerMenu = true;

                var image = new Image();
                image.Width = 18;
                image.Height = 18;
                image.Margin = new Thickness(0, 2, 0, 2);
                image.GetThemeBinder().SetMenuIconBinding(Image.SourceProperty);
                image.SetBinding(Image.OpacityProperty, new Binding(nameof(Window.IsActive)) { Source = MainWindow.Current, Converter = new BooleanToOpacityConverter() });

                var topMenu = new MenuItem();
                topMenu.Header = image;
                foreach (var item in items)
                {
                    topMenu.Items.Add(item);
                }
                menu.Items.Add(topMenu);
            }
            else
            {
                menu.Margin = new Thickness(0, 0, 40, 0);
                foreach (var item in items)
                {
                    menu.Items.Add(item);
                }
            }

            // サブメニューのColorを固定にする
            if (style != null)
            {
                foreach (MenuItem item in menu.Items)
                {
                    foreach (MenuItem subItem in item.Items.OfType<MenuItem>())
                    {
                        subItem.Style = style;
                    }
                }
            }

            return menu;
        }

    }
}
