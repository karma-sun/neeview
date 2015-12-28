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
    public enum DragControlCenter
    {
        View,
        Target,
    }

    public enum ViewOrigin
    {
        Center,
        LeftTop,
        RightTop,
    }

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


        private bool _IsEnableTranslateAnimation;
        private bool _IsTranslateAnimated;

        private TranslateTransform _TranslateTransform;

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



        #region Property: Angle
        private double _Angle;
        public double Angle
        {
            get { return _Angle; }
            set { _Angle = value; OnPropertyChanged(); }
        }
        #endregion

        #region Property: Scale
        private double _Scale = 1.0;
        public double Scale
        {
            get { return _Scale; }
            set { _Scale = value; OnPropertyChanged(); }
        }
        #endregion

        //public bool IsStartPositionCenter { get; set; }
        public ViewOrigin ViewOrigin { get; set; }

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
        private bool _LockMoveX;
        private bool _LockMoveY;

        // 回転、拡縮の中心
        public DragControlCenter DragControlCenter { get; set; } = DragControlCenter.View;

        // 回転スナップ。0で無効
        public double SnapAngle { get; set; } = 45;

        // 拡縮スナップ。0で無効;
        public double SnapScale { get; set; } = 0;

        private FrameworkElement _Sender;
        private FrameworkElement _Target;
        private FrameworkElement _TargetView;

        private bool _IsButtonDown = false;
        private bool _IsDragging = false;
        private bool _IsDragAngle { get { return _DragMode == (1 << 0); } }
        private bool _IsDragScale { get { return _DragMode == (1 << 1); } }
        private int _DragMode;

        public event EventHandler<MouseButtonEventArgs> MouseClickEventHandler;
        bool _IsEnableClickEvent;

        private Point _StartPoint;
        private Point _EndPoint;
        private Point _BasePosition;
        private double _BaseAngle;
        private double _BaseScale;
        private Point _Center;


        public MouseDragController(FrameworkElement sender, FrameworkElement targetView, FrameworkElement targetShadow)
        {
            _Sender = sender;
            _Target = targetShadow;
            _TargetView = targetView;

            _Sender.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            _Sender.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
            _Sender.PreviewMouseWheel += OnMouseWheel;
            _Sender.PreviewMouseMove += OnMouseMove;

            BindTransform(_Target, false);
            BindTransform(_TargetView, true);
        }

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

        public void ClearClickEventHandler()
        {
            MouseClickEventHandler = null;
        }

        double ViewHorizontalDirection => (ViewOrigin == ViewOrigin.LeftTop) ? 1.0 : -1.0;

        public void Reset()
        {
            _LockMoveX = IsLimitMove;
            _LockMoveY = IsLimitMove;

            Angle = 0;
            Scale = 1.0;

            if (ViewOrigin == ViewOrigin.Center)
            {
                // nop.
                Position = new Point(0, 0);
            }
            else
            {
                // レイアウト更新
                _Sender.UpdateLayout();

                var rect = GetRealSize(_Target, _Sender);
                var view = new Size(_Sender.ActualWidth, _Sender.ActualHeight);

                var pos = new Point(0, 0);
                if (rect.Height > view.Height)
                {
                    pos.Y = (rect.Height - view.Height) * 0.5;
                }
                if (rect.Width > view.Width)
                {
                    pos.X = (rect.Width - view.Width) * 0.5 * ViewHorizontalDirection;
                }
                Position = pos;
            }
        }

        public void SnapView()
        {
            if (!IsLimitMove) return;

            // レイアウト更新
            _Sender.UpdateLayout();

            var rect = GetRealSize(_Target, _Sender);
            var view = new Size(_Sender.ActualWidth, _Sender.ActualHeight);

            double margin = 1.0;

            var pos = Position;

            // ウィンドウサイズ変更直後はrectのスクリーン座標がおかしい可能性があるのでPositionから計算しなおす
            rect.X = pos.X - rect.Width * 0.5 + view.Width * 0.5;
            rect.Y = pos.Y - rect.Height * 0.5 + view.Height * 0.5;

            if (rect.Width <= view.Width + margin)
            {
                pos.X = 0.0;
            }
            else
            {
                if (rect.Left > 0)
                {
                    pos.X -= rect.Left;
                }
                else if (rect.Right < view.Width)
                {
                    pos.X += view.Width - rect.Right;
                }
            }

            if (rect.Height <= view.Height + margin)
            {
                pos.Y = 0.0;
            }
            else
            {
                if (rect.Top > 0)
                {
                    pos.Y -= rect.Top;
                }
                else if (rect.Bottom < view.Height)
                {
                    pos.Y += view.Height - rect.Bottom;
                }
            }

            Position = pos;
        }

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


        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_IsButtonDown) return;

            _IsButtonDown = false;

            _Sender.ReleaseMouseCapture();

            _Sender.Cursor = null;

            if (_IsEnableClickEvent && !_IsDragging && MouseClickEventHandler != null)
            {
                MouseClickEventHandler(sender, e);
            }
        }


        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // クリック系のイベントを無効にする
            _IsEnableClickEvent = false;
        }

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
                        _Center = (Point)(_Target.PointToScreen(new Point(_Target.ActualWidth * _Target.RenderTransformOrigin.X, _Target.ActualHeight * _Target.RenderTransformOrigin.Y)) - _Sender.PointToScreen(new Point(0, 0)));
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

        public void ScrollUp()
        {
            //DispatcherTimer ... なめらかスクロール

            //Debug.WriteLine("ScrollUp");

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

        public void ScrollDown()
        {
            //Debug.WriteLine("ScrollDown");

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

        public void ScaleUp()
        {
            //Debug.WriteLine("ScaleUp");
            _BaseScale = Scale;
            _BasePosition = Position;
            DoScale(1.2 * _BaseScale);
        }

        public void ScaleDown()
        {
            //Debug.WriteLine("ScaleDown");
            _BaseScale = Scale;
            _BasePosition = Position;
            DoScale(0.8 * _BaseScale);
        }

        // 移動
        private void DragMove(Point start, Point end)
        {
            var pos0 = Position;
            var pos1 = _EndPoint - _StartPoint + _BasePosition;
            var move = pos1 - pos0;

            DoMove(move);
        }

        private void UpdateLock()
        {
            var rect = GetRealSize(_Target, _Sender);
            var view = new Size(_Sender.ActualWidth, _Sender.ActualHeight);

            double margin = 0.1;

            if (_LockMoveX)
            {
                if (rect.Left < 0 - margin || rect.Right > view.Width + margin)
                {
                    _LockMoveX = false;
                }
            }
            if (_LockMoveY)
            {
                if (rect.Top < 0 - margin || rect.Bottom > view.Height + margin)
                {
                    _LockMoveY = false;
                }
            }
        }

        private void DoMove(Vector move)
        {
            var rect = GetRealSize(_Target, _Sender);
            var pos0 = Position;
            var pos1 = Position + move;
            var view = new Size(_Sender.ActualWidth, _Sender.ActualHeight);

            var margin = new Point(
                rect.Width < view.Width ? 0 : rect.Width - view.Width,
                rect.Height < view.Height ? 0 : rect.Height - view.Height);

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
                if (move.X < 0 && rect.Left + move.X < -margin.X)
                {
                    move.X = -margin.X - rect.Left;
                    if (move.X > 0) move.X = 0;
                }
                else if (move.X > 0 && rect.Right + move.X > view.Width + margin.X)
                {
                    move.X = view.Width + margin.X - rect.Right;
                    if (move.X < 0) move.X = 0;
                }
                if (move.Y < 0 && rect.Top + move.Y < -margin.Y)
                {
                    move.Y = -margin.Y - rect.Top;
                    if (move.Y > 0) move.Y = 0;
                }
                else if (move.Y > 0 && rect.Bottom + move.Y > view.Height + margin.Y)
                {
                    move.Y = view.Height + margin.Y - rect.Bottom;
                    if (move.Y < 0) move.Y = 0;
                }
            }

            Position = pos0 + move;

            _StartPoint += pos1 - Position;
        }

        public static Rect GetRealSize(FrameworkElement target, FrameworkElement parent)
        {
            Point[] pos = new Point[4];
            double width = target.ActualWidth;
            double height = target.ActualHeight;

            pos[0] = target.TranslatePoint(new Point(0, 0), parent);
            pos[1] = target.TranslatePoint(new Point(width, 0), parent);
            pos[2] = target.TranslatePoint(new Point(0, height), parent);
            pos[3] = target.TranslatePoint(new Point(width, height), parent);

            Point min = new Point(pos.Min(e => e.X), pos.Min(e => e.Y));
            Point max = new Point(pos.Max(e => e.X), pos.Max(e => e.Y));

            return new Rect(min, max);
        }

        // 回転
        public void DragAngle(Point start, Point end)
        {
            var v0 = start - _Center;
            var v1 = end - _Center;

            double angle1 = NormalizeLoopRange(_BaseAngle + Vector.AngleBetween(v0, v1), -180, 180);

            if (SnapAngle > 0)
            {
                angle1 = Math.Floor((angle1 + SnapAngle * 0.5) / SnapAngle) * SnapAngle;
            }

            Angle = angle1;

            if (DragControlCenter == DragControlCenter.View)
            {
                double angle = Angle - _BaseAngle;
                RotateTransform m = new RotateTransform(angle);
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

        private void DoScale(double scale1)
        {
            if (SnapScale > 0)
            {
                scale1 = Math.Floor((scale1 + SnapScale * 0.5) / SnapScale) * SnapScale;
            }

            Scale = scale1;

            if (DragControlCenter == DragControlCenter.View)
            {
                var scale = Scale / _BaseScale;
                Position = new Point(_BasePosition.X * scale, _BasePosition.Y * scale);
            }
        }
    }
}
