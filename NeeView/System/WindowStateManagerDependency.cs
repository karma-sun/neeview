using NeeView.ComponentModel;
using NeeView.Windows;
using System.ComponentModel;

namespace NeeView
{
    public class WindowStateManagerDependency : IWindowStateManagerDependency
    {
        private WindowChromeAccessor _chrome;
        private TabletModeWatcher _tabletModeWatcher;
        private WeakBindableBase<WindowConfig> _windowConfig;

        public WindowStateManagerDependency(WindowChromeAccessor chrome, TabletModeWatcher tabletModeWatcher)
        {
            _chrome = chrome;
            _tabletModeWatcher = tabletModeWatcher;

            _windowConfig = new WeakBindableBase<WindowConfig>(Config.Current.Window);
            _windowConfig.AddPropertyChanged(nameof(WindowConfig.MaximizeWindowGapWidth), (s, e) =>
            {
                _chrome.MaximizeWindowGapWidth = Config.Current.Window.MaximizeWindowGapWidth;
            });
        }

        public bool IsTabletMode => _tabletModeWatcher.IsTabletMode;

        public double MaximizedWindowThickness => Config.Current.Window.MaximizeWindowGapWidth;


        public void ResumeWindowChrome()
        {
            _chrome.IsSuspended = false;
        }

        public void SuspendWindowChrome()
        {
            _chrome.IsSuspended = true;
        }
    }
}
