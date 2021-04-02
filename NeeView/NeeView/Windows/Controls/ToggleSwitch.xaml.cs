using System;
using System.Collections.Generic;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView.Windows.Controls
{
    /// <summary>
    /// ToggleSwitch.xaml の相互作用ロジック
    /// </summary>
    public partial class ToggleSwitch : UserControl
    {
        #region Fields

        private Storyboard _onAnimation;
        private Storyboard _offAnimation;

        private bool _pressed;
        private Point _startPos;
        private double _startX;
        private const double _max = 20;

        #endregion

        #region Dependency Properties

        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register("Stroke", typeof(Brush), typeof(ToggleSwitch), new PropertyMetadata(Brushes.Black, BrushProperty_Changed));


        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(Brush), typeof(ToggleSwitch), new PropertyMetadata(Brushes.White, BrushProperty_Changed));


        public Brush CheckedBrush
        {
            get { return (Brush)GetValue(CheckedBrushProperty); }
            set { SetValue(CheckedBrushProperty, value); }
        }

        public static readonly DependencyProperty CheckedBrushProperty =
            DependencyProperty.Register("CheckedBrush", typeof(Brush), typeof(ToggleSwitch), new PropertyMetadata(Brushes.SteelBlue, BrushProperty_Changed));


        public Brush CheckedThumbBrush
        {
            get { return (Brush)GetValue(CheckedThumbBrushProperty); }
            set { SetValue(CheckedThumbBrushProperty, value); }
        }

        public static readonly DependencyProperty CheckedThumbBrushProperty =
            DependencyProperty.Register("CheckedThumbBrush", typeof(Brush), typeof(ToggleSwitch), new PropertyMetadata(Brushes.White, BrushProperty_Changed));

        private static void BrushProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ToggleSwitch)?.UpdateBrush();
        }


        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(ToggleSwitch), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IsCheckedProperty_Changed));

        private static void IsCheckedProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ToggleSwitch control)
            {
                control.UpdateBrush();
                control.UpdateThumb();
            }
        }

        #endregion


        public ToggleSwitch()
        {
            InitializeComponent();
            this.Root.DataContext = this;

            _onAnimation = this.Root.Resources["OnAnimation"] as Storyboard;
            _offAnimation = this.Root.Resources["OffAnimation"] as Storyboard;
        }


        private void UpdateBrush()
        {
            if (_pressed)
            {
                this.rectangle.Fill = Brushes.Gray;
                this.rectangle.Stroke = Brushes.Gray;
                this.ellipse.Fill = Brushes.White;
            }
            else if (this.IsChecked)
            {
                this.rectangle.Fill = this.CheckedBrush;
                this.rectangle.Stroke = this.CheckedBrush;
                this.ellipse.Fill = this.CheckedThumbBrush;
            }
            else
            {
                this.rectangle.Fill = this.Fill;
                this.rectangle.Stroke = this.Stroke;
                this.ellipse.Fill = this.Stroke;
            }
        }

        private void UpdateThumb()
        {
            if (this.IsLoaded)
            {
                if (this.IsChecked)
                {
                    this.Root.BeginStoryboard(_onAnimation);
                }
                else
                {
                    this.Root.BeginStoryboard(_offAnimation);
                }
            }
            else
            {
                if (this.IsChecked)
                {
                    OnAnimation_Completed(this, null);
                }
                else
                {
                    OffAnimation_Completed(this, null);
                }
            }
        }

        private void BaseGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Focus();

            MouseInputHelper.CaptureMouse(this, this.Root);

            _startPos = e.GetPosition(this.Root);
            _pressed = true;
            _startX = this.IsChecked ? _max : 0.0;

            UpdateBrush();
        }

        private void BaseGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MouseInputHelper.ReleaseMouseCapture(this, this.Root);

            _pressed = false;

            var pos = e.GetPosition(this.Root);
            var dx = pos.X - _startPos.X;

            if (Math.Abs(dx) > SystemParameters.MinimumHorizontalDragDistance)
            {
                this.IsChecked = dx > 0;
            }
            else
            {
                this.IsChecked = !this.IsChecked;
            }

            UpdateBrush();
        }

        private void BaseGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_pressed) return;

            var pos = e.GetPosition(this.Root);
            var dx = _startX + pos.X - _startPos.X;
            if (dx < 0.0) dx = 0.0;
            if (dx > _max) dx = _max;

            this.thumbTranslate.X = dx;
        }

        private void OnAnimation_Completed(object sender, EventArgs e)
        {
            this.thumbTranslate.X = _max;
        }

        private void OffAnimation_Completed(object sender, EventArgs e)
        {
            this.thumbTranslate.X = 0.0;
        }

        private void ToggleSwitch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Enter)
            {
                IsChecked = !IsChecked;
                e.Handled = true;
            }
        }

    }
}
