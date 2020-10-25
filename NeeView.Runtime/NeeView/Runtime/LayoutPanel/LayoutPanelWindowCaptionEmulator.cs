using NeeView.Windows;
using NeeView.Windows.Controls;
using System.Windows;

namespace NeeView.Runtime.LayoutPanel
{
    public class LayoutPanelWindowCaptionEmulator : WindowCaptionEmulator
    {
        private WindowStyle _windowStyle;

        public LayoutPanelWindowCaptionEmulator(Window window, FrameworkElement target) : base(window, target)
        {
        }

        protected override void OnWindowStateChange(object sender, WindowStateChangeEventArgs e)
        {
            _windowStyle = Window.WindowStyle;
            Window.WindowStyle = WindowStyle.None;
            base.OnWindowStateChange(sender, e);
        }

        protected override void OnWindowStateChanged(object sender, WindowStateChangeEventArgs e)
        {
            Window.WindowStyle = _windowStyle;
            base.OnWindowStateChanged(sender, e);
        }
    }
}
