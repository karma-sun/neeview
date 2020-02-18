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
        public bool IsVisibled = true;

        public AliasNameAttribute(string aliasName)
        {
            AliasName = aliasName;
        }
    }

    public static class AliasNameExtensions
    {
        #region Generics

        public static AliasNameAttribute GetAliasNameAttribute<T>(T value)
            where T : struct
        {
            return value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(AliasNameAttribute), false)
                .Cast<AliasNameAttribute>()
                .FirstOrDefault();
        }

        public static string GetAliasName<T>(T value)
            where T : struct
        {
            var raw = GetAliasNameAttribute(value)?.AliasName;
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

        public static Dictionary<T, string> GetVisibledAliasNameDictionary<T>()
            where T : struct
        {
            var type = typeof(T);

            return Enum.GetValues(type)
                .Cast<T>()
                .Select(e => (Key: e, Attribute: GetAliasNameAttribute(e)))
                .Where(e => e.Attribute == null || e.Attribute.IsVisibled)
                .ToDictionary(e => e.Key, e => ResourceService.GetString(e.Attribute?.AliasName) ?? e.Key.ToString());
        }

        public static string GetTips<T>(T value)
            where T : struct
        {
            return GetAliasNameAttribute(value)?.Tips;
        }

        #endregion

        #region Extension Methods

        public static AliasNameAttribute ToAliasNameAttribute(this Enum value)
        {
            return value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(AliasNameAttribute), false)
                .Cast<AliasNameAttribute>()
                .FirstOrDefault();
        }

        public static string ToAliasName(this Enum value)
        {
            var raw = value.ToAliasNameAttribute()?.AliasName;
            return ResourceService.GetString(raw) ?? value.ToString();
        }

        public static Dictionary<Enum, string> AliasNameDictionary(this Type type)
        {
            if (!type.IsEnum) throw new ArgumentException("not enum", nameof(type));

            return Enum.GetValues(type)
                .Cast<Enum>()
                .ToDictionary(e => e, e => e.ToAliasName());
        }

        public static Dictionary<Enum, string> VisibledAliasNameDictionary(this Type type)
        {
            if (!type.IsEnum) throw new ArgumentException("not enum", nameof(type));

            return Enum.GetValues(type)
                .Cast<Enum>()
                .Select(e => (Key: e, Attribute: e.ToAliasNameAttribute()))
                .Where(e => e.Attribute == null || e.Attribute.IsVisibled)
                .ToDictionary(e => e.Key, e => ResourceService.GetString(e.Attribute?.AliasName) ?? e.Key.ToString());
        }

        #endregion
    }
}

