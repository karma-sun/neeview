// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;

namespace NeeView
{
    /// <summary>
    /// フルスクリーン変更イベント引数
    /// </summary>
    public class FullScreenChangedEventArgs : EventArgs
    {
        public bool IsFullScreen { get; set; }
    }

    /// <summary>
    /// フルスクリーン管理
    /// </summary>
    public class FullScreenManager : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        private MainWindow _window;

        // 状態更新の再入を防ぐためのフラグ
        private bool _isUpdating;

        /// <summary>
        /// フルスクリーン状態の変更イベント
        /// </summary>
        public event EventHandler<FullScreenChangedEventArgs> Changed;

        /// <summary>
        /// フルスクリーン状態フラグ
        /// </summary>
        #region Property: IsFullScreen
        private bool _isFullScreen;
        public bool IsFullScreen
        {
            get { return _isFullScreen; }
            set
            {
                if (_isFullScreen != value)
                {
                    UpdateState(value);
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        /// <summary>
        /// フルスクリーン状態を切り替え
        /// </summary>
        public void ToggleFullScreen()
        {
            IsFullScreen = !IsFullScreen;
        }

        /// <summary>
        /// WindowStateMemento property.
        /// 元のウィンドウ状態を記憶
        /// </summary>
        public WindowState WindowStateMemento { get; set; } = WindowState.Normal;

        /// <summary>
        /// WindowStyleMemento property.
        /// 既定のウィンドウスタイル。フルスクリーン解除時に適用する
        /// </summary>
        private WindowStyle _WindowStyleMemento = WindowStyle.SingleBorderWindow;
        public WindowStyle WindowStyleMemento
        {
            get { return _WindowStyleMemento; }
            set
            {
                if (_WindowStyleMemento != value)
                {
                    _WindowStyleMemento = value;
                    UpdateWindowStyle();
                    RaisePropertyChanged();
                }
            }
        }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="window"></param>
        public FullScreenManager(MainWindow window)
        {
            _window = window;

            _window.PreviewKeyDown += OnPreviewKeyDown;
        }

        /// <summary>
        /// キーチェック。Escでフルスクリーン解除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (IsFullScreen && e.Key == System.Windows.Input.Key.Escape)
            {
                IsFullScreen = false;
                e.Handled = true;
            }
        }


        /// <summary>
        /// 状態更新
        /// </summary>
        /// <param name="isFullScreen"></param>
        private void UpdateState(bool isFullScreen)
        {
            if (_isUpdating) return;
            _isUpdating = true;

            if (isFullScreen)
            {
                _isFullScreen = true;
                WindowStateMemento = _window.WindowState;

                _window.ResizeMode = ResizeMode.NoResize;
                _window.WindowStyle = WindowStyle.None;
                if (_window.WindowState == WindowState.Maximized) _window.WindowState = WindowState.Normal;
                _window.WindowState = WindowState.Maximized;
            }
            else
            {
                _isFullScreen = false;

                _window.ResizeMode = ResizeMode.CanResize;
                _window.WindowStyle = this.WindowStyleMemento;
                if (_window.WindowState != WindowState.Normal)
                {
                    if (WindowStateMemento == WindowState.Maximized) _window.WindowState = WindowState.Normal;
                    _window.WindowState = WindowStateMemento;
                }
            }
            
            _isUpdating = false;

            //
            Changed?.Invoke(this, new FullScreenChangedEventArgs() { IsFullScreen = this.IsFullScreen });
        }


        /// <summary>
        /// ウィンドウスタイルの適用
        /// </summary>
        private void UpdateWindowStyle()
        {
            _window.WindowStyle = _isFullScreen ? WindowStyle.None : this.WindowStyleMemento;
        }
    }
}
