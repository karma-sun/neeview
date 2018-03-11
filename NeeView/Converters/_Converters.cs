using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace NeeView
{
    class _Converters
    {
        // dummy.
    }


    // etc..


    public class DoubleSubConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double v0 && double.TryParse((string)parameter, out double v1))
            {
                return Math.Max(v0 - v1, 0.0);
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

    //
    public class PixelToDevicePixelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int v0)
            {
                return (double)v0 / Config.Current.Dpi.DpiScaleX;
            }
            else if (value is double v1)
            {
                return v1 / Config.Current.Dpi.DpiScaleX;
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
