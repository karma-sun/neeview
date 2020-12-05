using NeeLaboratory.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// MenuBar : ViewModel
    /// </summary>
    public class MenuBarViewModel : BindableBase
    {
        private MenuBar _model;
        private Menu _mainMenu;
        private MainWindowCaptionEmulator _windowCaptionEmulator;
        private bool _isHighContrast = SystemParameters.HighContrast;

#if DEBUG
        private DebugMenu _debugMenu = new DebugMenu();
#endif

        public MenuBarViewModel(FrameworkElement control, MenuBar model)
        {
            _model = model;
            _model.CommandGestureChanged += (s, e) => MainMenu?.UpdateInputGestureText();
            Config.Current.MenuBar.AddPropertyChanged(nameof(MenuBarConfig.IsHamburgerMenu), (s, e) => InitializeMainMenu());

            InitializeMainMenu();
            InitializeWindowCaptionEmulator(control, model.WindowStateManager);

            SystemParameters.StaticPropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SystemParameters.HighContrast))
                {
                    IsHighContrast = SystemParameters.HighContrast;
                }
            };
        }


        public MenuBar Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        public Menu MainMenu
        {
            get { return _mainMenu; }
            set { _mainMenu = value; RaisePropertyChanged(); }
        }

        public Window Window { get; private set; }

        public Config Config => Config.Current;

        public Dictionary<string, RoutedUICommand> BookCommands => RoutedCommandTable.Current.Commands;

        public WindowTitle WindowTitle => WindowTitle.Current;

        public bool IsHighContrast
        {
            get { return _isHighContrast; }
            set { SetProperty(ref _isHighContrast, value); }
        }


        public bool IsCaptionEnabled
        {
            get { return _windowCaptionEmulator.IsEnabled; }
            set
            {
                if (_windowCaptionEmulator.IsEnabled != value)
                {
                    _windowCaptionEmulator.IsEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }

        private void UpdateCaptionEnabled()
        {
            IsCaptionEnabled = !Config.Current.Window.IsCaptionVisible || Model.WindowStateManager.IsFullScreen;
        }


        private void InitializeWindowCaptionEmulator(FrameworkElement control, WindowStateManager windowStateManamger)
        {
            this.Window = System.Windows.Window.GetWindow(control);

            _windowCaptionEmulator = new MainWindowCaptionEmulator(Window, control, windowStateManamger);
            UpdateCaptionEnabled();

            Config.Current.Window.AddPropertyChanged(nameof(WindowConfig.IsCaptionVisible), (s, e) => UpdateCaptionEnabled());
            Model.WindowStateManager.AddPropertyChanged(nameof(WindowStateManager.IsFullScreen), (s, e) => UpdateCaptionEnabled());
        }

        private void InitializeMainMenu()
        {
            var style = new Style(typeof(MenuItem));
            var dataTrigger = new DataTrigger() { Binding = new Binding(nameof(IsHighContrast)), Value = false };
            dataTrigger.Setters.Add(new Setter(MenuItem.ForegroundProperty, SystemColors.ControlTextBrush));
            style.Triggers.Add(dataTrigger);

            this.MainMenu = CreateMainMenu(style);
            this.MainMenu.UpdateInputGestureText();

            BindingOperations.SetBinding(MainMenu, Menu.BackgroundProperty, new Binding(nameof(Menu.Background)) { ElementName = "MainMenuJoint" });
            BindingOperations.SetBinding(MainMenu, Menu.ForegroundProperty, new Binding(nameof(Menu.Foreground)) { ElementName = "MainMenuJoint" });
        }

        private Menu CreateMainMenu(Style style)
        {
            var items = _model.MainMenuSource.CreateMenuItems();
#if DEBUG
            items.Add(_debugMenu.CreateDevMenuItem());
#endif

            var menu = new Menu();
            if (Config.Current.MenuBar.IsHamburgerMenu)
            {
                var image = new Image();
                image.Width = 18;
                image.Height = 18;
                image.Margin = new Thickness(4, 2, 4, 2);
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
