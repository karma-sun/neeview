// from http://youku.io/questions/313305/c-sharp-wpf-slider-issue-with-ismovetopointenabled

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    public class SliderTools : DependencyObject
    {
        public static UIElement GetFocusTo(DependencyObject obj) { return (UIElement)obj.GetValue(FocusToProperty); }
        public static void SetFocusTo(DependencyObject obj, UIElement value) { obj.SetValue(FocusToProperty, value); }
        public static readonly DependencyProperty FocusToProperty = DependencyProperty.RegisterAttached("FocusTo", typeof(UIElement), typeof(SliderTools), new PropertyMetadata(null));

        public static bool GetMoveToPointOnDrag(DependencyObject obj) { return (bool)obj.GetValue(MoveToPointOnDragProperty); }
        public static void SetMoveToPointOnDrag(DependencyObject obj, bool value) { obj.SetValue(MoveToPointOnDragProperty, value); }
        public static readonly DependencyProperty MoveToPointOnDragProperty = DependencyProperty.RegisterAttached("MoveToPointOnDrag", typeof(bool), typeof(SliderTools), new PropertyMetadata
        {
            PropertyChangedCallback = (obj, changeEvent) =>
            {
                var slider = (Slider)obj;
                if ((bool)changeEvent.NewValue)
                {
                    slider.PreviewMouseLeftButtonDown += (s, e) =>
                    {
                        slider.CaptureMouse();
                        var element = GetFocusTo(obj);
                        if (element != null) element.Focus();
                    };
                    slider.PreviewMouseLeftButtonUp += (s, e) =>
                    {
                        slider.ReleaseMouseCapture();
                    };
                    slider.MouseMove += (obj2, mouseEvent) =>
                    {
                        if (mouseEvent.LeftButton == MouseButtonState.Pressed)
                        {
                            slider.RaiseEvent(new MouseButtonEventArgs(mouseEvent.MouseDevice, mouseEvent.Timestamp, MouseButton.Left)
                            {
                                RoutedEvent = UIElement.PreviewMouseLeftButtonDownEvent,
                                Source = mouseEvent.Source,
                            });
                        }
                    };
                }
            }
        });
    }

}
