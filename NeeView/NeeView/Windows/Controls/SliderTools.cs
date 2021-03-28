using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace NeeView.Windows.Controls
{
    // https://stackoverflow.com/questions/2909862/slider-does-not-drag-in-combination-with-ismovetopointenabled-behaviour
    public class SliderTools : DependencyObject
    {
        public static bool GetMoveToPointOnDrag(DependencyObject obj)
        {
            return (bool)obj.GetValue(MoveToPointOnDragProperty);
        }

        public static void SetMoveToPointOnDrag(DependencyObject obj, bool value)
        {
            obj.SetValue(MoveToPointOnDragProperty, value);
        }

        public static readonly DependencyProperty MoveToPointOnDragProperty =
            DependencyProperty.RegisterAttached("MoveToPointOnDrag", typeof(bool), typeof(SliderTools), new PropertyMetadata(false, MoveToPointOnDragPropertyChanged));


        private static void MoveToPointOnDragPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var slider = d as Slider;
            if (d is null) return;

            if ((bool)e.NewValue)
            {
                slider.PreviewMouseLeftButtonDown += Slider_PreviewMouseLeftButtonDown;
            }
            else
            {
                slider.PreviewMouseLeftButtonDown -= Slider_PreviewMouseLeftButtonDown;
            }
        }

        private static void Slider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var slider = sender as Slider;
            if (slider is null) return;

            var track = slider.Template.FindName("PART_Track", slider) as Track;
            if (track is null) return;

            var thumb = track.Thumb;
            if (thumb is null || thumb.IsMouseOver) return;

            // マウスポインターの位置に値を更新
            slider.Value = track.ValueFromPoint(e.GetPosition(track));
            slider.UpdateLayout();

            // Thumbをドラッグしたのと同じ効果をさせる
            thumb.RaiseEvent(new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = e.Source,
            });

            e.Handled = true;
        }
    }

}
