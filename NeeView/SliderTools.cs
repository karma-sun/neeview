// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
    // from http://youku.io/questions/313305/c-sharp-wpf-slider-issue-with-ismovetopointenabled
    public class SliderTools : DependencyObject
    {
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
