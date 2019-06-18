using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// 真偽値で値を有効に変換する
    /// </summary>
    public class BooleanToValueConverter : IMultiValueConverter
    {
        public object DefaultValue { get; set; } = DependencyProperty.UnsetValue;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is bool isEnabled && isEnabled)
            {
                return values[1];
            }

            return DefaultValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 真偽値で値を有効に変換する
    /// 既定値は Streatch.Uniform
    /// </summary>
    public class BooleanToStretchConverter : BooleanToValueConverter
    {
        public BooleanToStretchConverter()
        {
            DefaultValue = Stretch.Uniform;
        }
    }

    /// <summary>
    /// ブラシを選択する
    /// values[0]が有色ブラシであればそれを返す。さもなくばvalues[1]を返す
    /// </summary>
    public class BrushesSelectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is Brush brush0)
            {
                if (brush0 is SolidColorBrush solidColorBrush && solidColorBrush.Color.A != 0)
                {
                    return brush0;
                }
                if (values[1] is Brush brush1)
                {
                    return brush1;
                }
            }

            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
