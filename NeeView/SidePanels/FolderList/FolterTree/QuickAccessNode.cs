using NeeLaboratory.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace NeeView
{
    public class QuickAccessNode : BindableBase, IFolderTreeNode
    {
        public QuickAccessNode(QuickAccess quickAccess)
        {
            QuickAccess = quickAccess;
        }

        public QuickAccess QuickAccess { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public bool IsExpanded
        {
            get { return false; }
            set { }
        }

        public string DispName => QuickAccess.Name;

        public ImageSource Icon => FileIconCollection.Current.CreateDefaultFolderIcon(16.0);

        public ObservableCollection<IFolderTreeNode> Children { get => null; set { } }

        public void RefreshIcon()
        {
            RaisePropertyChanged(nameof(Icon));
        }
    }
}
