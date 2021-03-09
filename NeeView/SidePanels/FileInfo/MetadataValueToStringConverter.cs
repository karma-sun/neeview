using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    public class MetadataValueToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) return null;

            switch (value)
            {
                case IEnumerable<string> strings:
                    return string.Join("; ", strings);
                case DateTime dateTime:
                    return dateTime.ToString(Config.Current.Information.DateTimeFormat);
                case Enum _:
                    return AliasNameExtensions.GetAliasName(value);
                default:
                    return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
