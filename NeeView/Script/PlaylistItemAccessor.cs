namespace NeeView
{
    public class PlaylistItemAccessor
    {
        private PlaylistItem _source;

        public PlaylistItemAccessor(PlaylistItem source)
        {
            _source = source;
        }

        internal PlaylistItem Source => _source;


        [WordNodeMember]
        public string Name => _source.Name;

        [WordNodeMember]
        public string Path => _source.Path;
    }

}
