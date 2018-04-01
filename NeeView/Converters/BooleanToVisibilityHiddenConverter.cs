using System;
using System.Globalization;
using System.Windows;

namespace NeeView
{
    [System.Windows.Data.ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityHiddenConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

