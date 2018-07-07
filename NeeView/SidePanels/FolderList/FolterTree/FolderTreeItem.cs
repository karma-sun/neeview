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

        public BitmapSource Icon => FileIconCollection.Current.CreateFileIcon(Path, IO.FileIconType.File, 16.0, false, false);


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

        public void RefreshIcon()
        {
            RaisePropertyChanged(nameof(Icon));
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
}
