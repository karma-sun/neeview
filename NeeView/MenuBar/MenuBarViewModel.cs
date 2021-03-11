using NeeLaboratory.ComponentModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
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
        private MainWindowCaptionEmulator _windowCaptionEmulator;


        public MenuBarViewModel(MenuBar model, FrameworkElement control)
        {
            _model = model;

            InitializeWindowCaptionEmulator(control, model.WindowStateManager);


            _model.AddPropertyChanged(nameof(MenuBar.MainMenu),
                (s, e) => RaisePropertyChanged(nameof(MainMenu)));

            WindowEnvironment.Current.AddPropertyChanged(nameof(WindowEnvironment.IsHighContrast),
                (s, e) => RaisePropertyChanged(nameof(IsHighContrast)));

        }

        public MenuBar Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        public Menu MainMenu => _model.MainMenu;

        public Window Window { get; private set; }

        public Config Config => Config.Current;

        public Dictionary<string, RoutedUICommand> BookCommands => RoutedCommandTable.Current.Commands;

        public WindowTitle WindowTitle => WindowTitle.Current;

        public bool IsHighContrast => WindowEnvironment.Current.IsHighContrast;

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
    }
}
