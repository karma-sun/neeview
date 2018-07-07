using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

namespace NeeView
{
    // drive
    public class DriveTreeItem : FolderTreeItem, IDisposable
    {
        private FileSystemWatcher _fileSystemWatcher;


        public DriveTreeItem(DriveInfo drive) : base(null, drive.Name)
        {
            DriveName = (string.IsNullOrEmpty(drive.VolumeLabel) ? Properties.Resources.WordLocalDisk : drive.VolumeLabel) + " (" + drive.Name.TrimEnd('\\') + ")";

            InitializeWatcher(drive.Name);
            StartWatch();
        }

        public string DriveName { get; private set; }

        ///public BitmapSource Icon => FileIconCollection.Current.CreateFileIcon(Path, IO.FileIconType.File, 16.0, false, false);


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
        #endregion

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
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
