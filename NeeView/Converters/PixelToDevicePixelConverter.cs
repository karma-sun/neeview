using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    public class PixelToDevicePixelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int v0)
            {
                return (double)v0 / Environment.Dpi.DpiScaleX;
            }
            else if (value is double v1)
            {
                return v1 / Environment.Dpi.DpiScaleX;
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
