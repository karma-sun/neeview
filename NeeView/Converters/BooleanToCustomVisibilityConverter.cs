using System;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// 結果を指定可能なBooleanToVisibilityConverter
    /// </summary>
    [System.Windows.Data.ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToCustomVisibilityConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var visibilisy = (Visibility)parameter;

            if ((bool)value)
            {
                return visibilisy;
            }
            else
            {
                return visibilisy == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

