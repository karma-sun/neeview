// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using System;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// タッチ入力処理既定クラス
    /// </summary>
    public abstract class TouchInputBase : BindableBase
    {
        /// <summary>
        /// 状態遷移通知
        /// </summary>
        public EventHandler<TouchInputStateEventArgs> StateChanged;


        /// <summary>
        /// 状態コンテキスト
        /// </summary>
        protected TouchInputContext _context;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="context"></param>
        public TouchInputBase(TouchInputContext context)
        {
            _context = context;
        }

        /// <summary>
        /// タッチによるコマンド発動
        /// </summary>
        public EventHandler<TouchGestureEventArgs> TouchGestureChanged;


        /// <summary>
        /// 状態開始時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public virtual void OnOpened(FrameworkElement sender, object parameter) { }

        /// <summary>
        /// 状態終了時処理
        /// </summary>
        /// <param name="sender"></param>
        public virtual void OnClosed(FrameworkElement sender) { }

        /// <summary>
        /// 各種入力イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public abstract void OnStylusDown(object sender, StylusDownEventArgs e);
        public abstract void OnStylusUp(object sender, StylusEventArgs e);
        public abstract void OnStylusMove(object sender, StylusEventArgs e);
        public virtual void OnStylusSystemGesture(object sender, StylusSystemGestureEventArgs e) { }

        /// <summary>
        /// 状態遷移：既定状態に移動
        /// </summary>
        protected void ResetState()
        {
            StateChanged?.Invoke(this, new TouchInputStateEventArgs(TouchInputState.Normal));
        }

        /// <summary>
        /// 状態遷移：指定状態に移動
        /// </summary>
        /// <param name="state"></param>
        /// <param name="parameter"></param>
        protected void SetState(TouchInputState state, object parameter = null)
        {
            StateChanged?.Invoke(this, new TouchInputStateEventArgs(state, parameter));
        }

        /// <summary>
        /// 押されているマウスボタンのビットマスク作成
        /// </summary>
        /// <param name="e">元になるデータ</param>
        /// <returns></returns>
        protected MouseButtonBits CreateMouseButtonBits(MouseEventArgs e)
        {
            return MouseButtonBitsExtensions.Create(e);
        }

        /// <summary>
        /// 押されているマウスボタンのビットマスク作成
        /// </summary>
        /// <returns></returns>
        protected MouseButtonBits CreateMouseButtonBits()
        {
            return MouseButtonBitsExtensions.Create();
        }

        /// <summary>
        /// 押されているボタンを１つだけ返す
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected MouseButton? GetMouseButton(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                return MouseButton.Left;
            }
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                return MouseButton.Middle;
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                return MouseButton.Right;
            }
            if (e.XButton1 == MouseButtonState.Pressed)
            {
                return MouseButton.XButton1;
            }
            if (e.XButton2 == MouseButtonState.Pressed)
            {
                return MouseButton.XButton2;
            }

            return null;
        }


        /// <summary>
        /// タッチ座標からコマンド発行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ExecuteTouchGesture(object sender, StylusEventArgs e)
        {
            // タッチジェスチャー判定
            var point = e.GetPosition(_context.Sender);
            var xRate = point.X / _context.Sender.ActualWidth;
            var yRate = point.Y / _context.Sender.ActualHeight;

            // TouchCenter を優先的に判定
            if (TouchGesture.TouchCenter.IsTouched(xRate, yRate))
            {
                TouchGestureChanged?.Invoke(this, new TouchGestureEventArgs(e, TouchGesture.TouchCenter));
                if (e.Handled) return;
            }

            // TouchLeft / Right
            var gesture = TouchGestureExtensions.GetTouchGestureLast(xRate, yRate);
            TouchGestureChanged?.Invoke(this, new TouchGestureEventArgs(e, gesture));
        }

    }
}
