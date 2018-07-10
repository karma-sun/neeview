using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class DirectoryNode : DirectoryNodeBase
    {
        internal static partial class NativeMethods
        {
            [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
            public static extern int StrCmpLogicalW(string psz1, string psz2);
        }

        public DirectoryNode(DirectoryNode parent, string name)
        {
            Parent = parent;
            Name = name;
        }

        public DirectoryNode(DirectoryNode parent, string name, bool isDelayAll) : this(parent, name)
        { 
            if (isDelayAll)
            {
                IsDelayCreateChildremAll = true;
                DelayCreateChildren();
            }
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

        public override string Key => Name;

        public DirectoryNode Parent { get; set; }

        public string Path => Parent != null ? LoosePath.Combine(Parent.Path, Name) : Name;

        public DriveDirectoryNode Drive => this is DriveDirectoryNode drive ? drive : Parent?.Drive;

        public BitmapSource Icon => FileIconCollection.Current.CreateFileIcon(Path, IO.FileIconType.Directory, 16.0, false, false);

        public bool IsDelayCreateChildremAll { get; set; }



        protected virtual void OnException(DirectoryNode sender, NotifyCrateDirectoryChildrenExcepionEventArgs e)
        {
            Parent?.OnException(sender, e);
        }

        public override void RefreshChildren(bool isForce)
        {
            var directory = new DirectoryInfo(Path);

            try
            {
                Children = new ObservableCollection<IFolderTreeNode>(directory.GetDirectories()
                    .Where(e => (e.Attributes & FileAttributes.Hidden) == 0)
                    .Select(e => new DirectoryNode(this, e.Name, IsDelayCreateChildremAll)));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                DelayCreateChildren();

                if (isForce)
                {
                    var isDriveRefresh = !(ex is UnauthorizedAccessException || ex is System.Security.SecurityException);
                    OnException(this, new NotifyCrateDirectoryChildrenExcepionEventArgs(ex, isDriveRefresh));
                }
            }
        }

        public void RefreshIcon()
        {
            RaisePropertyChanged(nameof(Icon));
        }

        public void Add(string name)
        {
            if (_children == null) return;

            ////Debug.WriteLine($"ADD: " + name);

            var directory = _children.Cast<DirectoryNode>().FirstOrDefault(e => e.Name == name);
            if (directory == null)
            {
                directory = new DirectoryNode(this, name, IsDelayCreateChildremAll);
                _children.Add(directory);
                Sort(directory);
            }
        }

        public void Remove(string name)
        {
            if (_children == null) return;

            ////Debug.WriteLine($"REMOVE: " + name);

            var directory = _children.Cast<DirectoryNode>().FirstOrDefault(e => e.Name == name);
            if (directory != null)
            {
                _children.Remove(directory);
                directory.Parent = null;
            }
        }

        public void Rename(string oldName, string name)
        {
            if (_children == null) return;

            ////Debug.WriteLine($"RENAME: " + oldName + " -> " + name);

            var directory = _children.Cast<DirectoryNode>().FirstOrDefault(e => e.Name == oldName);
            if (directory != null)
            {
                directory.Name = name;
                Sort(directory);
            }
        }

        /// <summary>
        /// 指定した子を適切な位置に並び替える
        /// </summary>
        /// <param name="child"></param>
        private void Sort(DirectoryNode child)
        {
            if (_children == null) return;

            var oldIndex = _children.IndexOf(child);
            if (oldIndex < 0) return;

            for (int index = 0; index < _children.Count; ++index)
            {
                var directory = (DirectoryNode)_children[index];
                if (directory == child) continue;

                if (NativeMethods.StrCmpLogicalW(child.Name, directory.Name) < 0)
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


    public class NotifyCrateDirectoryChildrenExcepionEventArgs : EventArgs
    {
        public NotifyCrateDirectoryChildrenExcepionEventArgs(Exception exception, bool isRefresh)
        {
            Exception = exception;
            IsRefresh = isRefresh;
        }

        public Exception Exception { get; set; }
        public bool IsRefresh { get; set; }
    }

}
