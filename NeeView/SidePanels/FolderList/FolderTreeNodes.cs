using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

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

        protected ObservableCollection<ITreeViewNode> _children;
        public virtual ObservableCollection<ITreeViewNode> Children
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

    // folder
    public class FolderTreeItem : TreeViewNodeBase
    {
        public FolderTreeItem(FolderTreeItem parent, string name)
        {
            Parent = parent;
            Name = name;
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value.TrimEnd('\\').Contains('\\'))
                {
                    Debugger.Break();
                }
                SetProperty(ref _name, value);
            }
        }

        public FolderTreeItem Parent { get; set; }

        public string Path => Parent != null ? LoosePath.Combine(Parent.Path, Name) : Name;

        internal static partial class NativeMethods
        {
            [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
            public static extern int StrCmpLogicalW(string psz1, string psz2);
        }

        protected override void RefreshChildren()
        {
            var directory = new DirectoryInfo(Path);

            try
            {
                Children = new ObservableCollection<ITreeViewNode>(directory.GetDirectories()
                    .Where(e => (e.Attributes & FileAttributes.Hidden) == 0)
                    .Select(e => new FolderTreeItem(this, e.Name)));
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }


        /// <summary>
        /// 現在インスタンス化されている指定パスのFolderTreeItemを取得する
        /// </summary>
        public FolderTreeItem GetFolderTreeItem(string path)
        {
            var tokens = path.TrimEnd(LoosePath.Separator).Split(LoosePath.Separator);
            return GetFolderTreeItem(tokens, 0);
        }

        /// <summary>
        /// 現在インスタンス化されている指定パスのFolderTreeItemを取得する
        /// </summary>
        public FolderTreeItem GetFolderTreeItem(string[] tokens, int depth)
        {
            if (tokens == null) throw new ArgumentNullException(nameof(tokens));

            if (tokens.Length - 1 < depth || Name.TrimEnd(LoosePath.Separator) != tokens[depth])
            {
                return null;
            }

            if (tokens.Length - 1 == depth)
            {
                return this;
            }

            var folder = _children?.Cast<FolderTreeItem>().FirstOrDefault(e => e.Name.TrimEnd(LoosePath.Separator) == tokens[depth + 1]);
            if (folder != null)
            {
                return folder.GetFolderTreeItem(tokens, depth + 1);
            }

            return null;
        }

        public void Add(string name)
        {
            if (_children == null) return;

            Debug.WriteLine($"ADD: " + name);

            var folder = new FolderTreeItem(this, name);
            _children.Add(folder);
            Sort(folder);
        }

        public void Remove(string name)
        {
            if (_children == null) return;

            var folder = _children.Cast<FolderTreeItem>().FirstOrDefault(e => e.Name == name);
            if (folder != null)
            {
                _children.Remove(folder);
                folder.Parent = null;
            }
        }
        
        public void Rename(string oldName, string name)
        {
            if (_children == null) return;

            Debug.WriteLine($"RENAME: " + oldName + " -> " + name);

            var folder = _children.Cast<FolderTreeItem>().FirstOrDefault(e => e.Name == oldName);
            if (folder != null)
            {
                folder.Name = name;
                Sort(folder);
            }
        }

        /// <summary>
        /// 指定した子を適切な位置に並び替える
        /// </summary>
        /// <param name="child"></param>
        private void Sort(FolderTreeItem child)
        {
            if (_children == null) return;

            var oldIndex = _children.IndexOf(child);
            if (oldIndex < 0) return;

            for (int index=0; index<_children.Count; ++index)
            {
                var folder = (FolderTreeItem)_children[index];
                if (folder == child) continue;

                if (NativeMethods.StrCmpLogicalW(child.Name, folder.Name) < 0)
                {
                    if (oldIndex != index - 1)
                    {
                        _children.Move(oldIndex, index);
                    }
                    return;
                }
            }

        }
    }

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

    #endregion
}
