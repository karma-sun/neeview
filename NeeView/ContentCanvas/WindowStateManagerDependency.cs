using NeeView.Windows;
using System.ComponentModel;

namespace NeeView
{
    public class WindowStateManagerDependency : IWindowStateManagerDependency
    {
        private WindowChromeAccessor _chrome;
        private TabletModeWatcher _tabletModeWatcher;

        public WindowStateManagerDependency(WindowChromeAccessor chrome, TabletModeWatcher tabletModeWatcher)
        {
            _chrome = chrome;
            _tabletModeWatcher = tabletModeWatcher;

            Config.Current.Window.AddPropertyChanged(nameof(WindowConfig.MaximizeWindowGapWidth), (s, e) =>
            {
                _chrome.MaximizeWindowGapWidth = Config.Current.Window.MaximizeWindowGapWidth;
            });
        }

        public bool IsTabletMode => _tabletModeWatcher.IsTabletMode;

        public bool IsWindows7 => Environment.IsWindows7;

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
