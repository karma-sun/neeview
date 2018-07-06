using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private List<ITreeViewNode> _children;
        public virtual List<ITreeViewNode> Children
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

    #region FolderTree

    // root folder
    public class RootFolderTreeItem : TreeViewNodeBase
    {
        public RootFolderTreeItem()
        {
            IsExpanded = true;
        }

        public string Name => "PC";

        protected override void RefreshChildren()
        {
            Children = new List<ITreeViewNode>(DriveInfo.GetDrives()
                .Select(e => new DriveTreeItem(e)));
        }
    }

    // folder
    public class FolderTreeItem : TreeViewNodeBase
    {
        public FolderTreeItem(string path)
        {
            Path = path;
        }

        public string Path { get; set; }

        public virtual string Name => LoosePath.GetFileName(Path);

        protected override void RefreshChildren()
        {
            var directory = new DirectoryInfo(Path);

            Children = new List<ITreeViewNode>(directory.GetDirectories()
                .Where(e => (e.Attributes & FileAttributes.Hidden) == 0)
                .Select(e => new FolderTreeItem(e.FullName)));
        }
    }

    // drive
    public class DriveTreeItem : FolderTreeItem
    {
        private string _name;

        public DriveTreeItem(DriveInfo drive) : base(drive.Name)
        {
            _name = (string.IsNullOrEmpty(drive.VolumeLabel) ? Properties.Resources.WordLocalDisk : drive.VolumeLabel) + " (" + drive.Name.TrimEnd('\\') + ")";
        }

        public override string Name => _name;
    }

    #endregion
}
