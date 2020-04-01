using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using NeeLaboratory;
using NeeView.Data;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;

namespace NeeView
{
    /// <summary>
    /// コマンドパラメータ（基底）
    /// </summary>
    [DataContract]
    [JsonConverter(typeof(JsonCommandParameterConverter))]
    public abstract class CommandParameter : ICloneable
    {
        public object Clone()
        {
            return MemberwiseClone();
        }

        public abstract bool MemberwiseEquals(CommandParameter other);
    }


    /// <summary>
    /// 操作反転コマンドパラメータ基底
    /// </summary>
    [DataContract]
    public class ReversibleCommandParameter : CommandParameter
    {
        [DataMember]
        [PropertyMember("@ParamCommandParameterIsReverse", Tips = "@ParamCommandParameterIsReverseTips")]
        public bool IsReverse { get; set; } = true;

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            IsReverse = true;
        }

        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as ReversibleCommandParameter;
            if (target == null) return false;
            return this == target || (this.IsReverse == target.IsReverse);
        }
    }


    /// <summary>
    /// JsonConverter for CommandParameter.
    /// Support polymorphism.
    /// </summary>
    public sealed class JsonCommandParameterConverter : JsonConverter<CommandParameter>
    {
        // NOTE: need add polymorphism class type.
        public static Type[] KnownTypes { get; set; } = new Type[]
        {
            typeof(ReversibleCommandParameter),
            typeof(MoveSizePageCommandParameter),
            typeof(ToggleStretchModeCommandParameter),
            typeof(StretchModeCommandParameter),
            typeof(ViewScrollCommandParameter),
            typeof(ViewScaleCommandParameter),
            typeof(ViewRotateCommandParameter),
            typeof(MovePagemarkInBookCommandParameter),
            typeof(ScrollPageCommandParameter),
            typeof(FocusMainViewCommandParameter),
            typeof(ExportImageAsCommandParameter),
            typeof(ExportImageCommandParameter),
            typeof(OpenExternalAppCommandParameter),
            typeof(CopyFileCommandParameter),
        };

        public static JsonSerializerOptions GetSerializerOptions()
        {
            var options = new JsonSerializerOptions();
            options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.WriteIndented = false;
            options.IgnoreReadOnlyProperties = false;
            options.IgnoreNullValues = false;
            return options;
        }

        public override CommandParameter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "Type")
            {
                throw new JsonException();
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException();
            }
            var typeString = reader.GetString();

            Type type = KnownTypes.FirstOrDefault(e => e.Name == typeString);
            Debug.Assert(type != null);

            if (!reader.Read() || reader.GetString() != "Value")
            {
                throw new JsonException();
            }
            if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            object instance;
            if (type != null)
            {
                instance = JsonSerializer.Deserialize(ref reader, type, options);
            }
            else
            {
                Debug.WriteLine($"Nor support type: {typeString}");
                reader.Skip();
                instance = null;
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException();
            }

            return (CommandParameter)instance;
        }

        public override void Write(Utf8JsonWriter writer, CommandParameter value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            var type = value.GetType();
            Debug.Assert(KnownTypes.Contains(type));
            writer.WriteString("Type", type.Name);
            writer.WritePropertyName("Value");
            JsonSerializer.Serialize(writer, value, type, options);

            writer.WriteEndObject();
        }
    }
}
