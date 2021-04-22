using NeeLaboratory.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NeeView
{
    public class Playlist
    {
        public static readonly FormatVersion FormatVersion = new FormatVersion("NeeView.Playlist", 2, 0, 0);

        public Playlist()
        {
            Items = new List<PlaylistItem>();
        }

        public Playlist(IEnumerable<string> items)
        {
            Items = items.Select(e => new PlaylistItem(e)).ToList();
        }

        public Playlist(IEnumerable<PlaylistItem> items)
        {
            Items = items.ToList();
        }

        public FormatVersion Format { get; set; } = FormatVersion;

        public List<PlaylistItem> Items { get; set; }
    }


    public static class PlaylistTools
    {
        public static void Save(this Playlist playlist, string path, bool overwrite)
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
            return Deserialize(json);
        }

        public static async Task<Playlist> LoadAsync(string path)
        {
            var json = await FileTools.ReadAllBytesAsync(path);
            return Deserialize(json);
        }

        private static Playlist Deserialize(byte[] json)
        {
            var fileHeader = JsonSerializer.Deserialize<PlaylistFileHeader>(json, UserSettingTools.GetSerializerOptions());

#pragma warning disable CS0612 // 型またはメンバーが旧型式です
            if (fileHeader.Format.Name == PlaylistV1.FormatVersion)
            {
                var playlistV1 = JsonSerializer.Deserialize<PlaylistV1>(json, UserSettingTools.GetSerializerOptions());
                return playlistV1.ToPlaylist();
            }
#pragma warning restore CS0612 // 型またはメンバーが旧型式です

            else
            {
                return JsonSerializer.Deserialize<Playlist>(json, UserSettingTools.GetSerializerOptions());
            }
        }


        private class PlaylistFileHeader
        {
            public FormatVersion Format { get; set; }
        }
    }

}
