// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

// TODO: 整備
// TODO: 関数が大きすぎる？細分化を検討

namespace NeeView
{
    // 回転、拡大操作の中心
    public enum DragControlCenter
    {
        View, // ビューエリアの中心
        Target, // コンテンツの中心
    }

    public enum TransformChangeType
    {
        None,
        Position,
        Angle,
        Scale,
        LoupeScale,
    };


    // 値変化の原因
    public enum TransformActionType
    {
        None,
        Reset,
        Angle,
        Scale,
        FlipHorizontal,
        FlipVertical,
        LoupeScale,
    }

    // 変化通知イベントの引数
    public class TransformEventArgs : EventArgs
    {
        /// <summary>
        /// 変化の原因となった操作
        /// </summary>
        public TransformChangeType ChangeType { get; set; }

        /// <summary>
        /// 変化したもの
        /// </summary>
        public TransformActionType ActionType { get; set; }

        public double Scale { get; set; } = 1.0;
        public double LoupeScale { get; set; } = 1.0;
        public double Angle { get; set; }
        public bool IsFlipHorizontal { get; set; }
        public bool IsFlipVertical { get; set; }

        //
        public TransformEventArgs(TransformChangeType changeType, TransformActionType actionType)
        {
            ChangeType = changeType;
            ActionType = actionType;
        }
    }


    /// <summary>
    /// ドラッグ操作
    /// </summary>
    public class MouseInputDrag : MouseInputBase, INotifyPropertyChanged
    {
        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        // 角度、スケール変更イベント
        public event EventHandler<TransformEventArgs> TransformChanged;



        // 移動アニメーション有効フラグ(内部管理)
        private bool _isEnableTranslateAnimation;

        // 移動アニメーション中フラグ(内部管理)
        private bool _isTranslateAnimated;

        // コンテンツの平行移動行列。アニメーション用。
        private TranslateTransform _translateTransform;

        // 変化要因
        private TransformActionType _actionType;


        // コンテンツの座標 (アニメーション対応)
        #region Property: Position
        private Point _position;
        public Point Position
        {
            get { return _position; }
            set
            {
                if (_isEnableTranslateAnimation)
                {
                    Duration duration = TimeSpan.FromMilliseconds(100); // 100msアニメ

                    if (!_isTranslateAnimated)
                    {
                        // 開始
                        _isTranslateAnimated = true;
                        _translateTransform.BeginAnimation(TranslateTransform.XProperty,
                            new DoubleAnimation(_position.X, value.X, duration), HandoffBehavior.SnapshotAndReplace);
                        _translateTransform.BeginAnimation(TranslateTransform.YProperty,
                            new DoubleAnimation(_position.Y, value.Y, duration), HandoffBehavior.SnapshotAndReplace);
                    }
                    else
                    {
                        // 継続
                        _translateTransform.BeginAnimation(TranslateTransform.XProperty,
                            new DoubleAnimation(value.X, duration), HandoffBehavior.Compose);
                        _translateTransform.BeginAnimation(TranslateTransform.YProperty,
                            new DoubleAnimation(value.Y, duration), HandoffBehavior.Compose);
                    }
                }
                else
                {
                    if (_isTranslateAnimated)
                    {
                        // 解除
                        _translateTransform.ApplyAnimationClock(TranslateTransform.XProperty, null);
                        _translateTransform.ApplyAnimationClock(TranslateTransform.YProperty, null);
                        _isTranslateAnimated = false;
                    }
                }

                _position = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        private Point _defaultPosition;

        // コンテンツの角度
        #region Property: Angle
        private double _angle;
        public double Angle
        {
            get { return _angle; }
            set
            {
                _angle = value;
                RaisePropertyChanged();

                var args = new TransformEventArgs(TransformChangeType.Angle, _actionType)
                {
                    Scale = Scale,
                    Angle = Angle,
                    IsFlipHorizontal = IsFlipHorizontal,
                    IsFlipVertical = IsFlipVertical
                };
                TransformChanged?.Invoke(this, args);
            }
        }
        #endregion

        private double _defaultAngle;

        // コンテンツの拡大率
        #region Property: Scale
        private double _scale = 1.0;
        public double Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ScaleX));
                RaisePropertyChanged(nameof(ScaleY));

                var args = new TransformEventArgs(TransformChangeType.Angle, _actionType)
                {
                    Scale = Scale,
                    Angle = Angle,
                    IsFlipHorizontal = IsFlipHorizontal,
                    IsFlipVertical = IsFlipVertical
                };
                TransformChanged?.Invoke(this, args);
            }
        }
        #endregion

        private double _defaultScale;

        // コンテンツのScaleX
        public double ScaleX
        {
            get { return _isFlipHorizontal ? -_scale : _scale; }
        }

        // コンテンツのScaleY
        public double ScaleY
        {
            get { return _isFlipVertical ? -_scale : _scale; }
        }

        // 左右反転
        #region Property: IsFlipHorizontal
        private bool _isFlipHorizontal;
        public bool IsFlipHorizontal
        {
            get { return _isFlipHorizontal; }
            set
            {
                if (_isFlipHorizontal != value)
                {
                    _isFlipHorizontal = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(ScaleX));
                }
            }
        }
        #endregion

        // 上下反転
        #region Property: IsFlipVertical
        private bool _isFlipVertical;
        public bool IsFlipVertical
        {
            get { return _isFlipVertical; }
            set
            {
                if (_isFlipVertical != value)
                {
                    _isFlipVertical = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(ScaleY));
                }
            }
        }
        #endregion



        public TransformGroup TransformView { get; private set; }
        public TransformGroup TransformCalc { get; private set; }


        //
        public MouseInputDrag(MouseInputContext context) : base(context)
        {
            this.TransformView = CreateTransformGroup();
            this.TransformCalc = CreateTransformGroup();

            _translateTransform = this.TransformView.Children.OfType<TranslateTransform>().First();
        }


        // パラメータとトランスフォームを対応させる
        private TransformGroup CreateTransformGroup()
        {
            var scaleTransform = new ScaleTransform();
            BindingOperations.SetBinding(scaleTransform, ScaleTransform.ScaleXProperty, new Binding(nameof(ScaleX)) { Source = this });
            BindingOperations.SetBinding(scaleTransform, ScaleTransform.ScaleYProperty, new Binding(nameof(ScaleY)) { Source = this });

            var rotateTransform = new RotateTransform();
            BindingOperations.SetBinding(rotateTransform, RotateTransform.AngleProperty, new Binding(nameof(Angle)) { Source = this });

            var translateTransform = new TranslateTransform();
            BindingOperations.SetBinding(translateTransform, TranslateTransform.XProperty, new Binding("Position.X") { Source = this });
            BindingOperations.SetBinding(translateTransform, TranslateTransform.YProperty, new Binding("Position.Y") { Source = this });

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(scaleTransform);
            transformGroup.Children.Add(rotateTransform);
            transformGroup.Children.Add(translateTransform);

            return transformGroup;
        }


        // 開始時の基準
        public DragViewOrigin ViewOrigin { get; set; }

        // ウィンドウ枠内の移動に制限するフラグ
        private bool _isLimitMove;
        public bool IsLimitMove
        {
            get { return _isLimitMove; }
            set
            {
                if (_isLimitMove == value) return;
                _isLimitMove = value;
                _lockMoveX = (_lockMoveX || Position.X == 0) & _isLimitMove;
                _lockMoveY = (_lockMoveY || Position.Y == 0) & _isLimitMove;
            }
        }

        // X方向の移動制限フラグ
        private bool _lockMoveX;

        // Y方向の移動制限フラグ
        private bool _lockMoveY;


        // 水平スクロールの正方向
        public double ViewHorizontalDirection { get; set; } = 1.0;

        // 初期化
        // コンテンツ切り替わり時等
        public void Reset(bool isResetScale, bool isResetAngle, bool isResetFlip, double angle)
        {
            _actionType = TransformActionType.Reset;

            _lockMoveX = IsLimitMove;
            _lockMoveY = IsLimitMove;

            if (isResetAngle)
            {
                Angle = angle;
            }
            if (isResetScale)
            {
                Scale = 1.0;
            }
            if (isResetFlip)
            {
                IsFlipHorizontal = false;
                IsFlipVertical = false;
            }

            if (ViewOrigin == DragViewOrigin.Center)
            {
                Position = new Point(0, 0);
            }
            else
            {
                // レイアウト更新
                _context.Sender.UpdateLayout();

                var area = GetArea();
                var pos = new Point(0, 0);
                if (area.Target.Height > area.View.Height)
                {
                    var verticalDirection = (ViewOrigin == DragViewOrigin.LeftTop || ViewOrigin == DragViewOrigin.RightTop) ? 1.0 : -1.0;
                    pos.Y = (area.Target.Height - area.View.Height) * 0.5 * verticalDirection;
                }
                if (area.Target.Width > area.View.Width)
                {
                    var horizontalDirection = (ViewOrigin == DragViewOrigin.LeftTop || ViewOrigin == DragViewOrigin.LeftBottom) ? 1.0 : -1.0;
                    pos.X = (area.Target.Width - area.View.Width) * 0.5 * horizontalDirection;
                }
                Position = pos;
            }

            _defaultPosition = Position;
            _defaultScale = Scale;
            _defaultAngle = Angle;
        }

        // 最後にリセットした値に戻す(角度以外)
        public void ResetDefault()
        {
            Scale = _defaultScale;
            Position = _defaultPosition;
            //_lockMoveX = IsLimitMove;
            //_lockMoveY = IsLimitMove;
        }

        /// <summary>
        /// 表示コンテンツのエリア情報取得
        /// </summary>
        /// <returns></returns>
        private DragArea GetArea()
        {
            return new DragArea(_context.Sender, _context.TargetShadow);
        }

        // ビューエリアサイズ変更に追従する
        public void SnapView()
        {
            if (!IsLimitMove) return;

            // レイアウト更新
            _context.Sender.UpdateLayout();

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


        #region Scroll method

        // スクロール↑コマンド
        // 縦方向にスクロールできない場合、横方向にスクロールする
        public void ScrollUp(double rate)
        {
            _isEnableTranslateAnimation = true;

            UpdateLock();
            if (!_lockMoveY)
            {
                DoMove(new Vector(0, _context.Sender.ActualHeight * rate));
            }
            else
            {
                DoMove(new Vector(_context.Sender.ActualWidth * rate * ViewHorizontalDirection, 0));
            }

            _isEnableTranslateAnimation = false;
        }

        // スクロール↓コマンド
        // 縦方向にスクロールできない場合、横方向にスクロールする
        public void ScrollDown(double rate)
        {
            _isEnableTranslateAnimation = true;

            UpdateLock();
            if (!_lockMoveY)
            {
                DoMove(new Vector(0, _context.Sender.ActualHeight * -rate));
            }
            else
            {
                DoMove(new Vector(_context.Sender.ActualWidth * -rate * ViewHorizontalDirection, 0));
            }

            _isEnableTranslateAnimation = false;
        }

        /// <summary>
        /// N字スクロール
        /// </summary>
        /// <param name="direction">次方向:+1 / 前方向:-1</param>
        /// <param name="bookReadDirection">右開き:+1 / 左開き:-1</param>
        /// <param name="allowVerticalScroll">縦方向スクロール許可</param>
        /// <param name="margin">最小移動距離</param>
        /// <param name="isAnimate">スクロールアニメ</param>
        /// <returns>スクロールしたか</returns>
        public bool ScrollN(int direction, int bookReadDirection, bool allowVerticalScroll, double margin, bool isAnimate)
        {
            var delta = GetNScrollDelta(direction, bookReadDirection, allowVerticalScroll, margin);

            if (delta.X != 0.0 || delta.Y != 0.0)
            {
                ////Debug.WriteLine(delta);
                UpdateLock();

                if (isAnimate) _isEnableTranslateAnimation = true;
                DoMove(new Vector(delta.X, delta.Y));
                if (isAnimate) _isEnableTranslateAnimation = false;

                return true;
            }
            else
            {
                return false;
            }
        }

        // N字スクロール：スクロール距離を計算
        private Point GetNScrollDelta(int direction, int bookReadDirection, bool allowVerticalScroll, double margin)
        {
            Point delta = new Point();

            var area = GetArea();

            if (allowVerticalScroll)
            {
                if (direction > 0)
                {
                    delta.Y = GetNScrollVerticalToBottom(area, margin);
                }
                else
                {
                    delta.Y = GetNScrollVerticalToTop(area, margin);
                }
            }

            if (delta.Y == 0.0)
            {
                if (direction * bookReadDirection > 0)
                {
                    delta.X = GetNScrollHorizontalToLeft(area, margin);
                }
                else
                {
                    delta.X = GetNScrollHorizontalToRight(area, margin);
                }

                if (delta.X != 0.0)
                {
                    if (direction > 0)
                    {
                        delta.Y = GetNScrollVerticalMoveToTop(area);
                    }
                    else
                    {
                        delta.Y = GetNScrollVerticalMoveToBottom(area);
                    }
                }
            }

            return delta;
        }

        // N字スクロール：上方向スクロール距離取得
        private double GetNScrollVerticalToTop(DragArea area, double margin)
        {
            if (area.Over.Top < -margin)
            {
                double dy = Math.Abs(area.Over.Top);
                if (dy > area.View.Height) dy = area.View.Height;
                return dy;
            }
            else
            {
                return 0.0;
            }
        }

        // N字スクロール：下方向スクロール距離取得
        private double GetNScrollVerticalToBottom(DragArea area, double margin)
        {
            if (area.Over.Bottom > margin)
            {
                double dy = Math.Abs(area.Over.Bottom);
                if (dy > area.View.Height) dy = area.View.Height;
                return -dy;
            }
            else
            {
                return 0.0;
            }
        }

        // N字スクロール：上端までの移動距離取得
        private double GetNScrollVerticalMoveToTop(DragArea area)
        {
            return Math.Abs(area.Over.Top);
        }

        // N字スクロール：下端までの移動距離取得
        private double GetNScrollVerticalMoveToBottom(DragArea area)
        {
            return -Math.Abs(area.Over.Bottom);
        }

        // N字スクロール：左方向スクロール距離取得
        private double GetNScrollHorizontalToLeft(DragArea area, double margin)
        {
            if (area.Over.Left < -margin)
            {
                double dx = Math.Abs(area.Over.Left);
                if (dx > area.View.Width) dx = area.View.Width;
                return dx;
            }
            else
            {
                return 0.0;
            }
        }

        // N字スクロール：右方向スクロール距離取得
        private double GetNScrollHorizontalToRight(DragArea area, double margin)
        {
            if (area.Over.Right > margin)
            {
                double dx = Math.Abs(area.Over.Right);
                if (dx > area.View.Width) dx = area.View.Width;
                return -dx;
            }
            else
            {
                return 0.0;
            }
        }

        #endregion

        #region Scale method
        // 拡大コマンド
        public void ScaleUp(double scaleDelta)
        {
            _baseScale = Scale;
            _basePosition = Position;
            DoScale(_baseScale * (1.0 + scaleDelta));
        }

        // 縮小コマンド
        public void ScaleDown(double scaleDelta)
        {
            _baseScale = Scale;
            _basePosition = Position;
            DoScale(_baseScale / (1.0 + scaleDelta));
        }
        #endregion

        #region Rotate method
        // 回転コマンド
        public void Rotate(double angle)
        {
            _baseAngle = Angle;
            _basePosition = Position;
            DoRotate(NormalizeLoopRange(_baseAngle + angle, -180, 180));
        }
        #endregion

        #region Flip method
        // 反転コマンド
        public void ToggleFlipHorizontal()
        {
            DoFlipHorizontal(!IsFlipHorizontal);
        }

        // 反転コマンド
        public void FlipHorizontal(bool isFlip)
        {
            DoFlipHorizontal(isFlip);
        }

        // 反転コマンド
        public void ToggleFlipVertical()
        {
            DoFlipVertical(!IsFlipVertical);
        }

        // 反転コマンド
        public void FlipVertical(bool isFlip)
        {
            DoFlipVertical(isFlip);
        }
        #endregion



        public override void OnOpened(FrameworkElement sender, object parameter)
        {
            sender.CaptureMouse();
            sender.Cursor = Cursors.Hand;
            _action = null;

            InitializeDragParameter(_context.StartPoint);
        }

        public override void OnClosed(FrameworkElement sender)
        {
            sender.ReleaseMouseCapture();
        }

        public override void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            // nop.
        }

        public override void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            // ドラッグ解除
            if (CreateMouseButtonBits(e) == MouseButtonBits.None)
            {
                ResetState();
            }
        }

        //
        public override void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // コマンド実行
            MouseWheelChanged?.Invoke(sender, e);

            // ドラッグ解除
            if (e.Handled)
            {
                ResetState();
            }
        }

        //
        public override void OnMouseMove(object sender, MouseEventArgs e)
        {
            var dragKey = new DragKey(CreateMouseButtonBits(e), Keyboard.Modifiers);
            if (!dragKey.IsValid) return;

            var point = e.GetPosition(_context.Sender);

            // update action
            if (_action == null || _action.DragKey != dragKey)
            {
                var action = ModelContext.DragActionTable.GetAction(dragKey);

                if (action != _action && action?.Exec != null)
                {
                    _action = action;
                    InitializeDragParameter(point);
                }
            }

            // exec action
            _action?.Exec?.Invoke(_context.StartPoint, point);
        }

        #region Actions

        // ドラッグアクション
        private DragAction _action;

        // ドラッグパラメータ初期化
        private void InitializeDragParameter(Point pos)
        {
            _context.StartPoint = pos;
            _baseFlipPoint = _context.StartPoint;
            var windowDiff = _context.Window.PointToScreen(new Point(0, 0)) - new Point(_context.Window.Left, _context.Window.Top);
            _startPointFromWindow = _context.Sender.TranslatePoint(_context.StartPoint, _context.Window) + windowDiff;

            if (DragControlCenter == DragControlCenter.View)
            {
                _center = new Point(_context.Sender.ActualWidth * 0.5, _context.Sender.ActualHeight * 0.5);
            }
            else
            {
                _center = (Point)(_context.TargetShadow.PointToScreen(
                    new Point(_context.TargetShadow.ActualWidth * _context.TargetShadow.RenderTransformOrigin.X, _context.TargetShadow.ActualHeight * _context.TargetShadow.RenderTransformOrigin.Y))
                    - _context.Sender.PointToScreen(new Point(0, 0)));
            }

            _basePosition = Position;
            _baseAngle = Angle;
            _baseScale = Scale;
        }

        /// <summary>
        /// 操作の中心座標
        /// </summary>
        private Point _center;




        // 移動制限更新
        // ビューエリアサイズを超える場合、制限をOFFにする
        private void UpdateLock()
        {
            var area = GetArea();

            double margin = 0.1;

            if (_lockMoveX)
            {
                if (area.Over.Left + margin < 0 || area.Over.Right - margin > 0)
                {
                    _lockMoveX = false;
                }
            }
            if (_lockMoveY)
            {
                if (area.Over.Top + margin < 0 || area.Over.Bottom - margin > 0)
                {
                    _lockMoveY = false;
                }
            }
        }

        #endregion

        #region Drag Move

        //
        private Point _basePosition;

        // 移動
        public void DragMove(Point start, Point end)
        {
            DragMoveEx(start, end, 1.0);
        }

        // 移動(速度スケール依存)
        public void DragMoveScale(Point start, Point end)
        {
            var area = GetArea();
            var scaleX = area.Target.Width / area.View.Width;
            var scaleY = area.Target.Height / area.View.Height;
            var scale = scaleX > scaleY ? scaleX : scaleY;
            scale = scale < 1.0 ? 1.0 : scale;

            DragMoveEx(start, end, scale);
        }

        private void DragMoveEx(Point start, Point end, double scale)
        {
            var pos0 = Position;
            var pos1 = (end - _context.StartPoint) * scale + _basePosition;
            var move = pos1 - pos0;

            DoMove(move);
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

            if (_lockMoveX)
            {
                move.X = 0;
            }
            if (_lockMoveY)
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
        }

        #endregion

        #region Drag Angle

        // 回転、拡縮の中心
        public DragControlCenter DragControlCenter { get; set; } = DragControlCenter.View;

        // 回転スナップ。0で無効
        public double AngleFrequency { get; set; } = 0;

        private double _baseAngle;

        // 回転
        public void DragAngle(Point start, Point end)
        {
            var v0 = start - _center;
            var v1 = end - _center;

            double angle = NormalizeLoopRange(_baseAngle + Vector.AngleBetween(v0, v1), -180, 180);

            DoRotate(angle);
        }

        // 回転実行
        private void DoRotate(double angle)
        {
            if (AngleFrequency > 0)
            {
                angle = Math.Floor((angle + AngleFrequency * 0.5) / AngleFrequency) * AngleFrequency;
            }

            _actionType = TransformActionType.Angle;
            Angle = angle;

            if (DragControlCenter == DragControlCenter.View)
            {
                RotateTransform m = new RotateTransform(Angle - _baseAngle);
                Position = m.Transform(_basePosition);
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

        #endregion

        #region Drag Scale

        // 拡縮スナップ。0で無効;
        public double SnapScale { get; set; } = 0;

        private double _baseScale;


        // 拡縮
        public void DragScale(Point start, Point end)
        {
            var v0 = start - _center;
            var v1 = end - _center;

            var scale1 = v1.Length / v0.Length * _baseScale;

            DoScale(scale1);
        }


        // 拡縮
        public void DragScaleSlider(Point start, Point end)
        {
            var scale1 = System.Math.Pow(2, (end.X - start.X) * 0.01) * _baseScale;
            DoScale(scale1);
        }



        // 拡縮実行
        private void DoScale(double scale)
        {
            if (SnapScale > 0)
            {
                scale = Math.Floor((scale + SnapScale * 0.5) / SnapScale) * SnapScale;
            }

            _actionType = TransformActionType.Scale;
            Scale = scale;

            if (DragControlCenter == DragControlCenter.View)
            {
                var rate = Scale / _baseScale;
                Position = new Point(_basePosition.X * rate, _basePosition.Y * rate);
            }
        }

        #endregion

        #region Drag Flip

        //
        private Point _baseFlipPoint;

        // 反転
        public void DragFlipHorizontal(Point start, Point end)
        {
            const double margin = 16;

            if (_baseFlipPoint.X + margin < end.X)
            {
                DoFlipHorizontal(true);
                _baseFlipPoint.X = end.X - margin;
            }
            else if (_baseFlipPoint.X - margin > end.X)
            {
                DoFlipHorizontal(false);
                _baseFlipPoint.X = end.X + margin;
            }
        }

        // 反転実行
        private void DoFlipHorizontal(bool isFlip)
        {
            if (IsFlipHorizontal != isFlip)
            {
                IsFlipHorizontal = isFlip;

                _actionType = TransformActionType.FlipHorizontal;

                // 角度を反転
                Angle = -NormalizeLoopRange(Angle, -180, 180);

                // 座標を反転
                if (DragControlCenter == DragControlCenter.View)
                {
                    Position = new Point(-Position.X, Position.Y);
                }
            }
        }


        // 反転
        public void DragFlipVertical(Point start, Point end)
        {
            const double margin = 16;

            if (_baseFlipPoint.Y + margin < end.Y)
            {
                DoFlipVertical(true);
                _baseFlipPoint.Y = end.Y - margin;
            }
            else if (_baseFlipPoint.Y - margin > end.Y)
            {
                DoFlipVertical(false);
                _baseFlipPoint.Y = end.Y + margin;
            }
        }

        // 反転実行
        private void DoFlipVertical(bool isFlip)
        {
            if (IsFlipVertical != isFlip)
            {
                IsFlipVertical = isFlip;

                _actionType = TransformActionType.FlipVertical;

                // 角度を反転
                Angle = 90 - NormalizeLoopRange(Angle + 90, -180, 180);

                // 座標を反転
                if (DragControlCenter == DragControlCenter.View)
                {
                    Position = new Point(Position.X, -Position.Y);
                }
            }
        }

        #endregion

        #region Drag Window

        private Point _startPointFromWindow;

        // ウィンドウ移動
        public void DragWindowMove(Point start, Point end)
        {
            if (_context.Window.WindowState == WindowState.Normal)
            {
                var pos = _context.Sender.PointToScreen(end) - _startPointFromWindow;
                _context.Window.Left = pos.X;
                _context.Window.Top = pos.Y;
            }
        }

        #endregion

    }
}
