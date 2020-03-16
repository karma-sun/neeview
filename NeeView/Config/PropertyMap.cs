using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace NeeView
{
    /// <summary>
    /// DataMember属性のプロパティで構成されたアクセスマップ
    /// </summary>
    public class PropertyMap
    {
        public class PropertyItem
        {
            public PropertyItem(object source, PropertyInfo property)
            {
                Source = source;
                Property = property;
            }

            public object Source { get; private set; }

            public PropertyInfo Property { get; private set; }

            public object GetValue()
            {
                return Property.GetValue(Source);
            }

            public void SetValue(object value)
            {
                Property.SetValue(Source, value); 
            }
        }


        private object _source;
        private Dictionary<string, object> _items;
        

        public PropertyMap(object source)
        {
            _source = source;

            var type = _source.GetType();
            var properties = type.GetProperties();

            _items = new Dictionary<string, object>();
            foreach (var property in properties)
            {
                var attribute = (DataMemberAttribute)property.GetCustomAttribute(typeof(DataMemberAttribute));
                if (attribute == null) continue;

                var key = attribute.Name ?? property.Name;

                if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    _items.Add(key, new PropertyMap(property.GetValue(_source)));
                }
                else
                {
                    _items.Add(key, new PropertyItem(_source, property));
                }
            }
        }

        public object this[string key]
        {
            get
            {
                if (_items[key] is PropertyItem dataMember)
                {
                    return dataMember.GetValue();
                }
                else
                {
                    return _items[key];
                }
            }
            set
            {
                if (_items[key] is PropertyItem dataMember)
                {
                    dataMember.SetValue(value);
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
            _items.Add(memberName ?? propertyName, new PropertyItem(source, property));
        }
    }

}