namespace NeeView
{
    public class BookshelfFolderListPresenter : FolderListPresenter
    {
        public BookshelfFolderListPresenter(FolderListView folderListView, FolderList folderList) : base(folderListView, folderList)
        {
            folderListView.SearchBoxFocusChanged += FolderListView_SearchBoxFocusChanged;
        }

        private void FolderListView_SearchBoxFocusChanged(object sender, FocusChangedEventArgs e)
        {
            // リストのフォーカス更新を停止
            FolderListBox.IsFocusEnabled = !e.IsFocused;
        }
    }
}
