using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace NeeView
{
    // folder
    public class FolderTreeItem : FolderTreeNode
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

        public override string Key => Name;

        public string Path => Parent is FolderTreeItem parent ? LoosePath.Combine(parent.Path, Name) : Name;

        public BitmapSource Icon => FileIconCollection.Current.CreateFileIcon(Path, IO.FileIconType.File, 16.0, false, false);


        internal static partial class NativeMethods
        {
            [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
            public static extern int StrCmpLogicalW(string psz1, string psz2);
        }

        public override void RefreshChildren()
        {
            var directory = new DirectoryInfo(Path);

            try
            {
                Children = new ObservableCollection<ITreeViewNode>(directory.GetDirectories()
                    .Where(e => (e.Attributes & FileAttributes.Hidden) == 0)
                    .Select(e => new FolderTreeItem(this, e.Name)));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                OnException(this, ex);
            }
        }

        public void RefreshIcon()
        {
            RaisePropertyChanged(nameof(Icon));
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

            for (int index = 0; index < _children.Count; ++index)
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
}
