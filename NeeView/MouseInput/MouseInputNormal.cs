// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// マウス通常入力状態
    /// </summary>
    public class MouseInputNormal : MouseInputBase
    {
        /// <summary>
        /// ボタン押されている？
        /// </summary>
        private bool _isButtonDown;

        /// <summary>
        /// 長押し判定用タイマー
        /// </summary>
        private DispatcherTimer _timer = new DispatcherTimer();

        /// <summary>
        /// 長押し有効？
        /// </summary>
        public LongButtonDownMode LongLeftButtonDownMode { get; internal set; }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="context"></param>
        public MouseInputNormal(MouseInputContext context) : base(context)
        {
            _timer.Interval = TimeSpan.FromMilliseconds(1000);
            _timer.Tick += OnTimeout;
        }

        /// <summary>
        /// マウスボタンが一定時間押され続けた時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimeout(object sender, object e)
        {
            _timer.Stop();

            if (CreateMouseButtonBits() == MouseButtonBits.LeftButton && Keyboard.Modifiers == ModifierKeys.None)
            {
                // 左ボタン単体長押しならルーペモードへ
                if (LongLeftButtonDownMode == LongButtonDownMode.Loupe)
                {
                    SetState(MouseInputState.Loupe, true);
                }
            }
        }

        /// <summary>
        /// 状態開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public override void OnOpened(FrameworkElement sender, object parameter)
        {
            _isButtonDown = false;
            sender.Cursor = null;
        }

        /// <summary>
        /// 状態終了
        /// </summary>
        /// <param name="sender"></param>
        public override void OnClosed(FrameworkElement sender)
        {
            _timer.Stop();
        }


        /// <summary>
        /// マウスボタンが押されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isButtonDown = true;
            _context.Sender.Focus();

            _context.StartPoint = e.GetPosition(_context.Sender);

            // ダブルクリック？
            if (e.ClickCount >= 2)
            {
                // コマンド決定
                MouseButtonChanged?.Invoke(sender, e);
                if (e.Handled)
                {
                    // その後の操作は全て無効
                    _isButtonDown = false;

                    _timer.Stop();

                    return;
                }
            }

            // 長押し判定開始
            _timer.Start();
        }
        
        /// <summary>
        /// マウスボタンが離されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isButtonDown) return;

            // コマンド決定
            // 離されたボタンがメインキー、それ以外は装飾キー
            MouseButtonChanged?.Invoke(sender, e);

            // その後の操作は全て無効
            _isButtonDown = false;

            _timer.Stop();
        }

        /// <summary>
        /// マウスホイール処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // コマンド決定
            // ホイールがメインキー、それ以外は装飾キー
            MouseWheelChanged?.Invoke(sender, e);

            // その後の操作は全て無効
            _isButtonDown = false;

            _timer.Stop();
        }


        /// <summary>
        /// マウス移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isButtonDown) return;

            var point = e.GetPosition(_context.Sender);

            var deltaX = Math.Abs(point.X - _context.StartPoint.X);
            var deltaY = Math.Abs(point.Y - _context.StartPoint.Y);

            // drag check
            if (deltaX > SystemParameters.MinimumHorizontalDragDistance || deltaY > SystemParameters.MinimumVerticalDragDistance)
            {
                // ドラッグ開始。処理をドラッグ系に移行
                var action = ModelContext.DragActionTable.GetActionType(new DragKey(CreateMouseButtonBits(e), Keyboard.Modifiers));
                if (action == DragActionType.Gesture)
                {
                    SetState(MouseInputState.Gesture);
                }
                else
                {
                    SetState(MouseInputState.Drag);
                }
            }
        }
    }
}
