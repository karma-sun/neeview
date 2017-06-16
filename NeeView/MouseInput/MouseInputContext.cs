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
        /// <summary>
        /// コントロール初期化
        /// </summary>
        /// <param name="window"></param>
        /// <param name="sender"></param>
        /// <param name="targetView"></param>
        /// <param name="targetShadow"></param>
        public void Initialize(Window window, FrameworkElement sender, FrameworkElement targetView, FrameworkElement targetShadow, DragTransform dragTransform, MouseGestureCommandCollection gestureCommandCollection)
        {
            this.Window = window;
            this.Sender = sender;
            this.TargetView = targetView;
            this.TargetShadow = targetShadow;
            this.DragTransform = dragTransform;
            this.GestureCommandCollection = gestureCommandCollection;
        }

        /// <summary>
        /// 所属ウィンドウ
        /// </summary>
        public Window Window { get; set; }

        /// <summary>
        /// イベント受取エレメント
        /// </summary>
        public FrameworkElement Sender { get; set; }

        /// <summary>
        /// 操作対象エレメント
        /// アニメーション対応
        /// </summary>
        public FrameworkElement TargetView { get; set; }

        /// <summary>
        /// 操作対象エレメント計算用
        /// アニメーション非対応。非表示の矩形のみ。
        /// 表示領域計算にはこちらを利用する
        /// </summary>
        public FrameworkElement TargetShadow { get; set; }


        //
        public DragTransform DragTransform { get; set; }
        
        
        /// <summary>
        /// ジェスチャーコマンドテーブル
        /// </summary>
        public MouseGestureCommandCollection GestureCommandCollection { get; set; }


        /// <summary>
        /// ドラッグ開始座標
        /// </summary>
        public Point StartPoint { get; set; }

        /// <summary>
        /// 入力スタイラスデバイス
        /// </summary>
        public StylusDevice StylusDevice { get; set; }
    }

}
