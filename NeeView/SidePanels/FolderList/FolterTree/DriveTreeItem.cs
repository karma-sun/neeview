using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

namespace NeeView
{
    // drive
    public class DriveTreeItem : FolderTreeItem, IDisposable
    {
        private FileSystemWatcher _fileSystemWatcher;

        private static readonly Dictionary<DriveType, string> _driveTypeNames = new Dictionary<DriveType, string>
        {
            [DriveType.Unknown] = "",
            [DriveType.NoRootDirectory] = "",
            [DriveType.Removable] = Properties.Resources.WordRemovableDrive,
            [DriveType.Fixed] = Properties.Resources.WordFixedDrive,
            [DriveType.Network] = Properties.Resources.WordNetworkDrive,
            [DriveType.CDRom] = Properties.Resources.WordCDRomDrive,
            [DriveType.Ram] = Properties.Resources.WordRamDrive,
        };

        public DriveTreeItem(RootFolderTreeItem parent, DriveInfo drive) : base(null, drive.Name)
        {
            Parent = parent;

            try
            {
                if (drive.IsReady)
                {
                    DriveName = string.Format("{0} ({1})", string.IsNullOrEmpty(drive.VolumeLabel) ? _driveTypeNames[drive.DriveType] : drive.VolumeLabel, drive.Name.TrimEnd('\\'));

                    InitializeWatcher(drive.Name);
                    StartWatch();
                }
                else
                {
                    DriveName = string.Format("{0} ({1})", _driveTypeNames[drive.DriveType], drive.Name.TrimEnd('\\'));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                DriveName = string.Format("{0} ({1})", _driveTypeNames[drive.DriveType], drive.Name.TrimEnd('\\'));
            }

            if (drive.DriveType != DriveType.Fixed)
            {
                DelayCreateChildren();
            }

        }

        public string DriveName { get; private set; }

        public override string Key => Name.TrimEnd(LoosePath.Separator);


        #region FilesSystemWatcher

        private void InitializeWatcher(string path)
        {
            _fileSystemWatcher = new FileSystemWatcher();

            try
            {
                _fileSystemWatcher.Path = path;
                _fileSystemWatcher.IncludeSubdirectories = true;
                _fileSystemWatcher.NotifyFilter = NotifyFilters.DirectoryName;
                _fileSystemWatcher.Created += Watcher_Creaded;
                _fileSystemWatcher.Deleted += Watcher_Deleted;
                _fileSystemWatcher.Renamed += Watcher_Renamed;
                _fileSystemWatcher.Error += Watcher_Error;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                _fileSystemWatcher.Dispose();
                _fileSystemWatcher = null;
            }
        }

        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            var ex = e.GetException();
            Debug.WriteLine($"FileSystemWatcher Error!! : {ex.ToString()} : {ex.Message}");

            // recoverty...
            ////var path = _fileSystemWatcher.Path;
            ////TerminateWatcher();
            ////InitializeWatcher(path);
        }

        private void TerminateWatcher()
        {
            if (_fileSystemWatcher != null)
            {
                _fileSystemWatcher.EnableRaisingEvents = false;
                _fileSystemWatcher.Error -= Watcher_Error;
                _fileSystemWatcher.Created -= Watcher_Creaded;
                _fileSystemWatcher.Deleted -= Watcher_Deleted;
                _fileSystemWatcher.Renamed -= Watcher_Renamed;
                _fileSystemWatcher.Dispose();
                _fileSystemWatcher = null;
            }
        }

        private void StartWatch()
        {
            if (_fileSystemWatcher != null)
            {
                _fileSystemWatcher.EnableRaisingEvents = true;
            }
        }

        private void Watcher_Creaded(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine("Create: " + e.FullPath);

            var path = LoosePath.GetDirectoryName(e.FullPath);

            var parent = GetFolderTreeItem(path);
            if (parent != null)
            {
                var name = LoosePath.GetFileName(e.FullPath);
                App.Current.Dispatcher.BeginInvoke((Action)(() => parent.Add(name)));
            }
            else
            {
                Debug.WriteLine("Skip create");
            }
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine("Delete: " + e.FullPath);

            var path = LoosePath.GetDirectoryName(e.FullPath);

            var parent = GetFolderTreeItem(path);
            if (parent != null)
            {
                var name = LoosePath.GetFileName(e.FullPath);
                App.Current.Dispatcher.BeginInvoke((Action)(() => parent.Remove(name)));
            }
            else
            {
                Debug.WriteLine("Skip delete");
            }
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            Debug.WriteLine("Rename: " + e.OldFullPath + " -> " + e.FullPath);

            var path = LoosePath.GetDirectoryName(e.OldFullPath);

            var parent = GetFolderTreeItem(path);
            if (parent != null)
            {
                var oldName = LoosePath.GetFileName(e.OldFullPath);
                var name = LoosePath.GetFileName(e.FullPath);
                App.Current.Dispatcher.BeginInvoke((Action)(() => parent.Rename(oldName, name)));
            }
            else
            {
                Debug.WriteLine("Skip rename");
            }
        }

        private FolderTreeItem GetFolderTreeItem(string path)
        {
            return Parent?.GetFolderTreeNode(path, false, false) as FolderTreeItem;
        }

        #endregion

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Parent = null;
                    TerminateWatcher();
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
