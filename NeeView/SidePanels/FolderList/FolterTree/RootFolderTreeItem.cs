using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace NeeView
{
    // root folder
    public class RootFolderTreeItem : FolderTreeNode, IDisposable
    {
        public RootFolderTreeItem()
        {
            IsExpanded = true;
        }

        public string Name => "PC";

        public override string Key => "";


        public override void RefreshChildren()
        {
            Children = new ObservableCollection<ITreeViewNode>(DriveInfo.GetDrives()
                .Select(e => new DriveTreeItem(e)));
        }

        /// <summary>
        /// 指定パスまで展開した状態で初期化する
        /// </summary>
        public void SyncFolder(string path)
        {
            this.RefreshChildren();

            if (path != null)
            {
                var node = GetFolderTreeNode(path, true, true) as FolderTreeItem;
                if (node != null)
                {
                    var parent = node.Parent;
                    while (parent != null)
                    {
                        parent.IsExpanded = true;
                        parent = parent.Parent;
                    }

                    node.IsSelected = true;
                    this.IsExpanded = true;
                }
            }
        }


        public void Terminate()
        {
            if (_children != null)
            {
                foreach (var child in _children)
                {
                    if (child is FolderTreeItem folder)
                    {
                        folder.Parent = null;
                    }
                    if (child is DriveTreeItem drive)
                    {
                        drive.Dispose();
                    }
                }

                Children = null;
            }
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Terminate();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

    }
}
