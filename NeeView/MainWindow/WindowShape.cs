using NeeLaboratory.ComponentModel;
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

        #region Fields

        /// <summary>
        /// 管理するWindow
        /// </summary>
        private Window _window;

        /// <summary>
        /// 枠なしChrome
        /// </summary>
        private WindowChrome _chrome;

        /// <summary>
        /// 直前の状態
        /// </summary>
        private WindowStateEx _oldState;

        /// <summary>
        /// 最後の安定状態。フルスクリーン切り替えで使用される
        /// </summary>
        private WindowStateEx _lastState;

        /// <summary>
        /// Windows7?
        /// </summary>
        private bool _isWindows7;

        private WindowChromeFrame _windowChromeFrame = WindowChromeFrame.WindowFrame;
        private Thickness _windowBorderThickness;
        private bool _isCaptionVisible = true;
        private bool _isTopmost;
        private bool _isFullScreen;
        private bool _isFullScreenWithTaskBar;
        private WindowChrome _windowChrome;
        private WindowStateEx _state;
        private bool _IsEnabled;
        private bool _isProcessing;
        private double _maximizeWindowGapWidth = 8.0;

        #endregion

        #region Constructors

        private WindowShape()
        {
            _window = MainWindow.Current;

            // キャプション非表示時に適用するChrome
            _chrome = new WindowChrome();
            _chrome.CornerRadius = new CornerRadius();
            _chrome.UseAeroCaptionButtons = false;
            _chrome.CaptionHeight = 0;

            // Windows7以前の場合、フルスクリーン解除時にタスクバーを手前にする処理を追加
            _isWindows7 = Config.Current.IsWindows7();

            //
            _isTopmost = _window.Topmost;

            switch (_window.WindowState)
            {
                case WindowState.Normal:
                    this.State = WindowStateEx.Normal;
                    break;
                case WindowState.Minimized:
                    this.State = WindowStateEx.Minimized;
                    break;
                case WindowState.Maximized:
                    this.State = WindowStateEx.Maximized;
                    break;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// 状態変更イベント
        /// </summary>
        public event EventHandler StateChanged;

        #endregion

        #region Properties

        /// <summary>
        /// WindowChromeFrame property.
        /// </summary>
        [PropertyMember("@ParamWindowShapeChromeFrame")]
        public WindowChromeFrame WindowChromeFrame
        {
            get { return _windowChromeFrame; }
            set
            {
                if (_windowChromeFrame != value)
                {
                    _windowChromeFrame = value;
                    Refresh();
                }
            }
        }

        /// <summary>
        /// WindowBorderThickness property.
        /// </summary>
        public Thickness WindowBorderThickness
        {
            get { return _windowBorderThickness; }
            set { if (_windowBorderThickness != value) { _windowBorderThickness = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// IsCaptionVisible property.
        /// </summary>
        public bool IsCaptionVisible
        {
            get { return _isCaptionVisible; }
            set { if (_isCaptionVisible != value) { _isCaptionVisible = value; Refresh(); } }
        }

        public bool CanCaptionVisible
        {
            get => IsCaptionVisible && !IsFullScreen;
        }


        /// <summary>
        /// IsTopmost property.
        /// </summary>
        public bool IsTopmost
        {
            get { return _isTopmost; }
            set
            {
                if (_isTopmost != value)
                {
                    _isTopmost = value;
                    _window.Topmost = _isTopmost;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// IsFullScreen property.
        /// </summary>
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

        [PropertyMember("@ParamWindowShapeIsFullScreenWithTaskBar")]
        public bool IsFullScreenWithTaskBar
        {
            get { return _isFullScreenWithTaskBar; }
            set { SetProperty(ref _isFullScreenWithTaskBar, value); }
        }



        /// <summary>
        /// 現在のWindowChrome
        /// </summary>
        public WindowChrome WindowChrome
        {
            get { return _windowChrome; }
            private set
            {
                if (_windowChrome != value)
                {
                    _windowChrome = value;
                    WindowChrome.SetWindowChrome(_window, _windowChrome);
                    SetHook();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// State property.
        /// 現在の状態
        /// </summary>
        public WindowStateEx State
        {
            get { return _state; }
            private set
            {
                if (_state != value)
                {
                    _state = value;
                    this.IsFullScreen = _state == WindowStateEx.FullScreen;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 処理中
        /// </summary>
        public bool IsProcessing
        {
            get { return _isProcessing; }
        }

        /// <summary>
        /// IsEnabled property.
        /// </summary>
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set
            {
                if (_IsEnabled != value)
                {
                    _IsEnabled = value;
                    if (_IsEnabled) Refresh();
                    RaisePropertyChanged();
                }
            }
        }

        [PropertyRange("@ParamWindowShapeMaximizeWindowGapWidth", 0, 16, TickFrequency = 1, IsEditable = true, Tips = "@ParamWindowShapeMaximizeWindowGapWidthTips"), DefaultValue(8.0)]
        public double MaximizeWindowGapWidth
        {
            get { return _maximizeWindowGapWidth; }
            set
            {
                if (SetProperty(ref _maximizeWindowGapWidth, value))
                {
                    UpdateWindowBorderThickness();
                }
            }
        }

        #endregion

        #region Methods

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
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        //
        public void UpdateWindowBorderThickness()
        {
            if (_isWindows7 && _windowChromeFrame == WindowChromeFrame.WindowFrame && this.WindowChrome != null && _window.WindowState != WindowState.Maximized)
            {
                var x = 1.0 / Config.Current.RawDpi.DpiScaleX;
                var y = 1.0 / Config.Current.RawDpi.DpiScaleY;
                this.WindowBorderThickness = new Thickness(x, y, x, y);
            }
            else
            {
                this.WindowBorderThickness = default;
            }

            if (_windowChrome != null && _window.WindowState == WindowState.Maximized)
            {
                var x = _maximizeWindowGapWidth / Config.Current.RawDpi.DpiScaleX;
                var y = _maximizeWindowGapWidth / Config.Current.RawDpi.DpiScaleY;
                _window.BorderThickness = new Thickness(x, y, x, y);
            }
            else
            {
                _window.BorderThickness = default;
            }
        }

        //
        public void ToggleCaptionVisible()
        {
            IsCaptionVisible = !IsCaptionVisible;
        }

        //
        public bool ToggleTopmost()
        {
            IsTopmost = !IsTopmost;
            return IsTopmost;
        }

        //
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
            bool isChanged = this.State != state;

            _oldState = this.State;
            this.State = state;

            if (state == WindowStateEx.Normal || state == WindowStateEx.Maximized)
            {
                _lastState = state;
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
            if (!_isWindows7 || _state != WindowStateEx.FullScreen) return;

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
            //Debug.WriteLine("ToNormal");
            BeginEdit();

            _window.ResizeMode = ResizeMode.CanResize;
            _window.WindowStyle = WindowStyle.SingleBorderWindow;
            _window.WindowState = WindowState.Normal;

            this.WindowChrome = IsCaptionVisible ? null : _chrome;
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
            if (_state == WindowStateEx.Minimized && _oldState == WindowStateEx.FullScreen)
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
            ////Debug.WriteLine("ToMaximized");
            BeginEdit();

            ToMaximizeInner();

            this.WindowChrome = IsCaptionVisible ? null : _chrome;
            UpdateWindowBorderThickness();

            UpdateState(WindowStateEx.Maximized);
            EndEdit();
        }

        private void ToMaximizeInner()
        {
            _window.Topmost = false;
            _window.WindowStyle = WindowStyle.SingleBorderWindow;
            _window.ResizeMode = ResizeMode.CanResize;
            _window.WindowState = WindowState.Maximized;
            _window.Topmost = _isTopmost;
        }

        /// <summary>
        /// フルスクリーンにする
        /// </summary>
        private void ToFullScreen()
        {
            ////Debug.WriteLine("ToFullScreen");
            BeginEdit();

            if (_isFullScreenWithTaskBar)
            {
                ToMaximizeInner();
                this.WindowChrome = _chrome;
            }
            else
            {
                ToFullScreenInner();
                this.WindowChrome = null;
            }

            UpdateWindowBorderThickness();

            UpdateState(WindowStateEx.FullScreen);
            EndEdit();
        }

        private void ToFullScreenInner()
        {
            _window.ResizeMode = ResizeMode.CanMinimize;
            if (_window.WindowState == WindowState.Maximized) _window.WindowState = WindowState.Normal;
            _window.WindowStyle = WindowStyle.None;
            _window.WindowState = WindowState.Maximized;
        }

        /// <summary>
        /// フルスクリーン状態のON/OFF
        /// </summary>
        /// <param name="isFullScreen"></param>
        public void SetFullScreen(bool isFullScreen)
        {
            if (isFullScreen && _state != WindowStateEx.FullScreen)
            {
                ToFullScreen();
            }
            else if (!isFullScreen && _state == WindowStateEx.FullScreen)
            {
                if (_lastState == WindowStateEx.Maximized)
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
        /// 状態を最新にする
        /// </summary>
        public void Refresh()
        {
            if (!this.IsEnabled) return;

            _chrome.GlassFrameThickness = _windowChromeFrame == WindowChromeFrame.None ? new Thickness(0) : new Thickness(1);
            _window.Topmost = IsTopmost;
            _isFullScreen = _state == WindowStateEx.FullScreen;
            SetWindowState(_state);
            UpdateWindowBorderThickness();
            RaisePropertyChanged(null);
        }

        public void SetHook()
        {
            HwndSource hwnd = (HwndSource)PresentationSource.FromVisual(_window);
            if (hwnd == null) return;
            Debug.WriteLine($"SetHook {hwnd.Handle}");
            hwnd.RemoveHook(HookProc);
            hwnd.AddHook(HookProc);
        }

        private IntPtr HookProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x0084 /*WM_NCHITTEST*/ )
            {
                // This prevents a crash in WindowChromeWorker._HandleNCHitTest
                try
                {
                    var x = lParam.ToInt32();
                    ////DebugInfo.Current?.SetMessage($"WM_NCHITTEST.LPARAM: {x:#,0}");
                    ////Debug.WriteLine($"{x:#,0}");
                }
                catch (OverflowException)
                {
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

#endregion

#region Memento
        [DataContract]
        public class Memento
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


            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            public Memento Clone()
            {
                return (Memento)this.MemberwiseClone();
            }
        }

        // Memento一時保存
        public Memento SnapMemento { get; set; }

        /// <summary>
        /// 現在のMementoを記憶。Window.Closed()ではWindow情報が取得できないため。
        /// </summary>
        public void CreateSnapMemento()
        {
            this.SnapMemento = CreateMemento();
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.State = this.State;
            memento.IsCaptionVisible = this.IsCaptionVisible;
            memento.IsTopMost = this.IsTopmost;
            memento.IsFullScreenWithTaskBar = this.IsFullScreenWithTaskBar;
            memento.MaximizeWindowGapWidth = this.MaximizeWindowGapWidth;

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            _isTopmost = memento.IsTopMost;
            _isCaptionVisible = memento.IsCaptionVisible;
            _state = memento.State;
            _isFullScreenWithTaskBar = memento.IsFullScreenWithTaskBar;
            _maximizeWindowGapWidth = memento.MaximizeWindowGapWidth;
        }

#endregion
    }

}
