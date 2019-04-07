using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// VideoSlider.xaml の相互作用ロジック
    /// </summary>
    public partial class VideoSlider : UserControl
    {
        #region Fields

        private Brush _grayTruchBrush = new SolidColorBrush(Color.FromArgb(0x80, 0x80, 0x80, 0x80));

        private double _dragPointX;

        #endregion

        #region DependencyProperties

        //
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(VideoSlider), new PropertyMetadata(0.0, OnParameterChanged));

        //
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Maximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(VideoSlider), new PropertyMetadata(1.0, OnParameterChanged));

        //
        private static void OnParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VideoSlider control)
            {
                control.UpdateThumbPosition();
            }
        }

        //
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(VideoSlider), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged, OnValueCoerce));

        //
        private static object OnValueCoerce(DependencyObject d, object baseValue)
        {
            var control = d as VideoSlider;
            if (control != null)
            {
                return NeeLaboratory.MathUtility.Clamp((double)baseValue, control.Minimum, control.Maximum);
            }
            else
            {
                return baseValue;
            }
        }

        //
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //Debug.WriteLine("{0} -> {1}", (double)e.OldValue, (double)e.NewValue);

            var control = d as VideoSlider;
            if (control != null)
            {
                control.UpdateThumbPosition();

                RoutedPropertyChangedEventArgs<double> args = new RoutedPropertyChangedEventArgs<double>((double)e.OldValue, (double)e.NewValue);
                args.RoutedEvent = VideoSlider.ValueChangedEvent;
                control.RaiseEvent(args);
            }
        }

        //
        public bool IsDirectionReversed
        {
            get { return (bool)GetValue(IsDirectionReversedProperty); }
            set { SetValue(IsDirectionReversedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDirectionReversed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDirectionReversedProperty =
            DependencyProperty.Register("IsDirectionReversed", typeof(bool), typeof(VideoSlider), new PropertyMetadata(false, OnDirectionReversedChanged));

        //
        private static void OnDirectionReversedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VideoSlider control)
            {
                control.UpdateSliderLayout();
            }
        }


        public double ThumbSize
        {
            get { return (double)GetValue(ThumbSizeProperty); }
            set { SetValue(ThumbSizeProperty, value); }
        }

        public static readonly DependencyProperty ThumbSizeProperty =
            DependencyProperty.Register("ThumbSize", typeof(double), typeof(VideoSlider), new PropertyMetadata(25.0, OnThumbSizeChanged));

        private static void OnThumbSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VideoSlider control)
            {
                control.UpdateSliderLayout();
            }
        }


        public Brush SliderBrush
        {
            get { return (Brush)GetValue(SliderBrushProperty); }
            set { SetValue(SliderBrushProperty, value); }
        }

        public static readonly DependencyProperty SliderBrushProperty =
            DependencyProperty.Register("SliderBrush", typeof(Brush), typeof(VideoSlider), new PropertyMetadata(Brushes.SteelBlue, OnSliderBrushChanged));

        private static void OnSliderBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VideoSlider control)
            {
                control.UpdateSliderLayout();
            }
        }


        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(Brush), typeof(VideoSlider), new PropertyMetadata(Brushes.Transparent, OnFillChanged));

        private static void OnFillChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VideoSlider control)
            {
                control.Thumb.Background = control.Fill;
            }
        }




        #endregion

        #region Constructors

        public VideoSlider()
        {
            InitializeComponent();
        }

        #endregion

        #region Events

        /// <summary>
        /// Event correspond to Value changed event
        /// </summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(VideoSlider));

        /// <summary>
        /// Add / Remove ValueChangedEvent handler
        /// </summary>
        public event RoutedPropertyChangedEventHandler<double> ValueChanged { add { AddHandler(ValueChangedEvent, value); } remove { RemoveHandler(ValueChangedEvent, value); } }


        /// <summary>
        /// Event correspond to Value changed event
        /// </summary>
        public static readonly RoutedEvent DragStartedEvent = EventManager.RegisterRoutedEvent("DragStarted", RoutingStrategy.Bubble, typeof(DragStartedEventHandler), typeof(VideoSlider));

        /// <summary>
        /// Add / Remove ValueChangedEvent handler
        /// </summary>
        public event DragStartedEventHandler DragStarted { add { AddHandler(DragStartedEvent, value); } remove { RemoveHandler(DragStartedEvent, value); } }



        /// <summary>
        /// Event correspond to Value changed event
        /// </summary>
        public static readonly RoutedEvent DragCompletedEvent = EventManager.RegisterRoutedEvent("DragCompleted", RoutingStrategy.Bubble, typeof(DragCompletedEventHandler), typeof(VideoSlider));

        /// <summary>
        /// Add / Remove ValueChangedEvent handler
        /// </summary>
        public event DragCompletedEventHandler DragCompleted { add { AddHandler(DragCompletedEvent, value); } remove { RemoveHandler(DragCompletedEvent, value); } }


        /// <summary>
        /// Event correspond to Value changed event
        /// </summary>
        public static readonly RoutedEvent DragDeltaEvent = EventManager.RegisterRoutedEvent("DragDelta", RoutingStrategy.Bubble, typeof(DragDeltaEventHandler), typeof(VideoSlider));

        /// <summary>
        /// Add / Remove ValueChangedEvent handler
        /// </summary>
        public event DragDeltaEventHandler DragDelta { add { AddHandler(DragDeltaEvent, value); } remove { RemoveHandler(DragDeltaEvent, value); } }

        #endregion Events

        #region EventHandlers

        //
        private void Root_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(this.Root);
            SetValuePosition(pos.X - this.ThumbSize * 0.5);

            this.Thumb.UpdateLayout(); // 座標をここで反映させる

            // Thumbをドラッグしたのと同じ効果をさせる
            this.Thumb.RaiseEvent(new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = e.Source,
            });

            e.Handled = true;
        }

        //
        private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateThumbPosition();
        }

        /// <summary>
        /// ドラッグ開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            this.Thumb.Background = this.SliderBrush;

            _dragPointX = Canvas.GetLeft(this.Thumb);
            ////Debug.WriteLine($"Thumb: DragStart: {_dragPointX}");

            DragStartedEventArgs args = new DragStartedEventArgs(e.HorizontalOffset, e.VerticalOffset);
            args.RoutedEvent = VideoSlider.DragStartedEvent;
            RaiseEvent(args);
        }

        /// <summary>
        /// ドラッグ終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            this.Thumb.Background = this.Fill;

            UpdateThumbPosition();

            DragCompletedEventArgs args = new DragCompletedEventArgs(e.HorizontalChange, e.VerticalChange, e.Canceled);
            args.RoutedEvent = VideoSlider.DragCompletedEvent;
            RaiseEvent(args);
        }

        /// <summary>
        /// ドラッグ中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            _dragPointX += e.HorizontalChange;
            ////Debug.WriteLine($"Thumb: {_dragPointX} (offset={e.HorizontalChange})");

            SetValuePosition(_dragPointX);

            DragDeltaEventArgs args = new DragDeltaEventArgs(e.HorizontalChange, e.VerticalChange);
            args.RoutedEvent = VideoSlider.DragDeltaEvent;
            RaiseEvent(args);
        }

        #endregion

        #region Methods

        // 座標 > 値
        private void SetValuePosition(double x)
        {
            var value = GetValueFromPosition(x);

            if (value > Maximum) value = Maximum;
            if (value < Minimum) value = Minimum;

            this.Value = value;
            UpdateThumbPosition(value);
        }

        // 値を設定
        private void SetValue(double value)
        {
            if (value > Maximum) value = Maximum;
            if (value < Minimum) value = Minimum;

            this.Value = value;
        }

        // IsDirectionReversed を値に反映
        private double GetReversedValue(double value)
        {
            return this.IsDirectionReversed ? (this.Maximum - value + this.Minimum) : value;
        }

        // 現在の値で座標を更新
        private void UpdateThumbPosition()
        {
            UpdateThumbPosition(this.Value);
        }

        // 指定した値で座標を更新
        private void UpdateThumbPosition(double value)
        {
            double min = 0.0;
            double max = this.Root.ActualWidth - this.ThumbSize;

            var x = Maximum > Minimum
                ? GetReversedValue(value) * (max - min) / (Maximum - Minimum) + min
                : min;

            x = Math.Max(x, 0);

            Canvas.SetLeft(this.Thumb, x);
            this.LeftTracColumn.Width = new GridLength(x);
        }

        // 座標から値を計算
        private double GetValueFromPosition(double x)
        {
            double min = 0.0;
            double max = this.Root.ActualWidth - this.ThumbSize;

            var value = GetReversedValue(x * (Maximum - Minimum) / (max - min) + Minimum);

            if (value > Maximum) value = Maximum;
            if (value < Minimum) value = Minimum;

            return value;
        }

        // 表示の更新
        private void UpdateSliderLayout()
        {
            this.LeftTrac.Fill = this.IsDirectionReversed ? _grayTruchBrush : this.SliderBrush;
            this.RightTrack.Fill = this.IsDirectionReversed ? this.SliderBrush : _grayTruchBrush;

            this.LeftTrac.Margin = new Thickness(this.ThumbSize * 0.5, 0, -1, 0);
            this.RightTrack.Margin = new Thickness(-1, 0, this.ThumbSize * 0.5, 0);

            this.Thumb.Foreground = this.SliderBrush;
            this.Thumb.Width = this.ThumbSize;
            this.Thumb.Height = this.ThumbSize;
            this.ThumbColumn.Width = new GridLength(this.ThumbSize);
            this.RootCanvas.Height = this.ThumbSize;

            UpdateThumbPosition();
        }

        #endregion
    }
}
