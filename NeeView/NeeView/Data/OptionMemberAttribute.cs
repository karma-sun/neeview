using System;
using System.Reflection;

namespace NeeView.Data
{
    //
    [AttributeUsage(AttributeTargets.Property)]
    public class OptionMemberAttribute : OptionBaseAttribute
    {
        public string ShortName;
        public string LongName;
        public string Default;
        public bool HasParameter;
        public bool RequireParameter;

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
        public string HelpText => ResourceService.GetString(_attribute.HelpText);

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
                    throw new NotSupportedException(string.Format(Properties.Resources.OptionErrorArgument, _info.PropertyType));
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
                    throw new NotSupportedException(string.Format(Properties.Resources.OptionErrorArgument, _info.PropertyType.Name));
            }
        }
    }
}
