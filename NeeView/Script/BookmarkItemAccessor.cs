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

        public string Name => _source.DispName;

        public string Path => _source.TargetPath.SimplePath;
    }
}
