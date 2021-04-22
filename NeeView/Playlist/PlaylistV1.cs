using System;
using System.Collections.Generic;

namespace NeeView
{
    [Obsolete]
    public class PlaylistV1
    {
        public const string FormatVersion = "NeeViewPlaylist.1";

        public PlaylistV1()
        {
            Items = new List<string>();
        }

        public PlaylistV1(IEnumerable<string> items)
        {
            Items = new List<string>(items);
        }

        public string Format { get; set; } = FormatVersion;

        public List<string> Items { get; set; }
    }

    [Obsolete]
    public static class PlaylistV1Extensions
    {
        public static Playlist ToPlaylist(this PlaylistV1 self)
        {
            return new Playlist(self.Items);
        }
    }
}
