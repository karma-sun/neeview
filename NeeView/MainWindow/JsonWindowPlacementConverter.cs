// from http://grabacr.net/archives/1585
using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeeView
{
    /// <summary>
    /// WINDOWPLACEMENTのJSONを1パラメータにするコンバータ（未使用）
    /// </summary>
    public sealed class JsonWindowPlacementConverter : JsonConverter<WINDOWPLACEMENT>
    {
        private static bool _isOneLine = true;

        public static JsonSerializerOptions GetSerializerOptions()
        {
            var options = new JsonSerializerOptions();
            options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.WriteIndented = false;
            options.IgnoreReadOnlyProperties = false;
            options.IgnoreNullValues = false;
            return options;
        }


        public override WINDOWPLACEMENT Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // type: OneLine
            if (reader.TokenType == JsonTokenType.String)
            {
                return WINDOWPLACEMENT.Parse(reader.GetString());
            }


            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var placement = new WINDOWPLACEMENT();

            while (true)
            {
                if (!reader.Read())
                {
                    throw new JsonException();
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    switch (reader.GetString())
                    {
                        case nameof(placement.Length):
                            placement.Length = JsonSerializer.Deserialize<int>(ref reader, options);
                            break;
                        case nameof(placement.ShowCmd):
                            placement.ShowCmd = JsonSerializer.Deserialize<SW>(ref reader, options);
                            break;
                        case nameof(placement.MinPosition):
                            placement.MinPosition = JsonSerializer.Deserialize<POINT>(ref reader, options);
                            break;
                        case nameof(placement.MaxPosition):
                            placement.MaxPosition = JsonSerializer.Deserialize<POINT>(ref reader, options);
                            break;
                        case nameof(placement.NormalPosition):
                            placement.NormalPosition = JsonSerializer.Deserialize<RECT>(ref reader, options);
                            break;
                        default:
                            if (!reader.TrySkip())
                            {
                                throw new JsonException();
                            }
                            break;
                    }
                }
                else if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }
                else
                {
                    throw new JsonException();
                }
            }

            return placement;
        }

        public override void Write(Utf8JsonWriter writer, WINDOWPLACEMENT value, JsonSerializerOptions options)
        {
            if (_isOneLine)
            {
                writer.WriteStringValue(value.ToString());
            }
            else
            {
                writer.WriteStartObject();
                writer.WritePropertyName(nameof(value.Length));
                JsonSerializer.Serialize(writer, value.Length, options);
                writer.WritePropertyName(nameof(value.ShowCmd));
                JsonSerializer.Serialize(writer, value.ShowCmd, options);
                writer.WritePropertyName(nameof(value.MinPosition));
                JsonSerializer.Serialize(writer, value.MinPosition, options);
                writer.WritePropertyName(nameof(value.MaxPosition));
                JsonSerializer.Serialize(writer, value.MaxPosition, options);
                writer.WritePropertyName(nameof(value.NormalPosition));
                JsonSerializer.Serialize(writer, value.NormalPosition, options);
                writer.WriteEndObject();
            }

        }
    }
}
