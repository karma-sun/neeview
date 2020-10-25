using NeeView.Windows;
using NeeView.Windows.Controls;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    public class MainWindowCaptionEmulator : WindowCaptionEmulator
    {
        public MainWindowCaptionEmulator(Window window, FrameworkElement target) : base(window, target)
        {
        }


        protected override void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Config.Current.Window.IsCaptionEmulateInFullScreen && WindowShape.Current.IsFullScreen) return;

            base.OnMouseLeftButtonDown(sender, e);
        }

        protected override void OnWindowStateChange(object sender, WindowStateChangeEventArgs e)
        {
            // NOTE: 瞬時に切り替わるようにするため一時的に変更。WindowShapeSelectorで修正される
            Window.WindowStyle = WindowStyle.None;

            base.OnWindowStateChange(sender, e);
        }
    }

}
