// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// フルスクリーン切り替え
    /// </summary>
    public class FullScreen
    {
        private Window _Window;
        private bool _IsFullScreened;

        private bool _IsMemento;
        private WindowState _WindowState;

        // モードが変化した時の通知
        public event EventHandler NotifyMenuVisibilityChanged;

        // フルスクリーン設定
        public bool IsFullScreened
        {
            get { return _IsFullScreened; }
            set { if (value) ToFullScreen(); else Cancel(); }
        }

        // コンストラクタ
        public FullScreen(Window window)
        {
            _Window = window;
        }

        // フルスクリーン トグル切り替え
        public void Toggle()
        {
            if (!_IsFullScreened) ToFullScreen(); else Cancel();
        }

        // フルスクリーン設定
        private void ToFullScreen()
        {
            if (_IsFullScreened) return;

            _IsFullScreened = true;

            _WindowState = _Window.WindowState;
            _IsMemento = true;

            NotifyMenuVisibilityChanged?.Invoke(this, null);

            if (_Window.WindowState == WindowState.Maximized) _Window.WindowState = WindowState.Normal;
            _Window.WindowState = WindowState.Maximized;
        }

        // フルスクリーン解除
        private void Cancel()
        {
            if (!_IsFullScreened) return;

            if (_IsMemento)
            {
                _Window.WindowState = _WindowState;
            }

            _IsFullScreened = false;

            NotifyMenuVisibilityChanged?.Invoke(this, null);
        }
    }
}
