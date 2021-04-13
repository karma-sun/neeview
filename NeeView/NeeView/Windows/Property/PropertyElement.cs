using System;
using System.ComponentModel;
using System.Diagnostics;
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


    public class PropertyValueSource : IValueSetter
    {
        private object _source;
        private PropertyInfo _info;

        public PropertyValueSource(object source, PropertyInfo info)
        {
            _source = source;
            _info = info;

            if (source is INotifyPropertyChanged notify)
            {
                notify.PropertyChanged += (s, e) =>
                {
                    if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == _info.Name)
                    {
                        ValueChanged?.Invoke(s, e);
                    }
                };
            }
        }

        public PropertyValueSource(object source, string propertyName) : this(source, source.GetType().GetProperty(propertyName))
        {
        }


        public string Name => _info.Name;


        public event EventHandler ValueChanged;


        public void SetValue(object value)
        {
            _info.SetValue(_source, value);
        }

        public object GetValue()
        {
            return _info.GetValue(_source);
        }
    }


    /// <summary>
    /// プロパティ項目表示編集
    /// </summary>
    public class PropertyMemberElement : PropertyDrawElement, IValueSetter
    {
        private PropertyInfo _info;

        public event EventHandler ValueChanged;


        public PropertyMemberElement(object source, PropertyInfo info, PropertyMemberAttribute attribute, PropertyMemberElementOptions options)
        {
            InitializeCommon(source, info, attribute, options);

            switch (attribute)
            {
                case PropertyPercentAttribute percentAttribute:
                    InitializeByPercentAttribute(percentAttribute);
                    break;

                case PropertyRangeAttribute rangeAttribute:
                    InitializeByRangeAttribute(rangeAttribute);
                    break;

                case PropertyPathAttribute pathAttribute:
                    InitializeByPathAttribute(pathAttribute);
                    break;

                case PropertyStringsAttribute stringsAttribute:
                    InitializeByStringsAttribute(stringsAttribute);
                    break;

                default:
                    InitializeByDefaultAttribute(attribute);
                    break;
            }
        }


        public object Source { get; set; }
        public string Path => _info.Name;
        public string Name { get; set; }
        public string Tips { get; set; }
        public bool IsVisible { get; set; }
        public object Default { get; set; }
        public bool IsObsolete { get; set; }
        public string EmptyMessage { get; set; }
        public PropertyMemberElementOptions Options { get; set; }


        private void InitializeCommon(object source, PropertyInfo info, PropertyMemberAttribute attribute, PropertyMemberElementOptions options)
        {
            Source = source;
            Name = options.Name ?? PropertyMemberAttributeExtensions.GetPropertyName(info, attribute) ?? info.Name;
            Tips = PropertyMemberAttributeExtensions.GetPropertyTips(info, attribute);
            IsVisible = attribute != null ? attribute.IsVisible : true;
            EmptyMessage = attribute?.EmptyMessage;
            Options = options;

            this.Default = GetDefaultValue(source, info);
            this.IsObsolete = GetObsoleteAttribute(info) != null;

            _info = info;

            if (source is INotifyPropertyChanged notify)
            {
                notify.PropertyChanged += (s, e) =>
                {
                    if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == _info.Name)
                    {
                        ValueChanged?.Invoke(s, e);
                    }
                };
            }
        }

        private void InitializeByDefaultAttribute(PropertyMemberAttribute attribute)
        {
            if (_info.PropertyType.IsEnum)
            {
                this.TypeValue = new PropertyValue_Enum(this, _info.PropertyType);
                return;
            }

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
                    else if (_info.PropertyType == typeof(TimeSpan))
                    {
                        this.TypeValue = new PropertyValue_TimeSpan(this);
                    }
                    else
                    {
                        this.TypeValue = new PropertyValue_Object(this);
                    }
                    break;
            }
        }

        private void InitializeByRangeAttribute(PropertyRangeAttribute attribute)
        {
            IValueSetter value = attribute.RangeProperty != null ? (IValueSetter)new PropertyValueSource(this.Source, attribute.RangeProperty) : this;

            TypeCode typeCode = Type.GetTypeCode(_info.PropertyType);
            switch (typeCode)
            {
                case TypeCode.Int32:
                    this.TypeValue = CreatePropertyValue(new RangeProfile_Integer(value, attribute.Minimum, attribute.Maximum, attribute.TickFrequency, attribute.IsEditable, attribute.Format));
                    break;
                case TypeCode.Double:
                    this.TypeValue = CreatePropertyValue(new RangeProfile_Double(value, attribute.Minimum, attribute.Maximum, attribute.TickFrequency, attribute.IsEditable, attribute.Format));
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private PropertyValue CreatePropertyValue(RangeProfile_Integer profile)
        {
            if (profile.IsEditable)
            {
                return new PropertyValue_EditableIntegerRange(this, profile);
            }
            else
            {
                return new PropertyValue_IntegerRange(this, profile);
            }
        }

        private PropertyValue CreatePropertyValue(RangeProfile_Double profile)
        {
            if (profile.IsEditable)
            {
                return new PropertyValue_EditableDoubleRange(this, profile);
            }
            else
            {
                return new PropertyValue_DoubleRange(this, profile);
            }
        }

        private void InitializeByPercentAttribute(PropertyPercentAttribute attribute)
        {
            IValueSetter value = attribute.RangeProperty != null ? (IValueSetter)new PropertyValueSource(this.Source, attribute.RangeProperty) : this;

            TypeCode typeCode = Type.GetTypeCode(_info.PropertyType);
            switch (typeCode)
            {
                case TypeCode.Double:
                    this.TypeValue = new PropertyValue_Percent(this, new RangeProfile_Double(value, attribute.Minimum, attribute.Maximum, attribute.TickFrequency, attribute.IsEditable, attribute.Format));
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void InitializeByPathAttribute(PropertyPathAttribute attribute)
        {
            TypeCode typeCode = Type.GetTypeCode(_info.PropertyType);
            switch (typeCode)
            {
                case TypeCode.String:
                    this.TypeValue = new PropertyValue_FilePath(this, attribute.FileDialogType, attribute.Filter, attribute.Note, attribute.DefaultFileName);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void InitializeByStringsAttribute(PropertyStringsAttribute attribute)
        {
            TypeCode typeCode = Type.GetTypeCode(_info.PropertyType);
            switch (typeCode)
            {
                case TypeCode.String:
                    this.TypeValue = new PropertyValue_StringMap(this, attribute.Strings);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }


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

        private ObsoleteAttribute GetObsoleteAttribute(PropertyInfo info)
        {
            return (ObsoleteAttribute)(Attribute.GetCustomAttribute(info, typeof(ObsoleteAttribute)));
        }


        public Type GetValueType()
        {
            return _info.PropertyType;
        }

        public string GetValueString()
        {
            return TypeValue.GetValueString();
        }


        public bool HasCustomValue
        {
            get { return !Default.Equals(GetValue()); }
        }

        public void ResetValue()
        {
            SetValue(Default);
        }

        public void SetValue(object value)
        {
            _info.SetValue(this.Source, value);
        }

        public void SetValueFromString(string value)
        {
            TypeValue.SetValueFromString(value);
        }

        public object GetValue()
        {
            return _info.GetValue(this.Source);
        }

        public object GetValue(object source)
        {
            return _info.GetValue(source);
        }

        public PropertyValue TypeValue { get; set; }


        public static PropertyMemberElement Create(object source, string name)
        {
            return Create(source, name, PropertyMemberElementOptions.Default);
        }

        /// <summary>
        /// オブジェクトとプロパティ名から PropertyMemberElement を作成する
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static PropertyMemberElement Create(object source, string name, PropertyMemberElementOptions options)
        {
            var info = source.GetType().GetProperty(name);
            if (info != null)
            {
                var attribute = GetPropertyMemberAttribute(info);
                if (attribute != null)
                {
                    return new PropertyMemberElement(source, info, attribute, options);
                }
            }
            Debugger.Break();
            return null;
        }

        /// <summary>
        /// PropertyMember属性取得
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private static PropertyMemberAttribute GetPropertyMemberAttribute(MemberInfo info)
        {
            return (PropertyMemberAttribute)Attribute.GetCustomAttributes(info, typeof(PropertyMemberAttribute)).FirstOrDefault();
        }
    }
}
