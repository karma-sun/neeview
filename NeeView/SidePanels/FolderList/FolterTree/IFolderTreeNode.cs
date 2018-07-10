namespace NeeView
{
    public interface IFolderTreeNode
    {
        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }
    }
}
