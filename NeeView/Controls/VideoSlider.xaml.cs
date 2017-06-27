// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
            DependencyProperty.Register("Value", typeof(double), typeof(VideoSlider), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

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
            SetValuePosition(pos.X - 12.5);

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
            this.Thumb.Background = new SolidColorBrush(Colors.SteelBlue);

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
            this.Thumb.Background = new SolidColorBrush(Colors.Transparent);

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
            double value = Canvas.GetLeft(this.Thumb) + e.HorizontalChange;
            SetValuePosition(value);

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
            double max = this.Root.ActualWidth - 25.0;

            var x = GetReversedValue(value) * (max - min) / (Maximum - Minimum) + min;

            Canvas.SetLeft(this.Thumb, x);
        }

        // 座標から値を計算
        private double GetValueFromPosition(double x)
        {
            double min = 0.0;
            double max = this.Root.ActualWidth - 25.0;

            var value = GetReversedValue(x * (Maximum - Minimum) / (max - min) + Minimum);

            if (value > Maximum) value = Maximum;
            if (value < Minimum) value = Minimum;

            return value;
        }

        // 表示の更新
        private void UpdateSliderLayout()
        {
            this.LeftTrac.Fill = this.IsDirectionReversed ? Brushes.Gray : Brushes.SteelBlue;
            this.RightTrack.Fill = this.IsDirectionReversed ? Brushes.SteelBlue : Brushes.Gray;
            UpdateThumbPosition();
        }

        #endregion
    }
}
