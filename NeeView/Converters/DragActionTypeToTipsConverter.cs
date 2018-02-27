﻿using System;
using System.Windows.Data;

namespace NeeView
{
    // ドラッグ操作Tips表示用コンバータ
    [ValueConversion(typeof(DragActionType), typeof(string))]
    public class DragActionTypeToTipsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is DragActionType ? ((DragActionType)value).ToTips() : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
