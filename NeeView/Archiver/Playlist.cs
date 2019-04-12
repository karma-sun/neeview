using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace NeeView
{
    [DataContract]
    public class Playlist
    {
        public Playlist()
        {
            Items = new List<string>();
        }

        public Playlist(IEnumerable<string> items)
        {
            Items = new List<string>(items);
        }

        [DataMember]
        public string Format { get; private set; } = "NeeViewPlaylist.1";

        [DataMember]
        public List<string> Items { get; private set; }

        [OnDeserialized]
        private void Deserialized(StreamingContext c)
        {
            Items = Items ?? new List<string>();
        }
    }


    public static class PlaylistFile
    {
        public static void Save(string path, Playlist playlist, bool overwrite)
        {
            var fileMode = overwrite ? FileMode.Create : FileMode.CreateNew;
            using (var stream = new FileStream(path, fileMode, FileAccess.Write))
            {
                Write(stream, playlist);
            }
        }

        public static Playlist Load(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return Read(stream);
            }
        }


        public static void Write(Stream stream, Playlist playlist)
        {
            using (var writer = JsonReaderWriterFactory.CreateJsonWriter(stream, Encoding.UTF8, false, true, "  "))
            {
                var settings = new DataContractJsonSerializerSettings() { UseSimpleDictionaryFormat = true };
                var serializer = new DataContractJsonSerializer(typeof(Playlist), settings);
                serializer.WriteObject(writer, playlist);
            }
        }

        public static Playlist Read(Stream stream)
        {
            var serializer = new DataContractJsonSerializer(typeof(Playlist));
            return (Playlist)serializer.ReadObject(stream);
        }
    }

}
