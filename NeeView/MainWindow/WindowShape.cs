using NeeLaboratory.ComponentModel;
using NeeView.Windows;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shell;

namespace NeeView
{
    /// <summary>
    /// WindowStateEx
    /// </summary>
    public enum WindowStateEx
    {
        None, // 未設定
        Normal,
        Minimized,
        Maximized,
        FullScreen,
    }


    [Obsolete]
    public enum WindowChromeFrameV1
    {
        None,
        WindowFrame,
        Line,
    }

    /// <summary>
    /// WindowChromeFrame Type
    /// </summary>
    public enum WindowChromeFrame
    {
        [AliasName("@EnumWindowChromeFrameNone")]
        None,

        [AliasName("@EnumWindowChromeFrameWindowFrame")]
        WindowFrame,
    }


    /// <summary>
    /// WindowShape Selector.
    /// 標準のウィンドウ状態にフルスクリーン状態を加えたもの
    /// </summary>
    public class WindowShape : BindableBase
    {
        static WindowShape() => Current = new WindowShape();
        public static WindowShape Current { get; }

        #region NativeApi

        internal static class NativeMethods
        {
            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            public static extern IntPtr FindWindow(string className, string windowTitle);

            [DllImport("user32.dll")]
            public static extern int SetForegroundWindow(IntPtr hwnd);
        }

        #endregion


        private Window _window;
        private WindowStateEx _oldState;
        private WindowStateEx _nowState;
        private WindowStateEx _nextState;
        private Thickness _windowBorderThickness;
        private bool _isFullScreen;
        private bool _isEnabled;
        private bool _isProcessing;
        private MainWindowChromeAccessor _windowChromeAccessor;


        private WindowShape()
        {
            _window = MainWindow.Current;

            _windowChromeAccessor = new MainWindowChromeAccessor(_window);

            Config.Current.Window.AddPropertyChanged(nameof(WindowConfig.WindowChromeFrame), (s, e) =>
            {
                Refresh();
            });

            Config.Current.Window.AddPropertyChanged(nameof(WindowConfig.IsCaptionVisible), (s, e) =>
            {
                Refresh();
            });

            Config.Current.Window.AddPropertyChanged(nameof(WindowConfig.IsTopmost), (s, e) =>
            {
                _window.Topmost = Config.Current.Window.IsTopmost;
            });

            Config.Current.Window.AddPropertyChanged(nameof(WindowConfig.State), (s, e) =>
            {
                if (IsEnabled && !IsProcessing)
                {
                    Debug.Assert(Config.Current.Window.State != _nowState);
                    SetWindowState(Config.Current.Window.State);
                }
            });


            if (_window.Topmost != Config.Current.Window.IsTopmost)
            {
                _window.Topmost = Config.Current.Window.IsTopmost;
            }
        }

        private void ValidateWindowState()
        {
            if (Config.Current.Window.State != WindowStateEx.None) return;

            switch (_window.WindowState)
            {
                case WindowState.Normal:
                    Config.Current.Window.State = WindowStateEx.Normal;
                    break;
                case WindowState.Minimized:
                    Config.Current.Window.State = WindowStateEx.Minimized;
                    break;
                case WindowState.Maximized:
                    Config.Current.Window.State = WindowStateEx.Maximized;
                    break;
            }
        }



        public event EventHandler StateChanged;



        public WindowStateEx OldWindowState => _oldState;

        public WindowStateEx NextWindowState => _nextState;

        public Thickness WindowBorderThickness
        {
            get { return _windowBorderThickness; }
            set { if (_windowBorderThickness != value) { _windowBorderThickness = value; RaisePropertyChanged(); } }
        }

        public bool CanCaptionVisible
        {
            get => Config.Current.Window.IsCaptionVisible && !IsFullScreen;
        }

        public bool IsFullScreen
        {
            get { return _isFullScreen; }
            private set
            {
                if (SetProperty(ref _isFullScreen, value))
                {
                    RaisePropertyChanged(nameof(CanCaptionVisible));
                }
            }
        }

        public bool IsProcessing
        {
            get { return _isProcessing; }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (SetProperty(ref _isEnabled, value))
                {
                    Refresh();
                }
            }
        }



        /// <summary>
        /// ウィンドウ状態イベントの初期設定
        /// </summary>
        public void InitializeStateChangeAction()
        {
            _window.StateChanged += Window_StateChanged;
        }

        /// <summary>
        /// ウィンドウ状態イベント処理
        /// </summary>
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (!this.IsEnabled) return;

            if (this.IsProcessing)
            {
                //Debug.WriteLine($"Skip: {_window.WindowState}");
                return;
            }

            switch (_window.WindowState)
            {
                case WindowState.Normal:
                    ToNormal();
                    break;
                case WindowState.Minimized:
                    ToMinimized();
                    break;
                case WindowState.Maximized:
                    ToMaximizedMaybe();
                    break;
            }
        }

        public void UpdateWindowBorderThickness()
        {
            // NOTE: Windows7 only
            if (!Environment.IsWindows7) return;
            var dipScale = (_window is IHasDpiScale hasDpiScale) ? hasDpiScale.GetDpiScale() : new DpiScale(1.0, 1.0);

            if (_windowChromeAccessor.IsEnabled && _window.WindowState != WindowState.Maximized && Config.Current.Window.WindowChromeFrame == WindowChromeFrame.WindowFrame)
            {
                var x = 1.0 / dipScale.DpiScaleX;
                var y = 1.0 / dipScale.DpiScaleY;
                this.WindowBorderThickness = new Thickness(x, y, x, y);
            }
            else
            {
                this.WindowBorderThickness = default;
            }
        }

        public void ToggleCaptionVisible()
        {
            Config.Current.Window.IsCaptionVisible = !Config.Current.Window.IsCaptionVisible;
        }

        public bool ToggleTopmost()
        {
            Config.Current.Window.IsTopmost = !Config.Current.Window.IsTopmost;
            return Config.Current.Window.IsTopmost;
        }

        public void ToggleFullScreen()
        {
            SetFullScreen(!IsFullScreen);
        }

        /// <summary>
        /// ウィンドウを最前列に移動
        /// </summary>
        public void OneTopmost()
        {
            var temp = _window.Topmost;
            _window.Topmost = true;
            _window.Topmost = temp;
        }

        /// <summary>
        /// 状態変更
        /// </summary>
        /// <param name="state"></param>
        private void SetWindowState(WindowStateEx state)
        {
            switch (state)
            {
                default:
                case WindowStateEx.Normal:
                    ToNormal();
                    break;
                case WindowStateEx.Minimized:
                    ToMinimized();
                    break;
                case WindowStateEx.Maximized:
                    ToMaximized();
                    break;
                case WindowStateEx.FullScreen:
                    ToFullScreen();
                    break;
            }
        }

        /// <summary>
        /// 現在の状態を記憶
        /// </summary>
        /// <param name="state"></param>
        private void UpdateState(WindowStateEx state)
        {
            bool isChanged = Config.Current.Window.State != state;

            _oldState = Config.Current.Window.State;
            _nowState = state;
            Config.Current.Window.State = state;
            this.IsFullScreen = state == WindowStateEx.FullScreen;

            if (state == WindowStateEx.Normal || state == WindowStateEx.Maximized)
            {
                Config.Current.Window.LastState = state;
            }

            if (isChanged) StateChanged?.Invoke(this, null);
        }


        /// <summary>
        /// 処理開始
        /// </summary>
        private void BeginEdit()
        {
            Debug.Assert(_isProcessing == false);
            _isProcessing = true;
        }

        /// <summary>
        /// 処理終了
        /// </summary>
        private void EndEdit()
        {
            _isProcessing = false;
        }

        /// <summary>
        /// タスクバーを手前に表示しし直す処理 (for Windows7)
        /// </summary>
        private void RecoveryTaskBar()
        {
            if (!Environment.IsWindows7 || Config.Current.Window.State != WindowStateEx.FullScreen) return;

            ////Debug.WriteLine("Recovery TaskBar");

            //_window.Visibility = Visibility.Hidden;
            //_window.Visibility = Visibility.Visible;

            IntPtr hTaskbarWnd = NativeMethods.FindWindow("Shell_TrayWnd", null);
            NativeMethods.SetForegroundWindow(hTaskbarWnd);
            _window.Activate();
        }

        /// <summary>
        /// 通常ウィンドウにする
        /// </summary>
        private void ToNormal()
        {
            _nextState = WindowStateEx.Normal;

            //Debug.WriteLine("ToNormal");
            BeginEdit();

            _window.ResizeMode = ResizeMode.CanResize;
            _window.WindowStyle = WindowStyle.SingleBorderWindow;
            _window.WindowState = WindowState.Normal;

            SetWindowChrome(true);
            UpdateWindowBorderThickness();

            RecoveryTaskBar();

            UpdateState(WindowStateEx.Normal);
            EndEdit();
        }

        /// <summary>
        /// 最小化する
        /// </summary>
        private void ToMinimized()
        {
            _nextState = WindowStateEx.Minimized;

            //Debug.WriteLine("ToMinimimzed");
            BeginEdit();

            _window.WindowState = WindowState.Minimized;

            UpdateState(WindowStateEx.Minimized);
            EndEdit();
        }

        /// <summary>
        /// 最大化、もしくはフルスクリーンにする。
        /// 最小化からの復帰用
        /// </summary>
        private void ToMaximizedMaybe()
        {
            //Debug.WriteLine("ToMaximizedMaybe");
            if (Config.Current.Window.State == WindowStateEx.Minimized && _oldState == WindowStateEx.FullScreen)
            {
                ToFullScreen();
            }
            else
            {
                ToMaximized();
            }
        }

        /// <summary>
        /// 最大化する
        /// </summary>
        private void ToMaximized()
        {
            _nextState = WindowStateEx.Maximized;

            ////Debug.WriteLine("ToMaximized");
            BeginEdit();

            ToMaximizeInner();

            SetWindowChrome(false);
            UpdateWindowBorderThickness();

            UpdateState(WindowStateEx.Maximized);
            EndEdit();
        }

        private void ToMaximizeInner()
        {
            var topmost = _window.Topmost;

            _window.Topmost = false;
            _window.WindowStyle = WindowStyle.SingleBorderWindow;
            _window.ResizeMode = ResizeMode.CanResize;
            _window.WindowState = WindowState.Maximized;
            _window.Topmost = topmost;
        }

        /// <summary>
        /// フルスクリーンにする
        /// </summary>
        private void ToFullScreen()
        {
            _nextState = WindowStateEx.FullScreen;

            ////Debug.WriteLine("ToFullScreen");
            BeginEdit();

            if (Config.Current.Window.IsFullScreenWithTaskBar)
            {
                ToMaximizeInner();
                SetWindowChrome(false);
            }
            else
            {
                ResetWindowChrome();
                ToFullScreenInner();
            }

            UpdateWindowBorderThickness();

            UpdateState(WindowStateEx.FullScreen);
            EndEdit();
        }

        private void ToFullScreenInner()
        {
            // NOTE: Windows7やタブレットモードでフルスクリーンでもタスクバーが隠れないことがある現象の対処。Windowsショートカットでのモニタ間移動の障害になるため用途を限定する
            if (Environment.IsWindows7 || TabletModeWatcher.Current.IsTabletMode)
            {
                _window.ResizeMode = ResizeMode.CanMinimize;
            }

            if (_window.WindowState == WindowState.Maximized && _window.WindowStyle != WindowStyle.None)
            {
                _window.WindowState = WindowState.Normal;
            }
            _window.WindowStyle = WindowStyle.None;
            _window.WindowState = WindowState.Maximized;
        }

        /// <summary>
        /// フルスクリーン状態のON/OFF
        /// </summary>
        /// <param name="isFullScreen"></param>
        public void SetFullScreen(bool isFullScreen)
        {
            if (isFullScreen && Config.Current.Window.State != WindowStateEx.FullScreen)
            {
                ToFullScreen();
            }
            else if (!isFullScreen && Config.Current.Window.State == WindowStateEx.FullScreen)
            {
                if (Config.Current.Window.LastState == WindowStateEx.Maximized || TabletModeWatcher.Current.IsTabletMode)
                {
                    ToMaximized();
                }
                else
                {
                    ToNormal();
                }
            }
        }

        /// <summary>
        /// WindowChromeの適用
        /// </summary>
        /// <param name="isGlassFrameEnabled">GlassFrameの有効設定</param>
        private void SetWindowChrome(bool isGlassFrameEnabled)
        {
            if (Config.Current.Window.IsCaptionVisible)
            {
                _windowChromeAccessor.IsEnabled = false;
            }
            else
            {
                _windowChromeAccessor.UpdateGlassFrameThickness(isGlassFrameEnabled);
                _windowChromeAccessor.IsEnabled = true;
            }
        }

        /// <summary>
        /// WindowChromeの解除
        /// </summary>
        private void ResetWindowChrome()
        {
            _windowChromeAccessor.IsEnabled = false;
        }

        /// <summary>
        /// 状態を最新にする
        /// </summary>
        public void Refresh()
        {
            if (!this.IsEnabled) return;

            ValidateWindowState();

            _window.Topmost = Config.Current.Window.IsTopmost;
            _isFullScreen = Config.Current.Window.State == WindowStateEx.FullScreen;
            SetWindowState(Config.Current.Window.State);
            UpdateWindowBorderThickness();
            RaisePropertyChanged(null);
        }


        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public WindowStateEx State { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsCaptionVisible { get; set; }

            [DataMember]
            public bool IsTopMost { get; set; }

            [DataMember]
            public bool IsFullScreenWithTaskBar { get; set; }

            [DataMember, DefaultValue(8.0)]
            public double MaximizeWindowGapWidth { get; set; }

            [DataMember]
            public WindowStateEx LastState { get; set; }


            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            public Memento Clone()
            {
                return (Memento)this.MemberwiseClone();
            }

            public void RestoreConfig(Config config)
            {
                config.Window.IsTopmost = IsTopMost;
                config.Window.IsCaptionVisible = IsCaptionVisible;
                config.Window.IsFullScreenWithTaskBar = IsFullScreenWithTaskBar;
                config.Window.MaximizeWindowGapWidth = MaximizeWindowGapWidth;
                config.Window.State = State;
                config.Window.LastState = LastState;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.State = Config.Current.Window.State;
            memento.LastState = Config.Current.Window.LastState;
            memento.IsCaptionVisible = Config.Current.Window.IsCaptionVisible;
            memento.IsTopMost = Config.Current.Window.IsTopmost;
            memento.IsFullScreenWithTaskBar = Config.Current.Window.IsFullScreenWithTaskBar;
            memento.MaximizeWindowGapWidth = Config.Current.Window.MaximizeWindowGapWidth;

            return memento;
        }

        #endregion
    }
}
