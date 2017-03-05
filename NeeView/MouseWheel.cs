// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// マウスホイール処理
    /// </summary>
    public class MouseWheel
    {
        // ホイールイベントハンドラ
        public event EventHandler<MouseWheelEventArgs> MouseWheelEventHandler;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="sender"></param>
        public MouseWheel(FrameworkElement sender)
        {
            sender.PreviewMouseWheel += OnMouseWheel;
        }

        /// <summary>
        /// イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            MouseWheelEventHandler?.Invoke(sender, e);
        }

        /// <summary>
        /// イベントハンドラ初期化
        /// </summary>
        internal void ClearWheelEventHandler()
        {
            MouseWheelEventHandler = null;
        }
    }
}
