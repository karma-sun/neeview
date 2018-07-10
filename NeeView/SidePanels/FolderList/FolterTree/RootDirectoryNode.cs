using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NeeView
{
    public class RootDirectoryNode : DirectoryNodeBase
    {
        public RootDirectoryNode()
        {
            IsExpanded = true;

            WindowMessage.Current.DriveChanged += WindowMessage_DriveChanged;
            WindowMessage.Current.MediaChanged += WindowMessage_MediaChanged;
            WindowMessage.Current.DirectoryChanged += WindowMessage_DirectoryChanged;
        }

        public string Name => "PC";

        public override string Key => null;

        public void Refresh()
        {
            this.RefreshChildren(true);
            this.IsExpanded = true;
        }

        public override void RefreshChildren(bool isForce)
        {
            try
            {
                Children = new ObservableCollection<IFolderTreeNode>(DriveInfo.GetDrives()
                    .Select(e => new DriveDirectoryNode(e)));
            }
            catch(Exception ex)
            {
                FolderTreeModel.Current.ShowToast(ex.Message);
            }
        }

        private void WindowMessage_DriveChanged(object sender, DriveChangedEventArgs e)
        {
            if (_children == null) return;

            App.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    if (e.IsAlive)
                    {
                        var driveInfo = DriveInfo.GetDrives().FirstOrDefault(d => d.Name == e.Name);
                        if (driveInfo != null)
                        {
                            AddDrive(driveInfo);
                        }
                    }
                    else
                    {
                        var drive = _children.Cast<DriveDirectoryNode>().FirstOrDefault(d => d.Name == e.Name);
                        if (drive != null)
                        {
                            _children.Remove(drive);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }));
        }

        private void AddDrive(DriveInfo driveInfo)
        {
            if (driveInfo == null) return;
            if (_children == null) return;

            var drive = _children.Cast<DriveDirectoryNode>().FirstOrDefault(d => d.Name == driveInfo.Name);
            if (drive != null)
            {
                drive.Refresh();
                return;
            }

            for (int index = 0; index < _children.Count; ++index)
            {
                if (string.Compare(driveInfo.Name, ((DriveDirectoryNode)_children[index]).Name) < 0)
                {
                    _children.Insert(index, new DriveDirectoryNode(driveInfo));
                    break;
                }

                if (index == _children.Count - 1)
                {
                    _children.Add(new DriveDirectoryNode(driveInfo));
                    break;
                }
            }
        }


        private void WindowMessage_MediaChanged(object sender, MediaChangedEventArgs e)
        {
            if (_children == null) return;

            App.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    var drive = _children.Cast<DriveDirectoryNode>().FirstOrDefault(d => d.Name == e.Name);
                    if (drive == null)
                    {
                        return;
                    }

                    drive.Refresh();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }));
        }


        private void WindowMessage_DirectoryChanged(object sender, DirectoryChangedEventArgs e)
        {
            App.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    switch (e.ChangeType)
                    {
                        case DirectoryChangeType.Created:
                            Directory_Creaded(e.FullPath);
                            break;
                        case DirectoryChangeType.Deleted:
                            Directory_Deleted(e.FullPath);
                            break;
                        case DirectoryChangeType.Renamed:
                            Directory_Renamed(e.OldFullPath, e.FullPath);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }));
        }

        private void Directory_Creaded(string fullpath)
        {
            ////Debug.WriteLine("Create: " + fullpath);

            var directory = LoosePath.GetDirectoryName(fullpath);

            var parent = GetDirectoryNode(directory);
            if (parent != null)
            {
                var name = LoosePath.GetFileName(fullpath);
                App.Current.Dispatcher.BeginInvoke((Action)(() => parent.Add(name)));
            }
            else
            {
                ////Debug.WriteLine("Skip create");
            }
        }

        private void Directory_Deleted(string fullpath)
        {
            ////Debug.WriteLine("Delete: " + fullpath);

            var directory = LoosePath.GetDirectoryName(fullpath);

            var parent = GetDirectoryNode(directory);
            if (parent != null)
            {
                var name = LoosePath.GetFileName(fullpath);
                App.Current.Dispatcher.BeginInvoke((Action)(() => parent.Remove(name)));
            }
            else
            {
                ////Debug.WriteLine("Skip delete");
            }
        }

        private void Directory_Renamed(string oldFullpath, string fullpath)
        {
            ////Debug.WriteLine("Rename: " + oldFullpath + " -> " + fullpath);

            var directory = LoosePath.GetDirectoryName(oldFullpath);

            var parent = GetDirectoryNode(directory);
            if (parent != null)
            {
                var oldName = LoosePath.GetFileName(oldFullpath);
                var name = LoosePath.GetFileName(fullpath);
                App.Current.Dispatcher.BeginInvoke((Action)(() => parent.Rename(oldName, name)));
            }
            else
            {
                ////Debug.WriteLine("Skip rename");
            }
        }

        private DirectoryNode GetDirectoryNode(string path)
        {
            return GetDirectoryNode(path, false, false) as DirectoryNode;
        }



        /// <summary>
        /// 指定パスまで展開した状態で初期化する
        /// </summary>
        public void SyncDirectory(string path)
        {
            this.RefreshChildren(true);

            if (path != null)
            {
                var node = GetDirectoryNode(path, true, true) as DirectoryNode;
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
    }
}
