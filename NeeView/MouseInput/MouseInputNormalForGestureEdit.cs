// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// マウス通常入力状態（ジェスチャーエディット用）
    /// </summary>
    public class MouseInputNormalForGestureEdit : MouseInputBase
    {
        /// <summary>
        /// ボタン押されている？
        /// </summary>
        private bool _isButtonDown;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="context"></param>
        public MouseInputNormalForGestureEdit(MouseInput context) : base(context)
        {
        }

        /// <summary>
        /// 状態開始処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public override void OnOpened(FrameworkElement sender, object parameter)
        {
            _isButtonDown = false;
        }

        /// <summary>
        /// 状態終了処理
        /// </summary>
        /// <param name="sender"></param>
        public override void OnClosed(FrameworkElement sender)
        {
        }

        /// <summary>
        /// ボタン押されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isButtonDown = true;
            _context.StartPoint = e.GetPosition(_context.Sender);
        }


        /// <summary>
        /// ボタン離されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isButtonDown) return;

            // 入力決定
            MouseButtonChanged?.Invoke(sender, e);

            // その後の操作は全て無効
            _isButtonDown = false;
        }

        /// <summary>
        /// ホイール処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // 入力決定
            MouseWheelChanged?.Invoke(sender, e);

            // その後の操作は全て無効
            _isButtonDown = false;
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

            if (deltaX > SystemParameters.MinimumHorizontalDragDistance || deltaY > SystemParameters.MinimumVerticalDragDistance)
            {
                var action = DragActionTable.Current.GetActionType(new DragKey(CreateMouseButtonBits(e), Keyboard.Modifiers));
                if (action == DragActionType.Gesture)
                {
                    SetState(MouseInputState.Gesture);
                }
            }
        }
    }
}
