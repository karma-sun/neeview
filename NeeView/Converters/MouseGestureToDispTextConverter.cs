using System;
using System.Windows.Data;

namespace NeeView
{
    // ジェスチャー表示用コンバータ
    [ValueConversion(typeof(string), typeof(string))]
    public class MouseGestureToDispTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;
            return new MouseGestureSequence((string)value).ToDispString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
