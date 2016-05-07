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
    /// 右ボタンドラッグでゼスチャ入力を行う
    /// ゼスチャシーケンス生成まで。
    /// </summary>
    public class MouseGestureController : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        // マウス入力受付コントロール
        private FrameworkElement _Sender;

        private bool _IsButtonDown = false;
        private bool _IsDragging = false;

        private bool _IsEnableClickEvent;

        private Point _StartPoint;
        private Point _EndPoint;

        MouseGestureDirection _Direction;

        // ゼスチャ方向ベクトル
        static Dictionary<MouseGestureDirection, Vector> GestureDirectionVector = new Dictionary<MouseGestureDirection, Vector>
        {
            [MouseGestureDirection.None] = new Vector(0, 0),
            [MouseGestureDirection.Up] = new Vector(0, -1),
            [MouseGestureDirection.Right] = new Vector(1, 0),
            [MouseGestureDirection.Down] = new Vector(0, 1),
            [MouseGestureDirection.Left] = new Vector(-1, 0)
        };

        // 現在のゼスチャシーケンス
        #region Property: Gesture
        private MouseGestureSequence _Gesture;
        public MouseGestureSequence Gesture
        {
            get { return _Gesture; }
            set { _Gesture = value; OnPropertyChanged(); }
        }
        #endregion

        // ゼスチャー判定用最低ドラッグ距離
        private double GestureMinimumDistanceX;
        private double GestureMinimumDistanceY;

        private void InitializeGestureMinimumDistance()
        {
            double.TryParse(ConfigurationManager.AppSettings.Get("GestureMinimumDistanceX"), out GestureMinimumDistanceX);
            double.TryParse(ConfigurationManager.AppSettings.Get("GestureMinimumDistanceY"), out GestureMinimumDistanceY);

            if (GestureMinimumDistanceX < SystemParameters.MinimumHorizontalDragDistance)
                GestureMinimumDistanceX = SystemParameters.MinimumHorizontalDragDistance;
            if (GestureMinimumDistanceY < SystemParameters.MinimumVerticalDragDistance)
                GestureMinimumDistanceY = SystemParameters.MinimumVerticalDragDistance;
        }


        // コンストラクタ
        public MouseGestureController(FrameworkElement sender)
        {
            InitializeGestureMinimumDistance();

            _Sender = sender;

            _Gesture = new MouseGestureSequence();
            _Gesture.CollectionChanged += (s, e) => MouseGestureUpdateEventHandler.Invoke(this, _Gesture);

            _Sender.PreviewMouseRightButtonDown += OnMouseButtonDown;
            _Sender.PreviewMouseRightButtonUp += OnMouseButtonUp;
            _Sender.PreviewMouseWheel += OnMouseWheel;
            _Sender.PreviewMouseMove += OnMouseMove;
        }


        // ゼスチャ リセット
        public void Reset()
        {
            _Direction = MouseGestureDirection.None;
            _Gesture.Clear();
        }

        // ボタンが押された時の処理
        private void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            _StartPoint = e.GetPosition(_Sender);

            _IsButtonDown = true;
            _IsDragging = false;
            _IsEnableClickEvent = true;

            Reset();

            _Sender.CaptureMouse();
        }

        // ジェスチャ状態が変化したことを通知
        public event EventHandler<MouseGestureSequence> MouseGestureUpdateEventHandler;

        // ジェスチャコマンド実行通知
        public event EventHandler<MouseGestureEventArgs> MouseGestureExecuteEventHandler;

        // ドラッグされずにクリック判定されたときの通知
        public event EventHandler<MouseButtonEventArgs> MouseClickEventHandler;

        // コンテキストメニュー有効フラグ
        public ContextMenuSetting ContextMenuSetting { get; set; }

        // ボタンが離された時の処理
        private void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_IsButtonDown)
            {
                e.Handled = true;
                return;
            }

            _IsButtonDown = false;

            _Sender.ReleaseMouseCapture();

            e.Handled = true;

            if (_IsEnableClickEvent)
            {
                if (_Gesture.Count > 0)
                {
                    var args = new MouseGestureEventArgs(_Gesture);
                    MouseGestureExecuteEventHandler?.Invoke(this, args);
                    e.Handled = args.Handled;
                }
                else if (!_IsDragging)
                {
                    if (ContextMenuSetting.IsEnabled && !ContextMenuSetting.IsOpenByCtrl)
                    {
                        e.Handled = false;
                    }
                    else if (ContextMenuSetting.IsEnabled && ContextMenuSetting.IsOpenByCtrl && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                    {
                        e.Handled = false;
                    }
                    else
                    { 
                        MouseClickEventHandler(sender, e);
                        e.Handled = true;
                    }
                }
            }
        }

        // マウスカーソルが移動した時の処理
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_IsButtonDown) return;

            _EndPoint = e.GetPosition(_Sender);

            if (!_IsDragging)
            {
                if (Math.Abs(_EndPoint.X - _StartPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(_EndPoint.Y - _StartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    _IsDragging = true;
                    _StartPoint = e.GetPosition(_Sender);
                }
                else
                {
                    return;
                }
            }

            DragMove(_StartPoint, _EndPoint);
        }

        // ホイールイベント処理
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // クリック系のイベントを無効にする
            _IsEnableClickEvent = false;
        }

        // 移動
        private void DragMove(Point start, Point end)
        {
            var v1 = _EndPoint - _StartPoint;

            // 一定距離未満は判定しない
            if (Math.Abs(v1.X) < GestureMinimumDistanceX && Math.Abs(v1.Y) < GestureMinimumDistanceY) return;

            // 方向を決める
            // 斜め方向は以前の方向とする

            if (_Direction != MouseGestureDirection.None && Math.Abs(Vector.AngleBetween(GestureDirectionVector[_Direction], v1)) < 60)
            {
                // そのまま
            }
            else
            {
                foreach (MouseGestureDirection direction in Enum.GetValues(typeof(MouseGestureDirection)))
                {
                    var v0 = GestureDirectionVector[direction];
                    var angle = Vector.AngleBetween(GestureDirectionVector[direction], v1);
                    if (direction != MouseGestureDirection.None && Math.Abs(Vector.AngleBetween(GestureDirectionVector[direction], v1)) < 30)
                    {
                        _Direction = direction;
                        _Gesture.Add(_Direction);
                        break;
                    }
                }
            }

            // 開始点の更新
            _StartPoint = _EndPoint;
        }
    }
}
