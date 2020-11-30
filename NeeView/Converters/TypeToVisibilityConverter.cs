using System;
using System.Windows;
using System.Windows.Data;

namespace NeeView
{
    [ValueConversion(typeof(Type), typeof(Visibility))]
    public class TypeToVisibilityConverter : IValueConverter
    {
        public Type Type { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value != null && value.GetType() == Type) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
        }
    }
}
