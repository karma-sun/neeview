// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
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
    public class TransformChangedParam
    {
        public TransformChangeType ChangeType { get; set; }
        public TransformActionType ActionType { get; set; }

        public TransformChangedParam(TransformChangeType changeType, TransformActionType actionType)
        {
            ChangeType = changeType;
            ActionType = actionType;
        }
    }


    /// <summary>
    /// マウスドラッグ管理
    /// マウスドラッグでのコンテンツの表示変換を行う。
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
                OnPropertyChanged();
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
                OnPropertyChanged();
                TransformChanged?.Invoke(this, new TransformChangedParam(TransformChangeType.Angle, _actionType));
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
                OnPropertyChanged();
                OnPropertyChanged(nameof(ScaleX));
                OnPropertyChanged(nameof(ScaleY));
                TransformChanged?.Invoke(this, new TransformChangedParam(TransformChangeType.Scale, _actionType));
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
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ScaleX));
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
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ScaleY));
                }
            }
        }
        #endregion


        // ルーペーモード
        #region Property: IsLoupe
        private bool _isLoupe;
        public bool IsLoupe
        {
            get { return _isLoupe; }
            set
            {
                _isLoupe = value;

                if (_isLoupe)
                {
                    CancelOnce();
                    _sender.Cursor = Cursors.None;

                    var start = Mouse.GetPosition(_sender);
                    var center = new Point(_sender.ActualWidth * 0.5, _sender.ActualHeight * 0.5);
                    Vector v = start - center;

                    _loupeBasePosition = (Point)(-v + v / LoupeScale);
                    LoupePosition = _loupeBasePosition;
                }
                else
                {
                    LoupePosition = new Point();
                }

                OnPropertyChanged(nameof(LoupeScaleX));
                OnPropertyChanged(nameof(LoupeScaleY));
                TransformChanged?.Invoke(this, new TransformChangedParam(TransformChangeType.LoupeScale, TransformActionType.LoupeScale));
            }
        }
        #endregion

        #region Property: LoupePosition
        private Point _loupePosition;
        public Point LoupePosition
        {
            get { return _loupePosition; }
            set
            {
                _loupePosition = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LoupePositionX));
                OnPropertyChanged(nameof(LoupePositionY));
            }
        }
        public double LoupePositionX => IsLoupe ? LoupePosition.X : 0.0;
        public double LoupePositionY => IsLoupe ? LoupePosition.Y : 0.0;

        private Point _loupeBasePosition;
        #endregion


        #region Property: LoupeScale
        private double _loupeScale = 2.0;
        public double LoupeScale
        {
            get { return _loupeScale; }
            set
            {
                _loupeScale = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LoupeScaleX));
                OnPropertyChanged(nameof(LoupeScaleY));
                TransformChanged?.Invoke(this, new TransformChangedParam(TransformChangeType.LoupeScale, TransformActionType.LoupeScale));
            }
        }

        public double FixedLoupeScale => IsLoupe ? LoupeScale : 1.0;
        public double LoupeScaleX => FixedLoupeScale;
        public double LoupeScaleY => FixedLoupeScale;
        #endregion

        public void LoupeZoom(MouseWheelEventArgs e)
        {
            if (IsLoupe)
            {
                if (e.Delta > 0)
                {
                    LoupeZoomIn();
                }
                else
                {
                    LoupeZoomOut();
                }
                e.Handled = true;
            }
        }

        //
        public void LoupeZoomIn()
        {
            var newScale = LoupeScale + 1.0;
            if (newScale > 10.0) newScale = 10.0; // 最大 x10.0
            LoupeScale = newScale;
        }

        //
        public void LoupeZoomOut()
        {
            var newScale = LoupeScale - 1.0;
            if (newScale < 2.0) newScale = 2.0;
            LoupeScale = newScale;
        }





        // アクションキーバインド
        private Dictionary<DragKey, DragAction> _keyBindings;

        // アクションキーバインド設定
        public void SetKeyBindings(Dictionary<DragKey, DragAction> binding)
        {
            _keyBindings = binding;
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

        // 回転、拡縮の中心
        public DragControlCenter DragControlCenter { get; set; } = DragControlCenter.View;

        // 回転スナップ。0で無効
        public double SnapAngle { get; set; } = 45;

        // 拡縮スナップ。0で無効;
        public double SnapScale { get; set; } = 0;

        // ウィンドウ
        private Window _window;

        // マウス入力イベント受付コントロール。ビューエリア。
        private FrameworkElement _sender;

        // 移動対象。コンテンツ。
        private FrameworkElement _target;

        // 移動対象の影
        // 移動範囲計算用。_TargetViewと全く同じ大きさと座標で、アニメーションだけしない非表示コントロール。
        private FrameworkElement _targetShadow;

        //
        private bool _isButtonDown = false;
        private bool _isDragging = false;
        private MouseButton _dragMouseButton;
        private DragAction _action;

        // クリックイベント
        // ドラッグされずにマウスボタンが離された時にに発行する
        public event EventHandler<MouseButtonEventArgs> MouseClickEventHandler;

        // 角度、スケール変更イベント
        public event EventHandler<TransformChangedParam> TransformChanged;

        // 角度、スケール変更終了イベント
        public event EventHandler TransformEnd;

        private bool _isEnableClickEvent;

        private Point _startPoint;
        private Point _endPoint;
        private Point _basePosition;
        private double _baseAngle;
        private double _baseScale;
        private Point _center;
        private Point _baseFlipPoint;
        private Point _startPointFromWindow;

        /// <summary>
        ///  コンストラクタ
        /// </summary>
        /// <param name="sender">ビューエリア、マウスイベント受付コントロール</param>
        /// <param name="targetView">対象コンテンツ</param>
        /// <param name="targetShadow">対象コンテンツの影。計算用</param>
        public MouseDragController(Window window, FrameworkElement sender, FrameworkElement targetView, FrameworkElement targetShadow)
        {
            _window = window;
            _sender = sender;
            _target = targetView;
            _targetShadow = targetShadow;

            _sender.PreviewMouseDown += OnMouseButtonDown;
            _sender.PreviewMouseUp += OnMouseButtonUp;
            _sender.PreviewMouseWheel += OnMouseWheel;
            _sender.PreviewMouseMove += OnMouseMove;

            BindTransform(_target, true);
            BindTransform(_targetShadow, false);
        }

        // パラメータとトランスフォームを対応させる
        private void BindTransform(FrameworkElement element, bool isView)
        {
            element.RenderTransformOrigin = new Point(0.5, 0.5);

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

            //
            var loupeTransraleTransform = new TranslateTransform();
            BindingOperations.SetBinding(loupeTransraleTransform, TranslateTransform.XProperty, new Binding(nameof(LoupePositionX)) { Source = this });
            BindingOperations.SetBinding(loupeTransraleTransform, TranslateTransform.YProperty, new Binding(nameof(LoupePositionY)) { Source = this });

            var loupeScaleTransform = new ScaleTransform();
            BindingOperations.SetBinding(loupeScaleTransform, ScaleTransform.ScaleXProperty, new Binding(nameof(LoupeScaleX)) { Source = this });
            BindingOperations.SetBinding(loupeScaleTransform, ScaleTransform.ScaleYProperty, new Binding(nameof(LoupeScaleY)) { Source = this });

            transformGroup.Children.Add(loupeTransraleTransform);
            transformGroup.Children.Add(loupeScaleTransform);

            element.RenderTransform = transformGroup;

            if (isView)
            {
                _translateTransform = translateTransform;
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
                _sender.UpdateLayout();

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


        // ビューエリアサイズ変更に追従する
        public void SnapView()
        {
            if (!IsLimitMove) return;

            // レイアウト更新
            _sender.UpdateLayout();

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
        public void ScrollUp(double rate)
        {
            _isEnableTranslateAnimation = true;

            UpdateLock();
            if (!_lockMoveY)
            {
                DoMove(new Vector(0, _sender.ActualHeight * rate));
            }
            else
            {
                DoMove(new Vector(_sender.ActualWidth * rate * ViewHorizontalDirection, 0));
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
                DoMove(new Vector(0, _sender.ActualHeight * -rate));
            }
            else
            {
                DoMove(new Vector(_sender.ActualWidth * -rate * ViewHorizontalDirection, 0));
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
                Debug.WriteLine(delta);
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

        // 回転コマンド
        public void Rotate(double angle)
        {
            _baseAngle = Angle;
            _basePosition = Position;
            DoRotate(NormalizeLoopRange(_baseAngle + angle, -180, 180));
        }

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


        // 入力からアクション取得
        private DragAction GetAction(MouseButton butto, ModifierKeys keys)
        {
            var key = new DragKey(butto, keys);
            DragAction command;
            if (_keyBindings.TryGetValue(key, out command))
            {
                return command;
            }
            else
            {
                return null;
            }
        }


        private bool _isCancel = false;

        /// <summary>
        /// 次にマウスボタンが押されるまで一時的に無効化する
        /// </summary>
        public void CancelOnce()
        {
            _isCancel = true;
        }

        // マウスボタンが押された時の処理
        private void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            _sender.Focus();

            if (_isButtonDown) return;

            if (e.ChangedButton != MouseButton.Left && e.ChangedButton != MouseButton.Middle) return;

            _startPoint = e.GetPosition(_sender);

            _isButtonDown = true;
            _dragMouseButton = e.ChangedButton;
            _isDragging = false;
            _isEnableClickEvent = true;
            _isCancel = false;

            _action = null;

            _sender.CaptureMouse();
        }

        // マウスボタンが離された時の処理
        private void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isButtonDown) return;

            if (e.ChangedButton != _dragMouseButton) return;

            _isButtonDown = false;

            _sender.ReleaseMouseCapture();

            if (_sender.Cursor != Cursors.None)
            {
                _sender.Cursor = null;
            }

            TransformEnd?.Invoke(this, null);

            if (_isCancel) return;

            if (_isEnableClickEvent && !_isDragging && MouseClickEventHandler != null)
            {
                MouseClickEventHandler(sender, e);
            }
        }

        // マウスホイールの処理
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // クリック系のイベントを無効にする
            _isEnableClickEvent = false;
        }

        // マウスポインタが移動した時の処理
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isButtonDown) return;

            _endPoint = e.GetPosition(_sender);

            //
            if (IsLoupe)
            {
                DragLoupeMove(_startPoint, _endPoint);
                return;
            }

            if (_isCancel) return;

            if (!_isDragging)
            {
                if (Math.Abs(_endPoint.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(_endPoint.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    _isDragging = true;

                    InitializeDragParameter(_endPoint);

                    _sender.Cursor = Cursors.Hand;
                }
                else
                {
                    return;
                }
            }

            // update action
            var action = GetAction(_dragMouseButton, Keyboard.Modifiers);
            if (_action == null)
            {
                _action = action;
            }
            else if (action != _action)
            {
                if (Keyboard.Modifiers != ModifierKeys.None || action.IsGroupCompatible(_action))
                {
                    _action = action;
                    InitializeDragParameter(_endPoint);
                }
            }


            _action?.Exec(_startPoint, _endPoint);
        }

        // ドラッグパラメータ初期化
        private void InitializeDragParameter(Point pos)
        {
            _startPoint = pos;
            _baseFlipPoint = _startPoint;
            var windowDiff = _window.PointToScreen(new Point(0, 0)) - new Point(_window.Left, _window.Top);
            _startPointFromWindow = _sender.TranslatePoint(_startPoint, _window) + windowDiff;

            if (DragControlCenter == DragControlCenter.View)
            {
                _center = new Point(_sender.ActualWidth * 0.5, _sender.ActualHeight * 0.5);
            }
            else
            {
                _center = (Point)(_targetShadow.PointToScreen(new Point(_targetShadow.ActualWidth * _targetShadow.RenderTransformOrigin.X, _targetShadow.ActualHeight * _targetShadow.RenderTransformOrigin.Y)) - _sender.PointToScreen(new Point(0, 0)));
            }

            _basePosition = Position;
            _baseAngle = Angle;
            _baseScale = Scale;
        }


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
            var pos1 = (_endPoint - _startPoint) * scale + _basePosition;
            var move = pos1 - pos0;

            DoMove(move);
        }


        // ルーペ移動
        public void DragLoupeMove(Point start, Point end)
        {
            LoupePosition = _loupeBasePosition - (end - start);
        }



        private DragArea GetArea()
        {
            return new DragArea(_sender, _targetShadow);
        }

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
            if (SnapAngle > 0)
            {
                angle = Math.Floor((angle + SnapAngle * 0.5) / SnapAngle) * SnapAngle;
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


        // ウィンドウ移動
        public void DragWindowMove(Point start, Point end)
        {
            if (_window.WindowState == WindowState.Normal)
            {
                var pos = _sender.PointToScreen(_endPoint) - _startPointFromWindow;
                _window.Left = pos.X;
                _window.Top = pos.Y;
            }
        }
    }
}
