using NeeView.Windows;
using System.Windows;

namespace NeeView
{
    public class MainWindowChromeAccessor : WindowChromeAccessor
    {
        public MainWindowChromeAccessor(Window window) : base(window)
        {
            // NOTE: スライダーを操作しやすいように下辺のみリサイズ領域を狭める
            this.WindowChrome.ResizeBorderThickness = new Thickness(8, 8, 8, 4);

            Config.Current.Window.AddPropertyChanged(nameof(WindowConfig.WindowChromeFrame), (s, e) => UpdateGlassFrameThickness());
            Config.Current.Window.AddPropertyChanged(nameof(WindowConfig.MaximizeWindowGapWidth), (s, e) => UpdateMaximizeWindowGapWidth());
            UpdateGlassFrameThickness();
            UpdateMaximizeWindowGapWidth();
        }

        public void UpdateMaximizeWindowGapWidth()
        {
            this.MaximizeWindowGapWidth = Config.Current.Window.MaximizeWindowGapWidth;
        }

        public void UpdateGlassFrameThickness()
        {
            UpdateGlassFrameThickness(this.Window.WindowState != WindowState.Maximized);
        }

        public void UpdateGlassFrameThickness(bool isGlassFrameEnabled)
        {
            if (isGlassFrameEnabled && Config.Current.Window.WindowChromeFrame != WindowChromeFrame.None)
            {
                this.WindowChrome.GlassFrameThickness = new Thickness(1);
            }
            else
            {
                this.WindowChrome.GlassFrameThickness = new Thickness(0);
            }
        }

    }
}
