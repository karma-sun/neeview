using NeeView.Windows;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    [Obsolete]
    public class PixelToDevicePixelConverter : IValueConverter
    {
        public IHasDpiScale DpiScale { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (DpiScale is null) throw new InvalidOperationException();

            var dpi = DpiScale.GetDpiScale().DpiScaleX;

            if (value is int v0)
            {
                return (double)v0 / dpi;
            }
            else if (value is double v1)
            {
                return v1 / dpi;
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
