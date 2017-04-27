using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;

namespace NeeView
{
    /// <summary>
    /// WindowShape
    /// </summary>
    public enum WindowShape
    {
        None, // 未設定
        Normal,
        Minimized,
        Maximized,
        FullScreen,
    }

    /// <summary>
    /// WindowShape Selector.
    /// 標準のウィンドウ状態にフルスクリーン状態を加えたもの
    /// </summary>
    public class WindowShapeSelector : INotifyPropertyChanged
    {
        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        //
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// 状態変更イベント
        /// </summary>
        public event EventHandler ShapeChanged;

        /// <summary>
        /// IsUseChrome property.
        /// Caption非表示でChromeを使用するフラグ
        /// </summary>
        public bool IsUseChrome
        {
            get { return _isUseChrome; }
            set { if (_isUseChrome != value) { _isUseChrome = value; Reflesh(); } }
        }

        //
        private bool _isUseChrome = false;


        /// <summary>
        /// IsCaptionVisible property.
        /// </summary>
        public bool IsCaptionVisible
        {
            get { return _isCaptionVisible; }
            set
            {
                if (_isCaptionVisible != value) { _isCaptionVisible = value; Reflesh(); }
            }
        }

        //
        private bool _isCaptionVisible = true;


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


        /// <summary>
        /// 管理するWindow
        /// </summary>
        private Window _window;

        /// <summary>
        /// 枠なしChrome
        /// </summary>
        private WindowChrome _chrome;

        /// <summary>
        /// Shape property.
        /// 現在の状態
        /// </summary>
        public WindowShape Shape
        {
            get { return _shape; }
        }

        //
        private WindowShape _shape;

        /// <summary>
        /// 直前の状態
        /// </summary>
        private WindowShape _oldShape;

        /// <summary>
        /// 最後の安定状態。フルスクリーン切り替えで使用される
        /// </summary>
        private WindowShape _last;

        /// <summary>
        /// フルスクリーン解除時のタスクバー復帰
        /// </summary>
        private bool _isRecoveryTaskBar;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="window"></param>
        public WindowShapeSelector(Window window)
        {
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
            _isRecoveryTaskBar = os.Version.Major < 6 || (os.Version.Major == 6 && os.Version.Minor <= 1); // Windows7 = 6.1

            //
            _isTopmost = _window.Topmost;

            switch (_window.WindowState)
            {
                case WindowState.Normal:
                    _shape = WindowShape.Normal;
                    break;
                case WindowState.Minimized:
                    _shape = WindowShape.Minimized;
                    break;
                case WindowState.Maximized:
                    _shape = WindowShape.Maximized;
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
        /// <param name="shape"></param>
        public void SetWindowShape(WindowShape shape)
        {
            switch (shape)
            {
                case WindowShape.Normal:
                    ToNormal();
                    break;
                case WindowShape.Minimized:
                    ToMinimized();
                    break;
                case WindowShape.Maximized:
                    ToMaximized();
                    break;
                case WindowShape.FullScreen:
                    ToFullScreen();
                    break;
            }
        }

        /// <summary>
        /// 現在の状態を記憶
        /// </summary>
        /// <param name="shape"></param>
        private void UpdateShape(WindowShape shape)
        {
            bool isChanged = _shape != shape;

            _oldShape = _shape;
            _shape = shape;

            if (shape == WindowShape.Normal || shape == WindowShape.Maximized)
            {
                _last = shape;
            }

            if (isChanged) ShapeChanged?.Invoke(this, null);
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
            if (!_isRecoveryTaskBar || _shape != WindowShape.FullScreen) return;

            //Debug.WriteLine("Recovery TaskBar");
            _window.Visibility = Visibility.Hidden;
            _window.Visibility = Visibility.Visible;

            ////IntPtr hTaskbarWnd = FindWindow("Shell_TrayWnd", null);
            ////SetForegroundWindow(hTaskbarWnd);
            ////_window.Activate();
        }

        /// <summary>
        /// 通常ウィンドウにする
        /// </summary>
        public void ToNormal()
        {
            //Debug.WriteLine("ToNormal");
            BeginEdit();

            WindowChrome.SetWindowChrome(_window, IsCaptionVisible || !IsUseChrome ? null : _chrome);
            _window.ResizeMode = ResizeMode.CanResize;
            _window.WindowStyle = (IsCaptionVisible || IsUseChrome) ? WindowStyle.SingleBorderWindow : WindowStyle.None;
            _window.WindowState = WindowState.Normal;

            RecoveryTaskBar();

            UpdateShape(WindowShape.Normal);
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

            UpdateShape(WindowShape.Minimized);
            EndEdit();
        }

        /// <summary>
        /// 最大化、もしくはフルスクリーンにする。
        /// 最小化からの復帰用
        /// </summary>
        public void ToMaximizedMaybe()
        {
            //Debug.WriteLine("ToMaximizedMaybe");
            if (_shape == WindowShape.Minimized && _oldShape == WindowShape.FullScreen)
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

            WindowChrome.SetWindowChrome(_window, null);
            _window.Topmost = false;
            _window.WindowStyle = WindowStyle.SingleBorderWindow;
            _window.ResizeMode = ResizeMode.CanResize;
            if (_shape == WindowShape.FullScreen) _window.WindowState = WindowState.Normal;
            //_window.WindowStyle = WindowStyle.SingleBorderWindow;
            _window.WindowState = WindowState.Maximized;
            if (!IsCaptionVisible) _window.WindowStyle = WindowStyle.None;
            _window.Topmost = _isTopmost;

            RecoveryTaskBar();

            UpdateShape(WindowShape.Maximized);
            EndEdit();
        }

        /// <summary>
        /// フルスクリーンにする
        /// </summary>
        public void ToFullScreen()
        {
            //Debug.WriteLine("ToFullScreen");
            BeginEdit();

            WindowChrome.SetWindowChrome(_window, null);
            _window.ResizeMode = ResizeMode.NoResize;
            _window.WindowState = WindowState.Normal;
            _window.WindowStyle = WindowStyle.None;
            _window.WindowState = WindowState.Maximized;

            UpdateShape(WindowShape.FullScreen);
            EndEdit();
        }

        /// <summary>
        /// フルスクリーン状態のON/OFF
        /// </summary>
        /// <param name="isFullScreen"></param>
        public void SetFullScreen(bool isFullScreen)
        {
            if (isFullScreen && _shape != WindowShape.FullScreen)
            {
                ToFullScreen();
            }
            else if (!isFullScreen && _shape == WindowShape.FullScreen)
            {
                if (_last == WindowShape.Maximized)
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
            SetWindowShape(_shape);
            RaisePropertyChanged(null);
        }


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public WindowShape Shape { get; set; }

            [DataMember]
            public bool IsCaptionVisible { get; set; }

            [DataMember]
            public bool IsTopMost { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.Shape = Shape;
            memento.IsCaptionVisible = IsCaptionVisible;
            memento.IsTopMost = IsTopmost;

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            _isTopmost = memento.IsTopMost;
            _isCaptionVisible = memento.IsCaptionVisible;
            _shape = memento.Shape;

            Reflesh();
        }

        #endregion
    }
}
