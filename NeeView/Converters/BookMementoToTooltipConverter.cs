using System;
using System.Windows.Data;

namespace NeeView
{
    // Tooltip表示用コンバータ
    [ValueConversion(typeof(Book.Memento), typeof(string))]
    public class BookMementoToTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Book.Memento)
            {
                var bookMemento = (Book.Memento)value;
                return bookMemento.LastAccessTime == default(DateTime) ? bookMemento.Place : bookMemento.Place + "\n" + bookMemento.LastAccessTime;
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
