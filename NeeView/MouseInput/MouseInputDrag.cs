// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
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
    /// ドラッグ操作による変換
    /// </summary>
    public class DragTransform : BindableBase
    {
        public static DragTransform Current { get; private set; }


        // コンテンツの平行移動行列。アニメーション用。
        private TranslateTransform _translateTransform;


        //
        public DragTransform()
        {
            Current = this;

            this.TransformView = CreateTransformGroup();
            this.TransformCalc = CreateTransformGroup();

            _translateTransform = this.TransformView.Children.OfType<TranslateTransform>().First();
        }



        public TransformGroup TransformView { get; private set; }
        public TransformGroup TransformCalc { get; private set; }



        // 移動アニメーション有効フラグ(内部管理)
        private bool _isEnableTranslateAnimation;

        // 移動アニメーション中フラグ(内部管理)
        private bool _isTranslateAnimated;

        //
        public bool IsEnableTranslateAnimation
        {
            get { return _isEnableTranslateAnimation; }
            set { _isEnableTranslateAnimation = value; }
        }


        // コンテンツの座標 (アニメーション対応)
        private Point _position;
        public Point Position
        {
            get { return _position; }
            set
            {
                ////Debug.WriteLine($"Pos: {value}");

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

        // コンテンツの角度
        private double _angle;
        public double Angle
        {
            get { return _angle; }
            set
            {
                _angle = value;
                RaisePropertyChanged();

                /*
                var args = new TransformEventArgs(TransformChangeType.Angle, _actionType)
                {
                    Scale = Scale,
                    Angle = Angle,
                    IsFlipHorizontal = IsFlipHorizontal,
                    IsFlipVertical = IsFlipVertical
                };
                TransformChanged?.Invoke(this, args);
                */
            }
        }



        // コンテンツの拡大率
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

                /*
                var args = new TransformEventArgs(TransformChangeType.Scale, _actionType)
                {
                    Scale = Scale,
                    Angle = Angle,
                    IsFlipHorizontal = IsFlipHorizontal,
                    IsFlipVertical = IsFlipVertical
                };
                TransformChanged?.Invoke(this, args);
                */
            }
        }


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

        // 上下反転
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

    }


    /// <summary>
    /// ドラッグ操作
    /// </summary>
    public class MouseInputDrag : MouseInputBase
    {
        #region events

        // 角度、スケール変更イベント
        public event EventHandler<TransformEventArgs> TransformChanged;

        #endregion

        // View変換情報表示のスケール表示をオリジナルサイズ基準にする
        public bool IsOriginalScaleShowMessage { get; set; }

        // 回転、拡縮をコンテンツの中心基準にする
        public bool IsControlCenterImage { get; set; }

        // 拡大率キープ
        public bool IsKeepScale { get; set; }

        // 回転キープ
        public bool IsKeepAngle { get; set; }

        // 反転キープ
        public bool IsKeepFlip { get; set; }

        // 表示開始時の基準
        public bool IsViewStartPositionCenter { get; set; }



        private Point _defaultPosition;

        private double _defaultAngle;

        private double _defaultScale;




        private DragTransform _transform;

        //
        public MouseInputDrag(MouseInputContext context) : base(context)
        {
            _transform = context.DragTransform;
        }


        // 開始時の基準
        public DragViewOrigin ViewOrigin { get; set; }

        // ウィンドウ枠内の移動に制限するフラグ
        private bool _isLimitMove = true;
        public bool IsLimitMove
        {
            get { return _isLimitMove; }
            set
            {
                if (_isLimitMove == value) return;
                _isLimitMove = value;
                _lockMoveX = (_lockMoveX || _transform.Position.X == 0) & _isLimitMove;
                _lockMoveY = (_lockMoveY || _transform.Position.Y == 0) & _isLimitMove;
            }
        }

        // X方向の移動制限フラグ
        private bool _lockMoveX;

        // Y方向の移動制限フラグ
        private bool _lockMoveY;


        // 水平スクロールの正方向
        public double ViewHorizontalDirection { get; set; } = 1.0;


        //
        private void SetAngle(double angle, TransformActionType actionType)
        {
            _transform.Angle = angle;

            var args = new TransformEventArgs(TransformChangeType.Angle, actionType)
            {
                Scale = _transform.Scale,
                Angle = _transform.Angle,
                IsFlipHorizontal = _transform.IsFlipHorizontal,
                IsFlipVertical = _transform.IsFlipVertical
            };
            TransformChanged?.Invoke(this, args);
        }

        //
        private void SetScale(double scale, TransformActionType actionType)
        {
            _transform.Scale = scale;

            var args = new TransformEventArgs(TransformChangeType.Scale, actionType)
            {
                Scale = _transform.Scale,
                Angle = _transform.Angle,
                IsFlipHorizontal = _transform.IsFlipHorizontal,
                IsFlipVertical = _transform.IsFlipVertical
            };
            TransformChanged?.Invoke(this, args);
        }


        // ドラッグでビュー操作設定の更新
        public void SetMouseDragSetting(int direction, DragViewOrigin origin, PageReadOrder order)
        {
            this.DragControlCenter = this.IsControlCenterImage ? DragControlCenter.Target : DragControlCenter.View;

            if (origin == DragViewOrigin.None)
            {
                origin = this.IsViewStartPositionCenter
                    ? DragViewOrigin.Center
                    : order == PageReadOrder.LeftToRight
                        ? DragViewOrigin.LeftTop
                        : DragViewOrigin.RightTop;

                this.ViewOrigin = direction < 0 ? origin.Reverse() : origin;
                this.ViewHorizontalDirection = (origin == DragViewOrigin.LeftTop) ? 1.0 : -1.0;
            }
            else
            {
                this.ViewOrigin = direction < 0 ? origin.Reverse() : origin;
                this.ViewHorizontalDirection = (origin == DragViewOrigin.LeftTop || origin == DragViewOrigin.LeftBottom) ? 1.0 : -1.0;
            }
        }

        /// <summary>
        /// トランスフォーム初期化
        /// </summary>
        /// <param name="forceReset">すべての項目を初期化</param>
        /// <param name="angle">Nanでない場合はこの角度で初期化する</param>
        public void Reset(bool forceReset, double angle)
        {
            bool isResetScale = forceReset || !this.IsKeepScale;
            bool isResetAngle = forceReset || !this.IsKeepAngle || !double.IsNaN(angle);
            bool isResetFlip = forceReset || !this.IsKeepFlip;

            Reset(isResetScale, isResetAngle, isResetFlip, double.IsNaN(angle) ? 0.0 : angle); // DefaultViewAngle(isResetAngle));
        }



        // 初期化
        // コンテンツ切り替わり時等
        public void Reset(bool isResetScale, bool isResetAngle, bool isResetFlip, double angle)
        {
            ////_actionType = TransformActionType.Reset;

            _lockMoveX = IsLimitMove;
            _lockMoveY = IsLimitMove;

            if (isResetAngle)
            {
                SetAngle(angle, TransformActionType.Reset);
            }
            if (isResetScale)
            {
                SetScale(1.0, TransformActionType.Reset);
            }
            if (isResetFlip)
            {
                _transform.IsFlipHorizontal = false;
                _transform.IsFlipVertical = false;
            }

            if (ViewOrigin == DragViewOrigin.Center)
            {
                _transform.Position = new Point(0, 0);
            }
            else
            {
                // レイアウト更新
                _transform.Position = new Point(0, 0);

                _context.Sender.UpdateLayout();
                var area = GetArea();
                var pos = new Point(0, 0);
                var move = new Vector(0, 0);
                if (area.Target.Height > area.View.Height)
                {
                    var verticalDirection = (ViewOrigin == DragViewOrigin.LeftTop || ViewOrigin == DragViewOrigin.RightTop) ? 1.0 : -1.0;
                    move.Y = (area.Target.Height - area.View.Height + 1) * 0.5 * verticalDirection;
                }
                if (area.Target.Width > area.View.Width)
                {
                    var horizontalDirection = (ViewOrigin == DragViewOrigin.LeftTop || ViewOrigin == DragViewOrigin.LeftBottom) ? 1.0 : -1.0;
                    move.X = (area.Target.Width - area.View.Width + 1) * 0.5 * horizontalDirection;
                }

                if (move.X != 0 || move.Y != 0)
                {
                    var limitedPos = pos + GetLimitMove(area, move);
                    _transform.Position = limitedPos;
                }
            }

            _defaultPosition = _transform.Position;
            _defaultScale = _transform.Scale;
            _defaultAngle = _transform.Angle;
        }

        // 移動量限界計算
        private Vector GetLimitMove(DragArea area, Vector move)
        {
            var margin = new Point(
                area.Target.Width < area.View.Width ? 0 : area.Target.Width - area.View.Width,
                area.Target.Height < area.View.Height ? 0 : area.Target.Height - area.View.Height);

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

            return move;
        }

        // 最後にリセットした値に戻す(角度以外)
        public void ResetDefault()
        {
            SetScale(_defaultScale, TransformActionType.Reset);
            _transform.Position = _defaultPosition;
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
            var pos = _transform.Position;

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

            _transform.Position = pos;
        }


        #region Scroll method

        // スクロール↑コマンド
        // 縦方向にスクロールできない場合、横方向にスクロールする
        public void ScrollUp(double rate)
        {
            _transform.IsEnableTranslateAnimation = true;

            UpdateLock();
            if (!_lockMoveY)
            {
                DoMove(new Vector(0, _context.Sender.ActualHeight * rate));
            }
            else
            {
                DoMove(new Vector(_context.Sender.ActualWidth * rate * ViewHorizontalDirection, 0));
            }

            _transform.IsEnableTranslateAnimation = false;
        }

        // スクロール↓コマンド
        // 縦方向にスクロールできない場合、横方向にスクロールする
        public void ScrollDown(double rate)
        {
            _transform.IsEnableTranslateAnimation = true;

            UpdateLock();
            if (!_lockMoveY)
            {
                DoMove(new Vector(0, _context.Sender.ActualHeight * -rate));
            }
            else
            {
                DoMove(new Vector(_context.Sender.ActualWidth * -rate * ViewHorizontalDirection, 0));
            }

            _transform.IsEnableTranslateAnimation = false;
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

                if (isAnimate) _transform.IsEnableTranslateAnimation = true;
                DoMove(new Vector(delta.X, delta.Y));
                if (isAnimate) _transform.IsEnableTranslateAnimation = false;

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
            _baseScale = _transform.Scale;
            _basePosition = _transform.Position;
            DoScale(_baseScale * (1.0 + scaleDelta));
        }

        // 縮小コマンド
        public void ScaleDown(double scaleDelta)
        {
            _baseScale = _transform.Scale;
            _basePosition = _transform.Position;
            DoScale(_baseScale / (1.0 + scaleDelta));
        }
        #endregion

        #region Rotate method
        // 回転コマンド
        public void Rotate(double angle)
        {
            _baseAngle = _transform.Angle;
            _basePosition = _transform.Position;
            DoRotate(NormalizeLoopRange(_baseAngle + angle, -180, 180));
        }
        #endregion

        #region Flip method
        // 反転コマンド
        public void ToggleFlipHorizontal()
        {
            DoFlipHorizontal(!_transform.IsFlipHorizontal);
        }

        // 反転コマンド
        public void FlipHorizontal(bool isFlip)
        {
            DoFlipHorizontal(isFlip);
        }

        // 反転コマンド
        public void ToggleFlipVertical()
        {
            DoFlipVertical(!_transform.IsFlipVertical);
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
                var action = DragActionTable.Current.GetAction(dragKey);

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
            var windowDiff = PointToLogicalScreen(_context.Window, new Point(0, 0)) - new Point(_context.Window.Left, _context.Window.Top);
            _startPointFromWindow = _context.Sender.TranslatePoint(_context.StartPoint, _context.Window) + windowDiff;

            if (DragControlCenter == DragControlCenter.View)
            {
                _center = new Point(_context.Sender.ActualWidth * 0.5, _context.Sender.ActualHeight * 0.5);
            }
            else
            {
                _center = _context.TargetShadow.TranslatePoint(new Point(_context.TargetShadow.ActualWidth * _context.TargetShadow.RenderTransformOrigin.X, _context.TargetShadow.ActualHeight * _context.TargetShadow.RenderTransformOrigin.Y), _context.Sender);
            }

            _basePosition = _transform.Position;
            _baseAngle = _transform.Angle;
            _baseScale = _transform.Scale;
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
            var pos0 = _transform.Position;
            var pos1 = (end - _context.StartPoint) * scale + _basePosition;
            var move = pos1 - pos0;

            DoMove(move);
        }


        // 移動実行
        private void DoMove(Vector move)
        {
            var area = GetArea();
            var pos0 = _transform.Position;
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
                move = GetLimitMove(area, move);
            }

            _transform.Position = pos0 + move;
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

            //_actionType = TransformActionType.Angle;
            //Angle = angle;
            SetAngle(angle, TransformActionType.Angle);

            if (DragControlCenter == DragControlCenter.View)
            {
                RotateTransform m = new RotateTransform(_transform.Angle - _baseAngle);
                _transform.Position = m.Transform(_basePosition);
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

            //_actionType = TransformActionType.Scale;
            //Scale = scale;
            SetScale(scale, TransformActionType.Scale);

            if (DragControlCenter == DragControlCenter.View)
            {
                var rate = _transform.Scale / _baseScale;
                _transform.Position = new Point(_basePosition.X * rate, _basePosition.Y * rate);
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
            if (_transform.IsFlipHorizontal != isFlip)
            {
                _transform.IsFlipHorizontal = isFlip;

                ////_actionType = TransformActionType.FlipHorizontal;

                // 角度を反転
                var angle = -NormalizeLoopRange(_transform.Angle, -180, 180);

                SetAngle(angle, TransformActionType.FlipHorizontal);

                // 座標を反転
                if (DragControlCenter == DragControlCenter.View)
                {
                    _transform.Position = new Point(-_transform.Position.X, _transform.Position.Y);
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
            if (_transform.IsFlipVertical != isFlip)
            {
                _transform.IsFlipVertical = isFlip;

                ////_actionType = TransformActionType.FlipVertical;

                // 角度を反転
                var angle = 90 - NormalizeLoopRange(_transform.Angle + 90, -180, 180);
                SetAngle(angle, TransformActionType.FlipVertical);

                // 座標を反転
                if (DragControlCenter == DragControlCenter.View)
                {
                    _transform.Position = new Point(_transform.Position.X, -_transform.Position.Y);
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
                var pos = PointToLogicalScreen(_context.Sender, end) - _startPointFromWindow;
                _context.Window.Left = pos.X;
                _context.Window.Top = pos.Y;
            }
        }

        /// <summary>
        /// 座標を論理座標でスクリーン座標に変換
        /// </summary>
        private Point PointToLogicalScreen(Visual visual, Point point)
        {
            var pos = visual.PointToScreen(point); // デバイス座標

            var dpi = Config.Current.Dpi;
            pos.X = pos.X / dpi.DpiScaleX;
            pos.Y = pos.Y / dpi.DpiScaleY;
            return pos;
        }

        #endregion



        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public bool IsOriginalScaleShowMessage { get; set; }
            [DataMember]
            public bool IsLimitMove { get; set; }
            [DataMember]
            public double AngleFrequency { get; set; }
            [DataMember]
            public bool IsControlCenterImage { get; set; }
            [DataMember]
            public bool IsKeepScale { get; set; }
            [DataMember]
            public bool IsKeepAngle { get; set; }
            [DataMember]
            public bool IsKeepFlip { get; set; }
            [DataMember]
            public bool IsViewStartPositionCenter { get; set; }
        }


        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsOriginalScaleShowMessage = this.IsOriginalScaleShowMessage;
            memento.IsLimitMove = this.IsLimitMove;
            memento.AngleFrequency = this.AngleFrequency;
            memento.IsControlCenterImage = this.IsControlCenterImage;
            memento.IsKeepScale = this.IsKeepScale;
            memento.IsKeepAngle = this.IsKeepAngle;
            memento.IsKeepFlip = this.IsKeepFlip;
            memento.IsViewStartPositionCenter = this.IsViewStartPositionCenter;

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.IsOriginalScaleShowMessage = memento.IsOriginalScaleShowMessage;
            this.IsLimitMove = memento.IsLimitMove;
            this.AngleFrequency = memento.AngleFrequency;
            this.IsControlCenterImage = memento.IsControlCenterImage;
            this.IsKeepScale = memento.IsKeepScale;
            this.IsKeepAngle = memento.IsKeepAngle;
            this.IsKeepFlip = memento.IsKeepFlip;
            this.IsViewStartPositionCenter = memento.IsViewStartPositionCenter;
        }

        #endregion

    }
}
