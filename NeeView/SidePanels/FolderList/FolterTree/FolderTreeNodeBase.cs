using NeeLaboratory.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

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

        private static ObservableCollection<IFolderTreeNode> _dummyChildren = new ObservableCollection<IFolderTreeNode>() { new DummyNode() };

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
                    RefreshChildren(true);
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
                    Task.Run(() => RefreshChildren(false));
                }
                return _delayCreateChildren ? _dummyChildren : _children;
            }
            set
            {
                _isChildrenInitialized = true;
                _delayCreateChildren = false;
                SetProperty(ref _children, value);
            }
        }

        public ObservableCollection<IFolderTreeNode> ChildrenRaw => _children;

        public abstract string DispName { get; set; }

        public abstract ImageSource Icon { get; }

        public virtual void RefreshIcon()
        {
            RaisePropertyChanged(nameof(Icon));
        }

        public void ResetChildren(bool isDelay)
        {
            IsExpanded = false;

            _isChildrenInitialized = false;
            _delayCreateChildren = isDelay;
            _children = null;
            RaisePropertyChanged(nameof(Children));
        }

        /// <param name="isForce">falseの場合、生成失敗ののときはExpandされるまで遅延させる</param>
        public abstract void RefreshChildren(bool isForce);

        public void DelayCreateChildren()
        {
            IsExpanded = false;
            _isChildrenInitialized = false;
            _delayCreateChildren = true;
        }
    }

    // dummy node
    public class DummyNode : IFolderTreeNode
    {
        public bool IsSelected { get => false; set { } }
        public bool IsExpanded { get => false; set { } }

        public ObservableCollection<IFolderTreeNode> Children { get => null; set { } }

        public string DispName => null;
        public ImageSource Icon => null;

        public void RefreshIcon()
        {
        }
    }
}

