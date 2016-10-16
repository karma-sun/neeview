// from http://mslaboratory.blog.eonet.jp/default/2012/10/behaviordragdro-cee4.html

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;

namespace DragExtensions
{
    internal class DragBehavior
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

        private static readonly DependencyProperty s_startPointProperty = DependencyProperty.RegisterAttached("StartPoint", typeof(Point?), typeof(DragBehavior), new PropertyMetadata(null));

        private static readonly DependencyProperty s_dragAdornerProperty = DependencyProperty.RegisterAttached("DragAdorner", typeof(DragAdorner), typeof(DragBehavior), new PropertyMetadata(null));

        public static DragAdorner GetDragAdorner(DependencyObject obj)
        {
            return (DragAdorner)obj.GetValue(s_dragAdornerProperty);
        }
        public static void SetDragAdorner(DependencyObject obj, DragAdorner value)
        {
            obj.SetValue(s_dragAdornerProperty, value);
        }


        // ListBox
        private static readonly DependencyProperty s_listBoxProperty = DependencyProperty.RegisterAttached("ListBox", typeof(ListBox), typeof(DragBehavior), new PropertyMetadata(null));

        public static ListBox GetListBox(DependencyObject obj)
        {
            return (ListBox)obj.GetValue(s_listBoxProperty);
        }
        public static void SetListBox(DependencyObject obj, ListBox value)
        {
            obj.SetValue(s_listBoxProperty, value);
        }


        private static void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //e.Handled = true;
            UIElement element = sender as UIElement;
            element.SetValue(DragBehavior.s_startPointProperty, e.GetPosition(element));
        }

        private static void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            UIElement element = sender as UIElement;
            element.SetValue(DragBehavior.s_startPointProperty, null);
        }

        private static void OnMouseMove(object sender, MouseEventArgs e)
        {
            UIElement element = sender as UIElement;

            if (e.LeftButton == MouseButtonState.Released) element.SetValue(DragBehavior.s_startPointProperty, null);
            if (element.GetValue(DragBehavior.s_startPointProperty) == null) return;

            Point startPoint = (Point)element.GetValue(DragBehavior.s_startPointProperty);
            Point point = e.GetPosition(element);
            if (element is ListBoxItem) point.Y = ((ListBoxItem)element).ActualHeight / 2;

            if (!IsDragging(startPoint, point)) return;

            var adornerElement = GetListBox(element) ?? element;
            DragAdorner adorner = new DragAdorner(adornerElement, element, 0.5, point);
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

            // ListBox Scroll
            var listbox = GetListBox(element);
            if (listbox != null)
            {
                var peer = ItemsControlAutomationPeer.CreatePeerForElement(listbox);
                var scrollProvider = peer.GetPattern(PatternInterface.Scroll) as IScrollProvider;

                if (scrollProvider.VerticallyScrollable)
                {
                    var cursor = WPFUtil.GetMousePosition(listbox) - listbox.TranslatePoint(new Point(0, 0), Window.GetWindow(listbox));
                    if (cursor.Y < 0)
                    {
                        scrollProvider.Scroll(ScrollAmount.NoAmount, ScrollAmount.SmallDecrement);
                    }
                    else if (cursor.Y > listbox.ActualHeight)
                    {
                        scrollProvider.Scroll(ScrollAmount.NoAmount, ScrollAmount.SmallIncrement);
                    }
                }
            }
        }
    }
}
