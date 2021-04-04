using System;
using System.Text.Json;
using System.IO;
using System.Windows;

namespace NeeView
{
    public static class ThemeProfileTools
    {
        public static void Save(ThemeProfile themeProfile, string path)
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(themeProfile, UserSettingTools.GetSerializerOptions());
            System.IO.File.WriteAllBytes(path, json);
        }

        public static void SaveFromContent(string contentPath, string path)
        {
            Uri uri = new Uri(contentPath, UriKind.Relative);
            var info = Application.GetContentStream(uri);

            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                info.Stream.CopyTo(fileStream);
            }
        }

            public static ThemeProfile Load(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                var isSuccess = ms.TryGetBuffer(out var buffer);
                if (!isSuccess) throw new IOException();
                return JsonSerializer.Deserialize<ThemeProfile>(buffer, UserSettingTools.GetSerializerOptions());
            }
        }

        public static ThemeProfile LoadFromContent(string contentPath)
        {
            Uri uri = new Uri(contentPath, UriKind.Relative);
            var info = Application.GetContentStream(uri);
            if (info is null) throw new FileNotFoundException($"No such theme: {contentPath}");
            return Load(info.Stream);
        }

        public static ThemeProfile LoadFromFile(string path)
        {
            using (var fs = File.OpenRead(path))
            {
                return Load(fs);
            }
        }

        public static ThemeProfile Merge(ThemeProfile baseProfile, ThemeProfile overwriteProfile)
        {
            var profile = (ThemeProfile)baseProfile.Clone();

            foreach(var pair in overwriteProfile.Colors)
            {
                profile[pair.Key] = pair.Value;
            }

            return profile;
        }
    }
}
