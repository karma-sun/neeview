namespace NeeView
{
    public class BookshelfItemAccessor
    {
        private FolderItem _source;

        public BookshelfItemAccessor(FolderItem source)
        {
            _source = source;
        }

        internal FolderItem Source => _source;

        public string Name => _source.DispName;

        public string Path => _source.TargetPath.SimplePath;
    }
}
