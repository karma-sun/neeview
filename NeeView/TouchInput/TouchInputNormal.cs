// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// タッチ通常入力状態
    /// </summary>
    public class TouchInputNormal : TouchInputBase
    {
        /// <summary>
        /// 押されている？
        /// </summary>
        private bool _isTouchDown;

        /// <summary>
        /// 現在のタッチデバイス
        /// </summary>
        private TouchContext _touch;

        //
        private TouchInputGesture _gesture;


        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="context"></param>
        public TouchInputNormal(TouchInputContext context, TouchInputGesture gesture) : base(context)
        {
            _gesture = gesture;
        }


        /// <summary>
        /// タッチ入力通知
        /// </summary>
        public EventHandler<TouchGestureEventArgs> TouchGestureChanged;

        /// <summary>
        /// 状態開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public override void OnOpened(FrameworkElement sender, object parameter)
        {
            ////Debug.WriteLine("TouchState: Normal");
            _isTouchDown = false;
        }

        /// <summary>
        /// 状態終了
        /// </summary>
        /// <param name="sender"></param>
        public override void OnClosed(FrameworkElement sender)
        {
        }


        /// <summary>
        /// マウスボタンが押されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnStylusDown(object sender, StylusDownEventArgs e)
        {
            // マルチタッチでドラッグへ
            if (_context.TouchMap.Count >= 2)
            {
                SetState(TouchInputState.Drag, _touch);
                return;
            }

            _context.TouchMap.TryGetValue(e.StylusDevice, out _touch);
            if (_touch == null) return;

            _isTouchDown = true;
            _context.Sender.Focus();

            e.Handled = true;
        }

        /// <summary>
        /// マウスボタンが離されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnStylusUp(object sender, StylusEventArgs e)
        {
            if (!_isTouchDown) return;
            if (e.StylusDevice != _touch.StylusDevice) return;

            // タッチジェスチャー判定
            ExecuteTouchGesture(sender, e);

            // その後の操作は全て無効
            _isTouchDown = false;
        }

        //
        private void ExecuteTouchGesture(object sender, StylusEventArgs e)
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


        /// <summary>
        /// マウス移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnStylusMove(object sender, StylusEventArgs e)
        {
            if (!_isTouchDown) return;
            if (e.StylusDevice != _touch.StylusDevice) return;

            var point = e.GetPosition(_context.Sender);

            var touchStart = _context.TouchMap[e.StylusDevice].StartPoint;
            var deltaX = Math.Abs(point.X - touchStart.X);
            var deltaY = Math.Abs(point.Y - touchStart.Y);

            // drag check
            if (deltaX > _gesture.GestureMinimumDistanceX || deltaY > _gesture.GestureMinimumDistanceY)
            {
                SetState(TouchInputState.Gesture, _touch);
            }
        }

    }
}
