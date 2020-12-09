namespace NeeView
{
    public class BookshelfFolderListPresenter : FolderListPresenter
    {
        private FolderListView _folderListView;
        private BookshelfFolderList _folderList;


        public BookshelfFolderListPresenter(FolderListView folderListView, BookshelfFolderList folderList) : base(folderListView, folderList)
        {
            _folderListView = folderListView;
            _folderList = folderList;

            folderListView.SearchBoxFocusChanged += FolderListView_SearchBoxFocusChanged;
        }


        public FolderListView FolderListView => _folderListView;

        public BookshelfFolderList FolderList => _folderList;


        private void FolderListView_SearchBoxFocusChanged(object sender, FocusChangedEventArgs e)
        {
            // リストのフォーカス更新を停止
            FolderListBox.IsFocusEnabled = !e.IsFocused;
        }

    }
}
