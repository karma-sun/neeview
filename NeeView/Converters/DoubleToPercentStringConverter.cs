using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    [ValueConversion(typeof(double), typeof(string))]
    public class DoubleToPercentStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double v)
            {
                return v.ToString("P0");
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valueWithoutPercentage = ((string)value).TrimEnd(' ', '%');
            return double.Parse(valueWithoutPercentage) / 100.0;
        }
    }
}
