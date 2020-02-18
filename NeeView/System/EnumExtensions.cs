using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public static class EnumExtensions
    {

        /// <summary>
        /// 文字列からEnumに変換
        /// </summary>
        public static TEnum ToEnum<TEnum>(this string s)
            where TEnum : struct
        {
            return Enum.TryParse(s, out TEnum result) ? result : default;
        }
    }
}

