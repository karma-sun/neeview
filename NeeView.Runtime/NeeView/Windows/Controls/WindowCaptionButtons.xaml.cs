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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView.Windows.Controls
{
    public partial class WindowCaptionButtons : UserControl, IHasMaximizeButton
    {
        private Window _window;
        private SnapLayoutPresenter _snapLayoutPresenter;


        public WindowCaptionButtons()
        {
            InitializeComponent();
            _snapLayoutPresenter = new SnapLayoutPresenter(this);

            this.Loaded += (s, e) => InitializeWindow(Window.GetWindow(this));
            this.Root.DataContext = this;
        }


        public bool IsMinimizeEnabled
        {
            get { return (bool)GetValue(IsMinimizeEnabledProperty); }
            set { SetValue(IsMinimizeEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsMinimizeEnabledProperty =
            DependencyProperty.Register("IsMinimizeEnabled", typeof(bool), typeof(WindowCaptionButtons), new PropertyMetadata(true));


        public bool IsMaximizeEnabled
        {
            get { return (bool)GetValue(IsMaximizeEnabledProperty); }
            set { SetValue(IsMaximizeEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsMaximizeEnabledProperty =
            DependencyProperty.Register("IsMaximizeEnabled", typeof(bool), typeof(WindowCaptionButtons), new PropertyMetadata(true));


        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness", typeof(double), typeof(WindowCaptionButtons), new PropertyMetadata(1.0));



        public double ForegroundOpacity
        {
            get { return (double)GetValue(ForegroundOpacityProperty); }
            set { SetValue(ForegroundOpacityProperty, value); }
        }

        public static readonly DependencyProperty ForegroundOpacityProperty =
            DependencyProperty.Register("ForegroundOpacity", typeof(double), typeof(WindowCaptionButtons), new PropertyMetadata(1.0));



        public void InitializeWindow(Window window)
        {
            if (window == null) return;

            if (_window != null)
            {
                _window.StateChanged -= Window_StateChanged;
                _snapLayoutPresenter.Detach(_window);
            }

            _window = window;
            _window.StateChanged += Window_StateChanged;
            _snapLayoutPresenter.Attach(_window);

            Window_StateChanged(this, null);
        }


        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);
            UpdateStrokeThickness(newDpi);
        }

        public void UpdateStrokeThickness(DpiScale dpi)
        {
            StrokeThickness = Math.Max(Math.Floor(dpi.DpiScaleX), 1.0) / dpi.DpiScaleX;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (_window == null) return;

            if (_window.WindowState == WindowState.Maximized)
            {
                //this.Root.Margin = new Thickness(0, 0, 2, 0);
                this.CaptionRestoreButton.Visibility = Visibility.Visible;
                this.CaptionMaximizeButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                //this.Root.Margin = new Thickness();
                this.CaptionRestoreButton.Visibility = Visibility.Collapsed;
                this.CaptionMaximizeButton.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// 現在の最大化ボタンを取得
        /// </summary>
        /// <returns></returns>
        public Button GetMaximizeButton()
        {
            return IsMaximizeEnabled
                ? this.CaptionMaximizeButton.IsVisible ? this.CaptionMaximizeButton : this.CaptionRestoreButton
                : null;
        }

        /// <summary>
        /// 最大化ボタンの背景変更 (別手段)
        /// </summary>
        public void SetMaximizeButtonBackground(Brush brush)
        {
            this.CaptionMaximizeButtonBase.Background = brush;
        }
    }
}
