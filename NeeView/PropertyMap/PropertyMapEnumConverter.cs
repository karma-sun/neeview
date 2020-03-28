using System;

namespace NeeView
{
    public class PropertyMapEnumConverter : PropertyMapConverter<Enum>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsEnum;
        }


        public override object Read(PropertyMapSource source, Type typeToConvert, PropertyMapOptions options)
        {
            return source.GetValue().ToString();
        }

        public override void Write(PropertyMapSource source, object value, PropertyMapOptions options)
        {
            if (value is string s)
            {
                source.SetValue(Enum.Parse(source.PropertyInfo.PropertyType, s));
            }
            else
            {
                throw new InvalidCastException();
            }
        }
    }


}