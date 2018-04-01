using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView
{
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class BooleanToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolean)
            {
                Brush brush;

                switch (parameter)
                {
                    case Brush b:
                        brush = b;
                        break;

                    case Color c:
                        brush = new SolidColorBrush(c);
                        break;

                    default:
                        brush = Brushes.Gray;
                        break;
                }

                return boolean ? brush : null;
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

