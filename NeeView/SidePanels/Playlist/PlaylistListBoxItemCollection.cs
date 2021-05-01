using System.Collections.Generic;

namespace NeeView
{
    public class PlaylistListBoxItemCollection : List<PlaylistItem>
    {
        public static readonly string Format = FormatVersion.CreateFormatName(Environment.ProcessId.ToString(), nameof(PlaylistListBoxItemCollection));

        public PlaylistListBoxItemCollection()
        {
        }

        public PlaylistListBoxItemCollection(IEnumerable<PlaylistItem> collection) : base(collection)
        {
        }
    }
}
