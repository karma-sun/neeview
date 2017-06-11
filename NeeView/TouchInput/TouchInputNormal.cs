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
        /// ジェスチャー判定移行用距離
        /// </summary>
        const double _touchLimitDistance = 30.0;

        /// <summary>
        /// 押されている？
        /// </summary>
        private bool _isTouchDown;

        /// <summary>
        /// 現在のタッチデバイス
        /// </summary>
        private TouchContext _touch;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="context"></param>
        public TouchInputNormal(TouchInputContext context) : base(context)
        {
        }

        /// <summary>
        /// 状態開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public override void OnOpened(FrameworkElement sender, object parameter)
        {
            Debug.WriteLine("TouchState: Normal");

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
        public override void OnTouchDown(object sender, TouchEventArgs e)
        {
            // シングルタッチのみ対応
            // TODO: マルチタッチでドラッグへ
            if (_context.TouchMap.Count != 1)
            {
                _isTouchDown = false;
                return;
            }

            _context.TouchMap.TryGetValue(e.TouchDevice, out _touch);
            if (_touch == null) return;

            _isTouchDown = true;
            _context.Sender.Focus();
        }

        /// <summary>
        /// マウスボタンが離されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnTouchUp(object sender, TouchEventArgs e)
        {
            if (!_isTouchDown) return;
            if (e.TouchDevice != _touch.TouchDevice) return;

            // タッチジェスチャー判定
            TouchGesture gesture = TouchGesture.None;
            var touchPoint = e.GetTouchPoint(_context.Sender);

            // タッチエリア 左右判定
            if (touchPoint.Position.X < _context.Sender.ActualWidth * 0.5)
            {
                gesture = TouchGesture.TouchLeft;
            }
            else
            {
                gesture = TouchGesture.TouchRight;
            }

            // コマンド決定
            if (gesture != TouchGesture.None)
            {
                TouchGestureChanged?.Invoke(sender, new TouchGestureEventArgs(e, gesture));
            }

            // その後の操作は全て無効
            _isTouchDown = false;
        }


        /// <summary>
        /// マウス移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnTouchMove(object sender, TouchEventArgs e)
        {
            if (!_isTouchDown) return;
            if (e.TouchDevice != _touch.TouchDevice) return;

            var point = e.GetTouchPoint(_context.Sender);

            var touchStart = _context.TouchMap[e.TouchDevice].StartPoint;
            var deltaX = Math.Abs(point.Position.X - touchStart.Position.X);
            var deltaY = Math.Abs(point.Position.Y - touchStart.Position.Y);

            // drag check
            if (deltaX > _touchLimitDistance || deltaY > _touchLimitDistance)
            {
                SetState(TouchInputState.Gesture, _touch);
            }
        }

    }
}
