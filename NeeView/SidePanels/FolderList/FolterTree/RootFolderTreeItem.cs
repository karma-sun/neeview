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

            ContentRebuild.Current.DeviceChanged += (s, e) => UpdateDrives();
        }

        public string Name => "PC";

        public override string Key => null;

        public void Refresh()
        {
            this.RefreshChildren();
            this.IsExpanded = true;
        }

        public override void RefreshChildren()
        {
            if (_children != null)
            {
                foreach (var disposable in _children.OfType<IDisposable>())
                {
                    disposable.Dispose();
                }
            }

            Children = new ObservableCollection<ITreeViewNode>(DriveInfo.GetDrives()
                .Select(e => new DriveTreeItem(this, e)));
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

        //
        public void UpdateDrives()
        {
            if (_children == null) return;

            var drives = DriveInfo.GetDrives();

            // removes
            var removes = _children.Cast<DriveTreeItem>().Where(e => !drives.Any(d => d.Name == e.Name)).ToList();
            foreach (var item in removes)
            {
                _children.Remove(item);
                if (item is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            // add
            var adds = drives.Where(e => !_children.Cast<DriveTreeItem>().Any(d => d.Name == e.Name));
            foreach (var drive in adds)
            {
                for (int index = 0; index < _children.Count; ++index)
                {
                    if (string.Compare(drive.Name, ((DriveTreeItem)_children[index]).Name) < 0)
                    {
                        _children.Insert(index, new DriveTreeItem(this, drive));
                        break;
                    }

                    if (index == _children.Count - 1)
                    {
                        _children.Add(new DriveTreeItem(this, drive));
                        break;
                    }
                }
            }
        }

        protected override void OnException(FolderTreeNode sender, Exception e)
        {
            if (sender is FolderTreeItem folder)
            {
                // 所属しているドライブを更新
                // TODO ...
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
