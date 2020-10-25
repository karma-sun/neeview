using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeeView.Text.Json
{
    /// <summary>
    /// TimeSpanを文字列に変換する
    /// </summary>
    public sealed class JsonTimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return TimeSpan.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
