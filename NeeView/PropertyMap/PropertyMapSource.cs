using NeeView.Windows.Property;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NeeView
{
    public class PropertyMapSource : PropertyMapNode
    {
        private string _prefix;

        public PropertyMapSource(object source, PropertyInfo property, PropertyMapConverter converter, string prefix)
        {
            Source = source;
            PropertyInfo = property;
            IsReadOnly = property.GetCustomAttribute(typeof(PropertyMapReadOnlyAttribute)) != null;
            Converter = converter;
            _prefix = prefix;
        }

        public object Source { get; private set; }
        public PropertyInfo PropertyInfo { get; private set; }
        public bool IsReadOnly { get; private set; }

        public PropertyMapConverter Converter { get; private set; }


        public object Read(PropertyMapOptions options)
        {
            return Converter.Read(this, PropertyInfo.PropertyType, options);
        }

        public void Write(object value, PropertyMapOptions options)
        {
            if (IsReadOnly) return;
            Converter.Write(this, value, options);
        }

        public object GetValue()
        {
            return PropertyInfo.GetValue(Source);
        }

        public void SetValue(object value)
        {
            PropertyInfo.SetValue(Source, value);
        }

        public (string type, string description) CreateHelpHtml()
        {
            string typeString;
            if (PropertyInfo.PropertyType.IsEnum)
            {
                typeString = "<dl>" + string.Join("", PropertyInfo.PropertyType.VisibledAliasNameDictionary().Select(e => $"<dt>\"{e.Key}\"</dt><dd>{e.Value}</dd>")) + "</dl>";
            }
            else
            {
                typeString = Converter.GetTypeName(PropertyInfo.PropertyType);
            }

            var readOnly = (PropertyInfo.GetCustomAttribute<PropertyMapReadOnlyAttribute>() != null || !PropertyInfo.CanWrite) ? " (" + ResourceService.GetString("@Word.ReadOnly") + ")" : "";

            var attribute = PropertyInfo.GetCustomAttribute<PropertyMemberAttribute>();
            var description = attribute != null
                    ? PropertyMemberAttributeExtensions.GetPropertyName(PropertyInfo, attribute) + "<br/>" + PropertyMemberAttributeExtensions.GetPropertyTips(PropertyInfo, attribute)
                    : "";
            description = _prefix + new Regex("[\r\n]+").Replace(description, "<br/>") + readOnly;

            return (typeString, description);
        }
    }
}