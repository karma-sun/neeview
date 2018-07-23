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
        // Fields

        public static FolderTreeModel Current { get; } = new FolderTreeModel();

        private RootQuickAccessNode _rootQuickAccess;
        private RootDirectoryNode _rootFolder;
        private RootBookmarkFolderNode _rootBookmarkFolder;
        private Toast _toast;


        // Constructors

        public FolderTreeModel()
        {
            _rootQuickAccess = new RootQuickAccessNode();
            _rootFolder = new RootDirectoryNode();
            _rootBookmarkFolder = new RootBookmarkFolderNode();

            Items = new List<FolderTreeNodeBase>();
            Items.Add(_rootQuickAccess);
            Items.Add(_rootFolder);
            Items.Add(_rootBookmarkFolder);

            Config.Current.DpiChanged += Config_DpiChanged;

            QuickAccessCollection.Current.CollectionChanged += QuickAccess_CollectionChanged;
        }

        
        // Events

        public event EventHandler SelectedItemChanged;


        // Properties

        public List<FolderTreeNodeBase> Items { get; set; }

        private FolderTreeNodeBase _selectedItem;
        public FolderTreeNodeBase SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem != value)
                {
                    if (_selectedItem != null)
                    {
                        _selectedItem.IsSelected = false;
                    }
                    _selectedItem = value;
                    if (_selectedItem != null)
                    {
                        _selectedItem.IsSelected = true;
                    }
                }
            }
        }

        public BitmapSource FolderIcon => FileIconCollection.Current.CreateDefaultFolderIcon(16.0);


        // Methods

        private static IEnumerable<FolderTreeNodeBase> GetNodeWalker(IEnumerable<FolderTreeNodeBase> collection)
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
                    case FolderTreeNodeDelayBase node:
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
                    SelectedItem = quickAccess;
                    SelectedItemChanged?.Invoke(this, null);
                }
            }
        }

        public void ExpandRoot()
        {
            _rootQuickAccess.IsExpanded = true;
            _rootFolder.IsExpanded = true;
            _rootBookmarkFolder.IsExpanded = true;
        }

        public void SelectRootQuickAccess()
        {
            SelectedItem = _rootQuickAccess;
        }

        public void Decide(object item)
        {
            switch (item)
            {
                case QuickAccessNode quickAccess:
                    SetFolderListPlace(quickAccess.QuickAccessSource.Path);
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
                var node = _rootQuickAccess.Children.FirstOrDefault(e => ((QuickAccessNode)e).Source == item);
                if (node != null)
                {
                    SelectedItem = node;
                    SelectedItemChanged?.Invoke(this, null);
                }
                return;
            }

            QuickAccessCollection.Current.Add(new QuickAccess(path));
        }

        public void RemoveQuickAccess(QuickAccessNode item)
        {
            if (item == null)
            {
                return;
            }

            var next = item.Next ?? item.Previous ?? item.Parent;

            bool isRemoved = QuickAccessCollection.Current.Remove(item.QuickAccessSource);
            if (isRemoved)
            {
                if (next != null)
                {
                    SelectedItem = next;
                }
            }
        }

        public void RemoveBookmarkFolder(BookmarkFolderNode item)
        {
            if (item == null || item is RootBookmarkFolderNode)
            {
                return;
            }

            var next = item.Next ?? item.Previous ?? item.Parent;

            var memento = new TreeListNodeMemento<IBookmarkEntry>(item.BookmarkSource);

            bool isRemoved = BookmarkCollection.Current.Remove(item.BookmarkSource);
            if (isRemoved)
            {
                if (item.BookmarkSource.Value is BookmarkFolder)
                {
                    var count = item.BookmarkSource.Count(e => e.Value is Bookmark);
                    if (count > 0)
                    {
                        _toast = new Toast(string.Format(Properties.Resources.DialogPagemarkFolderDelete, count), Properties.Resources.WordRestore, () => BookmarkCollection.Current.Restore(memento));
                        ToastService.Current.Show(_toast);
                    }
                }

                if (next != null)
                {
                    next.IsSelected = true;
                    SelectedItem = next;
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

            var node = BookmarkCollection.Current.AddNewFolder(item.BookmarkSource);
            if (node == null)
            {
                return null;
            }

            var newItem = item.Children.OfType<BookmarkFolderNode>().FirstOrDefault(e => e.Source == node);
            if (newItem != null)
            {
                SelectedItem = newItem;
            }

            return newItem;
        }

        public void MoveQuickAccess(QuickAccessNode src, QuickAccessNode dst)
        {
            if (src == dst)
            {
                return;
            }
            var srcIndex = QuickAccessCollection.Current.Items.IndexOf(src.Source);
            if (srcIndex < 0)
            {
                return;
            }
            var dstIndex = QuickAccessCollection.Current.Items.IndexOf(dst.Source);
            if (dstIndex < 0)
            {
                return;
            }
            QuickAccessCollection.Current.Move(srcIndex, dstIndex);

            SelectedItem = src;
        }

        public void SyncDirectory(string place)
        {
            var query = new QueryPath(place);
            if (query.Scheme == QueryScheme.File)
            {
                _rootFolder.RefreshDriveChildren();
            }
            else
            {
                return;
            }

            var node = GetDirectoryNode(query, true, true);
            if (node != null)
            {
                var parent = node.Parent;
                while (parent != null)
                {
                    parent.IsExpanded = true;
                    parent = (parent as FolderTreeNodeBase)?.Parent;
                }

                SelectedItem = node;
                SelectedItemChanged?.Invoke(this, null);
            }
        }

        private FolderTreeNodeBase GetDirectoryNode(QueryPath query, bool createChildren, bool asFarAsPossible)
        {
            switch (query.Scheme)
            {
                case QueryScheme.File:
                    return _rootFolder.GetFolderTreeNode(query.Path, createChildren, asFarAsPossible);
                case QueryScheme.Bookmark:
                    return _rootBookmarkFolder.GetFolderTreeNode(query.Path, createChildren, asFarAsPossible);
                case QueryScheme.QuickAccess:
                    return _rootBookmarkFolder.GetFolderTreeNode(query.Path, createChildren, asFarAsPossible);
                default:
                    throw new NotImplementedException();
            }
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
