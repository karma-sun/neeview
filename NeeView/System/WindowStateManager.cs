﻿using NeeLaboratory.ComponentModel;
using NeeView.Windows;
using System;
using System.ComponentModel;
using System.Windows;

namespace NeeView
{
    public interface IWindowStateManagerDependency
    {
        bool IsTabletMode { get; }
        double MaximizedWindowThickness { get; }

        void SuspendWindowChrome();
        void ResumeWindowChrome();
    }


    public class WindowStateChangedEventArgs : EventArgs
    {
        public WindowStateChangedEventArgs(WindowStateEx oldState, WindowStateEx newtate)
        {
            OldState = oldState;
            NewState = newtate;
        }

        public WindowStateEx OldState { get; set; }
        public WindowStateEx NewState { get; set; }
    }


    public class WindowStateManager : BindableBase
    {
        private Window _window;
        private WindowStateEx _previousState;
        private WindowStateEx _currentState;
        private WindowStateEx _resumeState;
        private bool _isFullScreenMode;
        private bool _isFullScreen;
        private bool _isProgress;

        private IWindowStateManagerDependency _dependency;


        public WindowStateManager(Window window, IWindowStateManagerDependency dependency)
        {
            _window = window;
            _dependency = dependency;

            _window.StateChanged += Window_StateChanged;

            Update();
        }


        public event EventHandler<WindowStateChangedEventArgs> StateChanged;
        public event EventHandler<WindowStateChangedEventArgs> StateEditing;
        public event EventHandler<WindowStateChangedEventArgs> StateEdited;

        public Window Window => _window;
        public WindowStateEx CurrentState => _currentState;
        public WindowStateEx PreviousState => _previousState;

        public WindowStateEx ResumeState
        {
            get => _resumeState;
            set => _resumeState = value;
        }

        public bool IsFullScreen
        {
            get { return _isFullScreen; }
            private set { SetProperty(ref _isFullScreen, value); }
        }



        private void UpdateIsFullScreen()
        {
            IsFullScreen = _isFullScreenMode && _window.WindowState == WindowState.Maximized;
        }

        private void SetFullScreenMode(bool isEnabled)
        {
            _isFullScreenMode = isEnabled;
            UpdateIsFullScreen();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (_isProgress) return;

            Update();
        }

        public void Update()
        {
            UpdateIsFullScreen();

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

        public WindowStateEx GetWindowState()
        {
            if (IsFullScreen)
            {
                return WindowStateEx.FullScreen;
            }
            else
            {
                switch (_window.WindowState)
                {
                    case WindowState.Minimized:
                        return WindowStateEx.Minimized;
                    case WindowState.Maximized:
                        return WindowStateEx.Maximized;
                    case WindowState.Normal:
                        return WindowStateEx.Normal;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        public void SetWindowState(WindowStateEx state)
        {
            if (_isProgress) return;

            switch (state)
            {
                default:
                case WindowStateEx.Normal:
                    ToNormalize();
                    break;
                case WindowStateEx.Minimized:
                    ToMinimize();
                    break;
                case WindowStateEx.Maximized:
                    ToMaximize();
                    break;
                case WindowStateEx.FullScreen:
                    ToFullScreen();
                    break;
            }
        }


        private void BeginEdit(WindowStateChangedEventArgs editArgs)
        {
            StateEditing?.Invoke(this, editArgs);
            _isProgress = true;
        }

        private void EndEdit(WindowStateChangedEventArgs editArgs)
        {
            var nowState = GetWindowState();
            if (nowState != _currentState)
            {
                _previousState = _currentState;
                _currentState = nowState;
                StateChanged?.Invoke(this, new WindowStateChangedEventArgs(_previousState, _currentState));
            }

            _isProgress = false;
            StateEdited?.Invoke(this, editArgs);
        }


        public void ToMinimize()
        {
            if (_isProgress) return;

            var editArgs = new WindowStateChangedEventArgs(_currentState, WindowStateEx.Minimized);
            BeginEdit(editArgs);

            _window.ResizeMode = ResizeMode.CanResize;
            _window.WindowStyle = WindowStyle.None;
            _window.WindowState = WindowState.Minimized;

            EndEdit(editArgs);
        }

        public void ToNormalize()
        {
            if (_isProgress) return;

            var editArgs = new WindowStateChangedEventArgs(_currentState, WindowStateEx.Normal);
            BeginEdit(editArgs);

            SetFullScreenMode(false);
            _resumeState = WindowStateEx.Normal;

            _window.ResizeMode = ResizeMode.CanResize;
            _window.WindowStyle = WindowStyle.None;
            _window.WindowState = WindowState.Normal;

            UpdateWindowChrome();

            if (_currentState == WindowStateEx.FullScreen || _currentState == WindowStateEx.Maximized)
            {
                Windows7Tools.RecoveryTaskBar(_window);
            }

            EndEdit(editArgs);
        }

        public void ToMximizeMaybe()
        {
            if (_isFullScreenMode)
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
            if (_isProgress) return;

            var editArgs = new WindowStateChangedEventArgs(_currentState, WindowStateEx.Maximized);
            BeginEdit(editArgs);

            SetFullScreenMode(false);
            _resumeState = WindowStateEx.Maximized;

            // NOTE: タスクバー自動非表示モードでは直接ウィンドウサイズを変更する
            if (TaskBarNativeTools.IsAutoHide())
            {
                _window.ResizeMode = ResizeMode.CanResize;
                _window.WindowStyle = WindowStyle.None;
                _window.WindowState = WindowState.Maximized;
                WindowSizeTools.SetMaximizedWindowPos(_window);
            }
            else
            {
                _window.ResizeMode = ResizeMode.CanResize;
                _window.WindowStyle = WindowStyle.SingleBorderWindow;
                _window.WindowState = WindowState.Maximized;
            }

            UpdateWindowChrome();

            EndEdit(editArgs);
        }

        public void ToFullScreen()
        {
            if (_isProgress) return;

            var editArgs = new WindowStateChangedEventArgs(_currentState, WindowStateEx.FullScreen);
            BeginEdit(editArgs);

            // NOTE: Windowsショートカットによる移動ができなくなるので、Windows7とタブレットに限定する
            if (Windows7Tools.IsWindows7 || _dependency.IsTabletMode)
            {
                _window.ResizeMode = ResizeMode.CanMinimize;
            }

            if (_window.WindowState == WindowState.Maximized)
            {
                // NOTE: タブレットモードでは通常ウィンドウを経由できないので直接ウィンドウサイズを変更する
                if (_dependency.IsTabletMode)
                {
                    WindowSizeTools.SetFullScreenWindowPos(_window);
                }
                else
                {
                    _window.WindowState = WindowState.Normal;
                }
            }

            _window.WindowStyle = WindowStyle.None;
            _window.WindowState = WindowState.Maximized;

            SetFullScreenMode(true);

            UpdateWindowChrome();

            EndEdit(editArgs);
        }


        public void ToggleMinimize()
        {
            if (_window.WindowState != WindowState.Minimized)
            {
                SystemCommands.MinimizeWindow(_window);
            }
            else
            {
                SystemCommands.RestoreWindow(_window);
            }
        }

        public void ToggleMaximize()
        {
            if (_window.WindowState != WindowState.Maximized)
            {
                SystemCommands.MaximizeWindow(_window);
            }
            else
            {
                SystemCommands.RestoreWindow(_window);
            }
        }

        public void ToggleFullScreen()
        {
            if (IsFullScreen)
            {
                ReleaseFullScreen();
            }
            else
            {
                ToFullScreen();
            }
        }

        public void ReleaseFullScreen()
        {
            if (!IsFullScreen) return;

            if (_resumeState == WindowStateEx.Maximized || _dependency.IsTabletMode)
            {
                ToMaximize();
            }
            else
            {
                ToNormalize();
            }
        }

        public void SetFullScreen(bool isFullScreen)
        {
            if (isFullScreen)
            {
                ToFullScreen();
            }
            else
            {
                ReleaseFullScreen();
            }
        }


        private void UpdateWindowChrome()
        {
            if (IsFullScreen)
            {
                _dependency.SuspendWindowChrome();
            }
            else
            {
                _dependency.ResumeWindowChrome();
            }
        }


        public WindowPlacement StoreWindowPlacement(bool withAeroSnap)
        {
            return WindowPlacementTools.StoreWindowPlacement(_window, withAeroSnap).WithIsFullScreeen(IsFullScreen);
        }

        public void RestoreWindowPlacement(WindowPlacement placement)
        {
            if (placement.IsFullScreen)
            {
                ToFullScreen();
            }

            WindowPlacementTools.RestoreWindowPlacement(_window, placement);
        }
    }
}
