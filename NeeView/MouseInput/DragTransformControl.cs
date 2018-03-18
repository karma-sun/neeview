using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

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
        //
        public TransformEventArgs(TransformActionType actionType)
        {
            this.ActionType = actionType;
        }

        /// <summary>
        /// 変化したもの
        /// </summary>
        public TransformActionType ActionType { get; set; }
    }


    /// <summary>
    /// ドラッグ操作
    /// </summary>
    public class DragTransformControl
    {
        public static DragTransformControl Current { get; private set; }

        #region Fields

        /// <summary>
        /// ウィンドウ
        /// </summary>
        private Window _window;

        /// <summary>
        /// 操作領域
        /// </summary>
        private FrameworkElement _sender;

        /// <summary>
        /// 操作エレメント
        /// </summary>
        private FrameworkElement _target;

        /// <summary>
        /// 操作エレメント変換パラメータ
        /// </summary>
        private DragTransform _transform;


        private Point _defaultPosition;

        private double _defaultAngle;

        private double _defaultScale;

        // X方向の移動制限フラグ
        private bool _lockMoveX;

        // Y方向の移動制限フラグ
        private bool _lockMoveY;

        #endregion

        #region Constructors

        //
        public DragTransformControl(FrameworkElement sender, FrameworkElement target, DragTransform transform)
        {
            DragTransformControl.Current = this;

            _window = Window.GetWindow(sender);
            _sender = sender;
            _target = target;

            _transform = transform;
        }

        #endregion

        #region Properties

        // View変換情報表示のスケール表示をオリジナルサイズ基準にする
        [PropertyMember("@ParamDragTransformIsOriginalScaleShowMessage", Tips = "@ParamDragTransformIsOriginalScaleShowMessageTips")]
        public bool IsOriginalScaleShowMessage { get; set; }

        // 回転、拡縮をコンテンツの中心基準にする
        [PropertyMember("@ParamDragTransformIsControlCenterImage", Tips = "@ParamDragTransformIsControlCenterImage")]
        public bool IsControlCenterImage { get; set; }

        // 拡大率キープ
        [PropertyMember("@ParamDragTransformIsKeepScale")]
        public bool IsKeepScale { get; set; }

        // 回転キープ
        [PropertyMember("@ParamDragTransformIsKeepAngle", Tips = "@ParamDragTransformIsKeepAngleTips")]
        public bool IsKeepAngle { get; set; }

        // 反転キープ
        [PropertyMember("@ParamDragTransformIsKeepFlip")]
        public bool IsKeepFlip { get; set; }

        // 表示開始時の基準
        [PropertyMember("@ParamDragTransformIsViewStartPositionCenter", Tips = "@ParamDragTransformIsViewStartPositionCenterTips")]
        public bool IsViewStartPositionCenter { get; set; }

        // 開始時の基準
        public DragViewOrigin ViewOrigin { get; set; }

        // 水平スクロールの正方向
        public double ViewHorizontalDirection { get; set; } = 1.0;

        #endregion

        #region StateMachine


        private bool _isMouseButtonDown;

        /// <summary>
        /// ドラッグ開始座標
        /// </summary>
        private Point _startPoint;


        /// <summary>
        /// 
        /// </summary>
        public void ResetState()
        {
            _isMouseButtonDown = false;
            _action = null;
        }

        /// <summary>
        /// Change State
        /// </summary>
        /// <param name="buttons">マウスボタンの状態</param>
        /// <param name="keys">装飾キーの状態</param>
        /// <param name="pos">マウス座標(_context.Sender)</param>
        public void UpdateState(MouseButtonBits buttons, ModifierKeys keys, Point point)
        {
            if (_isMouseButtonDown)
            {
                StateDrag(buttons, keys, point);
            }
            else
            {
                StateIdle(buttons, keys, point);
            }
        }


        //
        private void StateIdle(MouseButtonBits buttons, ModifierKeys keys, Point point)
        {
            if (buttons != MouseButtonBits.None)
            {
                InitializeDragParameter(point);
                _isMouseButtonDown = true;
            }
        }

        //
        private void StateDrag(MouseButtonBits buttons, ModifierKeys keys, Point point)
        {
            if (buttons == MouseButtonBits.None)
            {
                _isMouseButtonDown = false;
                return;
            }

            var dragKey = new DragKey(buttons, keys);
            if (!dragKey.IsValid) return;

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
            _action?.Exec?.Invoke(_startPoint, point);
        }

        #endregion

        #region Methods

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
            _lockMoveX = _transform.IsLimitMove;
            _lockMoveY = _transform.IsLimitMove;

            if (isResetAngle)
            {
                _transform.SetAngle(angle, TransformActionType.Reset);
            }
            if (isResetScale)
            {
                _transform.SetScale(1.0, TransformActionType.Reset);
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

                _sender.UpdateLayout();
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
            _transform.SetScale(_defaultScale, TransformActionType.Reset);
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
            return new DragArea(_sender, _target);
        }

        // ビューエリアサイズ変更に追従する
        public void SnapView()
        {
            if (!_transform.IsLimitMove) return;

            // レイアウト更新
            _sender.UpdateLayout();

            var area = GetArea();
            _transform.Position = area.SnapView(_transform.Position);
        }

        #endregion

        #region Scroll method

        // スクロール↑コマンド
        // 縦方向にスクロールできない場合、横方向にスクロールする
        public void ScrollUp(ViewScrollCommandParameter parameter)
        {
            var rate = parameter.Scroll / 100.0;

            _transform.IsEnableTranslateAnimation = true;

            UpdateLock();
            if (!_lockMoveY)
            {
                DoMove(new Vector(0, _sender.ActualHeight * rate));
            }
            else if (parameter.AllowCrossScroll)
            {
                DoMove(new Vector(_sender.ActualWidth * rate * ViewHorizontalDirection, 0));
            }

            _transform.IsEnableTranslateAnimation = false;
        }

        // スクロール↓コマンド
        // 縦方向にスクロールできない場合、横方向にスクロールする
        public void ScrollDown(ViewScrollCommandParameter parameter)
        {
            var rate = parameter.Scroll / 100.0;

            _transform.IsEnableTranslateAnimation = true;

            UpdateLock();
            if (!_lockMoveY)
            {
                DoMove(new Vector(0, _sender.ActualHeight * -rate));
            }
            else if (parameter.AllowCrossScroll)
            {
                DoMove(new Vector(_sender.ActualWidth * -rate * ViewHorizontalDirection, 0));
            }

            _transform.IsEnableTranslateAnimation = false;
        }

        // スクロール←コマンド
        // 横方向にスクロールできない場合、縦方向にスクロールする
        public void ScrollLeft(ViewScrollCommandParameter parameter)
        {
            var rate = parameter.Scroll / 100.0;

            _transform.IsEnableTranslateAnimation = true;

            UpdateLock();

            if (!_lockMoveX)
            {
                DoMove(new Vector(_sender.ActualWidth * rate, 0));
            }
            else if (parameter.AllowCrossScroll)
            {
                DoMove(new Vector(0, _sender.ActualHeight * rate * ViewHorizontalDirection));
            }

            _transform.IsEnableTranslateAnimation = false;
        }

        // スクロール→コマンド
        // 横方向にスクロールできない場合、縦方向にスクロールする
        public void ScrollRight(ViewScrollCommandParameter parameter)
        {
            var rate = parameter.Scroll / 100.0;

            _transform.IsEnableTranslateAnimation = true;

            UpdateLock();

            if (!_lockMoveX)
            {
                DoMove(new Vector(_sender.ActualWidth * -rate, 0));
            }
            else if (parameter.AllowCrossScroll)
            {
                DoMove(new Vector(0, _sender.ActualHeight * -rate * ViewHorizontalDirection));
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
        public bool ScrollN(int direction, int bookReadDirection, bool allowVerticalScroll, double margin, bool isAnimate, double rate)
        {
            var delta = GetNScrollDelta(direction, bookReadDirection, allowVerticalScroll, margin, rate);

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
        private Point GetNScrollDelta(int direction, int bookReadDirection, bool allowVerticalScroll, double margin, double rate)
        {
            Point delta = new Point();

            var area = GetArea();

            if (allowVerticalScroll)
            {
                if (direction > 0)
                {
                    delta.Y = GetNScrollVerticalToBottom(area, margin, rate);
                }
                else
                {
                    delta.Y = GetNScrollVerticalToTop(area, margin, rate);
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
        private double GetNScrollVerticalToTop(DragArea area, double margin, double rate)
        {
            if (area.Over.Top < -margin)
            {
                double dy = Math.Abs(area.Over.Top);
                if (dy > area.View.Height * rate) dy = area.View.Height * rate;
                return dy;
            }
            else
            {
                return 0.0;
            }
        }

        // N字スクロール：下方向スクロール距離取得
        private double GetNScrollVerticalToBottom(DragArea area, double margin, double rate)
        {
            if (area.Over.Bottom > margin)
            {
                double dy = Math.Abs(area.Over.Bottom);
                if (dy > area.View.Height * rate) dy = area.View.Height * rate;
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
        public void ScaleUp(double scaleDelta, bool isSnap, double originalScale)
        {
            _baseScale = _transform.Scale;
            _basePosition = _transform.Position;

            var scale = _baseScale * (1.0 + scaleDelta);

            if (isSnap)
            {
                if (this.IsOriginalScaleShowMessage && originalScale > 0.0)
                {
                    // original scale 100% snap
                    if (_baseScale * originalScale < 0.99 && scale * originalScale > 0.99)
                    {
                        scale = 1.0 / originalScale;
                    }
                }
                else
                {
                    // visual scale 100% snap
                    if (_baseScale < 0.99 && scale > 0.99)
                    {
                        scale = 1.0;
                    }
                }
            }

            DoScale(scale);
        }

        // 縮小コマンド
        public void ScaleDown(double scaleDelta, bool isSnap, double originalScale)
        {
            _baseScale = _transform.Scale;
            _basePosition = _transform.Position;

            var scale = _baseScale / (1.0 + scaleDelta);

            if (isSnap)
            {
                if (this.IsOriginalScaleShowMessage && originalScale > 0.0)
                {
                    // original scale 100% snap
                    if (_baseScale * originalScale > 1.01 && scale * originalScale < 1.01)
                    {
                        scale = 1.0 / originalScale;
                    }
                }
                else
                {
                    // visual scale 100% snap
                    if (_baseScale > 1.01 && scale < 1.01)
                    {
                        scale = 1.0;
                    }
                }
            }

            DoScale(scale);
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

        #region Actions

        // ドラッグアクション
        private DragAction _action;

        // ドラッグパラメータ初期化
        private void InitializeDragParameter(Point pos)
        {
            _startPoint = pos;
            _baseFlipPoint = _startPoint;
            var windowDiff = PointToLogicalScreen(_window, new Point(0, 0)) - new Point(_window.Left, _window.Top);
            _startPointFromWindow = _sender.TranslatePoint(_startPoint, _window) + windowDiff;

            if (DragControlCenter == DragControlCenter.View)
            {
                _center = new Point(_sender.ActualWidth * 0.5, _sender.ActualHeight * 0.5);
            }
            else
            {
                _center = _target.TranslatePoint(new Point(_target.ActualWidth * _target.RenderTransformOrigin.X, _target.ActualHeight * _target.RenderTransformOrigin.Y), _sender);
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
            var pos1 = (end - _startPoint) * scale + _basePosition;
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

            if (_transform.IsLimitMove)
            {
                move = GetLimitMove(area, move);
            }

            _transform.Position = pos0 + move;
        }

        #endregion

        #region Drag Angle

        // 回転、拡縮の中心
        public DragControlCenter DragControlCenter { get; set; } = DragControlCenter.View;

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
            if (_transform.AngleFrequency > 0)
            {
                angle = Math.Floor((angle + _transform.AngleFrequency * 0.5) / _transform.AngleFrequency) * _transform.AngleFrequency;
            }

            _transform.SetAngle(angle, TransformActionType.Angle);

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

            _transform.SetScale(scale, TransformActionType.Scale);

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

                // 角度を反転
                var angle = -NormalizeLoopRange(_transform.Angle, -180, 180);

                _transform.SetAngle(angle, TransformActionType.FlipHorizontal);

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

                // 角度を反転
                var angle = 90 - NormalizeLoopRange(_transform.Angle + 90, -180, 180);
                _transform.SetAngle(angle, TransformActionType.FlipVertical);

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
            if (_window.WindowState == WindowState.Normal)
            {
                var pos = PointToLogicalScreen(_sender, end) - _startPointFromWindow;
                _window.Left = pos.X;
                _window.Top = pos.Y;
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
            this.IsControlCenterImage = memento.IsControlCenterImage;
            this.IsKeepScale = memento.IsKeepScale;
            this.IsKeepAngle = memento.IsKeepAngle;
            this.IsKeepFlip = memento.IsKeepFlip;
            this.IsViewStartPositionCenter = memento.IsViewStartPositionCenter;
        }

        #endregion

    }
}
