using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class AliasNameAttribute : Attribute
    {
        public string AliasName;
        public string Tips;
        public bool IsVisibled = true;

        public AliasNameAttribute()
        {
        }

        public AliasNameAttribute(string aliasName)
        {
            AliasName = aliasName;
        }
    }

    public static class AliasNameExtensions
    {
        #region Generics

        private static AliasNameAttribute GetAliasNameAttribute<T>(T value)
        {
            return value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(AliasNameAttribute), false)
                .Cast<AliasNameAttribute>()
                .FirstOrDefault();
        }

        private static string GetResourceKey<T>(T value, string postfix = null)
        {
            return $"@{value.GetType().Name}.{value}{postfix}";
        }

        private static string GetAliasName<T>(T value, AliasNameAttribute attribute)
        {
            var resourceKey = (attribute != null) ? attribute.AliasName ?? GetResourceKey(value) : null;
            var resourceString = ResourceService.GetResourceString(resourceKey, true);

#if DEBUG
            if (resourceKey != null && resourceString is null)
            {
                Debug.WriteLine($"Error: AliasName not found: {resourceKey}");
            }
#endif

            return resourceString ?? value.ToString();
        }

        public static string GetAliasName<T>(T value)
        {
            return GetAliasName(value, GetAliasNameAttribute(value));
        }

        private static string GetTips<T>(T value, AliasNameAttribute attribute)
        {
            var resourceKey = (attribute != null) ? attribute.Tips ?? GetResourceKey(value, "#Tips") : null;
            return ResourceService.GetResourceString(resourceKey, true);
        }

        public static string GetTips<T>(T value)
        {
            return GetTips(value, GetAliasNameAttribute(value));
        }


        public static Dictionary<T, string> GetAliasNameDictionary<T>()
        {
            Debug.Assert(typeof(T).IsEnum);

            var type = typeof(T);

            return Enum.GetValues(type)
                .Cast<T>()
                .ToDictionary(e => e, e => GetAliasName(e));
        }

        public static Dictionary<T, string> GetVisibledAliasNameDictionary<T>()
        {
            Debug.Assert(typeof(T).IsEnum);

            var type = typeof(T);

            return Enum.GetValues(type)
                .Cast<T>()
                .Select(e => (Key: e, Attribute: GetAliasNameAttribute(e)))
                .Where(e => e.Attribute == null || e.Attribute.IsVisibled)
                .ToDictionary(e => e.Key, e => GetAliasName(e.Key, e.Attribute));
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
            return GetAliasName(value);
        }

        public static Dictionary<Enum, string> AliasNameDictionary(this Type type)
        {
            if (!type.IsEnum) throw new ArgumentException("not enum", nameof(type));

            return Enum.GetValues(type)
                .Cast<Enum>()
                .Distinct()
                .ToDictionary(e => e, e => e.ToAliasName());
        }

        public static Dictionary<Enum, string> VisibledAliasNameDictionary(this Type type)
        {
            if (!type.IsEnum) throw new ArgumentException("not enum", nameof(type));

            return Enum.GetValues(type)
                .Cast<Enum>()
                .Distinct()
                .Select(e => (Key: e, Attribute: e.ToAliasNameAttribute()))
                .Where(e => e.Attribute == null || e.Attribute.IsVisibled)
                .ToDictionary(e => e.Key, e => GetAliasName(e.Key, e.Attribute));
        }

        #endregion
    }
}

