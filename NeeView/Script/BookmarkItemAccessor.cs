namespace NeeView
{
    public class BookmarkItemAccessor
    {
        private FolderItem _source;

        public BookmarkItemAccessor(FolderItem source)
        {
            _source = source;
        }

        internal FolderItem Source => _source;

        [WordNodeMember]
        public string Name => _source.DispName;

        [WordNodeMember]
        public string Path => _source.TargetPath.SimplePath;
    }
}
