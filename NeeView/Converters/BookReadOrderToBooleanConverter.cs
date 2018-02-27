using System;
using System.Windows.Data;

namespace NeeView
{
    // コンバータ：右開き、左開きフラグ
    [ValueConversion(typeof(PageReadOrder), typeof(bool))]
    public class BookReadOrderToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PageReadOrder mode0 = (PageReadOrder)value;
            PageReadOrder mode1 = (PageReadOrder)Enum.Parse(typeof(PageReadOrder), parameter as string);
            return (mode0 == mode1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
