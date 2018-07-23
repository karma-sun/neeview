using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace NeeView
{
    public class RootDirectoryNode : FolderTreeNodeDelayBase
    {
        public RootDirectoryNode()
        {
            WindowMessage.Current.DriveChanged += WindowMessage_DriveChanged;
            WindowMessage.Current.MediaChanged += WindowMessage_MediaChanged;
            WindowMessage.Current.DirectoryChanged += WindowMessage_DirectoryChanged;
        }


        public override string Name { get => QueryScheme.File.ToSchemeString(); set { } }

        public override string DispName { get => "PC"; set { } }

        public override ImageSource Icon => MainWindow.Current.Resources["ic_desktop_windows_24px"] as ImageSource;


        public void Refresh()
        {
            this.CreateChildren(true);
            this.IsExpanded = true;
        }

        public void RefreshDriveChildren()
        {
            if (_children != null)
            {
                foreach (var child in _children)
                {
                    child.RefreshChildren();
                }
            }
        }

        public override void CreateChildren(bool isForce)
        {
            try
            {
                Children = new ObservableCollection<FolderTreeNodeBase>(DriveInfo.GetDrives()
                    .Select(e => new DriveDirectoryNode(e, this)));
            }
            catch (Exception ex)
            {
                FolderTreeModel.Current.ShowToast(ex.Message);
            }
        }

        private void WindowMessage_DriveChanged(object sender, DriveChangedEventArgs e)
        {
            if (_children == null) return;

            ////Debug.WriteLine($"DriveChange: {e.Name}, {e.IsAlive}");

            App.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    var driveInfo = CreateDriveInfo(e.Name);

                    if (e.IsAlive)
                    {
                        if (driveInfo != null)
                        {
                            AddDrive(driveInfo);
                        }
                    }
                    else
                    {
                        var name = e.Name.TrimEnd(LoosePath.Separator);

                        var drive = _children.Cast<DriveDirectoryNode>().FirstOrDefault(d => d.Name == name);
                        if (drive != null)
                        {
                            if (driveInfo == null)
                            {
                                _children.Remove(drive);
                            }
                            else
                            {
                                drive.Refresh();
                            }
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

            var name = driveInfo.Name.TrimEnd(LoosePath.Separator);

            var drive = _children.Cast<DriveDirectoryNode>().FirstOrDefault(d => d.Name == name);
            if (drive != null)
            {
                drive.Refresh();
                return;
            }

            for (int index = 0; index < _children.Count; ++index)
            {
                if (string.Compare(name, _children[index].Name) < 0)
                {
                    _children.Insert(index, new DriveDirectoryNode(driveInfo, this));
                    break;
                }

                if (index == _children.Count - 1)
                {
                    _children.Add(new DriveDirectoryNode(driveInfo, this));
                    break;
                }
            }
        }

        private DriveInfo CreateDriveInfo(string name)
        {
            Debug.Assert(name.EndsWith("\\"));

            if (System.IO.Directory.GetLogicalDrives().Contains(name))
            {
                return new DriveInfo(name);
            }

            return null;
        }

        private void WindowMessage_MediaChanged(object sender, MediaChangedEventArgs e)
        {
            if (_children == null) return;

            ////Debug.WriteLine($"MediaChange: {e.Name}, {e.IsAlive}");

            App.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    var name = e.Name.TrimEnd(LoosePath.Separator);

                    var drive = _children.Cast<DriveDirectoryNode>().FirstOrDefault(d => d.Name == name);
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
                var node = new DirectoryNode(name, null);
                App.Current.Dispatcher.BeginInvoke((Action)(() => parent.Add(node)));
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
            return GetFolderTreeNode(path, false, false) as DirectoryNode;
        }
    }
}
