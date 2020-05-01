using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    ///  論理積(AND)
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class MultiBooleanAndConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.OfType<bool>().All(e => e);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
