// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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

        public static Dictionary<PreLoadMode, string> AliasNameDictionary()
        {
            return Enum.GetValues(typeof(PreLoadMode))
                .Cast<PreLoadMode>()
                .ToDictionary(e => e, e => e.ToAliasName());
        }

        public static Dictionary<T, string> AliasNameDictionary<T>()
            where T : struct
        {
            var type = typeof(T);

            return Enum.GetValues(type)
                .Cast<T>()
                .ToDictionary(e => e, e => type
                    .GetField(e.ToString())
                    .GetCustomAttributes(typeof(AliasNameAttribute), false)
                    .Cast<AliasNameAttribute>()
                    .FirstOrDefault()?.AliasName ?? e.ToString());
        }

        public static Dictionary<Enum, string> AliasNameList(this Type type)
        {
            if (!type.IsEnum) throw new ArgumentException("not enum", nameof(type));

            return Enum.GetValues(type)
                .Cast<Enum>()
                .ToDictionary(e => e, e => e.ToAliasName());
        }
    }
}

