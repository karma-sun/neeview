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
        private WindowCaptionEmulator _windowCaptionEmulator;

#if DEBUG
        private DebugMenu _debugMenu = new DebugMenu();
#endif

        public MenuBarViewModel(FrameworkElement control, MenuBar model)
        {
            _model = model;
            _model.CommandGestureChanged += (s, e) => MainMenu?.UpdateInputGestureText();
            _model.AddPropertyChanged(nameof(MenuBar.IsHamburgerMenu), (s, e) => InitializeMainMenu());

            InitializeMainMenu();
            InitializeWindowCaptionEmulator(control);
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
        public WindowCaptionEmulator WindowCaptionEmulator
        {
            get { return _windowCaptionEmulator; }
            set { if (_windowCaptionEmulator != value) { _windowCaptionEmulator = value; RaisePropertyChanged(); } }
        }

        public ThemeProfile ThemeProfile => ThemeProfile.Current;

        public Dictionary<CommandType, RoutedUICommand> BookCommands => RoutedCommandTable.Current.Commands;

        public WindowTitle WindowTitle => WindowTitle.Current;


        private void InitializeWindowCaptionEmulator(FrameworkElement control)
        {
            this.Window = System.Windows.Window.GetWindow(control);

            // window caption emulatr
            this.WindowCaptionEmulator = new WindowCaptionEmulator(Window, control);
            this.WindowCaptionEmulator.IsEnabled = !WindowShape.Current.IsCaptionVisible || WindowShape.Current.IsFullScreen;

            // IsCaptionVisible か IsFullScreen の変更を監視すべきだが、処理が軽いためプロパティ名の判定をしない
            WindowShape.Current.PropertyChanged +=
                (s, e) => this.WindowCaptionEmulator.IsEnabled = !WindowShape.Current.IsCaptionVisible || WindowShape.Current.IsFullScreen;
        }

        private void InitializeMainMenu()
        {
            this.MainMenu = CreateMainMenu(new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x22)));

            BindingOperations.SetBinding(MainMenu, Menu.BackgroundProperty, new Binding(nameof(Menu.Background)) { ElementName = "MainMenuJoint" });
            BindingOperations.SetBinding(MainMenu, Menu.ForegroundProperty, new Binding(nameof(Menu.Foreground)) { ElementName = "MainMenuJoint" });
        }

        private Menu CreateMainMenu(Brush foreground)
        {
            var items = _model.MainMenuSource.CreateMenuItems();
#if DEBUG
            items.Add(_debugMenu.CreateDevMenuItem());
#endif

            var menu = new Menu();
            if (_model.IsHamburgerMenu)
            {
                var converter = new PanelColorToImageSourceConverter()
                {
                    Dark = App.Current.Resources["ic_menu_24px_dark"] as ImageSource,
                    Light = App.Current.Resources["ic_menu_24px_light"] as ImageSource,
                };

                var image = new Image();
                image.Width = 18;
                image.Height = 18;
                image.Margin = new Thickness(4, 2, 4, 2);
                image.SetBinding(Image.SourceProperty, new Binding(nameof(ThemeProfile.MenuColor)) { Source = ThemeProfile, Converter = converter });
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
            foreach (MenuItem item in menu.Items)
            {
                foreach (MenuItem subItem in item.Items.OfType<MenuItem>())
                {
                    subItem.Foreground = foreground;
                }
            }

            return menu;
        }

    }
}
