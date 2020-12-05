using NeeView.Runtime.LayoutPanel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView
{
    public class ThemeBinder
    {
        public static PanelColorToBrushConverter _captionBackgroundConverter;
        public static PanelColorTobrushMultiConverter _captionForegroundConverter;
        public static PanelColorToImageSourceConverter _menuIconConverter;

        static ThemeBinder()
        {
            _captionBackgroundConverter = new PanelColorToBrushConverter()
            {
                Dark = (SolidColorBrush)App.Current.Resources["NVMenuBackgroundDark"],
                Light = (SolidColorBrush)App.Current.Resources["NVMenuBackgroundLight"],
            };

            _captionForegroundConverter = new PanelColorTobrushMultiConverter()
            {
                Dark = (SolidColorBrush)App.Current.Resources["NVMenuForegroundDark"],
                Light = (SolidColorBrush)App.Current.Resources["NVMenuForegroundLight"],
            };

            _menuIconConverter = new PanelColorToImageSourceConverter()
            {
                Dark = App.Current.Resources["ic_menu_24px_dark"] as ImageSource,
                Light = App.Current.Resources["ic_menu_24px_light"] as ImageSource,
            };

        }

        private FrameworkElement _element;


        public ThemeBinder(FrameworkElement element)
        {
            _element = element;
        }


        public void SetMenuBackgroundBinding(DependencyProperty property)
        {
            var binding = new Binding(nameof(ThemeConfig.MenuColor)) { Source = Config.Current.Theme, Converter = _captionBackgroundConverter, };
            _element.SetBinding(property, binding);
        }

        public void SetMenuForegroundBinding(DependencyProperty property)
        {
            var multiBinding = new MultiBinding() { Converter = _captionForegroundConverter };
            multiBinding.Bindings.Add(new Binding(nameof(ThemeConfig.MenuColor)) { Source = Config.Current.Theme });
            multiBinding.Bindings.Add(new Binding(nameof(LayoutPanelWindow.IsActive)) { Source = Window.GetWindow(_element) });
            _element.SetBinding(property, multiBinding);
        }

        public void SetMenuIconBinding(DependencyProperty property)
        {
            _element.SetBinding(property, new Binding(nameof(ThemeConfig.MenuColor)) { Source = Config.Current.Theme, Converter = _menuIconConverter });
        }
    }


    public static class FrameworkElementExtensions
    {
        public static ThemeBinder GetThemeBinder(this FrameworkElement self)
        {
            return new ThemeBinder(self);
        }
    }
}
