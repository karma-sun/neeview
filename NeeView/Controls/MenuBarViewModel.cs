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


        public MenuBarViewModel(FrameworkElement control, MenuBar model)
        {
            _model = model;
            _model.CommandGestureChanged += (s, e) => MainMenu?.UpdateInputGestureText();

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

        public Development Development => Development.Current;


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
            var menu = _model.MainMenuSource.CreateMenu();

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
