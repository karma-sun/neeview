// from http://mslaboratory.blog.eonet.jp/default/2012/10/behaviordragdro-cee4.html

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace DragExtensions
{
    class WPFUtil
    {
        public static T FindVisualParent<T>(DependencyObject d) where T : DependencyObject
        {
            if (d == null) return null;

            try
            {
                DependencyObject root = VisualTreeHelper.GetParent(d);

                if (root != null && root is T)
                {
                    return root as T;
                }
                else
                {
                    T parent = FindVisualParent<T>(root);
                    if (parent != null) return parent;
                }

                return null;
            }
            catch
            {
                if (d is FrameworkElement)
                {
                    FrameworkElement element = (FrameworkElement)d;
                    if (element.Parent is T) return element.Parent as T;
                    return FindVisualParent<T>(element.Parent);
                }
                else if (d is FrameworkContentElement)
                {
                    FrameworkContentElement element = (FrameworkContentElement)d;
                    if (element.Parent is T) return element.Parent as T;
                    return FindVisualParent<T>(element.Parent);
                }
                else
                {
                    return null;
                }
            }
        }

        public static T FindVisualParent<T>(DependencyObject d, string name) where T : DependencyObject
        {
            DependencyObject root = d;
            while (true)
            {
                DependencyObject parent = FindVisualParent<T>(root);
                if (parent == null)
                {
                    return null;
                }
                else
                {
                    if (parent is FrameworkElement)
                    {
                        if (((FrameworkElement)parent).Name == name) return parent as T;
                    }
                    else if (parent is FrameworkContentElement)
                    {
                        if (((FrameworkContentElement)parent).Name == name) return parent as T;
                    }
                    else
                    {
                        return null;
                    }

                    root = parent;
                }
            }
        }

        public static T FindVisualChild<T>(DependencyObject d) where T : DependencyObject
        {
            if (d == null) return null;

            try
            {
                for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(d) - 1; i++)
                {
                    DependencyObject root = VisualTreeHelper.GetChild(d, i);
                    if (root != null && root is T)
                    {
                        return root as T;
                    }
                    else
                    {
                        T child = FindVisualChild<T>(root);
                        if (child != null) return child;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public static T FindVisualChild<T>(DependencyObject d, string name) where T : DependencyObject
        {
            if (d == null) return null;

            try
            {
                for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(d) - 1; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(d, i);
                    if (child != null && child is T)
                    {
                        if (child is FrameworkElement)
                        {
                            if (((FrameworkElement)child).Name == name) return child as T;
                        }
                        else if (child is FrameworkContentElement)
                        {
                            if (((FrameworkContentElement)child).Name == name) return child as T;
                        }
                        else
                        {
                            return null;
                        }
                    }

                    T nextChild = FindVisualChild<T>(child, name);
                    if (nextChild != null) return nextChild;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        [DllImport("user32.dll")]
        private static extern void GetCursorPos(out POINT pt);

        [DllImport("user32.dll")]
        private static extern int ScreenToClient(IntPtr hwnd, ref POINT pt);

        private struct POINT
        {
            public UInt32 X;
            public UInt32 Y;
        }

        public static Point GetMousePosition(System.Windows.Media.Visual visual)
        {
            POINT point;
            GetCursorPos(out point);

            HwndSource source = (HwndSource)HwndSource.FromVisual(visual);
            IntPtr hwnd = source.Handle;

            ScreenToClient(hwnd, ref point);
            return new Point(point.X, point.Y);
        }
    }
}
