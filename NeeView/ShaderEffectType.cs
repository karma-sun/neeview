// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView
{
    /// <summary>
    /// エフェクトの種類
    /// </summary>
    public enum ShaderEffectType
    {
        None,
        Blur,
        Grayscale,
    }

    /// <summary>
    /// ShaderEffectType 拡張
    /// </summary>
    public static class ShaderEffectTypeExtension
    {
        //
        private static Dictionary<ShaderEffectType, Effect> _StaticEffectDictionary = new Dictionary<ShaderEffectType, Effect>()
        {
            [ShaderEffectType.None] = null,
            [ShaderEffectType.Blur] = new BlurEffect(),
            [ShaderEffectType.Grayscale] = new Effects.GrayscaleEffect(),
        };

        /// <summary>
        /// エフェクトの固定インスタンス取得
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static Effect GetStaticEffect(this ShaderEffectType key)
        {
            return _StaticEffectDictionary[key];
        }

        /// <summary>
        /// エフェクトをかけた後の色に変更
        /// </summary>
        /// <param name="key"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Color GetEffectedColor(this ShaderEffectType key, Color source)
        {
            // グレイスケール
            if (key == ShaderEffectType.Grayscale)
            {
                var Y = (int)((0.298912 * (source.R / 255.0) + 0.586611 * (source.G / 255.0) + 0.114478 * (source.B / 255.0)) * 255.0);
                byte y = (byte)NVUtility.Clamp(Y, 0, 255);
                return Color.FromArgb(source.A, y, y, y);
            }
            else
            {
                return source;
            }
        }
    }

    /// <summary>
    /// Converter To boolean
    /// </summary>
    [ValueConversion(typeof(ShaderEffectType), typeof(bool))]
    public class ShaderEffectTypeToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ShaderEffectType mode0 = (ShaderEffectType)value;
            ShaderEffectType mode1 = (ShaderEffectType)Enum.Parse(typeof(ShaderEffectType), parameter as string);
            return (mode0 == mode1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
