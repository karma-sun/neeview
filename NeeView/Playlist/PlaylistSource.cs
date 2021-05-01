using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NeeView
{
    public class PlaylistSource
    {
        public static readonly FormatVersion FormatVersion = new FormatVersion("NeeView.Playlist", 2, 0, 0);

        public PlaylistSource()
        {
            Items = new List<PlaylistSourceItem>();
        }

        public PlaylistSource(IEnumerable<string> items)
        {
            Items = items.Select(e => new PlaylistSourceItem(e)).ToList();
        }

        public PlaylistSource(IEnumerable<PlaylistSourceItem> items)
        {
            Items = items.ToList();
        }

        public FormatVersion Format { get; set; } = FormatVersion;

        public List<PlaylistSourceItem> Items { get; set; }
    }

}
