﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NeeView
{
        // コンバータ：ファイルサイズのKB表示
    [ValueConversion(typeof(PageMode), typeof(bool))]
    public class FileSizeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var length = (long)value;
            return ByteToDispString(length);
        }

        public static string ByteToDispString(long length)
        {
            if (length < 0)
            {
                return "";
            }
            else if (length == 0)
            {
                return "0 KB";
            }
            else
            {
                return $"{(length + 1023) / 1024:#,0} KB";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
