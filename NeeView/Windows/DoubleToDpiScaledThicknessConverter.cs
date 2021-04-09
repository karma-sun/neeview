using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NeeView.Windows
{
    public class DoubleToDpiScaledThicknessConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is double value && values[1] is DpiScale dpi)
            {
                var x = value / dpi.DpiScaleX;
                var y = value / dpi.DpiScaleY;
                return new Thickness(x, y, x, y);
            }

            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
