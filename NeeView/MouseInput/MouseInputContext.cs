// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using System;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// MouseInputContext
    /// </summary>
    public class MouseInputContext : BindableBase
    {
        #region Constructors

        public MouseInputContext(FrameworkElement sender, MouseGestureCommandCollection gestureCommandCollection)
        {
            this.Sender = sender;
            this.GestureCommandCollection = gestureCommandCollection;
        }

        #endregion

        #region Properties

        /// <summary>
        /// イベント受取エレメント
        /// </summary>
        public FrameworkElement Sender { get; set; }
        
        /// <summary>
        /// ジェスチャーコマンドテーブル
        /// </summary>
        public MouseGestureCommandCollection GestureCommandCollection { get; set; }

        /// <summary>
        /// ドラッグ開始座標
        /// </summary>
        public Point StartPoint { get; set; }

        #endregion
    }

}
