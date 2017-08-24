// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NeeView.Windows.Property
{
    // 基底クラス
    public class PropertyDrawElement
    {
    }

    /// <summary>
    /// タイトル項目
    /// </summary>
    public class PropertyTitleElement : PropertyDrawElement
    {
        public string Name { get; set; }

        public PropertyTitleElement(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// プロパティ項目表示編集
    /// </summary>
    public class PropertyMemberElement : PropertyDrawElement, IValueSetter
    {
        public object Source { get; set; }
        public string Path => _info.Name;
        public string Name { get; set; }
        public string Tips { get; set; }
        public bool IsVisible { get; set; }
        public object Default { get; set; }
        public bool IsObsolete { get; set; }

        private PropertyInfo _info;

        private void Initialize(object source, PropertyInfo info, PropertyMemberAttribute attribute)
        {
            Source = source;
            Name = attribute.Name ?? info.Name;
            Tips = attribute.Tips;
            IsVisible = attribute.IsVisible;

            // Appxでは非表示
            if (!attribute.IsAppxVisible && Config.Current.IsAppxPackage)
            {
                this.IsVisible = false;
            }

            this.Default = GetDefaultValue(source, info);
            this.IsObsolete = GetObsoleteAttribute(info) != null;


            _info = info;
        }

        //
        public PropertyMemberElement(object source, PropertyInfo info, PropertyMemberAttribute attribute)
        {
            Initialize(source, info, attribute);

            TypeCode typeCode = Type.GetTypeCode(_info.PropertyType);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    this.TypeValue = new PropertyValue_Boolean(this);
                    break;
                case TypeCode.String:
                    this.TypeValue = new PropertyValue_String(this);
                    break;
                case TypeCode.Int32:
                    this.TypeValue = new PropertyValue_Integer(this);
                    break;
                case TypeCode.Double:
                    this.TypeValue = new PropertyValue_Double(this);
                    break;
                default:
                    if (_info.PropertyType == typeof(Point))
                    {
                        this.TypeValue = new PropertyValue_Point(this);
                    }
                    else if (_info.PropertyType == typeof(Color))
                    {
                        this.TypeValue = new PropertyValue_Color(this);
                    }
                    else if (_info.PropertyType == typeof(Size))
                    {
                        this.TypeValue = new PropertyValue_Size(this);
                    }
                    else
                    {
                        this.TypeValue = new PropertyValue_Object(this);
                    }
                    break;
            }
        }

        //
        public PropertyMemberElement(object source, PropertyInfo info, PropertyRangeAttribute attribute)
        {
            Initialize(source, info, attribute);

            TypeCode typeCode = Type.GetTypeCode(_info.PropertyType);
            switch (typeCode)
            {
                case TypeCode.Int32:
                    this.TypeValue = new PropertyValue_IntegerRange(this, (int)attribute.Minimum, (int)attribute.Maximum);
                    break;
                case TypeCode.Double:
                    this.TypeValue = new PropertyValue_DoubleRange(this, attribute.Minimum, attribute.Maximum);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        // enum
        public PropertyMemberElement(object source, PropertyInfo info, PropertyEnumAttribute attribute)
        {
            Initialize(source, info, attribute);

            if (_info.PropertyType.IsEnum)
            {
                this.TypeValue = new PropertyValue_Enum(this, _info.PropertyType);
            }
            else
            {
                throw new InvalidOperationException("not enum");
            }
        }

        //
        public PropertyMemberElement(object source, PropertyInfo info, PropertyPathAttribute attribute)
        {
            Initialize(source, info, attribute);

            TypeCode typeCode = Type.GetTypeCode(_info.PropertyType);
            switch (typeCode)
            {
                case TypeCode.String:
                    this.TypeValue = new PropertyValue_FilePath(this);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        //
        private static object GetDefaultValue(object source, PropertyInfo info)
        {
            var attributes = Attribute.GetCustomAttributes(info, typeof(DefaultValueAttribute));
            if (attributes != null && attributes.Length > 0)
            {
                return ((DefaultValueAttribute)attributes[0]).Value;
            }
            else
            {
                return info.GetValue(source); // もとの値
            }
        }

        //
        private ObsoleteAttribute GetObsoleteAttribute(PropertyInfo info)
        {
            return (ObsoleteAttribute)(Attribute.GetCustomAttribute(info, typeof(ObsoleteAttribute)));
        }


        //
        public Type GetValueType()
        {
            return _info.PropertyType;
        }

        //
        public string GetValueTypeString()
        {
            return TypeValue.GetTypeString();
        }

        public string GetValueString()
        {
            return TypeValue.GetValueString();
        }


        //
        public bool HasCustomValue
        {
            get { return !Default.Equals(GetValue()); }
        }

        //
        public void ResetValue()
        {
            SetValue(Default);
        }

        //
        public void SetValue(object value)
        {
            _info.SetValue(this.Source, value);
        }

        //
        public void SetValueFromString(string value)
        {
            TypeValue.SetValueFromString(value);
        }

        //
        public object GetValue()
        {
            return _info.GetValue(this.Source);
        }

        //
        public object GetValue(object source)
        {
            return _info.GetValue(source);
        }

        //
        public PropertyValue TypeValue { get; set; }
    }
}
