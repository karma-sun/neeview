using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class FolderTreeModel : BindableBase
    {
        public static FolderTreeModel Current { get; } = new FolderTreeModel();

        private RootQuickAccessNode _rootQuickAccess;
        private RootDirectoryNode _rootFolder;
        private RootBookmarkFolderNode _rootBookmarkFolder;

        private Toast _toast;

        public FolderTreeModel()
        {
            _rootQuickAccess = new RootQuickAccessNode();
            _rootFolder = new RootDirectoryNode();
            _rootBookmarkFolder = new RootBookmarkFolderNode();

            Items = new List<IFolderTreeNode>();
            Items.Add(_rootQuickAccess);
            Items.Add(_rootFolder);
            Items.Add(_rootBookmarkFolder);

            Config.Current.DpiChanged += Config_DpiChanged;

            QuickAccessCollection.Current.CollectionChanged += QuickAccess_CollectionChanged;
        }

        public event EventHandler SelectedItemChanged;


        public List<IFolderTreeNode> Items { get; set; }

        public BitmapSource FolderIcon => FileIconCollection.Current.CreateDefaultFolderIcon(16.0);


        private static IEnumerable<IFolderTreeNode> GetNodeWalker(IEnumerable<IFolderTreeNode> collection)
        {
            if (collection == null)
            {
                yield break;
            }

            foreach (var item in collection)
            {
                yield return item;

                foreach (var child in GetNodeWalker(item.Children))
                {
                    yield return child;
                }

                switch (item)
                {
                    case FolderTreeNodeBase node:
                        if (node.ChildrenRaw != null)
                        {
                            foreach (var child in GetNodeWalker(node.ChildrenRaw))
                            {
                                yield return child;
                            }
                        }
                        break;

                    default:
                        foreach (var child in GetNodeWalker(item.Children))
                        {
                            yield return child;
                        }
                        break;
                }
            }
        }

        private void Config_DpiChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(FolderIcon));

            foreach (var item in GetNodeWalker(Items))
            {
                item.RefreshIcon();
            }
        }

        private void QuickAccess_CollectionChanged(object sender, System.ComponentModel.CollectionChangeEventArgs e)
        {
            if (e.Action == System.ComponentModel.CollectionChangeAction.Add)
            {
                if (e.Element is QuickAccessNode quickAccess)
                {
                    quickAccess.IsSelected = true;
                    SelectedItemChanged?.Invoke(this, null);
                }
            }
        }

        internal void ExpandRoot()
        {
            _rootQuickAccess.IsExpanded = true;
            _rootFolder.IsExpanded = true;
            _rootBookmarkFolder.IsExpanded = true;
        }

        public void SelectRootQuickAccess()
        {
            _rootQuickAccess.IsSelected = true;
        }

        public void Decide(object item)
        {
            switch (item)
            {
                case QuickAccessNode quickAccess:
                    SetFolderListPlace(quickAccess.QuickAccess.Path);
                    break;

                case RootDirectoryNode rootFolder:
                    SetFolderListPlace("");
                    break;

                case DriveDirectoryNode drive:
                    if (drive.IsReady)
                    {
                        SetFolderListPlace(drive.Path);
                    }
                    break;

                case DirectoryNode folder:
                    SetFolderListPlace(folder.Path);
                    break;

                case BookmarkFolderNode bookmarkFolder:
                    SetFolderListPlace(bookmarkFolder.Path);
                    break;
            }
        }

        private void SetFolderListPlace(string path)
        {
            // TODO: リクエストの重複がありうる。キャンセル処理が必要?
            FolderList.Current.RequestPlace(path, null, FolderSetPlaceOption.UpdateHistory | FolderSetPlaceOption.ResetKeyword);
        }


        public void AddQuickAccess(object item)
        {
            switch (item)
            {
                case RootQuickAccessNode rootQuickAccess:
                    AddQuickAccess(FolderList.Current.GetCurentQueryPath());
                    break;

                case DirectoryNode folder:
                    AddQuickAccess(folder.Path);
                    break;
            }
        }

        public void AddQuickAccess(string path)
        {
            _rootQuickAccess.IsExpanded = true;

            if (path.StartsWith(Temporary.TempDirectory))
            {
                ToastService.Current.Show(new Toast(Properties.Resources.DialogQuickAccessTempError));
                return;
            }

            var item = QuickAccessCollection.Current.Items.FirstOrDefault(e => e.Path == path);
            if (item != null)
            {
                var node = _rootQuickAccess.Children.FirstOrDefault(e => ((QuickAccessNode)e).QuickAccess == item);
                if (node != null)
                {
                    node.IsSelected = true;
                    SelectedItemChanged?.Invoke(this, null);
                }
                return;
            }

            QuickAccessCollection.Current.Add(new QuickAccess(path));
        }


        // TODO: IFolderTreeNode に Parent, Preview, Next を実装して一般化すべき？
        private void SelectNext(IFolderTreeNode parent, IFolderTreeNode item)
        {
            if (item == null) return;

            if (item.IsSelected)
            {
                var index = parent.Children.IndexOf(item);
                if (index + 1 < parent.Children.Count)
                {
                    parent.Children[index + 1].IsSelected = true;
                }
                else if (index - 1 >= 0)
                {
                    parent.Children[index - 1].IsSelected = true;
                }
            }
        }

        public void RemoveQuickAccess(QuickAccessNode item)
        {
            if (item == null)
            {
                return;
            }

            ////_rootQuickAccess.SelectNext(item);
            SelectNext(_rootQuickAccess, item);
            QuickAccessCollection.Current.Remove(item.QuickAccess);
        }

        public void RemoveBookmarkFolder(BookmarkFolderNode item)
        {
            if (item == null || item is RootBookmarkFolderNode)
            {
                return;
            }

            var next = item.Next ?? item.Previous ?? item.Parent;

            var memento = new TreeListNodeMemento<IBookmarkEntry>(item.Source);

            bool isRemoved = BookmarkCollection.Current.Remove(item.Source);
            if (isRemoved)
            {
                if (item.Source.Value is BookmarkFolder)
                {
                    var count = item.Source.Count(e => e.Value is Bookmark);
                    if (count > 0)
                    {
                        _toast = new Toast(string.Format(Properties.Resources.DialogPagemarkFolderDelete, count), Properties.Resources.WordRestore, () => BookmarkCollection.Current.Restore(memento));
                        ToastService.Current.Show(_toast);
                    }
                }

                if (next != null)
                {
                    next.IsSelected = true;
                }
            }
        }


        public BookmarkFolderNode NewBookmarkFolder(BookmarkFolderNode item)
        {
            if (item == null)
            {
                return null;
            }

            item.IsExpanded = true;

            var node = BookmarkCollection.Current.AddNewFolder(item.Source);
            if (node == null)
            {
                return null;
            }

            return item.Children.OfType<BookmarkFolderNode>().FirstOrDefault(e => e.Source == node);
        }


        public void MoveQuickAccess(QuickAccessNode src, QuickAccessNode dst)
        {
            if (src == dst)
            {
                return;
            }
            var srcIndex = QuickAccessCollection.Current.Items.IndexOf(src.QuickAccess);
            if (srcIndex < 0)
            {
                return;
            }
            var dstIndex = QuickAccessCollection.Current.Items.IndexOf(dst.QuickAccess);
            if (dstIndex < 0)
            {
                return;
            }
            QuickAccessCollection.Current.Move(srcIndex, dstIndex);
        }

        public void SyncDirectory(string place)
        {
            _rootFolder.SyncDirectory(place);
        }

        public void RefreshDirectory()
        {
            _rootFolder.Refresh();
        }

        public void ShowToast(string message)
        {
            _toast?.Cancel();
            _toast = new Toast(message);
            ToastService.Current.Show(_toast);
        }
    }
}
