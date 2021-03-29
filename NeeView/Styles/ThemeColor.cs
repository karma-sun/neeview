using System;
using System.ComponentModel;
using System.Windows.Media;
using System.Text.Json.Serialization;
using System.Globalization;
using System.Text.Json;

namespace NeeView
{
    public enum ThemeColorType
    {
        Default,
        Color,
        Link,
    }


    [TypeConverter(typeof(ThemeColorTypeConverter))]
    [JsonConverter(typeof(ThemeColorJsonConverter))]
    public class ThemeColor
    {
        public ThemeColor()
        {
            ThemeColorType = ThemeColorType.Default;
        }

        public ThemeColor(Color color)
        {
            ThemeColorType = ThemeColorType.Color;
            Color = color;
        }

        public ThemeColor(string link)
        {
            ThemeColorType = ThemeColorType.Link;
            Link = link;
        }


        public ThemeColorType ThemeColorType { get; private set; }
        public Color Color { get; private set; }
        public string Link { get; private set; }


        public override string ToString()
        {
            switch (ThemeColorType)
            {
                case ThemeColorType.Default:
                    return "";
                case ThemeColorType.Color:
                    return Color.ToString();
                case ThemeColorType.Link:
                    return Link;
            }

            throw new InvalidOperationException();
        }

        public static ThemeColor Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return new ThemeColor();
            }
            else if (s.IndexOf('.') >= 0)
            {
                return new ThemeColor(s);
            }
            else
            {
                try
                {
                    return new ThemeColor((Color)ColorConverter.ConvertFromString(s));
                }
                catch (FormatException ex)
                {
                    throw new FormatException($"{ex.Message}: \"{s}\"", ex);
                }
            }
        }
    }


    public class ThemeColorTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return ThemeColor.Parse(value as string);
        }
    }


    public sealed class ThemeColorJsonConverter : JsonConverter<ThemeColor>
    {
        public override ThemeColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return ThemeColor.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, ThemeColor value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

}
