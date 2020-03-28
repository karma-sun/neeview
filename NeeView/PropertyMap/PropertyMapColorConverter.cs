using System;
using System.Windows.Media;

namespace NeeView
{
    public class PropertyMapColorConverter : PropertyMapConverter<Color>
    {
        public override string GetTypeName(Type typeToConvert)
        {
            return "\"#AARRGGBB\"";
        }

        public override object Read(PropertyMapSource source, Type typeToConvert, PropertyMapOptions options)
        {
            return source.GetValue().ToString();
        }

        public override void Write(PropertyMapSource source, object value, PropertyMapOptions options)
        {
            source.SetValue((Color)new ColorConverter().ConvertFrom(value));
        }
    }


}