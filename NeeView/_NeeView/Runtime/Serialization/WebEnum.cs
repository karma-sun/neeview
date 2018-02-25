// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Runtime.Serialization;

namespace NeeView.Runtime.Serialization
{
    /// <summary>
    /// 存在しないEnumをデシリアイズしたときにデフォルト値にして前方互換性を確保する.
    /// from https://social.msdn.microsoft.com/Forums/en-US/8b3ff476-e4e4-404b-b0a5-9aac745f87f4/wcf-enum-backward-compatibility?forum=wcf
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    public struct WebEnum<T>
    {
        public WebEnum(T value) : this()
        {
            Value = value;
        }

        public T Value { get; set; }

        [DataMember]
        internal string Name { get; set; }

        public static implicit operator WebEnum<T>(T value)
        {
            return new WebEnum<T>(value);
        }

        public static implicit operator T(WebEnum<T> value)
        {
            return value.Value;
        }

        [OnSerializing]
        internal void OnSerializing(StreamingContext context)
        {
            Name = Value.ToString();
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            if ((!string.IsNullOrEmpty(Name)) && (Enum.IsDefined(typeof(T), Name)))
            {
                Value = (T)Enum.Parse(typeof(T), Name);
            }
            else
            {
                Value = default(T);
            }
        }
    }
}
