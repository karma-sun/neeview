// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Windows.Data;

namespace NeeView
{
    //  長押し操作Tips表示用コンバータ
    [ValueConversion(typeof(TouchAction), typeof(string))]
    public class TouchActionToTipsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is TouchAction ? TouchActionExtensions.TouchActionTips[(TouchAction)value] : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
