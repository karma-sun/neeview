using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeeView
{
    [JsonConverter(typeof(DestinationFolderConverter))]
    public class DestinationFolder : ICloneable
    {
        private string _name = "";
        private string _path = "";


        public DestinationFolder()
        {
        }

        public DestinationFolder(string name, string path)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (path == null) throw new ArgumentNullException(nameof(path));

            Name = name;
            Path = path;
        }


        public string Name
        {
            get { return string.IsNullOrWhiteSpace(_name) ? LoosePath.GetFileName(_path) : _name; }
            set { _name = value; }
        }

        public string Path
        {
            get => _path;
            set => _path = value;
        }


        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(_path);
        }

        public override string ToString()
        {
            return Name + "|" + Path;
        }

        public static DestinationFolder Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return new DestinationFolder();

            var tokens = s.Split('|').Select(e => e.Trim()).ToList();

            switch(tokens.Count)
            {
                case 0:
                    return new DestinationFolder();
                case 1:
                    return new DestinationFolder(tokens[0], "");
                default:
                    return new DestinationFolder(tokens[0], tokens[1]);
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }


    public sealed class DestinationFolderConverter : JsonConverter<DestinationFolder>
    {
        public override DestinationFolder Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DestinationFolder.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DestinationFolder value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}