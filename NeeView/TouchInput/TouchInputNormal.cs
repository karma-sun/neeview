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
        /// ボタン押されている？
        /// </summary>
        private bool _isTouchDown;

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
            _isTouchDown = false;
        }

        /// <summary>
        /// 状態終了
        /// </summary>
        /// <param name="sender"></param>
        public override void OnClosed(FrameworkElement sender)
        {
        }


        private TouchContext _touchContext;

        /// <summary>
        /// マウスボタンが押されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnTouchDown(object sender, TouchEventArgs e)
        {
            // シングルタッチのみ対応
            if (_context.TouchMap.Count != 1)
            {
                _isTouchDown = false;
                return;
            }

            _context.TouchMap.TryGetValue(e.Device, out _touchContext);
            if (_touchContext == null) return;

            _isTouchDown = true;
            _context.Sender.Focus();

            _prevPoint = _touchContext.StartPoint;
            _prevTimestamp = _touchContext.StartTimestamp;
            _speed = default(Vector);
        }

        /// <summary>
        /// マウスボタンが離されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnTouchUp(object sender, TouchEventArgs e)
        {
            if (!_isTouchDown) return;
            if (e.TouchDevice != _touchContext.TouchDevice) return;

            // タッチジェスチャー判定
            TouchGesture gesture = TouchGesture.None;
            var touchPoint = e.GetTouchPoint(_context.Sender);

            // トータル移動距離
            var move = touchPoint.Position - _touchContext.StartPoint.Position;
            Debug.WriteLine($"Distance: {(int)move.Length}");


            // フリック判定 (1秒以内)
            if (e.Timestamp - _touchContext.StartTimestamp < 1000 && _speed.Length > 250.0 && move.Length > 32.0)
            {
                // 最終速度
                if (Math.Abs(_speed.X) < Math.Abs(_speed.Y))
                {
                    gesture = _speed.Y < 0.0 ? TouchGesture.FlickUp : TouchGesture.FlickDown;
                }
                else
                {
                    gesture = _speed.X < 0.0 ? TouchGesture.FlickLeft : TouchGesture.FlickRight;
                }
            }
            // タッチ判定
            else if (move.Length <= 8.0)
            {
                // タッチエリア 左右判定
                if (touchPoint.Position.X < _context.Sender.ActualWidth * 0.5)
                {
                    gesture = TouchGesture.TouchLeft;
                }
                else
                {
                    gesture = TouchGesture.TouchRight;
                }
            }

            // コマンド決定
            if (gesture != TouchGesture.None)
            {
                TouchGestureChanged?.Invoke(sender, new TouchGestureEventArgs(e, gesture));
            }

            // その後の操作は全て無効
            _isTouchDown = false;
        }


        // タッチジェスチャー判定
        private TouchGesture GetTouchGesture(object sender, TouchEventArgs e)
        {
            TouchGesture gesture = TouchGesture.None;

            var touchPoint = e.GetTouchPoint(_context.Sender);

            // トータル移動距離
            var move = touchPoint.Position - _touchContext.StartPoint.Position;
            Debug.WriteLine($"Distance: {(int)move.Length}");

            const double _touchLimitDistance = 16.0;
            const double _flickLimitSpeed = 250.0;
            const double _flickLimitDistance = 32.0;
            const int _flickLimitTime = 1000;

            // フリック判定 (1秒以内)
            if (e.Timestamp - _touchContext.StartTimestamp < _flickLimitTime && _speed.Length > _flickLimitSpeed && move.Length > _flickLimitDistance)
            {
                // 最終速度
                if (Math.Abs(_speed.X) < Math.Abs(_speed.Y))
                {
                    gesture = _speed.Y < 0.0 ? TouchGesture.FlickUp : TouchGesture.FlickDown;
                }
                else
                {
                    gesture = _speed.X < 0.0 ? TouchGesture.FlickLeft : TouchGesture.FlickRight;
                }
            }
            // タッチ判定
            else if (move.Length < _touchLimitDistance)
            {
                // タッチエリア 左右判定
                if (touchPoint.Position.X < _context.Sender.ActualWidth * 0.5)
                {
                    gesture = TouchGesture.TouchLeft;
                }
                else
                {
                    gesture = TouchGesture.TouchRight;
                }
            }

            return gesture;
        }


        private TouchPoint _prevPoint;
        private int _prevTimestamp;
        private Vector _speed;

        /// <summary>
        /// マウス移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnTouchMove(object sender, TouchEventArgs e)
        {
            if (!_isTouchDown) return;
            if (e.TouchDevice != _touchContext.TouchDevice) return;

            var touchPoint = e.GetTouchPoint(_context.Sender);
            var timestamp = e.Timestamp;

            // 経過時間が計測できない場合は計算しない
            if (timestamp <= _prevTimestamp) return;

            // 速度計算 (dot/sec)
            var speed = (touchPoint.Position - _prevPoint.Position) * 1000.0 / (timestamp - _prevTimestamp);

            // 速度は平均していく
            _speed = (_speed + speed) * 0.5;

            ////Debug.WriteLine($"TouchSpeed({timestamp - _prevTimestamp}): {(int)_speed.X}, {(int)_speed.Y}");

            _prevPoint = touchPoint;
            _prevTimestamp = timestamp;
        }



    }
}
