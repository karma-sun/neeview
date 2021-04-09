using System.Windows;

namespace NeeView.Windows
{
    public class WindowDpiScale
    {
        public static bool GetAttached(DependencyObject obj)
        {
            return (bool)obj.GetValue(AttachedProperty);
        }

        public static void SetAttached(DependencyObject obj, bool value)
        {
            obj.SetValue(AttachedProperty, value);
        }

        public static readonly DependencyProperty AttachedProperty =
            DependencyProperty.RegisterAttached("Attached", typeof(bool), typeof(WindowDpiScale), new PropertyMetadata(false, AttachedPropertyChanged));


        public static DpiScale GetDpiScale(DependencyObject obj)
        {
            return (DpiScale)obj.GetValue(DpiScaleProperty);
        }

        private static void SetDpiScale(DependencyObject obj, DpiScale value)
        {
            obj.SetValue(DpiScalePropertyKey, value);
        }

        public static readonly DependencyPropertyKey DpiScalePropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("DpiScale", typeof(DpiScale), typeof(WindowDpiScale), new PropertyMetadata(new DpiScale(1.0, 1.0)));

        public static readonly DependencyProperty DpiScaleProperty =
            DpiScalePropertyKey.DependencyProperty;


        private static void AttachedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window && (bool)e.NewValue)
            {
                window.DpiChanged += (s, ea) =>
                    SetDpiScale(d, ea.NewDpi);
            }
        }
    }
}
