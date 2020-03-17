using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows;

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
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            _items = new Dictionary<string, object>();
            foreach (var property in properties)
            {
                ////var attribute = (DataMemberAttribute)property.GetCustomAttribute(typeof(DataMemberAttribute));
                ////if (attribute == null) continue;

                ////var key = attribute.Name ?? property.Name;
                var key = property.Name;

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
                if (_items[key] is PropertyMapItem dataMember)
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
                if (_items[key] is PropertyMapItem dataMember)
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
            _items.Add(memberName ?? propertyName, new PropertyMapItem(source, property));
        }
    }



    public class PropertyMapItem
    {
        public PropertyMapItem(object source, PropertyInfo property)
        {
            Source = source;
            Property = property;
        }

        public object Source { get; private set; }

        public PropertyInfo Property { get; private set; }

        public virtual object GetValue()
        {
            return Property.GetValue(Source);
        }

        public virtual void SetValue(object value)
        {
            
            Property.SetValue(Source, Convert.ChangeType(value, Property.PropertyType));
        }
    }

    public class PropertyMapEnumItem : PropertyMapItem
    {
        public PropertyMapEnumItem(object source, PropertyInfo property) : base(source, property)
        {
            if (!Property.PropertyType.IsEnum) throw new ArgumentException();
        }
        
        public override object GetValue()
        {
            return Property.GetValue(Source)?.ToString();
        }

        public override void SetValue(object value)
        {
            if (value is string s)
            {
                Property.SetValue(Source, Enum.Parse(Property.PropertyType, s));
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
            if (Property.PropertyType != typeof(Size)) throw new ArgumentException();
        }

        public override object GetValue()
        {
            return Property.GetValue(Source)?.ToString();
        }

        public override void SetValue(object value)
        {
            if (value is string s)
            {
                Property.SetValue(Source, (Size)new SizeConverter().ConvertFrom(s));
            }
            else
            {
                throw new InvalidCastException();
            }
        }
    }

}