using NeeLaboratory.IO;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace NeeView
{
    public static class PlaylistSourceTools
    {
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public static void Save(this PlaylistSource playlist, string path, bool overwrite)
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

        public static PlaylistSource Load(string path)
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

        private static PlaylistSource Deserialize(byte[] json)
        {
            var fileHeader = JsonSerializer.Deserialize<PlaylistFileHeader>(json, UserSettingTools.GetSerializerOptions());

#pragma warning disable CS0612 // 型またはメンバーが旧型式です
            if (fileHeader.Format.Name == PlaylistSourceV1.FormatVersion)
            {
                var playlistV1 = JsonSerializer.Deserialize<PlaylistSourceV1>(json, UserSettingTools.GetSerializerOptions());
                return playlistV1.ToPlaylist();
            }
#pragma warning restore CS0612 // 型またはメンバーが旧型式です

            else
            {
                return JsonSerializer.Deserialize<PlaylistSource>(json, UserSettingTools.GetSerializerOptions());
            }
        }


        private class PlaylistFileHeader
        {
            public FormatVersion Format { get; set; }
        }
    }

}
