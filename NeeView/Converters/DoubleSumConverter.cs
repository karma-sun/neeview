using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// 複数の実数を合計する
    /// </summary>
    public class DoubleSumConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Cast<double>().Sum();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
