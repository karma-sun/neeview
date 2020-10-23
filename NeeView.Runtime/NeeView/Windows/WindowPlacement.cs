using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace NeeView.Runtime
{
    [ImmutableObject(true)]
    [JsonConverter(typeof(JsonWindowPlaceConverter))]
    public class WindowPlacement
    {
        public static WindowPlacement Empty { get; } = new WindowPlacement();

        public WindowPlacement()
        {
        }

        public WindowPlacement(WindowState windowState, int left, int top, int width, int height)
        {
            WindowState = windowState;
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }


        public WindowState WindowState { get; private set; }
        public int Left { get; private set; }
        public int Top { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public int Right => Left + Width;
        public int Bottom => Top + Height;


        public bool IsValid()
        {
            return Width > 0 || Height > 0;
        }

        public override string ToString()
        {
            return $"{WindowState},{Left},{Top},{Width},{Height}";
        }

        public static WindowPlacement Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return WindowPlacement.Empty;

            var tokens = s.Split(',');
            if (tokens.Length != 5)
            {
                Debug.WriteLine($"WindowPlacement.Parse(): InvalidCast: {s}");
                return WindowPlacement.Empty;
            }

            var placement = new WindowPlacement(
                (WindowState)(Enum.Parse(typeof(WindowState), tokens[0])),
                int.Parse(tokens[1]),
                int.Parse(tokens[2]),
                int.Parse(tokens[3]),
                int.Parse(tokens[4]));

            return placement;
        }


        public sealed class JsonWindowPlaceConverter : JsonConverter<WindowPlacement>
        {
            public override WindowPlacement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return WindowPlacement.Parse(reader.GetString());
            }

            public override void Write(Utf8JsonWriter writer, WindowPlacement value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.IsValid() ? value.ToString() : "");
            }
        }
    }

}
