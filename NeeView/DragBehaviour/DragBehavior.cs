// from http://mslaboratory.blog.eonet.jp/default/2012/10/behaviordragdro-cee4.html

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace DragExtensions
{
    class DragBehavior
    {
        public static readonly DependencyProperty IsEnableProperty = DependencyProperty.RegisterAttached("IsEnable", typeof(bool), typeof(DragBehavior), new PropertyMetadata(false, IsEnableChanged));

        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        public static bool GetIsEnable(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnableProperty);
        }
        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        public static void SetIsEnable(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnableProperty, value);
        }

        public static void IsEnableChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = sender as UIElement;
            if (element == null) return;

            if ((bool)e.NewValue)
            {
                element.PreviewMouseDown += OnPreviewMouseDown;
                element.PreviewMouseUp += OnPreviewMouseUp;
                element.MouseMove += OnMouseMove;
                element.QueryContinueDrag += OnQueryContinueDrag;
            }
            else
            {
                element.PreviewMouseDown -= OnPreviewMouseDown;
                element.PreviewMouseUp -= OnPreviewMouseUp;
                element.MouseMove -= OnMouseMove;
                element.QueryContinueDrag -= OnQueryContinueDrag;
            }
        }

        private static readonly DependencyProperty StartPointProperty = DependencyProperty.RegisterAttached("StartPoint", typeof(Point?), typeof(DragBehavior), new PropertyMetadata(null));

        private static readonly DependencyProperty DragAdornerProperty = DependencyProperty.RegisterAttached("DragAdorner", typeof(DragAdorner), typeof(DragBehavior), new PropertyMetadata(null));

        public static DragAdorner GetDragAdorner(DependencyObject obj)
        {
            return (DragAdorner)obj.GetValue(DragAdornerProperty);
        }
        public static void SetDragAdorner(DependencyObject obj, DragAdorner value)
        {
            obj.SetValue(DragAdornerProperty, value);
        }

        private static void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //e.Handled = true;
            UIElement element = sender as UIElement;
            element.SetValue(DragBehavior.StartPointProperty, e.GetPosition(element));
        }

        private static void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            UIElement element = sender as UIElement;
            element.SetValue(DragBehavior.StartPointProperty, null);
        }

        private static void OnMouseMove(object sender, MouseEventArgs e)
        {
            UIElement element = sender as UIElement;

            if (e.LeftButton == MouseButtonState.Released) element.SetValue(DragBehavior.StartPointProperty, null);
            if (element.GetValue(DragBehavior.StartPointProperty) == null) return;

            Point startPoint = (Point)element.GetValue(DragBehavior.StartPointProperty);
            Point point = e.GetPosition(element);

            if (!IsDragging(startPoint, point)) return;

            DragAdorner adorner = new DragAdorner(element, 0.5, point);
            DragBehavior.SetDragAdorner(element, adorner);

            DragDrop.DoDragDrop(element, element, DragDropEffects.Copy | DragDropEffects.Move);

            adorner.Remove();
            DragBehavior.SetDragAdorner(element, null);
        }

        private static Boolean IsDragging(Point pointA, Point pointB)
        {
            if (Math.Abs(pointA.X - pointB.X) > SystemParameters.MinimumHorizontalDragDistance) return true;
            if (Math.Abs(pointA.Y - pointB.Y) > SystemParameters.MinimumVerticalDragDistance) return true;
            return false;
        }

        private static void OnQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            UIElement element = sender as UIElement;
            DragAdorner adorner = DragBehavior.GetDragAdorner(element);
            if (adorner != null) adorner.Position = WPFUtil.GetMousePosition(element);
        }
    }


}
