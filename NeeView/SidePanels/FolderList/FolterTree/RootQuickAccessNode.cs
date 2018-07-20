using NeeLaboratory.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;

namespace NeeView
{

    public class RootQuickAccessNode : BindableBase, IFolderTreeNode
    {
        public RootQuickAccessNode()
        {
            RefreshChildren();
            QuickAccessCollection.Current.CollectionChanged += QuickAccessCollection_CollectionChanged;
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        private bool _IsExpanded;
        public bool IsExpanded
        {
            get { return _IsExpanded; }
            set { SetProperty(ref _IsExpanded, value); }
        }

        public string DispName => Properties.Resources.WordQuickAccess;

        public ImageSource Icon => MainWindow.Current.Resources["ic_pushpin"] as ImageSource;

        private ObservableCollection<IFolderTreeNode> _children;
        public ObservableCollection<IFolderTreeNode> Children
        {
            get { return _children; }
            set { SetProperty(ref _children, value); }
        }


        private void QuickAccessCollection_CollectionChanged(object sender, System.ComponentModel.CollectionChangeEventArgs e)
        {
            var item = e.Element as QuickAccess;

            switch (e.Action)
            {
                case CollectionChangeAction.Refresh:
                    RefreshChildren();
                    break;

                case CollectionChangeAction.Add:
                    var index = QuickAccessCollection.Current.Items.IndexOf(item);
                    InsertChild(index, item);
                    break;

                case CollectionChangeAction.Remove:
                    RemoveChild(item);
                    break;
            }
        }

        public void RefreshIcon()
        {
            RaisePropertyChanged(nameof(Icon));
        }

        private void RefreshChildren()
        {
            Children = new ObservableCollection<IFolderTreeNode>(QuickAccessCollection.Current.Items.Select(e => new QuickAccessNode(e)));
        }

        private void InsertChild(int index, QuickAccess item)
        {
            var node = new QuickAccessNode(item);
            Children.Insert(index, node);
        }

        private void RemoveChild(QuickAccess item)
        {
            var node = Children.FirstOrDefault(e => ((QuickAccessNode)e).QuickAccess == item);
            if (node != null)
            {
                Children.Remove(node);
            }
        }

        internal void SelectNext(QuickAccessNode item)
        {
            if (item == null) return;

            if (item.IsSelected)
            {
                var index = Children.IndexOf(item);
                if (index + 1 < Children.Count)
                {
                    Children[index + 1].IsSelected = true;
                }
                else if (index - 1 >= 0)
                {
                    Children[index - 1].IsSelected = true;
                }
            }
        }
    }
}
