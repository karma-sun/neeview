using System;
using System.ComponentModel;
using System.Windows;

namespace NeeView
{
    public interface IWindowStateManagerDependency
    {
        bool IsWindows7 { get; }
        bool IsTabletMode { get; }
        double MaximizedWindowThickness { get; }

        void SuspendWindowChrome();
        void ResumeWindowChrome();
    }
    

    public class WindowStateManager
    {
        private Window _window;
        private bool _isFullScreen;
        private WindowState _resumeState;
        private bool _isProgress;

        private IWindowStateManagerDependency _dependency;


        public WindowStateManager(Window window, IWindowStateManagerDependency dependency)
        {
            _window = window;
            _dependency = dependency;

            _window.StateChanged += Window_StateChanged;
            Update();
        }


        public event EventHandler StateChanged;


        public WindowState WindowState => _window.WindowState;

        public bool IsFullScreen => _isFullScreen;

        public double MaximizedChromeBorderThickness { get; set; } = 8.0;



        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (_isProgress) return;

            Update();
        }

        public void Update()
        {
            switch (_window.WindowState)
            {
                case WindowState.Minimized:
                    ToMinimize();
                    break;
                case WindowState.Normal:
                    ToNormalize();
                    break;
                case WindowState.Maximized:
                    ToMximizeMaybe();
                    break;
            }
        }

        private void BeginEdit()
        {
            _isProgress = true;
        }

        private void EndEdit()
        {
            _isProgress = false;
            StateChanged?.Invoke(this, null);
        }


        public void ToMinimize()
        {
            BeginEdit();

            _window.ResizeMode = ResizeMode.CanResize;
            _window.WindowState = WindowState.Minimized;

            UpdateWindowChrome();

            EndEdit();
        }

        public void ToNormalize()
        {
            BeginEdit();

            _isFullScreen = false;
            _resumeState = WindowState.Normal;

            _window.ResizeMode = ResizeMode.CanResize;
            _window.WindowStyle = WindowStyle.SingleBorderWindow;
            _window.WindowState = WindowState.Normal;

            UpdateWindowChrome();

            Windows7Tools.RecoveryTaskBar(_window);

            EndEdit();
        }

        public void ToMximizeMaybe()
        {
            if (_isFullScreen)
            {
                ToFullScreen();
            }
            else
            {
                ToMaximize();
            }
        }

        public void ToMaximize()
        {
            BeginEdit();

            _isFullScreen = false;
            _resumeState = WindowState.Maximized;

            _window.ResizeMode = ResizeMode.CanResize;
            _window.WindowStyle = WindowStyle.SingleBorderWindow;
            _window.WindowState = WindowState.Maximized;

            UpdateWindowChrome();

            EndEdit();
        }

        public void ToFullScreen()
        {
            BeginEdit();

            _isFullScreen = true;

            // NOTE: Windowsショートカットによる移動ができなくなるので、Windows7とタブレットに限定する
            if (_dependency.IsWindows7 || _dependency.IsTabletMode)
            {
                _window.ResizeMode = ResizeMode.CanMinimize;
            }

            if (_window.WindowState == WindowState.Maximized) _window.WindowState = WindowState.Normal;
            _window.WindowStyle = WindowStyle.None;

            _window.WindowState = WindowState.Maximized;

            UpdateWindowChrome();

            EndEdit();
        }


        public void ToggleFullScreen()
        {
            if (_window.WindowState == WindowState.Maximized && _isFullScreen)
            {
                if (_resumeState == WindowState.Maximized)
                {
                    ToMaximize();
                }
                else
                {
                    ToNormalize();
                }
            }
            else
            {
                ToFullScreen();
            }
        }


        private void UpdateWindowChrome()
        {
            if (_window.WindowState == WindowState.Maximized && _isFullScreen)
            {
                _dependency.SuspendWindowChrome();
            }
            else
            {
                _dependency.ResumeWindowChrome();
            }
        }
    }
}
