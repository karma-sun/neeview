using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public static class EnumExtensions
    {
        public static string ToAliasName(this Enum value)
        {
            return value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(AliasNameAttribute), false)
                .Cast<AliasNameAttribute>()
                .FirstOrDefault()?.AliasName ?? value.ToString();
        }

        public static Dictionary<Enum, string> AliasNameDictionary(this Type type)
        {
            if (!type.IsEnum) throw new ArgumentException("not enum", nameof(type));

            return Enum.GetValues(type)
                .Cast<Enum>()
                .ToDictionary(e => e, e => e.ToAliasName());
        }
    }


}

