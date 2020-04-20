using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NeeView
{
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class StringNullOrWhiteSpaceToVisibilityConverter : IValueConverter
    {
        public Visibility True { get; set; } = Visibility.Visible;
        public Visibility False { get; set; } = Visibility.Collapsed;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return True;
            }
            if (value is string s && string.IsNullOrWhiteSpace(s))
            {
                return True;
            }

            return False;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
