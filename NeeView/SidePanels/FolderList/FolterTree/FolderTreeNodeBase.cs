using NeeLaboratory.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// TreeViewNode基底.
    /// Childrenの遅延生成に対応
    /// </summary>
    public abstract class FolderTreeNodeBase : BindableBase, IFolderTreeNode
    {
        private bool _isChildrenInitialized;
        private bool _delayCreateChildren;

        public FolderTreeNodeBase()
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
            set
            {
                SetProperty(ref _isExpanded, value);
                if (_isExpanded == true && !_isChildrenInitialized)
                {
                    _isChildrenInitialized = true;
                    RefreshChildren();
                }
            }
        }

        protected ObservableCollection<IFolderTreeNode> _children;
        public virtual ObservableCollection<IFolderTreeNode> Children
        {
            get
            {
                if (!_isChildrenInitialized && !_delayCreateChildren)
                {
                    _isChildrenInitialized = true;
                    Task.Run(() => RefreshChildren());
                }
                return _children;
            }
            set
            {
                _isChildrenInitialized = true;
                _delayCreateChildren = false;
                SetProperty(ref _children, value);
            }
        }

        public bool IsChildrenValid => _children != null;

        public void ResetChildren(bool isDelay)
        {
            IsExpanded = false;

            _isChildrenInitialized = false;
            _delayCreateChildren = isDelay;
            _children = null;
            RaisePropertyChanged(nameof(Children));
        }

        public abstract void RefreshChildren();

        public void DelayCreateChildren()
        {
            if (!_isChildrenInitialized)
            {
                _delayCreateChildren = true;
            }
        }
    }
}
