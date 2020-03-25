using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using NeeView.Text;

namespace NeeView
{
    /// <summary>
    /// ファイル拡張子コレクション
    /// </summary>
    [DataContract]
    [PropertyMergeReferenceCopy]
    [JsonConverter(typeof(JsonFileTypeCollectionConverter))]
    public class FileTypeCollection : StringCollection
    {
        public FileTypeCollection()
        {
        }

        public FileTypeCollection(string exts) : base(exts)
        {
        }

        public FileTypeCollection(IEnumerable<string> exts) : base(exts)
        {
        }

        public override string ValidateItem(string item)
        {
            return string.IsNullOrWhiteSpace(item) ? null : "." + item.Trim().TrimStart('.').ToLower();
        }

        public FileTypeCollection Clone()
        {
            return (FileTypeCollection)MemberwiseClone();
        }


        public new static FileTypeCollection Parse(string s)
        {
            return new FileTypeCollection(s);
        }
    }

    public sealed class JsonFileTypeCollectionConverter : JsonConverter<FileTypeCollection>
    {
        public override FileTypeCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return FileTypeCollection.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, FileTypeCollection value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
