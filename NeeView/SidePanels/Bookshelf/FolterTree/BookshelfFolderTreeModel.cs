namespace NeeView
{
    /// <summary>
    /// Bookshelf の FolderTreeModel
    /// </summary>
    public class BookshelfFolderTreeModel : FolderTreeModel
    {
        public static BookshelfFolderTreeModel Current { get; private set; }

        public BookshelfFolderTreeModel(FolderList folderList) : base(folderList)
        {
            Current = this;
        }
    }
}
