using System;
using System.Linq;
using System.Windows;

namespace NeeView
{

    public interface IWindowStateControllable
    {
        void ToggleMinimize();
        void ToggleMaximize();
        void ToggleFullScreen();
    }

    public class WindowStateController
    {
        public IWindowStateControllable _defaultWindow;

        public WindowStateController(IWindowStateControllable defaultWindow)
        {
            if (defaultWindow is null) throw new ArgumentNullException();

            _defaultWindow = defaultWindow;
        }

        public void ToggleMinimize(object sender)
        {
            GetWindow(sender)?.ToggleMinimize();
        }

        public void ToggleMaximize(object sender)
        {
            GetWindow(sender)?.ToggleMaximize();
        }

        public void ToggleFullScreen(object sender)
        {
            GetWindow(sender)?.ToggleFullScreen();
        }

        // NOTE: no use sender
        private IWindowStateControllable GetWindow(object sender)
        {
            var window = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive) as IWindowStateControllable;
            return window ?? _defaultWindow;
        }

    }
}
