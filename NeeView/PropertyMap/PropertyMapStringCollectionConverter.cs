using NeeView.Text;
using System;

namespace NeeView
{
    public class PropertyMapStringCollectionConverter : PropertyMapConverter<StringCollection>
    {
        public override string GetTypeName(Type typeToConvert)
        {
            return "\"str1;str2;str3\"";
        }

        public override object Read(PropertyMapSource source, Type typeToConvert, PropertyMapOptions options)
        {
            return source.GetValue().ToString();
        }

        public override void Write(PropertyMapSource source, object value, PropertyMapOptions options)
        {
            if (value is string s)
            {
                source.SetValue(StringCollection.Parse(s));
            }
            else
            {
                throw new InvalidCastException();
            }
        }
    }


}