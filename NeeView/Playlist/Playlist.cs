using NeeLaboratory.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
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
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public static void Save(this Playlist playlist, string path, bool overwrite)
        {
            if (!overwrite && File.Exists(path))
            {
                throw new IOException();
            }

            _semaphore.Wait();
            try
            {
                Debug.WriteLine($"Save: {path}");
                var json = JsonSerializer.SerializeToUtf8Bytes(playlist, UserSettingTools.GetSerializerOptions());

                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllBytes(path, json);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static Playlist Load(string path)
        {
            _semaphore.Wait();
            try
            {
                Debug.WriteLine($"Load: {path}");
                var json = FileTools.ReadAllBytes(path, FileShare.Read);
                return Deserialize(json);
            }
            finally
            {
                _semaphore.Release();
            }
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
