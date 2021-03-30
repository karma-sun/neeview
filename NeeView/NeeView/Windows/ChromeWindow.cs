using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace NeeView.Windows
{
    public class ChromeWindowStyleAssistant
    {
        private Window _window;
        private WindowChromeAccessor _windowChrome;
        private WindowBorder _windowBorder;

        public ChromeWindowStyleAssistant(Window window)
        {
            _window = window;
            _windowChrome = new WindowChromeAccessor(_window);
            _windowBorder = new WindowBorder(_window, _windowChrome);
        }


        public WindowChromeAccessor WindowChrome => _windowChrome;


        public void Attach()
        {
            Config.Current.Window.PropertyChanged += WindowConfig_PropertyChanged;

            _window.SourceInitialized += Window_SourceInitialized;
            _window.Loaded += Window_Loaded;
            _window.Closed += Window_Closed;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            _windowChrome.IsEnabled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var windowBorder = _window.Template.FindName("WindowBorder", _window) as Border;
            if (windowBorder != null)
            {
                windowBorder.SetBinding(Border.BorderThicknessProperty, new Binding(nameof(WindowBorder.Thickness)) { Source = _windowBorder });
            }

            // CaptionBar があればその高さを設定
            var captionBar = _window.Template.FindName("PART_CaptionBar", _window) as CaptionBar;
            if (captionBar != null)
            {
                captionBar.SetBinding(CaptionBar.MinHeightProperty, new Binding(nameof(WindowChromeAccessor.CaptionHeight)) { Source = _windowChrome });
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Config.Current.Window.PropertyChanged -= WindowConfig_PropertyChanged;
        }

        private void WindowConfig_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WindowConfig.MaximizeWindowGapWidth))
            {
                _windowChrome.MaximizeWindowGapWidth = Config.Current.Window.MaximizeWindowGapWidth;
            }
        }
    }
}
