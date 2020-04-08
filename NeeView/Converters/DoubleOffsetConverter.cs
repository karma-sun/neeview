using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    public class DoubleOffsetConverter : IValueConverter
    {
        public double Offset { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double v0)
            {
                return v0 + Offset;
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
