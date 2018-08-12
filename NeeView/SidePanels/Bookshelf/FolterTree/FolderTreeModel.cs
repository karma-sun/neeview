using NeeLaboratory.ComponentModel;
using NeeView.Collections;
using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class RootFolderTree : FolderTreeNodeBase
    {
        public override string Name { get => null; set { } }
        public override string DispName { get => "@Bookshelf"; set { } }

        public override ImageSource Icon => null;
    }

    public class FolderTreeModel : BindableBase
    {
        // Fields

        public static FolderTreeModel Current { get; } = new FolderTreeModel();

        private RootFolderTree _root;
        private RootQuickAccessNode _rootQuickAccess;
        private RootDirectoryNode _rootFolder;
        private RootBookmarkFolderNode _rootBookmarkFolder;
        ////private RootPagemarkFolderNode _rootPagemarkFolder;

        // Constructors

        public FolderTreeModel()
        {
            _root = new RootFolderTree();

            _rootQuickAccess = new RootQuickAccessNode(_root);
            _rootFolder = new RootDirectoryNode(_root);
            _rootBookmarkFolder = new RootBookmarkFolderNode(_root);
            ////_rootPagemarkFolder = new RootPagemarkFolderNode(_root);

            _root.Children = new ObservableCollection<FolderTreeNodeBase>()
            {
                _rootQuickAccess,
                _rootFolder,
                _rootBookmarkFolder,
                ////_rootPagemarkFolder,
            };

            Config.Current.DpiChanged += Config_DpiChanged;
        }


        // Events

        public event EventHandler SelectedItemChanged;


        // Properties

        public RootFolderTree Root => _root;


        private FolderTreeNodeBase _selectedItem;
        public FolderTreeNodeBase SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                if (_selectedItem != null)
                {
                    _selectedItem.IsSelected = true;
                }
            }
        }

        public BitmapSource FolderIcon => FileIconCollection.Current.CreateDefaultFolderIcon(16.0);

        public bool IsFocusAtOnce { get; set; }


        // Methods

        public void FocusAtOnce()
        {
            IsFocusAtOnce = true;
        }

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

            foreach (var item in GetNodeWalker(_root.Children))
            {
                item.RefreshIcon();
            }
        }

        public void ExpandRoot()
        {
            _rootQuickAccess.IsExpanded = true;
            _rootFolder.IsExpanded = true;
            _rootBookmarkFolder.IsExpanded = true;
            ////_rootPagemarkFolder.IsExpanded = true;
        }

        public void SelectRootQuickAccess()
        {
            SelectedItem = _rootQuickAccess;
        }

        public void SelectRootBookmarkFolder()
        {
            SelectedItem = _rootBookmarkFolder;
        }

        public void SelectRootPagemarkFolder()
        {
            ////SelectedItem = _rootPagemarkFolder;
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
            FolderList.Current.RequestPlace(new QueryPath(path), null, FolderSetPlaceOption.UpdateHistory | FolderSetPlaceOption.ResetKeyword);
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

                case string filename:
                    AddQuickAccess(filename);
                    break;
            }
        }

        public void AddQuickAccess(string path)
        {
            InsertQuickAccess(0, path);
        }

        public void InsertQuickAccess(QuickAccessNode dst, string path)
        {
            var index = dst != null ? QuickAccessCollection.Current.Items.IndexOf(dst.Source) : 0;
            if (index < 0)
            {
                return;
            }

            InsertQuickAccess(index, path);
        }

        public void InsertQuickAccess(int index, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }
            if (path.StartsWith(Temporary.TempDirectory))
            {
                ToastService.Current.Show(new Toast(Properties.Resources.DialogQuickAccessTempError));
                return;
            }

            _rootQuickAccess.IsExpanded = true;

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

            QuickAccessCollection.Current.Insert(index, new QuickAccess(path));
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
                        var toast = new Toast(string.Format(Properties.Resources.DialogPagemarkFolderDelete, count), Properties.Resources.WordRestore, () => BookmarkCollection.Current.Restore(memento));
                        ToastService.Current.Show("FolderList", toast);
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

        internal void AddBookmarkTo(BookmarkFolderNode item)
        {
            var place = BookHub.Current.Book?.Place;
            if (place == null)
            {
                return;
            }

            var parentNode = item.BookmarkSource;

            // TODO: 重複チェックはBookmarkCollectionで行うようにする
            var node = parentNode.Children.FirstOrDefault(e => e.Value is Bookmark bookmark && bookmark.Place == place);
            if (node == null)
            {
                var unit = BookMementoCollection.Current.Set(place);
                node = new TreeListNode<IBookmarkEntry>(new Bookmark(unit));
                BookmarkCollection.Current.AddToChild(node, parentNode);
            }
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
        }

        public void SyncDirectory(string place)
        {
            var path = new QueryPath(place);
            if (path.Scheme == QueryScheme.File)
            {
                _rootFolder.RefreshDriveChildren();
            }
            else
            {
                return;
            }

            var node = GetDirectoryNode(path, true, true);
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

        private FolderTreeNodeBase GetDirectoryNode(QueryPath path, bool createChildren, bool asFarAsPossible)
        {
            switch (path.Scheme)
            {
                case QueryScheme.File:
                    return _rootFolder.GetFolderTreeNode(path.Path, createChildren, asFarAsPossible);
                case QueryScheme.Bookmark:
                    return _rootBookmarkFolder.GetFolderTreeNode(path.Path, createChildren, asFarAsPossible);
                ////case QueryScheme.Pagemark:
                ////    return _rootPagemarkFolder.GetFolderTreeNode(path.Path, createChildren, asFarAsPossible);
                case QueryScheme.QuickAccess:
                    return _rootBookmarkFolder.GetFolderTreeNode(path.Path, createChildren, asFarAsPossible);
                default:
                    throw new NotImplementedException();
            }
        }

        public void RefreshDirectory()
        {
            _rootFolder.Refresh();
        }
    }
}
