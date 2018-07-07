using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace NeeView
{
    // root folder
    public class RootFolderTreeItem : TreeViewNodeBase, IDisposable
    {
        public RootFolderTreeItem()
        {
            IsExpanded = true;
        }

        public string Name => "PC";

        protected override void RefreshChildren()
        {
            Children = new ObservableCollection<ITreeViewNode>(DriveInfo.GetDrives()
                .Select(e => new DriveTreeItem(e)));
        }

        public void Terminate()
        {
            if (_children != null)
            {
                foreach(var child in _children)
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
