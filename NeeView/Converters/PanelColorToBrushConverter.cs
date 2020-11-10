using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// テーマからブラシを取得
    /// </summary>
    public class PanelColorToBrushConverter : IValueConverter
    {
        public SolidColorBrush Dark { get; set; }
        public SolidColorBrush Light { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PanelColor panelColor)
            {
                return panelColor == PanelColor.Dark ? Dark : Light;
            }
            return Light;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// テーマからブラシを取得。IsActiveでなかったら半透明にする。
    /// </summary>
    public class PanelColorTobrushMultiConverter : IMultiValueConverter
    {
        public SolidColorBrush _dark;
        public SolidColorBrush _halfDark;
        public SolidColorBrush _light;
        public SolidColorBrush _halfLight;

        public SolidColorBrush Dark
        {
            get { return _dark; }
            set
            {
                _dark = value;
                _halfDark = CreateHalfSolidColorBrush(_dark);
            }
        }

        public SolidColorBrush Light
        {
            get { return _light; }
            set
            {
                _light = value;
                _halfLight = CreateHalfSolidColorBrush(_light);
            }
        }

        private SolidColorBrush CreateHalfSolidColorBrush(SolidColorBrush source)
        {
            var color = source.Color;
            color.A = (byte)(color.A >> 1);
            return new SolidColorBrush(color);
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(values[0] is PanelColor panelColor))
            {
                return _light;
            }

            if (values.Length <= 1 || !(values[1] is bool isActive))
            {
                return ToBrush(panelColor, true);
            }

            if (values.Length <= 2 || !(values[2] is bool isEnabled))
            {
                return ToBrush(panelColor, isActive);
            }

            return ToBrush(panelColor, isActive || !isEnabled);
        }

        private SolidColorBrush ToBrush(PanelColor color, bool isActive)
        {
            if (color == PanelColor.Dark)
            {
                return isActive ? _dark : _halfDark;
            }
            else
            {
                return isActive ? _light : _halfLight;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
