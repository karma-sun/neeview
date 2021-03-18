using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    public class DoubleMinConverter : IValueConverter
    {
        public double ReferenceValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double v)
            {
                return Math.Min(v, ReferenceValue);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

