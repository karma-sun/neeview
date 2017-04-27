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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// WindowCaptionボタン
    /// </summary>
    public partial class WindowCaptionButtons : UserControl
    {
        private Window _window;

        /// <summary>
        /// コンストラクター
        /// </summary>
        public WindowCaptionButtons()
        {
            InitializeComponent();
            this.Loaded += (s, e) => InitializeWindow(Window.GetWindow(this));
        }

        /// <summary>
        /// 初期化：ウィンドウ状態変化イベントに登録
        /// </summary>
        /// <param name="window"></param>
        public void InitializeWindow(Window window)
        {
            if (window == null) return;

            if (_window != null)
            {
                _window.StateChanged -= Window_StateChanged;
            }

            _window = window;
            _window.StateChanged += Window_StateChanged;

            Window_StateChanged(this, null);
        }

        /// <summary>
        /// ウィンドウ状態変化処理。
        /// ボタンを変化させる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (_window == null) return;

            if (_window.WindowState == WindowState.Maximized)
            {
                //this.Root.Margin = new Thickness(0, 0, 2, 0);
                this.CaptionRestoreButton.Visibility = Visibility.Visible;
                this.CaptionMaximizeButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                //this.Root.Margin = new Thickness();
                this.CaptionRestoreButton.Visibility = Visibility.Collapsed;
                this.CaptionMaximizeButton.Visibility = Visibility.Visible;
            }
        }
    }
}
