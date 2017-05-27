// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// ジェスチャーイベントデータ
    /// </summary>
    public class MouseGestureEventArgs
    {
        /// <summary>
        /// ジェスチャー
        /// </summary>
        public MouseGestureSequence Sequence { get; set; }

        /// <summary>
        /// 処理済フラグ
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="sequence"></param>
        public MouseGestureEventArgs(MouseGestureSequence sequence)
        {
            Sequence = sequence;
        }
    }

    /// <summary>
    /// マウスジェスチャー
    /// </summary>
    public class MouseInputGesture : MouseInputBase
    {
        /// <summary>
        /// ジェスチャー進捗通知
        /// </summary>
        public event EventHandler<MouseGestureEventArgs> MouseGestureProgressed;

        /// <summary>
        /// ジェスチャー確定通知
        /// </summary>
        public event EventHandler<MouseGestureEventArgs> MouseGestureChanged;


        /// <summary>
        /// 現在のジェスチャーシーケンス
        /// </summary>
        private MouseGestureSequence _gesture;


        // ジェスチャー判定用最低ドラッグ距離
        public static double GestureMinimumDistanceX = 30.0;
        public static double GestureMinimumDistanceY = 30.0;

        /// <summary>
        /// 判定用最低ドラッグ距離設定
        /// TODO: 設定方法
        /// </summary>
        /// <param name="deltaX"></param>
        /// <param name="deltaY"></param>
        public void SetGestureMinimumDistance(double deltaX, double deltaY)
        {
            GestureMinimumDistanceX = deltaX;
            GestureMinimumDistanceY = deltaY;
            if (GestureMinimumDistanceX < SystemParameters.MinimumHorizontalDragDistance)
                GestureMinimumDistanceX = SystemParameters.MinimumHorizontalDragDistance;
            if (GestureMinimumDistanceY < SystemParameters.MinimumVerticalDragDistance)
                GestureMinimumDistanceY = SystemParameters.MinimumVerticalDragDistance;
        }


        /// <summary>
        /// 現在のジェスチャー方向
        /// </summary>
        private MouseGestureDirection _direction;

        /// <summary>
        /// ジェスチャー方向ベクトル
        /// </summary>
        private static Dictionary<MouseGestureDirection, Vector> s_gestureDirectionVector = new Dictionary<MouseGestureDirection, Vector>
        {
            [MouseGestureDirection.None] = new Vector(0, 0),
            [MouseGestureDirection.Up] = new Vector(0, -1),
            [MouseGestureDirection.Right] = new Vector(1, 0),
            [MouseGestureDirection.Down] = new Vector(0, 1),
            [MouseGestureDirection.Left] = new Vector(-1, 0)
        };


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public MouseInputGesture(MouseInputContext context) : base(context)
        {
            _gesture = new MouseGestureSequence();
            _gesture.CollectionChanged += (s, e) => MouseGestureProgressed.Invoke(this, new MouseGestureEventArgs(_gesture));
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public void Reset()
        {
            _direction = MouseGestureDirection.None;
            _gesture.Clear();
        }


        /// <summary>
        /// 状態開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public override void OnOpened(FrameworkElement sender, object parameter)
        {
            sender.CaptureMouse();
            sender.Cursor = null;
            Reset();
        }

        /// <summary>
        /// 状態終了
        /// </summary>
        /// <param name="sender"></param>
        public override void OnClosed(FrameworkElement sender)
        {
            sender.ReleaseMouseCapture();
        }

        /// <summary>
        /// ボタン押されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            UpdateState(sender, e);
            if (e.Handled) return;

            // 右ボタンのみジェスチャー終端として認識
            if (e.ChangedButton == MouseButton.Left && _gesture.Count > 0)
            {
                // ジェスチャーコマンド実行
                _gesture.Add(MouseGestureDirection.Click);
                var args = new MouseGestureEventArgs(_gesture);
                MouseGestureChanged?.Invoke(sender, args);
            }

            // ジェスチャー解除
            ResetState();
        }

        /// <summary>
        /// ボタン離されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            // ジェスチャーコマンド実行
            if (_gesture.Count > 0)
            {
                var args = new MouseGestureEventArgs(_gesture);
                MouseGestureChanged?.Invoke(sender, args);
                e.Handled = args.Handled;
            }

            // ジェスチャー解除
            ResetState();
        }

        /// <summary>
        /// ホイール処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // ホイール入力確定
            MouseWheelChanged?.Invoke(sender, e);

            // ジェスチャー解除
            if (e.Handled)
            {
                ResetState();
            }
        }

        private void UpdateState(object sender, MouseEventArgs e)
        {
            // ジェスチャー認識前に他のドラッグに切り替わったら処理を切り替える
            if (_gesture.Count > 0) return;

            var action = DragActionTable.Current.GetActionType(new DragKey(CreateMouseButtonBits(e), Keyboard.Modifiers));
            if (action == DragActionType.Gesture)
            {
            }
            else
            {
                SetState(MouseInputState.Drag);
                e.Handled = true;
            }
        }

        /// <summary>
        /// マウス移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseMove(object sender, MouseEventArgs e)
        {
            UpdateState(sender, e);
            if (e.Handled) return;

            var point = e.GetPosition(_context.Sender);

            var v1 = point - _context.StartPoint;

            // 一定距離未満は判定しない
            if (Math.Abs(v1.X) < GestureMinimumDistanceX && Math.Abs(v1.Y) < GestureMinimumDistanceY) return;

            // 方向を決める
            // 斜め方向は以前の方向とする
            if (_direction != MouseGestureDirection.None && Math.Abs(Vector.AngleBetween(s_gestureDirectionVector[_direction], v1)) < 60)
            {
                // そのまま
            }
            else
            {
                foreach (MouseGestureDirection direction in s_gestureDirectionVector.Keys)
                {
                    var v0 = s_gestureDirectionVector[direction];
                    var angle = Vector.AngleBetween(s_gestureDirectionVector[direction], v1);
                    if (direction != MouseGestureDirection.None && Math.Abs(Vector.AngleBetween(s_gestureDirectionVector[direction], v1)) < 30)
                    {
                        _direction = direction;
                        _gesture.Add(_direction);
                        break;
                    }
                }
            }

            // 開始点の更新
            _context.StartPoint = point;
        }
    }


}
