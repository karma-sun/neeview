using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace NeeView.Windows.Controls
{
    public class SafeDoubleValueConverter : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;

            if (string.IsNullOrWhiteSpace(s))
            {
                return DependencyProperty.UnsetValue;
            }

            if (double.TryParse(s, out double result))
            {
                return result;
            }

            var regex = new Regex(@"[+-]?(?:\d+\.?\d*|\.\d+)");
            var match = regex.Match(s);
            if (match.Success)
            {
                return double.Parse(match.Value);
            }

            return value;
        }
    }
}
