using System;
using System.Windows.Data;

namespace NeeView
{
    // コンバータ：DPI調整
    [ValueConversion(typeof(double), typeof(double))]
    public class DpiScaleXInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double length;

            if (value is double)
                length = (double)value;
            else if (value is int)
                length = (int)value;
            else
                length = double.Parse((string)value);

            return length;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
