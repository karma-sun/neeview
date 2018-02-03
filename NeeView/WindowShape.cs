// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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

        #region NativeApi

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string className, string windowTitle);

        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hwnd);

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

        #endregion

        #region Constructors

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
            _chrome.GlassFrameThickness = new Thickness(0);
            _chrome.CaptionHeight = 0;
            _chrome.UseAeroCaptionButtons = false;
            _chrome.ResizeBorderThickness = new Thickness(8); // TODO: この値をシステムから取得したい

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
        private WindowChromeFrame _WindowChromeFrame;
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
        
        /// <summary>
        /// WindowBorderThickness property.
        /// </summary>
        private Thickness _windowBorderThickness;
        public Thickness WindowBorderThickness
        {
            get { return _windowBorderThickness; }
            set { if (_windowBorderThickness != value) { _windowBorderThickness = value; RaisePropertyChanged(); } }
        }


        /// <summary>
        /// IsCaptionVisible property.
        /// </summary>
        private bool _isCaptionVisible = true;
        public bool IsCaptionVisible
        {
            get { return _isCaptionVisible; }
            set { if (_isCaptionVisible != value) { _isCaptionVisible = value; Reflesh(); } }
        }


        /// <summary>
        /// IsTopmost property.
        /// </summary>
        private bool _isTopmost;
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
        private bool _isFullScreen;
        public bool IsFullScreen
        {
            get { return _isFullScreen; }
            private set { if (_isFullScreen != value) { _isFullScreen = value; RaisePropertyChanged(); } }
        }


        /// <summary>
        /// 現在のWindowChrome
        /// </summary>
        private WindowChrome _windowChrome;
        public WindowChrome WindowChrome
        {
            get { return _windowChrome; }
            private set
            {
                if (_windowChrome != value)
                {
                    _windowChrome = value;
                    WindowChrome.SetWindowChrome(_window, _windowChrome);
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// State property.
        /// 現在の状態
        /// </summary>
        private WindowStateEx _state;
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
        public bool IsProcessing { get; private set; }


        /// <summary>
        /// IsEnabled property.
        /// </summary>
        private bool _IsEnabled;
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set
            {
                if (_IsEnabled != value)
                {
                    _IsEnabled = value;
                    if (_IsEnabled) Reflesh();
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Methods

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
            if (this.WindowChromeFrame == WindowChromeFrame.Line && this.WindowChrome != null && _window.WindowState != WindowState.Maximized)
            {
                var x = 1.0 / Config.Current.RawDpi.DpiScaleX;
                var y = 1.0 / Config.Current.RawDpi.DpiScaleY;
                this.WindowBorderThickness = new Thickness(x, y, x, y);
            }
            else
            {
                this.WindowBorderThickness = default(Thickness);
            }

            _window.BorderThickness = (_windowChrome != null && _window.WindowState == WindowState.Maximized) ? _windowChrome.ResizeBorderThickness : default(Thickness);
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

            ////Debug.WriteLine("Recovery TaskBar");

            //_window.Visibility = Visibility.Hidden;
            //_window.Visibility = Visibility.Visible;

            IntPtr hTaskbarWnd = FindWindow("Shell_TrayWnd", null);
            SetForegroundWindow(hTaskbarWnd);
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
            _window.WindowStyle = IsCaptionVisible ? WindowStyle.SingleBorderWindow : WindowStyle.None;
            _window.WindowState = WindowState.Normal;

            this.WindowChrome = this.WindowChromeFrame != WindowChromeFrame.WindowFrame && !IsCaptionVisible ? _chrome : null;
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
            //Debug.WriteLine("ToMaximized");
            BeginEdit();

            // タイトルバー非表示時に最大化すると右に隙間ができてしまう対策
            if (!IsCaptionVisible && !_isWindows7)
            {
                _window.WindowState = WindowState.Normal;
            }

            _window.Topmost = false;
            _window.WindowStyle = WindowStyle.SingleBorderWindow;
            _window.ResizeMode = ResizeMode.CanResize;
            if (_state == WindowStateEx.FullScreen) _window.WindowState = WindowState.Normal;
            _window.WindowState = WindowState.Maximized;
            if (!IsCaptionVisible) _window.WindowStyle = WindowStyle.None;
            _window.Topmost = _isTopmost;

            this.WindowChrome = this.WindowChromeFrame != WindowChromeFrame.WindowFrame && !IsCaptionVisible ? _chrome : null;
            UpdateWindowBorderThickness();

            RecoveryTaskBar();

            UpdateState(WindowStateEx.Maximized);
            EndEdit();
        }

        /// <summary>
        /// フルスクリーンにする
        /// </summary>
        private void ToFullScreen()
        {
            //Debug.WriteLine("ToFullScreen");
            BeginEdit();

            _window.ResizeMode = ResizeMode.NoResize;
            if (_window.WindowState == WindowState.Maximized) _window.WindowState = WindowState.Normal;
            _window.WindowStyle = WindowStyle.None;
            _window.WindowState = WindowState.Maximized;

            this.WindowChrome = null;
            UpdateWindowBorderThickness();

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
            if (!this.IsEnabled) return;

            _window.Topmost = IsTopmost;
            _isFullScreen = _state == WindowStateEx.FullScreen;
            SetWindowState(_state);
            UpdateWindowBorderThickness();
            RaisePropertyChanged(null);
        }

        #endregion

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

            //
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

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            _isTopmost = memento.IsTopMost;
            _isCaptionVisible = memento.IsCaptionVisible;
            _state = memento.State;
        }

        #endregion
    }

}
