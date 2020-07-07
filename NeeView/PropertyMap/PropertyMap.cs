using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NeeView
{
    /// <summary>
    /// プロパティで構成されたアクセスマップ
    /// </summary>
    public class PropertyMap : PropertyMapNode, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Support

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion


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
                    RaisePropertyChanged(key);
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

        internal WordNode CreateWordNode(string name)
        {
            var node = new WordNode(name);
            if (_items.Any())
            {
                node.Children = new List<WordNode>();
                foreach (var item in _items)
                {
                    if (item.Value is PropertyMap propertyMap)
                    {
                        node.Children.Add(propertyMap.CreateWordNode(item.Key));
                    }
                    else
                    {
                        node.Children.Add(new WordNode(item.Key));
                    }
                }
            }
            return node;
        }

        internal string CreateHelpHtml(string prefix)
        {
            string s = "";

            foreach (var item in _items)
            {
                var name = prefix + "." + item.Key;
                if (item.Value is PropertyMap subMap)
                {
                    s += subMap.CreateHelpHtml(name);
                }
                else
                {
                    string type = "";
                    string description = "";
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