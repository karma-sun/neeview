using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// プロパティで構成されたアクセスマップ
    /// </summary>
    public class PropertyMap
    {
        private object _source;
        private Dictionary<string, object> _items;


        public PropertyMap(object source)
        {
            _source = source;
            var type = _source.GetType();

            _items = new Dictionary<string, object>();
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.GetCustomAttribute(typeof(PropertyMapIgnore)) != null) continue;

                var nameAttribute = (PropertyMapName)property.GetCustomAttribute(typeof(PropertyMapName));
                var key = nameAttribute?.Name ?? property.Name;

                if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    _items.Add(key, new PropertyMap(property.GetValue(_source)));
                }
                else
                {
                    if (property.PropertyType.IsEnum)
                    {
                        _items.Add(key, new PropertyMapEnumItem(_source, property));
                    }
                    else if (property.PropertyType == typeof(Size))
                    {
                        _items.Add(key, new PropertyMapSizeItem(_source, property));
                    }
                    else
                    {
                        _items.Add(key, new PropertyMapItem(_source, property));
                    }
                }
            }
        }

        public object this[string key]
        {
            get
            {
                if (_items[key] is PropertyMapItem item)
                {
                    return item.GetValue();
                }
                else
                {
                    return _items[key];
                }
            }
            set
            {
                if (_items[key] is PropertyMapItem item)
                {
                    item.SetValue(value);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        /// <summary>
        /// 外部からのプロパティの追加
        /// </summary>
        /// <param name="source"></param>
        /// <param name="propertyName"></param>
        internal void AddProperty(object source, string propertyName, string memberName = null)
        {
            var type = source.GetType();
            var property = type.GetProperty(propertyName);
            _items.Add(memberName ?? propertyName, new PropertyMapItem(source, property));
        }


        public string CreateHelpHtml(string prefix)
        {
            string s = string.Empty;

            foreach (var item in _items)
            {
                var name = prefix + "." + item.Key;
                if (item.Value is PropertyMap subMap)
                {
                    s += subMap.CreateHelpHtml(name);
                }
                else
                {
                    string type = string.Empty;
                    string description = string.Empty;
                    if (item.Value is PropertyMapItem valueItem)
                    {
                        (type, description) = valueItem.CreateHelpHtml();
                    }
                    s += $"<tr><td>{name}</td><td>{type}</td><td>{description}</td></tr>\r\n";
                }
            }

            return s;
        }
    }


    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyMapIgnore : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyMapName : Attribute
    {
        public string Name;

        public PropertyMapName()
        {
        }

        public PropertyMapName(string name)
        {
            Name = name;
        }
    }



    public class PropertyMapItem
    {
        protected object _source;
        protected PropertyInfo _property;

        public PropertyMapItem(object source, PropertyInfo property)
        {
            _source = source;
            _property = property;
        }

        public virtual object GetValue()
        {
            return _property.GetValue(_source);
        }

        public virtual void SetValue(object value)
        {
            _property.SetValue(_source, value != null ? Convert.ChangeType(value, _property.PropertyType) : null);
        }

        public (string type, string description) CreateHelpHtml()
        {
            string typeString;
            if (_property.PropertyType.IsEnum)
            {
                typeString = "<dl>" + string.Join("", _property.PropertyType.VisibledAliasNameDictionary().Select(e => $"<dt>\"{e.Key}\"</dt><dd>{e.Value}</dd>")) + "</dl>";
            }
            else
            {
                typeString = _property.PropertyType.ToManualString();
            }

            var attribute = _property.GetCustomAttribute<PropertyMemberAttribute>();
            var description = (attribute != null)
                ? ResourceService.GetString(attribute.Name) + "<br/>" + ResourceService.GetString(attribute.Tips)
                : string.Empty;
            description = new Regex("[\r\n]+").Replace(description, "<br/>");

            return (typeString, description);
        }
    }

    public static class TypeExtensions
    {
        public static string ToManualString(this Type type)
        {
            if (type.IsEnum)
            {
                return "enum";
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return "bool";
                case TypeCode.Int32:
                    return "int";
                case TypeCode.Double:
                    return "double";
                case TypeCode.String:
                    return "string";
            }

            if (type == typeof(Size))
            {
                return "\"width, height\"";
            }

            if (type == typeof(Color))
            {
                return "\"#AARRGGBB\"";
            }

            return "???";
        }
    }

    public class PropertyMapEnumItem : PropertyMapItem
    {
        public PropertyMapEnumItem(object source, PropertyInfo property) : base(source, property)
        {
            if (!property.PropertyType.IsEnum) throw new ArgumentException();
        }

        public override object GetValue()
        {
            return _property.GetValue(_source)?.ToString();
        }

        public override void SetValue(object value)
        {
            if (value is string s)
            {
                _property.SetValue(_source, Enum.Parse(_property.PropertyType, s));
            }
            else
            {
                throw new InvalidCastException();
            }
        }
    }


    public class PropertyMapSizeItem : PropertyMapItem
    {
        public PropertyMapSizeItem(object source, PropertyInfo property) : base(source, property)
        {
            if (property.PropertyType != typeof(Size)) throw new ArgumentException();
        }

        public override object GetValue()
        {
            return _property.GetValue(_source)?.ToString();
        }

        public override void SetValue(object value)
        {
            if (value is string s)
            {
                _property.SetValue(_source, (Size)new SizeConverter().ConvertFrom(s));
            }
            else
            {
                throw new InvalidCastException();
            }
        }
    }

}