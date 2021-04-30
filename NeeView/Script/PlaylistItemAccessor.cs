namespace NeeView
{
    public class PlaylistItemAccessor
    {
        private PlaylistListBoxItem _source;

        public PlaylistItemAccessor(PlaylistListBoxItem source)
        {
            _source = source;
        }

        internal PlaylistListBoxItem Source => _source;


        [WordNodeMember]
        public string Name => _source.Name;

        [WordNodeMember]
        public string Path => _source.Path;
    }

}
