// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Reflection;

namespace NeeView.Data
{
    //
    [AttributeUsage(AttributeTargets.Property)]
    public class OptionMemberAttribute : OptionBaseAttribute
    {
        // short option name
        public string ShortName { get; set; }

        // long option name
        public string LongName { get; set; }

        // 引数を省略した場合の既定値
        public string Default { get; set; }

        public bool HasParameter { get; set; }

        public bool RequireParameter { get; set; }

        public OptionMemberAttribute() { }
        public OptionMemberAttribute(string shortName, string longName)
        {
            ShortName = shortName;
            LongName = longName;
        }
    }


    //
    public class OptionMemberElement
    {
        public string LongName => _attribute.LongName;
        public string ShortName => _attribute.ShortName;
        public string Default => _attribute.Default;
        public bool HasParameter => _attribute.HasParameter;
        public bool RequireParameter => _attribute.RequireParameter;
        public string HelpText => _attribute.HelpText;

        public string PropertyName => _info.Name;

        private PropertyInfo _info;
        private OptionMemberAttribute _attribute;


        public OptionMemberElement(PropertyInfo info, OptionMemberAttribute attribute)
        {
            _info = info;
            _attribute = attribute;
        }

        /// <summary>
        /// オプション引数指定可能値を取得
        /// ヘルプ用
        /// </summary>
        /// <returns></returns>
        public string GetValuePrototpye()
        {
            if (_info.PropertyType.IsEnum)
            {
                return string.Join("|", Enum.GetNames(_info.PropertyType));
            }

            Type nullable = Nullable.GetUnderlyingType(_info.PropertyType);
            if ((nullable != null) && nullable.IsEnum)
            {
                return string.Join("|", Enum.GetNames(nullable));
            }

            TypeCode typeCode = Type.GetTypeCode(_info.PropertyType);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return "bool";
                case TypeCode.String:
                    return "string";
                case TypeCode.Int32:
                    return "number";
                case TypeCode.Double:
                    return "number";
                default:
                    throw new NotSupportedException($"{_info.PropertyType} はサポート外の引数型です。");
            }
        }

        //
        public void SetValue(object _source, string value)
        {
            if (_info.PropertyType.IsEnum)
            {
                _info.SetValue(_source, Enum.Parse(_info.PropertyType, value));
                return;
            }

            Type nullable = Nullable.GetUnderlyingType(_info.PropertyType);
            if ((nullable != null) && nullable.IsEnum)
            {
                _info.SetValue(_source, Enum.Parse(nullable, value));
                return;
            }

            TypeCode typeCode = Type.GetTypeCode(_info.PropertyType);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    _info.SetValue(_source, bool.Parse(value));
                    break;
                case TypeCode.String:
                    _info.SetValue(_source, value);
                    break;
                case TypeCode.Int32:
                    _info.SetValue(_source, int.Parse(value));
                    break;
                case TypeCode.Double:
                    _info.SetValue(_source, double.Parse(value));
                    break;
                default:
                    throw new NotSupportedException($"{_info.PropertyType.Name} はサポート外の引数型です。");
            }
        }
    }



}
