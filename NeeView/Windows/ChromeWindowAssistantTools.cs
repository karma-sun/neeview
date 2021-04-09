using System.Windows;

namespace NeeView.Windows
{
    public class ChromeWindowAssistantTools
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
            DependencyProperty.RegisterAttached("Attached", typeof(bool), typeof(ChromeWindowAssistantTools), new PropertyMetadata(false, AttachedPropertyChanged));

        private static void AttachedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window && (bool)e.NewValue)
            {
                var assistant = new ChromeWindowAssistant(window);
                assistant.Attach();
            }
        }


        public static bool GetIsSystemMenuEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSystemMenuEnabledProperty);
        }

        public static void SetIsSystemMenuEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSystemMenuEnabledProperty, value);
        }

        public static readonly DependencyProperty IsSystemMenuEnabledProperty =
            DependencyProperty.RegisterAttached("IsSystemMenuEnabled", typeof(bool), typeof(ChromeWindowAssistantTools), new PropertyMetadata(false));
    }
}
