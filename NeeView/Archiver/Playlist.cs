using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace NeeView
{
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


        public string Format { get; set; } = "NeeViewPlaylist.1";

        public List<string> Items { get; set; }
    }


    public static class PlaylistFile
    {
        public static void Save(string path, Playlist playlist, bool overwrite)
        {
            if (!overwrite && File.Exists(path))
            {
                throw new IOException();
            }

            var json = JsonSerializer.SerializeToUtf8Bytes(playlist, UserSettingTools.GetSerializerOptions());
            File.WriteAllBytes(path, json);
        }

        public static Playlist Load(string path)
        {
            var json = File.ReadAllBytes(path);
            return JsonSerializer.Deserialize<Playlist>(json, UserSettingTools.GetSerializerOptions());
        }
    }

}
