using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace NeeView.Windows
{
    public class ChromeWindowAssistant
    {
        private Window _window;
        private WindowChromeAccessor _windowChrome;
        private WindowStateManager _windowStateManager;

        public ChromeWindowAssistant(Window window)
        {
            _window = window;
            _windowChrome = new WindowChromeAccessor(_window);
            _windowStateManager = new WindowStateManager(_window, new WindowStateManagerDependency(_windowChrome, TabletModeWatcher.Current));
        }


        public WindowChromeAccessor WindowChrome => _windowChrome;

        public WindowStateManager WindowStateManager => _windowStateManager;


        public void Attach()
        {
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
            // CaptionBar があればその高さを設定
            var captionBar = _window.Template.FindName("PART_CaptionBar", _window) as CaptionBar;
            if (captionBar != null)
            {
                captionBar.SetBinding(CaptionBar.MinHeightProperty, new Binding(nameof(WindowChromeAccessor.CaptionHeight)) { Source = _windowChrome });
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
        }
    }
}
