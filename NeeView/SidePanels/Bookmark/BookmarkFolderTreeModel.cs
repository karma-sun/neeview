namespace NeeView
{
    /// <summary>
    /// Bookshelf の FolderTreeModel
    /// </summary>
    public class BookmarkFolderTreeModel : FolderTreeModel
    {
        public static BookmarkFolderTreeModel Current { get; private set; }

        public BookmarkFolderTreeModel(FolderList folderList) : base(folderList, FolderTreeCategory.BookmarkFolder)
        {
            Current = this;
        }
    }
}
