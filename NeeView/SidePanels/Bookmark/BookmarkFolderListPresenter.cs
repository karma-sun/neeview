namespace NeeView
{
    public class BookmarkFolderListPresenter : FolderListPresenter
    {
        private BookmarkListView _folderListView;
        private BookmarkFolderList _folderList;

        public BookmarkFolderListPresenter(BookmarkListView folderListView, BookmarkFolderList folderList) : base(folderListView, folderList)
        {
            _folderListView = folderListView;
            _folderList = folderList;
        }

        public BookmarkListView BookmarkListView => _folderListView;
        public BookmarkFolderList BookmarkFolderList => _folderList;

    }
}
