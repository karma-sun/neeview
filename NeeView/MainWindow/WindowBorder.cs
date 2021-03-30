using NeeLaboratory.ComponentModel;
using NeeView.ComponentModel;
using NeeView.Windows;
using System.Diagnostics;
using System.Windows;

namespace NeeView
{
    public class WindowBorder : BindableBase
    {
        private Window _window;
        private WindowChromeAccessor _windowChromeAccessor;
        private Thickness _thickness;
        private WeakBindableBase<ThemeManager> _themeManager;

        public WindowBorder(Window window, WindowChromeAccessor windowChromeAccessor)
        {
            _window = window;
            _windowChromeAccessor = windowChromeAccessor;

            _window.DpiChanged +=
                (s, e) => Update();

            _window.StateChanged +=
                (s, e) => Update();

            _windowChromeAccessor.AddPropertyChanged(nameof(WindowChromeAccessor.IsEnabled),
                (s, e) => Update());

            _themeManager = new WeakBindableBase<ThemeManager>(ThemeManager.Current);
            _themeManager.AddPropertyChanged(nameof(ThemeManager.ThemeProfile),
                (s, e) => Update());

            Update();
        }

        public Thickness Thickness
        {
            get { return _thickness; }
            set { SetProperty(ref _thickness, value); }
        }

        public void Update()
        {
            if (_window.WindowState == WindowState.Minimized) return;

            if (_windowChromeAccessor.IsEnabled && _window.WindowState == WindowState.Normal && _themeManager.Model.ThemeProfile.GetColor("Window.Border", 1.0).A > 0x00)
            {
                this.Thickness = new Thickness(1.0);
            }
            else
            {
                this.Thickness = default;
            }
        }
    }
}
