using System;
using System.Linq;
using System.Windows;

namespace NeeView
{
    public class WindowStateController
    {
        public IHasWindowController _defaultController;

        public WindowStateController(IHasWindowController defaultController)
        {
            if (defaultController is null) throw new ArgumentNullException();

            _defaultController = defaultController;
        }

        public void ToggleMinimize(object sender)
        {
            GetWindowController(sender)?.ToggleMinimize();
        }

        public void ToggleMaximize(object sender)
        {
            GetWindowController(sender)?.ToggleMaximize();
        }

        public void ToggleFullScreen(object sender)
        {
            GetWindowController(sender)?.ToggleFullScreen();
        }

        public void SetFullScreen(object sender, bool isFullScreen)
        {
            GetWindowController(sender).SetFullScreen(isFullScreen);
        }

        public void ToggleTopmost(object sender)
        {
            GetWindowController(sender).ToggleTopmost();
        }


        // NOTE: no use sender
        private WindowController GetWindowController(object sender)
        {
            var window = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive) as IHasWindowController;
            return (window ?? _defaultController).WindowController;
        }
    }

}
