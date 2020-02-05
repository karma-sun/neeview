using Microsoft.Xaml.Behaviors;
using NeeView.Windows.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    public class AutoHideBehavior : Behavior<FrameworkElement>
    {
        #region DependencyProperties

        public FrameworkElement Screen
        {
            get { return (FrameworkElement)GetValue(ScreenProperty); }
            set { SetValue(ScreenProperty, value); }
        }

        public static readonly DependencyProperty ScreenProperty =
            DependencyProperty.Register("Screen", typeof(FrameworkElement), typeof(AutoHideBehavior), new PropertyMetadata(null, OnScreenPropertyChanged));

        private static void OnScreenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AutoHideBehavior control)
            {
                if (e.OldValue != null)
                {
                    throw new InvalidOperationException("Screen set is once.");
                }
            }
        }


        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool), typeof(AutoHideBehavior), new PropertyMetadata(false, OnPropertyChanged));


        public double DelayTime
        {
            get { return (double)GetValue(DelayTimeProperty); }
            set { SetValue(DelayTimeProperty, value); }
        }

        public static readonly DependencyProperty DelayTimeProperty =
            DependencyProperty.Register("DelayTime", typeof(double), typeof(AutoHideBehavior), new PropertyMetadata(1000.0, OnPropertyChanged));


        public bool IsVisibleLocked
        {
            get { return (bool)GetValue(IsVisibleLockedProperty); }
            set { SetValue(IsVisibleLockedProperty, value); }
        }

        public static readonly DependencyProperty IsVisibleLockedProperty =
            DependencyProperty.Register("IsVisibleLocked", typeof(bool), typeof(AutoHideBehavior), new PropertyMetadata(false, OnPropertyChanged));


        public double HitTestMargin
        {
            get { return (double)GetValue(HitTestMarginProperty); }
            set { SetValue(HitTestMarginProperty, value); }
        }

        public static readonly DependencyProperty HitTestMarginProperty =
            DependencyProperty.Register("HitTestMargin", typeof(double), typeof(AutoHideBehavior), new PropertyMetadata(10.0));


        public bool IsMouseEnabled
        {
            get { return (bool)GetValue(IsMouseEnabledProperty); }
            set { SetValue(IsMouseEnabledProperty, value); }    
        }

        public static readonly DependencyProperty IsMouseEnabledProperty =
            DependencyProperty.Register("IsMouseEnabled", typeof(bool), typeof(AutoHideBehavior), new PropertyMetadata(true));


        public Dock Dock
        {
            get { return (Dock)GetValue(DockProperty); }
            set { SetValue(DockProperty, value); }
        }

        public static readonly DependencyProperty DockProperty =
            DependencyProperty.Register("Dock", typeof(Dock), typeof(AutoHideBehavior), new PropertyMetadata(Dock.Left));


        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AutoHideBehavior control)
            {
                control.UpdateVisibility();
            }
        }

        #endregion DependencyProperties


        private bool _isAttached;

        private DelayValue<Visibility> _delayVisibility;


        // ターゲット付近にマウスカーソルがある状態
        private bool _isMouseOvered;
        public bool IsMouseOvered
        {
            get { return _isMouseOvered; }
            private set
            {
                if ( _isMouseOvered != value)
                {
                    _isMouseOvered = value;
                    UpdateVisibility();
                }
            }
        }



        protected override void OnAttached()
        {
            base.OnAttached();

            if (this.Screen == null) throw new InvalidOperationException("Screen property is required.");

            _delayVisibility = new DelayValue<Visibility>(this.AssociatedObject.Visibility);
            _delayVisibility.ValueChanged += DelayVisibility_ValueChanged;

            this.Screen.MouseMove += Screen_MouseMove;
            this.Screen.MouseLeave += Screen_MouseLeave;

            _isAttached = true;
            UpdateVisibility();
        }

        protected override void OnDetaching()
        {
            _isAttached = false;

            base.OnDetaching();

            _delayVisibility.ValueChanged -= DelayVisibility_ValueChanged;

            this.Screen.MouseMove -= Screen_MouseMove;
            this.Screen.MouseLeave -= Screen_MouseLeave;

            BindingOperations.ClearBinding(this.AssociatedObject, FrameworkElement.VisibilityProperty);
        }


        private void DelayVisibility_ValueChanged(object sender, EventArgs e)
        {
            this.AssociatedObject.Visibility = _delayVisibility.Value;
        }

        /// <summary>
        /// マウス移動イベント.
        /// マウス位置で表示非表示を更新する
        /// </summary>
        private void Screen_MouseMove(object sender, MouseEventArgs e)
        {
            IsMouseOvered = IsMouseOver();
        }

        private void Screen_MouseLeave(object sender, MouseEventArgs e)
        {
            IsMouseOvered = false;
        }

        /// <summary>
        /// マウスカーソル位置による表示開始判定
        /// </summary>
        private bool IsMouseOver()
        {
            if (!this.IsMouseEnabled)
            {
                return false;
            }

            if (this.AssociatedObject.IsMouseOver)
            {
                return true;
            }

            var point = Mouse.GetPosition(this.Screen);

            switch (this.Dock)
            {
                case Dock.Left:
                    return point.X < this.HitTestMargin;

                case Dock.Top:
                    return point.Y < this.HitTestMargin;

                case Dock.Right:
                    return point.X > this.Screen.ActualWidth - this.HitTestMargin;

                case Dock.Bottom:
                    return point.Y > this.Screen.ActualHeight - this.HitTestMargin;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 表示更新
        /// </summary>
        /// <param name="now"></param>
        /// <param name="isForce"></param>
        private void UpdateVisibility(bool now = false, bool isForce = false)
        {
            if (!_isAttached) return;
            
            SetVisibility(CanVisible(), now, isForce);
        }

        private bool CanVisible()
        {
            return this.IsVisibleLocked || (this.IsEnabled ? _isMouseOvered : true);
        }

        private void SetVisibility(bool isVisible, bool now, bool isForce)
        {
            if (isVisible)
            {
                _delayVisibility.SetValue(Visibility.Visible, 0.0, isForce);
            }
            else
            {
                _delayVisibility.SetValue(Visibility.Collapsed, now ? 0.0 : this.DelayTime, isForce);
            }
        }

        /// <summary>
        /// 遅延非表示の場合に遅延時間を延長する。
        /// キー入力等での表示更新遅延時間のリセットに使用
        /// </summary>
        public void ResetDelayTime()
        {
            if (!CanVisible())
            {
                SetVisibility(false, false, true);
            }
        }

    }
}
