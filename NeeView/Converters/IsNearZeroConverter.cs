using System;
using System.Diagnostics;
using System.Windows.Data;

namespace NeeView
{
    // コンバータ：０に近ければTrue
    public class IsNearZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var v = System.Convert.ToDouble(value);
                return v < 0.01;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return true;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
