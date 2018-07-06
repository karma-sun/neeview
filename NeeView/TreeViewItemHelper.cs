using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView
{
    // from https://stackoverflow.com/questions/5047576/wpf-treeview-how-to-style-selected-items-with-rounded-corners-like-in-explorer
    // from  https://stackoverflow.com/questions/664632/highlight-whole-treeviewitem-line-in-wpf

    public static class TreeViewItemHelper
    {
        private static TreeViewItem CurrentItem;
        private static readonly RoutedEvent UpdateOverItemEvent = EventManager.RegisterRoutedEvent("UpdateOverItem", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TreeViewItemHelper));
        private static readonly DependencyPropertyKey IsMouseDirectlyOverItemKey = DependencyProperty.RegisterAttachedReadOnly("IsMouseDirectlyOverItem", typeof(bool), typeof(TreeViewItemHelper), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CalculateIsMouseDirectlyOverItem)));
        public static readonly DependencyProperty IsMouseDirectlyOverItemProperty = IsMouseDirectlyOverItemKey.DependencyProperty;

        static TreeViewItemHelper()
        {
            EventManager.RegisterClassHandler(typeof(TreeViewItem), UIElement.MouseEnterEvent, new MouseEventHandler(OnMouseTransition), true);
            EventManager.RegisterClassHandler(typeof(TreeViewItem), UIElement.MouseLeaveEvent, new MouseEventHandler(OnMouseTransition), true);
            EventManager.RegisterClassHandler(typeof(TreeViewItem), UpdateOverItemEvent, new RoutedEventHandler(OnUpdateOverItem));
        }
        public static bool GetIsMouseDirectlyOverItem(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMouseDirectlyOverItemProperty);
        }
        private static object CalculateIsMouseDirectlyOverItem(DependencyObject item, object value)
        {
            return item == CurrentItem;
        }
        private static void OnUpdateOverItem(object sender, RoutedEventArgs e)
        {
            CurrentItem = sender as TreeViewItem;
            CurrentItem.InvalidateProperty(IsMouseDirectlyOverItemProperty);
            e.Handled = true;
        }
        private static void OnMouseTransition(object sender, MouseEventArgs e)
        {
            lock (IsMouseDirectlyOverItemProperty)
            {
                if (CurrentItem != null)
                {
                    DependencyObject oldItem = CurrentItem;
                    CurrentItem = null;
                    oldItem.InvalidateProperty(IsMouseDirectlyOverItemProperty);
                }

                Mouse.DirectlyOver?.RaiseEvent(new RoutedEventArgs(UpdateOverItemEvent));
            }
        }
    }

    public static class TreeViewItemExtensions
    {
        public static int GetDepth(this TreeViewItem item)
        {
            TreeViewItem parent;
            while ((parent = GetParent(item)) != null)
            {
                return GetDepth(parent) + 1;
            }
            return 0;
        }

        private static TreeViewItem GetParent(TreeViewItem item)
        {
            var parent = VisualTreeHelper.GetParent(item);
            while (!(parent is TreeViewItem || parent is TreeView))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as TreeViewItem;
        }
    }

    public class LeftMarginMultiplierConverter : IValueConverter
    {
        public double Length { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as TreeViewItem;
            if (item == null)
                return new Thickness(0);

            return new Thickness(Length * item.GetDepth(), 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
