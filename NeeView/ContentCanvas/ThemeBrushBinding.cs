using NeeView.Runtime.LayoutPanel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView
{
    public class ThemeBrushBinding
    {
        public static PanelColorToBrushConverter _captionBackgroundConverter;
        public static PanelColorTobrushMultiConverter _captionForegroundConverter;

        static ThemeBrushBinding()
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
        }

        private FrameworkElement _element;

        public ThemeBrushBinding(FrameworkElement element)
        {
            _element = element;
        }


        public void SetMenuBackgroundBinding(DependencyProperty property)
        {
            var binding = new Binding(nameof(ThemeConfig.MenuColor))
            {
                Source = Config.Current.Theme,
                Converter = _captionBackgroundConverter,
            };
            _element.SetBinding(property, binding);
        }

        public void SetMenuForegroundBinding(DependencyProperty property)
        {
            var multiBinding = new MultiBinding() { Converter = _captionForegroundConverter };
            multiBinding.Bindings.Add(new Binding(nameof(ThemeConfig.MenuColor)) { Source = Config.Current.Theme });
            multiBinding.Bindings.Add(new Binding(nameof(LayoutPanelWindow.IsActive)) { Source = Window.GetWindow(_element) });
            _element.SetBinding(property, multiBinding);
        }

        public void SetPanelBackgroundBinding(DependencyProperty property)
        {
            var background = new Binding(nameof(SidePanelProfile.BackgroundBrushRaw)) { Source = SidePanelProfile.Current };
            _element.SetBinding(property, background);
        }
    }
}
