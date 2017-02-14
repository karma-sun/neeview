// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace NeeView
{
    /// <summary>
    /// 右ボタンドラッグでジェスチャー入力を行う
    /// ジェスチャーシーケンス生成まで。
    /// </summary>
    public class MouseGestureController : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        // マウス入力受付コントロール
        private FrameworkElement _sender;

        private bool _isButtonDown = false;
        private bool _isDragging = false;

        private bool _isEnableClickEvent;

        private Point _startPoint;
        private Point _endPoint;

        private MouseGestureDirection _direction;

        // ジェスチャー方向ベクトル
        private static Dictionary<MouseGestureDirection, Vector> s_gestureDirectionVector = new Dictionary<MouseGestureDirection, Vector>
        {
            [MouseGestureDirection.None] = new Vector(0, 0),
            [MouseGestureDirection.Up] = new Vector(0, -1),
            [MouseGestureDirection.Right] = new Vector(1, 0),
            [MouseGestureDirection.Down] = new Vector(0, 1),
            [MouseGestureDirection.Left] = new Vector(-1, 0)
        };

        // 現在のジェスチャーシーケンス
        #region Property: Gesture
        private MouseGestureSequence _gesture;
        public MouseGestureSequence Gesture
        {
            get { return _gesture; }
            set { _gesture = value; RaisePropertyChanged(); }
        }
        #endregion

        // ジェスチャー判定用最低ドラッグ距離
        private double _gestureMinimumDistanceX = 30.0;
        private double _gestureMinimumDistanceY = 30.0;

        public void InitializeGestureMinimumDistance(double deltaX, double deltaY)
        {
            _gestureMinimumDistanceX = deltaX;
            _gestureMinimumDistanceY = deltaY;
            if (_gestureMinimumDistanceX < SystemParameters.MinimumHorizontalDragDistance)
                _gestureMinimumDistanceX = SystemParameters.MinimumHorizontalDragDistance;
            if (_gestureMinimumDistanceY < SystemParameters.MinimumVerticalDragDistance)
                _gestureMinimumDistanceY = SystemParameters.MinimumVerticalDragDistance;
        }


        // コンストラクタ
        public MouseGestureController(FrameworkElement sender)
        {
            _sender = sender;

            _gesture = new MouseGestureSequence();
            _gesture.CollectionChanged += (s, e) => MouseGestureUpdateEventHandler.Invoke(this, _gesture);

            _sender.PreviewMouseDown += OnMouseButtonDown;
            _sender.PreviewMouseUp += OnMouseButtonUp;
            _sender.PreviewMouseWheel += OnMouseWheel;
            _sender.PreviewMouseMove += OnMouseMove;
        }



        // ジェスチャー リセット
        public void Reset()
        {
            _direction = MouseGestureDirection.None;
            _gesture.Clear();
        }

        // ボタン入力リセット
        public void ResetInput()
        {
            _isButtonDown = false;
        }

        // ボタンが押された時の処理
        private void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                OnMoouseRightButtonUp(sender, e);
            }
        }

        //
        private void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                OnMouseRightButtonDown(sender, e);
            }
            else if (e.ChangedButton == MouseButton.Left)
            {
                OnMouseLeftButtonDown(sender, e);
            }
        }


        // ボタンが押された時の処理
        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(_sender);

            _isButtonDown = true;
            _isDragging = false;
            _isEnableClickEvent = true;

            Reset();

            _sender.CaptureMouse();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isButtonDown)
            {
                return;
            }

            // 後処理
            _isButtonDown = false;

            _sender.ReleaseMouseCapture();

            // なんらかの理由でキャンセルされた？
            if (!_isEnableClickEvent) return;

            // ジェスチャーコマンド実行
            _gesture.Add(MouseGestureDirection.Click);
            var args = new MouseGestureEventArgs(_gesture);
            MouseGestureExecuteEventHandler?.Invoke(this, args);
            e.Handled = true;
        }

        // ジェスチャー状態が変化したことを通知
        public event EventHandler<MouseGestureSequence> MouseGestureUpdateEventHandler;

        // ジェスチャーコマンド実行通知
        public event EventHandler<MouseGestureEventArgs> MouseGestureExecuteEventHandler;

        // ドラッグされずにクリック判定されたときの通知
        public event EventHandler<MouseButtonEventArgs> MouseClickEventHandler;

        // コンテキストメニュー有効フラグ
        public ContextMenuSetting ContextMenuSetting { get; set; }

        // ボタンが離された時の処理
        private void OnMoouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isButtonDown)
            {
                e.Handled = true; // なんかおかしい？
                return;
            }

            // 後処理
            _isButtonDown = false;

            _sender.ReleaseMouseCapture();

            // なんらかの理由でキャンセルされた？
            if (!_isEnableClickEvent) return;

            // ジェスチャーコマンド実行
            if (_gesture.Count > 0)
            {
                var args = new MouseGestureEventArgs(_gesture);
                MouseGestureExecuteEventHandler?.Invoke(this, args);
                e.Handled = args.Handled;
            }

            // ドラッグ前ならば右クリックとして処理
            else if (!_isDragging)
            {
                MouseClickEventHandler(sender, e);

            }
        }



        // マウスカーソルが移動した時の処理
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isButtonDown) return;

            _endPoint = e.GetPosition(_sender);

            if (!_isDragging)
            {
                if (Math.Abs(_endPoint.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(_endPoint.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    _isDragging = true;
                    _startPoint = e.GetPosition(_sender);
                }
                else
                {
                    return;
                }
            }

            DragMove(_startPoint, _endPoint);
        }

        // ホイールイベント処理
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // クリック系のイベントを無効にする
            _isEnableClickEvent = false;
        }

        // 移動
        private void DragMove(Point start, Point end)
        {
            var v1 = _endPoint - _startPoint;

            // 一定距離未満は判定しない
            if (Math.Abs(v1.X) < _gestureMinimumDistanceX && Math.Abs(v1.Y) < _gestureMinimumDistanceY) return;

            // 方向を決める
            // 斜め方向は以前の方向とする

            if (_direction != MouseGestureDirection.None && Math.Abs(Vector.AngleBetween(s_gestureDirectionVector[_direction], v1)) < 60)
            {
                // そのまま
            }
            else
            {
                foreach (MouseGestureDirection direction in s_gestureDirectionVector.Keys) // Enum.GetValues(typeof(MouseGestureDirection)))
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
            _startPoint = _endPoint;
        }
    }
}
