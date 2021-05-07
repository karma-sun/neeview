using System;

namespace NeeView
{
    public class PlaylistItemRenamedEventArgs : EventArgs
    {
        public PlaylistItemRenamedEventArgs(PlaylistItem item, string oldName)
        {
            Item = item;
            OldName = oldName;
        }

        public PlaylistItem Item { get; private set; }
        public string OldName { get; private set; }
    }
}
