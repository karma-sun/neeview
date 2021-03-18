using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace NeeView
{
    public class DoubleMulConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Cast<double>().Aggregate((now, next) => now * next);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
