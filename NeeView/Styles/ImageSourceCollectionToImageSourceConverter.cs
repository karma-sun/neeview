using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// ImageSourceCollectionからDPIを加味した適切サイズの画像を取得
    /// </summary>
    public class ImageSourceCollectionToImageSourceConverter : IMultiValueConverter
    {
        public double Width { get; set; } = 16.0;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) throw new InvalidOperationException();

            if (!(values[0] is IImageSourceCollection frames))
            {
                return DependencyProperty.UnsetValue;
            }

            if (!(values[1] is double dpiScale))
            {
                return DependencyProperty.UnsetValue;
            }

            return frames.GetImageSource(Width * dpiScale);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
