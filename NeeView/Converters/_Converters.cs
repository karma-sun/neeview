using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView
{
    class _Converters
    {
        // dummy.
    }


    // etc..


    public class DoubleSubConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double v0 && double.TryParse((string)parameter, out double v1))
            {
                return Math.Max(v0 - v1, 0.0);
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //
    public class PixelToDevicePixelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int v0)
            {
                return (double)v0 / Config.Current.Dpi.DpiScaleX;
            }
            else if (value is double v1)
            {
                return v1 / Config.Current.Dpi.DpiScaleX;
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(TextWrapping))]
    public class BooleanToTextWrappingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isWrapped && isWrapped)
            {
                return TextWrapping.Wrap;
            }

            return TextWrapping.NoWrap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    ///  論理積(AND)
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class MultiBooleanAndConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.OfType<bool>().All(e => e);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>
    /// ImageSourceがnullの場合、デフォルト画像を割り当てる。サムネイル用
    /// </summary>
    [ValueConversion(typeof(ImageSource), typeof(ImageSource))]
    public class ImageSourceToThumbnailConverter : IValueConverter
    {
        private static readonly ImageSource _defaultThumbnail = MainWindow.Current.Resources["thumbnail_default"] as ImageSource;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return _defaultThumbnail;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
