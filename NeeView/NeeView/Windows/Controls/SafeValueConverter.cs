using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeView.Windows.Controls
{
    public class SafeValueConverter<T> : IValueConverter
    {
        private IValueConverter _converter;

        public SafeValueConverter()
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Int32:
                    _converter = new SafeIntegerValueConverter();
                    break;

                case TypeCode.Double:
                    _converter = new SafeDoubleValueConverter();
                    break;

                default:
                    throw new NotSupportedException();
            }
        }


        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (_converter is null) throw new InvalidOperationException();

            return _converter.ConvertBack(value, targetType, parameter, culture);
        }
    }
}
