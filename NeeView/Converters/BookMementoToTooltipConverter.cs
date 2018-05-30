using System;
using System.Windows.Data;

namespace NeeView
{
    // Tooltip表示用コンバータ
    [ValueConversion(typeof(BookHistory), typeof(string))]
    public class BookMementoToTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is BookHistory)
            {
                var record = (BookHistory)value;
                return record.LastAccessTime == default(DateTime) ? record.Place : record.Place + "\n" + record.LastAccessTime;
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
