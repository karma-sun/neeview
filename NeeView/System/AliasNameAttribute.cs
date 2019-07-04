using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class AliasNameAttribute : Attribute
    {
        public string AliasName;
        public string Tips;

        public AliasNameAttribute(string aliasName)
        {
            AliasName = aliasName;
        }
    }

    public static class AliasNameExtensions
    {
        public static string GetAliasName<T>(T value)
            where T : struct
        {
            var raw = value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(AliasNameAttribute), false)
                .Cast<AliasNameAttribute>()
                .FirstOrDefault()?.AliasName;

            return ResourceService.GetString(raw) ?? value.ToString();
        }

        public static Dictionary<T, string> GetAliasNameDictionary<T>()
            where T : struct
        {
            var type = typeof(T);

            return Enum.GetValues(type)
                .Cast<T>()
                .ToDictionary(e => e, e => GetAliasName(e));
        }


        public static string GetTips<T>(T value)
            where T : struct
        {
            return value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(AliasNameAttribute), false)
                .Cast<AliasNameAttribute>()
                .FirstOrDefault()?.Tips;
        }
    }
}

