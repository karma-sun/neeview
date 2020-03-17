using NeeView.Data;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public class ConfigAccessor
    {
        private Config _config;

        public ConfigAccessor(Config source)
        {
            _config = source;
        }


        private static JsonSerializerOptions GetSerializerOptions()
        {
            var options = new JsonSerializerOptions();
            options.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            options.WriteIndented = true;
            options.IgnoreReadOnlyProperties = true;
            options.Converters.Add(new JsonEnumFuzzyConverter());
            options.Converters.Add(new JsonColorConverter());
            options.Converters.Add(new JsonSizeConverter());
            return options;
        }

        public void Save(string path)
        {
#if true
            var jsonString = JsonSerializer.Serialize(_config, GetSerializerOptions());
            File.WriteAllText(path, jsonString);
#else
            var json = JsonSerializer.SerializeToUtf8Bytes(_config, GetSerializerOptions());
            using (var fs = File.Create(path))
            {
                fs.Write(json, 0, json.Length);
            }
#endif
        }


        public static Config Load(string path)
        {
            var jsonString = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Config>(jsonString, GetSerializerOptions());
        }


        public byte[] Serialize()
        {
            return Json.SerializeRaw(_config, null, true);
        }

        public void RestoreSerialized(byte[] memento)
        {
            var source = Json.Deserialize<Config>(memento);

            // TODO: Version互換性

            OverwriteProperties(source, _config);
        }


        public void OverwriteProperties(Config src)
        {
            OverwriteProperties(src, _config);
        }

        /// <summary>
        /// 他のインスタンスへプロパティを上書き
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        private static void OverwriteProperties(object src, object dst)
        {
            var type = src.GetType();
            if (type != dst.GetType()) throw new InvalidOperationException();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                // NOTE: DataMember
                ////var attribute = property.GetCustomAttribute(typeof(DataMemberAttribute));
                ////if (attribute == null) continue;
                ///
                if (property.Name == "_Version")
                {
                    continue;
                }

                if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    OverwriteProperties(property.GetValue(src), property.GetValue(dst));
                }
                else
                {
                    property.GetSetMethod()?.Invoke(dst, new object[] { property.GetValue(src) });
                }
            }
        }
    }



    /// <summary>
    /// Sizeを文字列に変換する
    /// </summary>
    public sealed class JsonSizeConverter : JsonConverter<Size>
    {
        public override Size Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return (Size)new SizeConverter().ConvertFrom(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, Size value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    /// <summary>
    /// Colorを文字列に変換する
    /// </summary>
    public sealed class JsonColorConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return (Color)ColorConverter.ConvertFromString(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    /// <summary>
    /// Enumを文字列に変換する。
    /// 文字列がEnumに変換できないときはdefault値とみなす
    /// </summary>
    public class JsonEnumFuzzyConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsEnum;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            JsonConverter converter = (JsonConverter)Activator.CreateInstance(
                typeof(EnumFuzzyConverter<>).MakeGenericType(typeToConvert),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { },
                culture: null);

            return converter;
        }


        public class EnumFuzzyConverter<T> : JsonConverter<T>
            where T : struct, Enum
        {
            public override bool CanConvert(Type type)
            {
                return type.IsEnum;
            }

            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return Enum.TryParse(reader.GetString(), out T value) ? value : default;
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }
    }

}