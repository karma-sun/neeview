using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
    public class ChromeWindowAssistantTools
    {
        public static bool GetAttached(DependencyObject obj)
        {
            return (bool)obj.GetValue(AttachedProperty);
        }

        public static void SetAttached(DependencyObject obj, bool value)
        {
            obj.SetValue(AttachedProperty, value);
        }

        public static readonly DependencyProperty AttachedProperty =
            DependencyProperty.RegisterAttached("Attached", typeof(bool), typeof(ChromeWindowAssistantTools), new PropertyMetadata(false, AttachedPropertyChanged));

        private static void AttachedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window && (bool)e.NewValue)
            {
                var assistant = new ChromeWindowStyleAssistant(window);
                assistant.Attach();
            }
        }


        public static bool GetIsSystemMenuEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSystemMenuEnabledProperty);
        }

        public static void SetIsSystemMenuEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSystemMenuEnabledProperty, value);
        }

        public static readonly DependencyProperty IsSystemMenuEnabledProperty =
            DependencyProperty.RegisterAttached("IsSystemMenuEnabled", typeof(bool), typeof(ChromeWindowAssistantTools), new PropertyMetadata(false));

    }

    public class ChromeWindowStyleAssistant
    {
        private Window _window;
        private WindowChromeAccessor _windowChrome;
        private WindowStateManager _windowStateManager;

        public ChromeWindowStyleAssistant(Window window)
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




    public class DoubleToDpiScaledThicknessConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is double value && values[1] is Visual visual)
            {
                var dpi = VisualTreeHelper.GetDpi(visual);
                var x = value / dpi.DpiScaleX;
                var y = value / dpi.DpiScaleY;
                return new Thickness(x, y, x, y);
            }

            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
