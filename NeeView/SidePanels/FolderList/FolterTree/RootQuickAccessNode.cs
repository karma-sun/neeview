using NeeLaboratory.ComponentModel;

namespace NeeView
{
    public class RootQuickAccessNode : BindableBase, IFolderTreeNode
    {
        public RootQuickAccessNode()
        {
        }

        public string Name { set; get; } = Properties.Resources.WordQuickAccess;

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        private bool _IsExpanded = true;
        public bool IsExpanded
        {
            get { return _IsExpanded; }
            set { SetProperty(ref _IsExpanded, value); }
        }

        public QuickAccessCollection Collection => QuickAccessCollection.Current;
    }
}
