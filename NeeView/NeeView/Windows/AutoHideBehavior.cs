using Microsoft.Xaml.Behaviors;
using NeeLaboratory.Windows.Media;
using NeeView.Windows.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// 自動非表示のフォーカスロックモード
    /// </summary>
    public enum AutoHideFocusLockMode
    {
        [AliasName("@EnumAutoHideFocusLockModeNone")]
        None,

        [AliasName("@EnumAutoHideFocusLockModeLogicalFocusLock")]
        LogicalFocusLock,
        
        [AliasName("@EnumAutoHideFocusLockModeLogicalTextBoxFocusLock")]
        LogicalTextBoxFocusLock,

        [AliasName("@EnumAutoHideFocusLockModeFocusLock")]
        FocusLock,
        
        [AliasName("@EnumAutoHideFocusLockModeTextBxFocusLock")]
        TextBoxFocusLock,
    }


    /// <summary>
    /// 自動非表示ビヘイビア
    /// </summary>
    public class AutoHideBehavior : Behavior<FrameworkElement>
    {
        #region DependencyProperties

        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(AutoHideBehavior), new PropertyMetadata(null));


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


        public Dock Dock
        {
            get { return (Dock)GetValue(DockProperty); }
            set { SetValue(DockProperty, value); }
        }

        public static readonly DependencyProperty DockProperty =
            DependencyProperty.Register("Dock", typeof(Dock), typeof(AutoHideBehavior), new PropertyMetadata(Dock.Left));


        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool), typeof(AutoHideBehavior), new PropertyMetadata(false, OnIsEnabledPropertyChanged));

        private static void OnIsEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AutoHideBehavior control)
            {
                var controlName = control.AssociatedObject.Name ?? control.AssociatedObject.ToString();
                Debug.WriteLine($"{controlName}.AutoHideBehavior.IsEnabled: {control.IsEnabled}");
                control.UpdateVisibility(UpdateVisibilityOption.All);
            }
        }


        public double DelayTime
        {
            get { return (double)GetValue(DelayTimeProperty); }
            set { SetValue(DelayTimeProperty, value); }
        }

        public static readonly DependencyProperty DelayTimeProperty =
            DependencyProperty.Register("DelayTime", typeof(double), typeof(AutoHideBehavior), new PropertyMetadata(1000.0, OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AutoHideBehavior control)
            {
                control.UpdateVisibility();
            }
        }

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
            DependencyProperty.Register("HitTestMargin", typeof(double), typeof(AutoHideBehavior), new PropertyMetadata(16.0));


        public bool IsMouseEnabled
        {
            get { return (bool)GetValue(IsMouseEnabledProperty); }
            set { SetValue(IsMouseEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsMouseEnabledProperty =
            DependencyProperty.Register("IsMouseEnabled", typeof(bool), typeof(AutoHideBehavior), new PropertyMetadata(true, OnIsMouseEnablePropertyChanged));

        private static void OnIsMouseEnablePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AutoHideBehavior control)
            {
                control.UpdateVisibility(UpdateVisibilityOption.UpdateMouseOver);
            }
        }

        public AutoHideFocusLockMode FocusLockMode
        {
            get { return (AutoHideFocusLockMode)GetValue(FocusLockModeProperty); }
            set { SetValue(FocusLockModeProperty, value); }
        }

        public static readonly DependencyProperty FocusLockModeProperty =
            DependencyProperty.Register("FocusLockMode", typeof(AutoHideFocusLockMode), typeof(AutoHideBehavior), new PropertyMetadata(AutoHideFocusLockMode.None, OnFocusLockModePropertyChangd));

        private static void OnFocusLockModePropertyChangd(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AutoHideBehavior control)
            {
                control.UpdateVisibility(UpdateVisibilityOption.UpdateFocusLock);
            }
        }


        public bool IsKeyDownDelayEnabled
        {
            get { return (bool)GetValue(IsKeyDownDelayEnabledProperty); }
            set { SetValue(IsKeyDownDelayEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsKeyDownDelayEnabledProperty =
            DependencyProperty.Register("IsKeyDownDelayEnabled", typeof(bool), typeof(AutoHideBehavior), new PropertyMetadata(true));


        #endregion DependencyProperties


        private DelayValue<Visibility> _delayVisibility;
        private bool _isAttached;
        private bool _isMouseOver;
        private bool _isFocusLock;
        

        protected override void OnAttached()
        {
            base.OnAttached();

            if (this.Screen == null) throw new InvalidOperationException("Screen property is required.");

            _delayVisibility = new DelayValue<Visibility>(this.AssociatedObject.Visibility);
            _delayVisibility.ValueChanged += DelayVisibility_ValueChanged;

            this.AssociatedObject.IsKeyboardFocusWithinChanged += AssociatedObject_IsKeyboardFocusWithinChanged;
            this.AssociatedObject.GotFocus += AssociatedObject_GotFocus;
            this.AssociatedObject.LostFocus += AssociatedObject_LostFocus;
            this.AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
            this.Screen.MouseMove += Screen_MouseMove;
            this.Screen.MouseLeave += Screen_MouseLeave;

            _isAttached = true;
            UpdateVisibility(UpdateVisibilityOption.All);
        }

        protected override void OnDetaching()
        {
            _isAttached = false;

            base.OnDetaching();

            _delayVisibility.ValueChanged -= DelayVisibility_ValueChanged;

            this.AssociatedObject.IsKeyboardFocusWithinChanged -= AssociatedObject_IsKeyboardFocusWithinChanged;
            this.AssociatedObject.GotFocus -= AssociatedObject_GotFocus;
            this.AssociatedObject.LostFocus -= AssociatedObject_LostFocus;
            this.AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
            this.Screen.MouseMove -= Screen_MouseMove;
            this.Screen.MouseLeave -= Screen_MouseLeave;

            BindingOperations.ClearBinding(this.AssociatedObject, FrameworkElement.VisibilityProperty);
        }


        private void DelayVisibility_ValueChanged(object sender, EventArgs e)
        {
            if (this.AssociatedObject.Visibility != _delayVisibility.Value)
            {
                var controlName = this.AssociatedObject.Name ?? this.AssociatedObject.ToString();
                Debug.WriteLine($"{controlName}.Visibility: {_delayVisibility.Value}");
            }

            this.AssociatedObject.Visibility = _delayVisibility.Value;
        }

        private void AssociatedObject_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsEnabled) return;
            UpdateVisibility(UpdateVisibilityOption.UpdateFocusLock);
        }

        private void AssociatedObject_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!IsEnabled) return;
            UpdateVisibility(UpdateVisibilityOption.UpdateFocusLock);
        }

        private void AssociatedObject_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsEnabled) return;
            UpdateVisibility(UpdateVisibilityOption.UpdateFocusLock);
        }

        private void AssociatedObject_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsEnabled) return;
            if (!this.IsKeyDownDelayEnabled) return;

            // 非表示要求の場合に遅延表示を再発行することで表示状態を延長する
            if (!CanVisible())
            {
                SetVisibility(false, false, true);
            }
        }

        private void Screen_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsEnabled) return;
            UpdateVisibility(UpdateVisibilityOption.UpdateMouseOver);
        }

        private void Screen_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!IsEnabled) return;
            UpdateVisibility(UpdateVisibilityOption.UpdateMouseOver);
        }


        /// <summary>
        /// 表示更新用フラグ
        /// </summary>
        [Flags]
        private enum UpdateVisibilityOption
        {
            None,
            Now = 0x0001,
            IsForce = 0x0002,
            UpdateMouseOver = 0x0004,
            UpdateFocusLock = 0x0008,

            All = Now | IsForce | UpdateMouseOver | UpdateFocusLock
        }

        /// <summary>
        /// 表示更新
        /// </summary>
        private void UpdateVisibility(UpdateVisibilityOption options = UpdateVisibilityOption.None)
        {
            if (!_isAttached) return;

            if ((options & UpdateVisibilityOption.UpdateMouseOver) == UpdateVisibilityOption.UpdateMouseOver)
            {
                UpdateMouseOverr();
            }

            if ((options & UpdateVisibilityOption.UpdateFocusLock) == UpdateVisibilityOption.UpdateFocusLock)
            {
                UpdateFocusLock();
            }

            var now = (options & UpdateVisibilityOption.Now) == UpdateVisibilityOption.Now;
            var isForce = (options & UpdateVisibilityOption.IsForce) == UpdateVisibilityOption.IsForce;
            SetVisibility(CanVisible(), now, isForce);
        }

        /// <summary>
        /// マウスカーソル位置による表示開始判定
        /// </summary>
        private void UpdateMouseOverr()
        {
            _isMouseOver = IsMouseOver();

            bool IsMouseOver()
            {
                if (!this.IsEnabled || !this.IsMouseEnabled)
                {
                    return false;
                }

                if (!this.Screen.IsMouseOver)
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
        }

        /// <summary>
        /// フォーカスによる表示判定
        /// </summary>
        private void UpdateFocusLock()
        {
            _isFocusLock = IsFocusLock();

            bool IsFocusLock()
            {
                switch (this.FocusLockMode)
                {
                    case AutoHideFocusLockMode.FocusLock:
                        return this.AssociatedObject.IsKeyboardFocusWithin;

                    case AutoHideFocusLockMode.TextBoxFocusLock:
                        return this.AssociatedObject.IsKeyboardFocusWithin && VisualTreeUtility.HasParentElement(Keyboard.FocusedElement as TextBox, this.AssociatedObject);

                    case AutoHideFocusLockMode.LogicalFocusLock:
                        return GetFocusedElement() != null;

                    case AutoHideFocusLockMode.LogicalTextBoxFocusLock:
                        return GetFocusedElement() is TextBox;

                    default:
                        return false;
                }
            }

            FrameworkElement GetFocusedElement()
            {
                var focusedElement = FocusManager.GetFocusedElement(FocusManager.GetFocusScope(this.AssociatedObject)) as FrameworkElement;
                return VisualTreeUtility.HasParentElement(focusedElement, this.AssociatedObject) ? focusedElement : null;
            }
        }


        private bool CanVisible()
        {
            return !this.IsEnabled || this.IsVisibleLocked || _isMouseOver || _isFocusLock;
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
        /// 遅延時間を延長する
        /// </summary>
        public void ResetDelayTime()
        {
            if (!_isAttached) return;
            if (!IsEnabled) return;

            if (!CanVisible())
            {
                SetVisibility(false, false, true);
            }
        }

        /// <summary>
        /// コントロールからBehavior取得
        /// </summary>
        public static AutoHideBehavior GetBehavior(FrameworkElement target)
        {
            var behavior = Interaction.GetBehaviors(target)
                .OfType<AutoHideBehavior>()
                .FirstOrDefault();

            return behavior;
        }

    }
}
