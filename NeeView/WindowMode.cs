using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// 
    /// </summary>
    public class WindowMode
    {
        private Window _Window;
        private bool _IsFullScreened;

        private bool _IsMemento;
        private WindowStyle _WindowStyle;
        private WindowState _WindowState;

        //
        public event EventHandler<bool> NotifyWindowModeChanged;

        //
        public bool IsFullScreened
        {
            get { return _IsFullScreened; }
            set { if (value) ToFullScreen(); else Restore(); }
        }

        //
        public WindowMode(Window window)
        {
            _Window = window;
        }

        //
        public void Toggle()
        {
            if (!_IsFullScreened) ToFullScreen(); else Restore();
        }

        //
        public void Cancel()
        {
            Restore();
        }

        //
        private void ToFullScreen()
        {
            if (_IsFullScreened) return;

            _WindowStyle = _Window.WindowStyle;
            _WindowState = _Window.WindowState;
            _IsMemento = true;

            _IsFullScreened = true;

            _Window.WindowStyle = WindowStyle.None;
            if (_Window.WindowState == WindowState.Maximized) _Window.WindowState = WindowState.Normal;
            _Window.WindowState = WindowState.Maximized;

            NotifyWindowModeChanged?.Invoke(this, _IsFullScreened);
        }

        //
        private void Restore()
        {
            if (!_IsFullScreened) return;

            if (_IsMemento)
            {
                _Window.WindowStyle = _WindowStyle;
                _Window.WindowState = _WindowState;
            }

            _IsFullScreened = false;

            NotifyWindowModeChanged?.Invoke(this, _IsFullScreened);
        }
    }
}
