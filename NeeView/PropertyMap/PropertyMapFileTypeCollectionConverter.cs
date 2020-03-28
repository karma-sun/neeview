using System;

namespace NeeView
{
    public class PropertyMapFileTypeCollectionConverter : PropertyMapConverter<FileTypeCollection>
    {
        public override string GetTypeName(Type typeToConvert)
        {
            return "\".ex1;.ex2;.ex3\"";
        }

        public override object Read(PropertyMapSource source, Type typeToConvert, PropertyMapOptions options)
        {
            return source.GetValue().ToString();
        }

        public override void Write(PropertyMapSource source, object value, PropertyMapOptions options)
        {
            if (value is string s)
            {
                source.SetValue(FileTypeCollection.Parse(s));
            }
            else
            {
                throw new InvalidCastException();
            }
        }
    }


}