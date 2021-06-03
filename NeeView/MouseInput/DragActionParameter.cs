using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeeView
{
    [JsonConverter(typeof(JsonDragActionParameterConverter))]
    public class DragActionParameter : BindableBase, ICloneable
    {
        private Func<object, object, bool> _equals;

        public DragActionParameter()
        {
            _equals = ObjectExtensions.MakeEqualsMethod(this.GetType());
        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        public bool MemberwiseEquals(DragActionParameter other)
        {
            return _equals(this, other);
        }
    }



    public class SensitiveDragActionParameter : DragActionParameter
    {
        private double _sensitivity = 1.0;

        /// <summary>
        /// 感度
        /// </summary>
        [PropertyRange(0.0, 2.0, TickFrequency = 0.05)]
        public double Sensitivity
        {
            get { return _sensitivity; }
            set { SetProperty(ref _sensitivity, value); }
        }
    }



    /// <summary>
    /// JsonConverter for DragActionParameter.
    /// Support polymorphism.
    /// </summary>
    public sealed class JsonDragActionParameterConverter : JsonConverter<DragActionParameter>
    {
        // NOTE: need add polymorphism class type.
        public static Type[] KnownTypes { get; set; } = new Type[]
        {
            typeof(SensitiveDragActionParameter),
        };


        public override DragActionParameter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

            return (DragActionParameter)instance;
        }

        public override void Write(Utf8JsonWriter writer, DragActionParameter value, JsonSerializerOptions options)
        {

            var type = value.GetType();
            Debug.Assert(KnownTypes.Contains(type));

            var def = (DragActionParameter)Activator.CreateInstance(type);
            if (value.MemberwiseEquals(def))
            {
                Debug.WriteLine($"{type} is default.");
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStartObject();
                writer.WriteString("Type", type.Name);
                writer.WritePropertyName("Value");
                JsonSerializer.Serialize(writer, value, type, options);
                writer.WriteEndObject();
            }

        }
    }
}
