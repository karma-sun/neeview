// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NeeView
{
    // 回転、拡大操作の中心
    public enum DragControlCenter
    {
        View, // ビューエリアの中心
        Target, // コンテンツの中心
    }


    /// <summary>
    /// マウスドラッグ管理
    /// マウスドラッグでのコンテンツの表示変換を行う。
    /// * 左クリック＋ドラッグ  -> 移動
    /// * Shift+左クリック＋ドラッグ -> 拡縮
    /// * Ctrl+左クリック＋ドラッグ -> 回転
    /// </summary>
    public class MouseDragController : INotifyPropertyChanged
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


        // 移動アニメーション有効フラグ(内部管理)
        private bool _IsEnableTranslateAnimation;

        // 移動アニメーション中フラグ(内部管理)
        private bool _IsTranslateAnimated;

        // コンテンツの平行移動行列。アニメーション用。
        private TranslateTransform _TranslateTransform;

        // コンテンツの座標 (アニメーション対応)
        #region Property: Position
        private Point _Position;
        public Point Position
        {
            get { return _Position; }
            set
            {
                if (_IsEnableTranslateAnimation)
                {
                    Duration duration = TimeSpan.FromMilliseconds(100); // 100msアニメ

                    if (!_IsTranslateAnimated)
                    {
                        // 開始
                        _IsTranslateAnimated = true;
                        _TranslateTransform.BeginAnimation(TranslateTransform.XProperty,
                            new DoubleAnimation(_Position.X, value.X, duration), HandoffBehavior.SnapshotAndReplace);
                        _TranslateTransform.BeginAnimation(TranslateTransform.YProperty,
                            new DoubleAnimation(_Position.Y, value.Y, duration), HandoffBehavior.SnapshotAndReplace);
                    }
                    else
                    {
                        // 継続
                        _TranslateTransform.BeginAnimation(TranslateTransform.XProperty,
                            new DoubleAnimation(value.X, duration), HandoffBehavior.Compose);
                        _TranslateTransform.BeginAnimation(TranslateTransform.YProperty,
                            new DoubleAnimation(value.Y, duration), HandoffBehavior.Compose);
                    }
                }
                else
                {
                    if (_IsTranslateAnimated)
                    {
                        // 解除
                        _TranslateTransform.ApplyAnimationClock(TranslateTransform.XProperty, null);
                        _TranslateTransform.ApplyAnimationClock(TranslateTransform.YProperty, null);
                        _IsTranslateAnimated = false;
                    }
                }

                _Position = value;
                OnPropertyChanged();
            }
        }
        #endregion

        // コンテンツの角度
        #region Property: Angle
        private double _Angle;
        public double Angle
        {
            get { return _Angle; }
            set { _Angle = value; OnPropertyChanged(); }
        }
        #endregion

        // コンテンツの拡大率
        #region Property: Scale
        private double _Scale = 1.0;
        public double Scale
        {
            get { return _Scale; }
            set
            {
                _Scale = value;
                OnPropertyChanged();
                ScaleChanged?.Invoke(this, _Scale);
            }
        }
        #endregion

        // 開始時の基準
        public DragViewOrigin ViewOrigin { get; set; }

        // ウィンドウ枠内の移動に制限するフラグ
        private bool _IsLimitMove;
        public bool IsLimitMove
        {
            get { return _IsLimitMove; }
            set
            {
                if (_IsLimitMove == value) return;
                _IsLimitMove = value;
                _LockMoveX = (_LockMoveX || Position.X == 0) & _IsLimitMove;
                _LockMoveY = (_LockMoveY || Position.Y == 0) & _IsLimitMove;
            }
        }

        // X方向の移動制限フラグ
        private bool _LockMoveX;

        // Y方向の移動制限フラグ
        private bool _LockMoveY;

        // 回転、拡縮の中心
        public DragControlCenter DragControlCenter { get; set; } = DragControlCenter.View;

        // 回転スナップ。0で無効
        public double SnapAngle { get; set; } = 45;

        // 拡縮スナップ。0で無効;
        public double SnapScale { get; set; } = 0;

        // マウス入力イベント受付コントロール。ビューエリア。
        private FrameworkElement _Sender;

        // 移動対象。コンテンツ。
        private FrameworkElement _Target;

        // 移動対象の影
        // 移動範囲計算用。_TargetViewと全く同じ大きさと座標で、アニメーションだけしない非表示コントロール。
        private FrameworkElement _TargetShadow;

        //
        private bool _IsButtonDown = false;
        private bool _IsDragging = false;
        private bool _IsDragAngle { get { return _DragMode == (1 << 0); } }
        private bool _IsDragScale { get { return _DragMode == (1 << 1); } }
        private int _DragMode;

        // クリックイベント
        // ドラッグされずにマウスボタンが離された時にに発行する
        public event EventHandler<MouseButtonEventArgs> MouseClickEventHandler;

        // スケール変更イベント
        public event EventHandler<double> ScaleChanged;


        bool _IsEnableClickEvent;

        private Point _StartPoint;
        private Point _EndPoint;
        private Point _BasePosition;
        private double _BaseAngle;
        private double _BaseScale;
        private Point _Center;


        /// <summary>
        ///  コンストラクタ
        /// </summary>
        /// <param name="sender">ビューエリア、マウスイベント受付コントロール</param>
        /// <param name="targetView">対象コンテンツ</param>
        /// <param name="targetShadow">対象コンテンツの影。計算用</param>
        public MouseDragController(FrameworkElement sender, FrameworkElement targetView, FrameworkElement targetShadow)
        {
            _Sender = sender;
            _Target = targetView;
            _TargetShadow = targetShadow;

            _Sender.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            _Sender.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
            _Sender.PreviewMouseWheel += OnMouseWheel;
            _Sender.PreviewMouseMove += OnMouseMove;

            BindTransform(_Target, true);
            BindTransform(_TargetShadow, false);
        }

        // パラメータとトランスフォームを対応させる
        private void BindTransform(FrameworkElement element, bool isView)
        {
            element.RenderTransformOrigin = new Point(0.5, 0.5);

            var scaleTransform = new ScaleTransform();
            BindingOperations.SetBinding(scaleTransform, ScaleTransform.ScaleXProperty, new Binding("Scale") { Source = this });
            BindingOperations.SetBinding(scaleTransform, ScaleTransform.ScaleYProperty, new Binding("Scale") { Source = this });

            var rotateTransform = new RotateTransform();
            BindingOperations.SetBinding(rotateTransform, RotateTransform.AngleProperty, new Binding("Angle") { Source = this });

            var translateTransform = new TranslateTransform();
            BindingOperations.SetBinding(translateTransform, TranslateTransform.XProperty, new Binding("Position.X") { Source = this });
            BindingOperations.SetBinding(translateTransform, TranslateTransform.YProperty, new Binding("Position.Y") { Source = this });

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(scaleTransform);
            transformGroup.Children.Add(rotateTransform);
            transformGroup.Children.Add(translateTransform);

            element.RenderTransform = transformGroup;

            if (isView)
            {
                _TranslateTransform = translateTransform;
            }
        }

        // クリックイベント登録クリア
        public void ClearClickEventHandler()
        {
            MouseClickEventHandler = null;
        }

        // 水平スクロールの正方向
        public double ViewHorizontalDirection { get; set; } = 1.0;

        // 初期化
        // コンテンツ切り替わり時等
        public void Reset()
        {
            _LockMoveX = IsLimitMove;
            _LockMoveY = IsLimitMove;

            Angle = 0;
            Scale = 1.0;

            if (ViewOrigin == DragViewOrigin.Center)
            {
                Position = new Point(0, 0);
            }
            else
            {
                // レイアウト更新
                _Sender.UpdateLayout();

                var area = GetArea();
                var pos = new Point(0, 0);
                if (area.Target.Height > area.View.Height)
                {
                    pos.Y = (area.Target.Height - area.View.Height) * 0.5;
                }
                if (area.Target.Width > area.View.Width)
                {
                    var horizontalDirection = (ViewOrigin == DragViewOrigin.LeftTop) ? 1.0 : -1.0;
                    pos.X = (area.Target.Width - area.View.Width) * 0.5 * horizontalDirection;
                }
                Position = pos;
            }
        }

        // ビューエリアサイズ変更に追従する
        public void SnapView()
        {
            if (!IsLimitMove) return;

            // レイアウト更新
            _Sender.UpdateLayout();

            double margin = 1.0;
            var area = GetArea();
            var pos = Position;

            // ウィンドウサイズ変更直後はrectのスクリーン座標がおかしい可能性があるのでPositionから計算しなおす
            var rect = new Rect()
            {
                X = pos.X - area.Target.Width * 0.5 + area.View.Width * 0.5,
                Y = pos.Y - area.Target.Height * 0.5 + area.View.Height * 0.5,
                Width = area.Target.Width,
                Height = area.Target.Height,
            };

            if (rect.Width <= area.View.Width + margin)
            {
                pos.X = 0;
            }
            else
            {
                if (rect.Left > 0)
                {
                    pos.X -= rect.Left;
                }
                else if (rect.Right < area.View.Width)
                {
                    pos.X += area.View.Width - rect.Right;
                }
            }

            if (rect.Height <= area.View.Height + margin)
            {
                pos.Y = 0;
            }
            else
            {
                if (rect.Top > 0)
                {
                    pos.Y -= rect.Top;
                }
                else if (rect.Bottom < area.View.Height)
                {
                    pos.Y += area.View.Height - rect.Bottom;
                }
            }

            Position = pos;
        }




        // スクロール↑コマンド
        // 縦方向にスクロールできない場合、横方向にスクロールする
        public void ScrollUp()
        {
            _IsEnableTranslateAnimation = true;

            UpdateLock();
            if (!_LockMoveY)
            {
                DoMove(new Vector(0, _Sender.ActualHeight * 0.25));
            }
            else
            {
                DoMove(new Vector(_Sender.ActualWidth * 0.25 * ViewHorizontalDirection, 0));
            }

            _IsEnableTranslateAnimation = false;
        }

        // スクロール↓コマンド
        // 縦方向にスクロールできない場合、横方向にスクロールする
        public void ScrollDown()
        {
            _IsEnableTranslateAnimation = true;

            UpdateLock();
            if (!_LockMoveY)
            {
                DoMove(new Vector(0, _Sender.ActualHeight * -0.25));
            }
            else
            {
                DoMove(new Vector(_Sender.ActualWidth * -0.25 * ViewHorizontalDirection, 0));
            }

            _IsEnableTranslateAnimation = false;
        }

        // スクロール←
        public bool ScrollLeft()
        {
            var area = GetArea();
            if (area.Over.Left < 0)
            {
                double dx = Math.Abs(area.Over.Left);
                if (dx > area.View.Width) dx = area.View.Width;

                UpdateLock();

                //_IsEnableTranslateAnimation = true;
                DoMove(new Vector(dx, 0));
                //_IsEnableTranslateAnimation = false;

                return true;
            }

            return false;
        }


        // スクロール→
        public bool ScrollRight()
        {
            var area = GetArea();
            if (area.Over.Right > 0)
            {
                double dx = Math.Abs(area.Over.Right);
                if (dx > area.View.Width) dx = area.View.Width;

                UpdateLock();

                //_IsEnableTranslateAnimation = true;
                DoMove(new Vector(-dx, 0));
                //_IsEnableTranslateAnimation = false;

                return true;
            }

            return false;
        }


        // 拡大コマンド
        public void ScaleUp()
        {
            _BaseScale = Scale;
            _BasePosition = Position;
            DoScale(1.2 * _BaseScale);
        }

        // 縮小コマンド
        public void ScaleDown()
        {
            _BaseScale = Scale;
            _BasePosition = Position;
            DoScale(0.8 * _BaseScale);
        }

        // 回転コマンド
        public void Rotate(double angle)
        {
            _BaseAngle = Angle;
            _BasePosition = Position;
            DoRotate(NormalizeLoopRange(_BaseAngle + angle, -180, 180));
        }


        // マウス左ボタンが押された時の処理
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _StartPoint = e.GetPosition(_Sender);

            _IsButtonDown = true;
            _IsDragging = false;
            _IsEnableClickEvent = true;

            _DragMode = 0;
            if ((Keyboard.Modifiers & ModifierKeys.Shift) > 0) _DragMode |= (1 << 0);
            if ((Keyboard.Modifiers & ModifierKeys.Control) > 0) _DragMode |= (1 << 1);

            _Sender.CaptureMouse();
        }


        // マウス左ボタンが離された時の処理
        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_IsButtonDown) return;

            _IsButtonDown = false;

            _Sender.ReleaseMouseCapture();

            if (_Sender.Cursor != Cursors.None)
            {
                _Sender.Cursor = null;
            }

            if (_IsEnableClickEvent && !_IsDragging && MouseClickEventHandler != null)
            {
                MouseClickEventHandler(sender, e);
            }
        }


        // マウスホイールの処理
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // クリック系のイベントを無効にする
            _IsEnableClickEvent = false;
        }

        // マウスポインタが移動した時の処理
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

                    if (DragControlCenter == DragControlCenter.View)
                    {
                        _Center = new Point(_Sender.ActualWidth * 0.5, _Sender.ActualHeight * 0.5);
                    }
                    else
                    {
                        _Center = (Point)(_TargetShadow.PointToScreen(new Point(_TargetShadow.ActualWidth * _TargetShadow.RenderTransformOrigin.X, _TargetShadow.ActualHeight * _TargetShadow.RenderTransformOrigin.Y)) - _Sender.PointToScreen(new Point(0, 0)));
                    }

                    _BasePosition = Position;
                    _BaseAngle = Angle;
                    _BaseScale = Scale;

                    _Sender.Cursor = Cursors.Hand;
                }
                else
                {
                    return;
                }
            }

            if (_IsDragAngle)
            {
                DragAngle(_StartPoint, _EndPoint);
            }
            else if (_IsDragScale)
            {
                DragScale(_StartPoint, _EndPoint);
            }
            else
            {
                DragMove(_StartPoint, _EndPoint);
            }
        }


        // 移動
        private void DragMove(Point start, Point end)
        {
            var pos0 = Position;
            var pos1 = _EndPoint - _StartPoint + _BasePosition;
            var move = pos1 - pos0;

            DoMove(move);
        }

        private DragArea GetArea()
        {
            return new DragArea(_Sender, _TargetShadow);
        }

        // 移動制限更新
        // ビューエリアサイズを超える場合、制限をOFFにする
        private void UpdateLock()
        {
            var area = GetArea();

            double margin = 0.1;

            if (_LockMoveX)
            {
                if (area.Over.Left + margin < 0 || area.Over.Right - margin > 0)
                {
                    _LockMoveX = false;
                }
            }
            if (_LockMoveY)
            {
                if (area.Over.Top + margin < 0 || area.Over.Bottom - margin > 0)
                {
                    _LockMoveY = false;
                }
            }
        }

        // 移動実行
        private void DoMove(Vector move)
        {
            var area = GetArea();
            var pos0 = Position;
            var pos1 = Position + move;

            var margin = new Point(
                area.Target.Width < area.View.Width ? 0 : area.Target.Width - area.View.Width,
                area.Target.Height < area.View.Height ? 0 : area.Target.Height - area.View.Height);

            UpdateLock();

            if (_LockMoveX)
            {
                move.X = 0;
            }
            if (_LockMoveY)
            {
                move.Y = 0;
            }

            if (IsLimitMove)
            {
                if (move.X < 0 && area.Target.Left + move.X < -margin.X)
                {
                    move.X = -margin.X - area.Target.Left;
                    if (move.X > 0) move.X = 0;
                }
                else if (move.X > 0 && area.Target.Right + move.X > area.View.Width + margin.X)
                {
                    move.X = area.View.Width + margin.X - area.Target.Right;
                    if (move.X < 0) move.X = 0;
                }
                if (move.Y < 0 && area.Target.Top + move.Y < -margin.Y)
                {
                    move.Y = -margin.Y - area.Target.Top;
                    if (move.Y > 0) move.Y = 0;
                }
                else if (move.Y > 0 && area.Target.Bottom + move.Y > area.View.Height + margin.Y)
                {
                    move.Y = area.View.Height + margin.Y - area.Target.Bottom;
                    if (move.Y < 0) move.Y = 0;
                }
            }

            Position = pos0 + move;

            _StartPoint += pos1 - Position;
        }

        // 回転
        public void DragAngle(Point start, Point end)
        {
            var v0 = start - _Center;
            var v1 = end - _Center;

            double angle = NormalizeLoopRange(_BaseAngle + Vector.AngleBetween(v0, v1), -180, 180);

            DoRotate(angle);
        }

        // 回転実行
        private void DoRotate(double angle)
        {
            if (SnapAngle > 0)
            {
                angle = Math.Floor((angle + SnapAngle * 0.5) / SnapAngle) * SnapAngle;
            }

            Angle = angle;

            if (DragControlCenter == DragControlCenter.View)
            {
                RotateTransform m = new RotateTransform(Angle - _BaseAngle);
                Position = m.Transform(_BasePosition);
            }
        }

        // 角度の正規化
        private double NormalizeLoopRange(double val, double min, double max)
        {
            if (min >= max) throw new ArgumentException("need min < max");

            if (val >= max)
            {
                return min + (val - min) % (max - min);
            }
            else if (val < min)
            {
                return max - (min - val) % (max - min);
            }
            else
            {
                return val;
            }
        }


        // 拡縮
        public void DragScale(Point start, Point end)
        {
            var v0 = start - _Center;
            var v1 = end - _Center;

            var scale1 = v1.Length / v0.Length * _BaseScale;

            DoScale(scale1);
        }

        // 拡縮実行
        private void DoScale(double scale)
        {
            if (SnapScale > 0)
            {
                scale = Math.Floor((scale + SnapScale * 0.5) / SnapScale) * SnapScale;
            }

            Scale = scale;

            if (DragControlCenter == DragControlCenter.View)
            {
                var rate = Scale / _BaseScale;
                Position = new Point(_BasePosition.X * rate, _BasePosition.Y * rate);
            }
        }
    }
}
