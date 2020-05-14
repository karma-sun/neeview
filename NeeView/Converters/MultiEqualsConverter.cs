using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace NeeView
{
    public class MultiEqualsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length > 1)
            {
                return values.Skip(1).All(e => e.Equals(values[0]));
            }
            else
            {
                return false;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
