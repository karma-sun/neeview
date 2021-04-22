using System.Collections.Generic;

namespace NeeView
{
    public class PlaylistListBoxItemCollection : List<PlaylistListBoxItem>
    {
        public static readonly string Format = FormatVersion.CreateFormatName(Environment.ProcessId.ToString(), nameof(PlaylistListBoxItemCollection));

        public PlaylistListBoxItemCollection()
        {
        }

        public PlaylistListBoxItemCollection(IEnumerable<PlaylistListBoxItem> collection) : base(collection)
        {
        }
    }
}
