﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace NeeView
{
    /// <summary>
    /// プロパティで構成されたアクセスマップ
    /// </summary>
    public class PropertyMap : PropertyMapNode
    {
        private static PropertyMapConverter _defaultConverter;
        private static PropertyMapOptions _defaultOptions;

        static PropertyMap()
        {
            _defaultConverter = new PropertyMapDefaultConverter();

            _defaultOptions = new PropertyMapOptions();
            _defaultOptions.Converters.Add(new PropertyMapEnumConverter());
            _defaultOptions.Converters.Add(new PropertyMapSizeConverter());
            _defaultOptions.Converters.Add(new PropertyMapPointConverter());
            _defaultOptions.Converters.Add(new PropertyMapColorConverter());
            _defaultOptions.Converters.Add(new PropertyMapFileTypeCollectionConverter());
            _defaultOptions.Converters.Add(new PropertyMapStringCollectionConverter());
        }


        private object _source;
        private Dictionary<string, PropertyMapNode> _items;

        private PropertyMapOptions _options;

        public PropertyMap(object source) : this(source, null, null)
        {
        }

        public PropertyMap(object source, string prefix, PropertyMapOptions options)
        {
            _source = source;
            _options = options ?? _defaultOptions;

            var type = _source.GetType();

            _items = new Dictionary<string, PropertyMapNode>();
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(e => e.Name))
            {
                if (property.GetCustomAttribute(typeof(PropertyMapIgnoreAttribute)) != null) continue;

                var nameAttribute = (PropertyMapNameAttribute)property.GetCustomAttribute(typeof(PropertyMapNameAttribute));
                var key = nameAttribute?.Name ?? property.Name;
                var converter = _options.Converters.FirstOrDefault(e => e.CanConvert(property.PropertyType));

                if (converter == null && property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    var labelAttribute = (PropertyMapLabelAttribute)property.GetCustomAttribute(typeof(PropertyMapLabelAttribute));
                    var newPrefix = labelAttribute != null ? prefix + ResourceService.GetString(labelAttribute.Label) + ": " : prefix;
                    _items.Add(key, new PropertyMap(property.GetValue(_source), newPrefix, options));
                }
                else
                {
                    _items.Add(key, new PropertyMapSource(source, property, converter ?? _defaultConverter, prefix));
                }
            }
        }

        public object this[string key]
        {
            get
            {
                if (_items[key] is PropertyMapSource item)
                {
                    return item.Read(_options);
                }
                else
                {
                    return _items[key];
                }
            }
            set
            {
                if (_items[key] is PropertyMapSource item)
                {
                    item.Write(value, _options);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }


        internal bool ContainsKey(string key)
        {
            return _items.ContainsKey(key);
        }



        /// <summary>
        /// 外部からのプロパティの追加
        /// </summary>
        internal void AddProperty(object source, string propertyName, string memberName = null)
        {
            var type = source.GetType();
            var property = type.GetProperty(propertyName);
            var converter = _options.Converters.FirstOrDefault(e => e.CanConvert(property.PropertyType)) ?? _defaultConverter;

            _items.Add(memberName ?? propertyName, new PropertyMapSource(source, property, converter, null));
        }


        internal string CreateHelpHtml(string prefix)
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
                    if (item.Value is PropertyMapSource valueItem)
                    {
                        (type, description) = valueItem.CreateHelpHtml();
                    }
                    s += $"<tr><td>{name}</td><td>{type}</td><td>{description}</td></tr>\r\n";
                }
            }

            return s;
        }
    }
}