using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    public class EnumToAliasNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return AliasNameExtensions.GetAliasName(value);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
