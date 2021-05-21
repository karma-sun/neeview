using System;
using System.Diagnostics;
using System.IO;

namespace NeeView.IO
{
    public class SingleFileWatcher : IDisposable
    {
        private string _path;
        private SingleFileWaterOptions _options;
        private FileSystemWatcher _watcher;
        private bool _disposedValue;

        public SingleFileWatcher(SingleFileWaterOptions options = SingleFileWaterOptions.None)
        {
            _options = options;
        }

        public event FileSystemEventHandler Changed;
        public event FileSystemEventHandler Deleted;
        public event RenamedEventHandler Renamed;


        public void Start(string path)
        {
            if (path == _path) return;

            Stop();

            if (string.IsNullOrEmpty(path) || !FileIO.Exists(path))
            {
                return;
            }

            ////Debug.WriteLine($"## WatchFile: {path}");
            _path = path;

            _watcher = new FileSystemWatcher();
            _watcher.Path = Path.GetDirectoryName(_path);
            _watcher.Filter = Path.GetFileName(_path);
            _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            _watcher.IncludeSubdirectories = false;
            _watcher.Changed += Watcher_Changed;
            _watcher.Deleted += Watcher_Deleted;
            _watcher.Renamed += Watcher_Renamed;

            _watcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            _watcher?.Dispose();
            _watcher = null;
        }


        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (Changed is null) return;

            AppDispatcher.BeginInvoke(() =>
            {
                Changed?.Invoke(sender, e);
            });
        }


        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if (Deleted is null) return;

            AppDispatcher.BeginInvoke(() =>
            {
                Deleted?.Invoke(sender, e);
            });
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (Renamed is null)
            {
                FollowRename(e.FullPath);
                return;
            }

            AppDispatcher.BeginInvoke(() =>
            {
                Renamed?.Invoke(sender, e);
                FollowRename(e.FullPath);
            });
        }

        private void FollowRename(string newPath)
        {
            if ((_options & SingleFileWaterOptions.FollowRename) == SingleFileWaterOptions.FollowRename)
            {
                Start(newPath);
            }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Stop();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
