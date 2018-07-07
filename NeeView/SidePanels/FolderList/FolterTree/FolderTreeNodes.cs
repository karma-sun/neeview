using NeeLaboratory.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NeeView
{
    #region TreeViewNodeBase

    public interface ITreeViewNode
    {
        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }
    }

    /// <summary>
    /// TreeViewNode基底.
    /// Childrenの遅延生成に対応
    /// </summary>
    public class TreeViewNodeBase : BindableBase, ITreeViewNode
    {
        private bool _isChildrenInitialized;

        public TreeViewNodeBase()
        {
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { SetProperty(ref _isExpanded, value); }
        }

        protected ObservableCollection<ITreeViewNode> _children;
        public virtual ObservableCollection<ITreeViewNode> Children
        {
            get
            {
                if (!_isChildrenInitialized)
                {
                    _isChildrenInitialized = true;
                    Task.Run(() => RefreshChildren());
                }
                return _children;
            }
            set
            {
                _isChildrenInitialized = true;
                SetProperty(ref _children, value);
            }
        }

        public bool IsChildrenValid => _children != null;

        protected virtual void RefreshChildren()
        {
        }
    }

    #endregion

    #region QuickAccesTree

    // root Quick Access
    public class RootQuickAccessTreeItem : BindableBase, ITreeViewNode
    {
        public RootQuickAccessTreeItem()
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

#endregion

}
