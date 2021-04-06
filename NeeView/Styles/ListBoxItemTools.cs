using System.Windows;

namespace NeeView
{
    public class ListBoxItemTools : DependencyObject
    {
        public static Thickness GetInnerMargin(DependencyObject obj)
        {
            return (Thickness)obj.GetValue(InnerMarginProperty);
        }

        public static void SetnnerMargin(DependencyObject obj, Thickness value)
        {
            obj.SetValue(InnerMarginProperty, value);
        }

        public static readonly DependencyProperty InnerMarginProperty =
            DependencyProperty.Register("InnerMargin", typeof(Thickness), typeof(ListBoxItemTools), new PropertyMetadata(new Thickness(0,-1,0,0)));
    }
}
