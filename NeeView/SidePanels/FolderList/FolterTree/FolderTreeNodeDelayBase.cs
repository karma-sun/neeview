using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// TreeViewNode基底.
    /// Childrenの遅延生成に対応
    /// </summary>
    public abstract class FolderTreeNodeDelayBase : FolderTreeNodeBase
    {
        private static readonly ObservableCollection<FolderTreeNodeBase> _dummyChildren = new ObservableCollection<FolderTreeNodeBase>() { new DummyNode() };


        public FolderTreeNodeDelayBase()
        {
        }

        
        /// <summary>Expandのタイミングまで子供の生成を遅らせる</summary>
        public bool IsDelayCreation { get; set; }

        private bool _isExpanded;
        public override bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                SetProperty(ref _isExpanded, value);
                if (_children == null && _isExpanded == true)
                {
                    CreateChildren(true);
                }
            }
        }

        public override ObservableCollection<FolderTreeNodeBase> Children
        {
            get
            {
                if (_children == null &&  !IsDelayCreation)
                {
                    CreateChildren(false);
                }
                return _children ?? _dummyChildren;
            }
            set
            {
                SetProperty(ref _children, value);
            }
        }

        /// <param name="isForce">falseの場合、生成失敗ののときはExpandされるまで遅延させる</param>
        public abstract void CreateChildren(bool isForce);

        public override void RefreshChildren()
        {
            base.RefreshChildren();
        }

        protected override void RealizeChildren()
        {
            CreateChildren(true);
        }

        public override string ToString()
        {
            return base.ToString() + " Name:" + Name;
        }
    }


    /// <summary>
    /// ダミーノード
    /// </summary>
    public class DummyNode : FolderTreeNodeBase
    {
        public override string Name { get => null; set { } }
        public override string DispName { get => null; set { } }

        public override ImageSource Icon => null;
    }
}

