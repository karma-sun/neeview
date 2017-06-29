// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Windows;

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
    /// ジェスチャー生成
    /// </summary>
    public class MouseGestureSequenceTracker
    {
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

        private double _gestureMinimumDistanceX = 30.0;
        private double _gestureMinimumDistanceY = 30.0;

        private MouseGestureSequence _sequence;
        private MouseGestureDirection _direction;

        private Point _origin;

        /// <summary>
        /// コンストラクター
        /// </summary>
        public MouseGestureSequenceTracker()
        {
            _sequence = new MouseGestureSequence();
            _sequence.CollectionChanged += (s, e) => GestureProgressed.Invoke(this, new MouseGestureEventArgs(_sequence));
        }


        /// <summary>
        /// ジェスチャー進捗通知
        /// </summary>
        public event EventHandler<MouseGestureEventArgs> GestureProgressed;


        /// <summary>
        /// ジェスチャーシーケンス
        /// </summary>
        public MouseGestureSequence Sequence => _sequence;

        /// <summary>
        /// GestureMinimumDistanceX property.
        /// </summary>
        public double GestureMinimumDistanceX
        {
            get { return _gestureMinimumDistanceX; }
            set { _gestureMinimumDistanceX = Math.Max(value, SystemParameters.MinimumHorizontalDragDistance); }
        }

        /// <summary>
        /// GestureMinimumDistanceY property.
        /// </summary>
        public double GestureMinimumDistanceY
        {
            get { return _gestureMinimumDistanceY; }
            set { _gestureMinimumDistanceY = Math.Max(value, SystemParameters.MinimumVerticalDragDistance); }
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public void Reset(Point point)
        {
            _direction = MouseGestureDirection.None;
            _sequence.Clear();

            _origin = point;
        }


        /// <summary>
        /// 入力更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Move(Point point)
        {
            var v1 = point - _origin;

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
                        _sequence.Add(_direction);
                        break;
                    }
                }
            }

            // 開始点の更新
            _origin = point;
        }

    }
}
