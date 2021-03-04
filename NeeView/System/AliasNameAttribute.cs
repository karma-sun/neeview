﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class AliasNameAttribute : Attribute
    {
        public string AliasName;
        public string Remarks;
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
        private static Dictionary<Type, Dictionary<Enum, string>> _cache = new Dictionary<Type, Dictionary<Enum, string>>();


        public static string GetAliasName(object value)
        {
            if (value is null) return null;

            var type = value.GetType();
            if (!type.IsEnum) return value.ToString();

            var map = GetAliasNameDictionary(type);
            return map.TryGetValue((Enum)value, out var name) ? name : value.ToString();
        }

        public static string ToAliasName<T>(this T value)
            where T : Enum
        {
            Debug.Assert(typeof(T) != typeof(Enum), "Not support System.Enum directory"); // Enumそのものの型は非対応

            var map = GetAliasNameDictionary(typeof(T));
            return map.TryGetValue(value, out var name) ? name : value.ToString();
        }

        // TODO: use _cache?
        public static Dictionary<T, string> GetAliasNameDictionary<T>()
        {
            Debug.Assert(typeof(T).IsEnum);

            var type = typeof(T);

            return Enum.GetValues(type)
                .Cast<T>()
                .ToDictionary(e => e, e => GetAliasNameInner(e));
        }

        // TODO: use _cache?
        public static Dictionary<T, string> GetVisibledAliasNameDictionary<T>()
        {
            Debug.Assert(typeof(T).IsEnum);

            var type = typeof(T);

            return Enum.GetValues(type)
                .Cast<T>()
                .Select(e => (Key: e, Attribute: GetAliasNameAttribute(e)))
                .Where(e => e.Attribute == null || e.Attribute.IsVisibled)
                .ToDictionary(e => e.Key, e => GetAliasNameInner(e.Key, e.Attribute));
        }

        // TODO: use _cache?
        public static Dictionary<Enum, string> VisibledAliasNameDictionary(this Type type)
        {
            if (!type.IsEnum) throw new ArgumentException("not enum", nameof(type));

            return Enum.GetValues(type)
                .Cast<Enum>()
                .Distinct()
                .Select(e => (Key: e, Attribute: GetAliasNameAttribute(e)))
                .Where(e => e.Attribute == null || e.Attribute.IsVisibled)
                .ToDictionary(e => e.Key, e => GetAliasNameInner(e.Key, e.Attribute));
        }



        private static Dictionary<Enum, string> GetAliasNameDictionary(Type type)
        {
            Debug.Assert(type.IsEnum);
            if (!_cache.TryGetValue(type, out var map))
            {
                map = CreateAliasNameDictionary(type);
                _cache.Add(type, map);
            }
            return map;
        }

        private static Dictionary<Enum, string> CreateAliasNameDictionary(Type type)
        {
            Debug.Assert(type.IsEnum);

            return Enum.GetValues(type)
                .Cast<object>()
                .ToDictionary(e => (Enum)e, e => GetAliasNameInner(e));
        }

        private static string GetAliasNameInner(object value)
        {
            return GetAliasNameInner(value, GetAliasNameAttribute(value));
        }

        private static string GetAliasNameInner(object value, AliasNameAttribute attribute)
        {
            var resourceKey = attribute?.AliasName ?? GetResourceKey(value);
            var resourceString = ResourceService.GetResourceString(resourceKey, true);

#if DEBUG
            if (resourceKey != null && resourceString is null)
            {
                Debug.WriteLine($"Error: AliasName not found: {resourceKey}");
            }
#endif

            return resourceString ?? value.ToString();
        }


        private static AliasNameAttribute GetAliasNameAttribute(object value)
        {
            return value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(AliasNameAttribute), false)
                .Cast<AliasNameAttribute>()
                .FirstOrDefault();
        }

        private static string GetResourceKey(object value, string postfix = null)
        {
            var type = value.GetType();
            return $"@{type.Name}.{value}{postfix}";
        }




        #region Remarks

        public static string GetRemarks<T>(T value)
        {
            return GetRemarks(value, GetAliasNameAttribute(value));
        }

        private static string GetRemarks<T>(T value, AliasNameAttribute attribute)
        {
            var resourceKey = attribute?.Remarks ?? GetResourceKey(value, ".Remarks");
            return ResourceService.GetResourceString(resourceKey, true);
        }

        #endregion Tips
    }
}

