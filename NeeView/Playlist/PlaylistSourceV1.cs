using System;
using System.Collections.Generic;

namespace NeeView
{
    [Obsolete]
    public class PlaylistSourceV1
    {
        public const string FormatVersion = "NeeViewPlaylist.1";

        public PlaylistSourceV1()
        {
            Items = new List<string>();
        }

        public PlaylistSourceV1(IEnumerable<string> items)
        {
            Items = new List<string>(items);
        }

        public string Format { get; set; } = FormatVersion;

        public List<string> Items { get; set; }
    }

    [Obsolete]
    public static class PlaylistV1Extensions
    {
        public static PlaylistSource ToPlaylist(this PlaylistSourceV1 self)
        {
            return new PlaylistSource(self.Items);
        }
    }
}
