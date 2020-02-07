using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    public class DoubleSubConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double v0 && double.TryParse((string)parameter, out double v1))
            {
                return Math.Max(v0 - v1, 0.0);
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
