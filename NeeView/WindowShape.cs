using NeeView.ComponentModel;
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

    //
    public enum WindowChromeFrame
    {
        WindowFrame, // ウィンドウフレームを使用
        None,
        Line,
    }


    /// <summary>
    /// WindowShape Selector.
    /// 標準のウィンドウ状態にフルスクリーン状態を加えたもの
    /// </summary>
    public class WindowShape : BindableBase
    {
        public static WindowShape Current { get; private set; }

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string className, string windowTitle);

        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hwnd);


        /// <summary>
        /// 状態変更イベント
        /// </summary>
        public event EventHandler StateChanged;


        /// <summary>
        /// WindowChromeFrame property.
        /// </summary>
        public WindowChromeFrame WindowChromeFrame
        {
            get { return _WindowChromeFrame; }
            set
            {
                if (_WindowChromeFrame != value)
                {
                    _WindowChromeFrame = value;
                    Reflesh();
                }
            }
        }

        private WindowChromeFrame _WindowChromeFrame = WindowChromeFrame.Line;


        /// <summary>
        /// WindowBorderThickness property.
        /// </summary>
        public Thickness WindowBorderThickness
        {
            get { return _windowBorderThickness; }
            set { if (_windowBorderThickness != value) { _windowBorderThickness = value; RaisePropertyChanged(); } }
        }

        private Thickness _windowBorderThickness;

        public void UpdateWindowBorderThickness()
        {
            if (this.WindowChromeFrame == WindowChromeFrame.Line && this.WindowChrome != null)
            {
                var x = 1.0 / Config.Current.RawDpi.DpiScaleX;
                var y = 1.0 / Config.Current.RawDpi.DpiScaleY;
                this.WindowBorderThickness = new Thickness(x, y, x, y);
            }
            else
            {
                this.WindowBorderThickness = default(Thickness);
            }
        }




        /// <summary>
        /// IsCaptionVisible property.
        /// </summary>
        public bool IsCaptionVisible
        {
            get { return _isCaptionVisible; }
            set { if (_isCaptionVisible != value) { _isCaptionVisible = value; Reflesh(); } }
        }

        //
        private bool _isCaptionVisible = true;

        public void ToggleCaptionVisible()
        {
            IsCaptionVisible = !IsCaptionVisible;
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

        //
        private bool _isTopmost;

        //
        public bool ToggleTopmost()
        {
            IsTopmost = !IsTopmost;
            return IsTopmost;
        }


        /// <summary>
        /// IsFullScreen property.
        /// </summary>
        public bool IsFullScreen
        {
            get { return _isFullScreen; }
            private set { if (_isFullScreen != value) { _isFullScreen = value; RaisePropertyChanged(); } }
        }

        private bool _isFullScreen;

        //
        public void ToggleFullScreen()
        {
            SetFullScreen(!IsFullScreen);
        }



        /// <summary>
        /// 管理するWindow
        /// </summary>
        private Window _window;

        /// <summary>
        /// 枠なしChrome
        /// </summary>
        private WindowChrome _chrome;


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
                    UpdateWindowBorderThickness();
                    RaisePropertyChanged();
                }
            }
        }

        private WindowChrome _windowChrome;



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

        //
        private WindowStateEx _state;


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

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="window"></param>
        public WindowShape(Window window)
        {
            Current = this;

            _window = window;

            // キャプション非表示時に適用するChrome
            _chrome = new WindowChrome();
            _chrome.CornerRadius = new CornerRadius();
            _chrome.GlassFrameThickness = new Thickness(1);
            _chrome.CaptionHeight = 0; // SystemParameters.CaptionHeight;
            _chrome.UseAeroCaptionButtons = false;
            //_chrome.ResizeBorderThickness = new Thickness(4);

            // Windows7以前の場合、フルスクリーン解除時にタスクバーを手前にする処理を追加
            var os = System.Environment.OSVersion;
            _isWindows7 = os.Version.Major < 6 || (os.Version.Major == 6 && os.Version.Minor <= 1); // Windows7 = 6.1

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

            _window.StateChanged += Window_StateChanged;
        }


        /// <summary>
        /// ウィンドウ状態イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_StateChanged(object sender, EventArgs e)
        {
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

        /// <summary>
        /// 状態変更
        /// </summary>
        /// <param name="state"></param>
        public void SetWindowState(WindowStateEx state)
        {
            switch (state)
            {
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
        /// 処理中
        /// </summary>
        public bool IsProcessing { get; private set; }

        /// <summary>
        /// 処理開始
        /// </summary>
        private void BeginEdit()
        {
            Debug.Assert(IsProcessing == false);
            IsProcessing = true;
        }

        /// <summary>
        /// 処理終了
        /// </summary>
        private void EndEdit()
        {
            IsProcessing = false;
        }

        /// <summary>
        /// タスクバーを手前に表示しし直す処理 (for Windows7)
        /// </summary>
        private void RecoveryTaskBar()
        {
            if (!_isWindows7 || _state != WindowStateEx.FullScreen) return;

            Debug.WriteLine("Recovery TaskBar");

            //_window.Visibility = Visibility.Hidden;
            //_window.Visibility = Visibility.Visible;

            IntPtr hTaskbarWnd = FindWindow("Shell_TrayWnd", null);
            SetForegroundWindow(hTaskbarWnd);
            _window.Activate();
        }

        /// <summary>
        /// 通常ウィンドウにする
        /// </summary>
        public void ToNormal()
        {
            //Debug.WriteLine("ToNormal");
            BeginEdit();

            this.WindowChrome = this.WindowChromeFrame != WindowChromeFrame.WindowFrame && !IsCaptionVisible ? _chrome : null;
            _window.ResizeMode = ResizeMode.CanResize;
            _window.WindowStyle = IsCaptionVisible ? WindowStyle.SingleBorderWindow : WindowStyle.None;
            _window.WindowState = WindowState.Normal;

            RecoveryTaskBar();

            UpdateState(WindowStateEx.Normal);
            EndEdit();
        }

        /// <summary>
        /// 最小化する
        /// </summary>
        public void ToMinimized()
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
        public void ToMaximizedMaybe()
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
        public void ToMaximized()
        {
            //Debug.WriteLine("ToMaximized");
            BeginEdit();

            // タイトルバー非表示時に最大化すると右に隙間ができてしまう対策
            if (!IsCaptionVisible && !_isWindows7)
            {
                _window.WindowState = WindowState.Normal;
            }

            this.WindowChrome = null;
            _window.Topmost = false;
            _window.WindowStyle = WindowStyle.SingleBorderWindow;
            _window.ResizeMode = ResizeMode.CanResize;
            if (_state == WindowStateEx.FullScreen) _window.WindowState = WindowState.Normal;
            _window.WindowState = WindowState.Maximized;
            if (!IsCaptionVisible) _window.WindowStyle = WindowStyle.None;
            _window.Topmost = _isTopmost;

            RecoveryTaskBar();

            UpdateState(WindowStateEx.Maximized);
            EndEdit();
        }

        /// <summary>
        /// フルスクリーンにする
        /// </summary>
        public void ToFullScreen()
        {
            //Debug.WriteLine("ToFullScreen");
            BeginEdit();

            this.WindowChrome = null;
            _window.ResizeMode = ResizeMode.NoResize;
            if (_state == WindowStateEx.Maximized) _window.WindowState = WindowState.Normal;
            _window.WindowStyle = WindowStyle.None;
            _window.WindowState = WindowState.Maximized;

            UpdateState(WindowStateEx.FullScreen);
            EndEdit();
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
        public void Reflesh()
        {
            _window.Topmost = IsTopmost;
            _isFullScreen = _state == WindowStateEx.FullScreen;
            SetWindowState(_state);
            UpdateWindowBorderThickness();
            RaisePropertyChanged(null);
        }


        /// <summary>
        /// WindowRect property.
        /// </summary>
        public Rect WindowRect
        {
            get { return _window.RestoreBounds; }
            set
            {
                if (value.IsEmpty) return;
                _window.Left = value.Left;
                _window.Top = value.Top;
                _window.Width = value.Width;
                _window.Height = value.Height;
            }
        }


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public WindowStateEx State { get; set; }

            [DataMember]
            public bool IsCaptionVisible { get; set; }

            [DataMember]
            public bool IsTopMost { get; set; }

            [DataMember]
            public Rect WindowRect { get; set; }

            //
            public Memento Clone()
            {
                return (Memento)this.MemberwiseClone();
            }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.State = this.State;
            memento.IsCaptionVisible = this.IsCaptionVisible;
            memento.IsTopMost = this.IsTopmost;
            memento.WindowRect = this.WindowRect;

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.WindowRect = memento.WindowRect;

            // Window状態をまとめて更新
            _isTopmost = memento.IsTopMost;
            _isCaptionVisible = memento.IsCaptionVisible;
            _state = memento.State;
            Reflesh();
        }

        //
        public Memento SnapMemento { get; private set; }

        /// <summary>
        /// 現在のMementoを記憶。Window.Closed()ではWindow情報が取得できないため。
        /// </summary>
        public void CreateSnapMemento()
        {
            this.SnapMemento = CreateMemento();
        }

        #endregion
    }

}
