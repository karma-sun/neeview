using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    public class PathToFileNameWithoutExtensionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string path)
            {
                return LoosePath.GetFileNameWithoutExtension(path);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
