using System;
using System.Windows.Data;

namespace NeeView
{
    // 常にfalseを返すコンバータ
    [ValueConversion(typeof(object), typeof(bool))]
    public class AnytToFalseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
        }
    }
}
