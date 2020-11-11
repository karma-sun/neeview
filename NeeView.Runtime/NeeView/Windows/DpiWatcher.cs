using System.Diagnostics;
using System.Windows;

namespace NeeView.Windows
{
    public class DpiWatcher
    {
        private Window _window;

        public DpiWatcher(Window window)
        {
            _window = window;
            _window.DpiChanged += Window_DpiChanged;
        }


        public DpiChangedEventHandler DpiChanged;


        public DpiScale Dpi { get; private set; } = new DpiScale(1, 1);


        private void Window_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            ////Debug.WriteLine($"DPI: {e.NewDpi.DpiScaleX}");

            Dpi = e.NewDpi;
            DpiChanged?.Invoke(sender, e);
        }
    }

}
