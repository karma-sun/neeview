using System;

namespace NeeView
{
    public abstract class PropertyMapConverter
    {
        public abstract bool CanConvert(Type typeToConvert);

        public abstract string GetTypeName(Type typeToConvert);

        public abstract object Read(PropertyMapSource source, Type typeToConvert, PropertyMapOptions options);

        public abstract void Write(PropertyMapSource source, object value, PropertyMapOptions options);
    }



    public abstract class PropertyMapConverter<T> : PropertyMapConverter
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(T);
        }

        public override string GetTypeName(Type typeToConvert)
        {
            return typeToConvert.ToManualString();
        }
    }

}