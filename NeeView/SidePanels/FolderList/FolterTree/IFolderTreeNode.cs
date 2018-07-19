using System.Collections.ObjectModel;
using System.Windows.Media;

namespace NeeView
{
    public interface IFolderTreeNode
    {
        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }

        ObservableCollection<IFolderTreeNode> Children { get; set; }

        string DispName { get; }
        ImageSource Icon { get; }

        void RefreshIcon();
    }
}
