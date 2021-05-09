using System;
using System.Text.Json.Serialization;
using System.IO;
using System.Text.Json;

namespace NeeView
{
    [ObjectMergeReferenceCopy]
    [JsonConverter(typeof(JsonThemeSourceConverter))]
    public class TheneSource
    {
        public TheneSource(ThemeType themeType)
        {
            if (themeType == ThemeType.Custom) throw new ArgumentException($"{nameof(themeType)} must not be {nameof(ThemeType.Custom)}.");

            Type = themeType;
            FileName = null;
        }

        public TheneSource(ThemeType themeType, string fileName)
        {
            if (themeType == ThemeType.Custom && string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException($"{ThemeType.Custom} requires {nameof(fileName)}.");

            if (themeType != ThemeType.Custom && !(fileName is null))
                throw new ArgumentException($"{nameof(fileName)} cannot be set except for {ThemeType.Custom}.");

            Type = themeType;
            FileName = fileName;
        }

        public ThemeType Type { get; private set; }

        public string FileName { get; private set; }

        public string FullName => (Type == ThemeType.Custom) ? Path.Combine(Config.Current.Theme.CustomThemeFolder, this.FileName) : null;


        public override string ToString()
        {
            return Type.ToString() + (FileName != null ? ("." + FileName) : "");
        }

        public static TheneSource Parse(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return new TheneSource(ThemeType.Dark);
            }

            var tokens = s.Split(new char[] { '.' }, 2);
            var themeType = (ThemeType)Enum.Parse(typeof(ThemeType), tokens[0]);
            var fileName = tokens.Length >= 2 ? tokens[1] : null;
            var themeId = new TheneSource(themeType, fileName);
            return themeId;
        }
    }



    public sealed class JsonThemeSourceConverter : JsonConverter<TheneSource>
    {
        public override TheneSource Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return TheneSource.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, TheneSource value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }


}
