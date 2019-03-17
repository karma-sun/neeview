using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public static class EnumExtensions
    {
        public static string ToAliasName(this Enum value)
        {
            var raw = value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(AliasNameAttribute), false)
                .Cast<AliasNameAttribute>()
                .FirstOrDefault()?.AliasName;

            return ResourceService.GetString(raw) ?? value.ToString();
        }

        public static Dictionary<Enum, string> AliasNameDictionary(this Type type)
        {
            if (!type.IsEnum) throw new ArgumentException("not enum", nameof(type));

            return Enum.GetValues(type)
                .Cast<Enum>()
                .ToDictionary(e => e, e => e.ToAliasName());
        }

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

