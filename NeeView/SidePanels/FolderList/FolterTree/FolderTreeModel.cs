using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
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


        private void Config_DpiChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(FolderIcon));

            foreach(var item in GetNodeWalker(Items))
            { 
                switch (item)
                {
                    case DirectoryNode folder:
                        folder.RefreshIcon();
                        break;
                }
            }
        }

        private static IEnumerable<IFolderTreeNode> GetNodeWalker(IEnumerable<IFolderTreeNode> collection)
        {
            if (collection == null)
            {
                yield break;
            }

            foreach (var item in collection)
            {
                yield return item;

                switch (item)
                {
                    case RootQuickAccessNode rootQuickAccess:
                        foreach (var child in rootQuickAccess.Collection.Items)
                        {
                            yield return child;
                        }
                        break;

                    case QuickAccess QuickAccess:
                        break;

                    case FolderTreeNodeBase node:
                        if (node.ChildrenRaw != null)
                        {
                            foreach(var child in GetNodeWalker(node.ChildrenRaw))
                            {
                                yield return child;
                            }
                        }
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
        }


        private void QuickAccess_CollectionChanged(object sender, System.ComponentModel.CollectionChangeEventArgs e)
        {
            if (e.Action == System.ComponentModel.CollectionChangeAction.Add)
            {
                if (e.Element is QuickAccess quickAccess)
                {
                    quickAccess.IsSelected = true;
                    SelectedItemChanged?.Invoke(this, null);
                }
            }
        }

        public void SelectRootQuickAccess()
        {
            _rootQuickAccess.IsSelected = true;
        }

        public void Decide(object item)
        {
            switch (item)
            {
                case QuickAccess quickAccess:
                    SetFolderListPlace(quickAccess.Path);
                    break;
                case RootDirectoryNode rootFolder:
                    SetFolderListPlace("");
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
            FolderList.Current.RequestPlace(path, null, FolderSetPlaceOption.IsUpdateHistory | FolderSetPlaceOption.ResetKeyword);
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
                item.IsSelected = true;
                SelectedItemChanged?.Invoke(this, null);
                return;
            }

            QuickAccessCollection.Current.Add(new QuickAccess(path));
        }


        public void Remove(object item)
        {
            switch (item)
            {
                case QuickAccess quickAccess:
                    QuickAccessCollection.Current.Remove(quickAccess);
                    break;
            }
        }


        public void MoveQuickAccess(QuickAccess src, QuickAccess dst)
        {
            if (src == dst)
            {
                return;
            }
            var srcIndex = QuickAccessCollection.Current.Items.IndexOf(src);
            if (srcIndex < 0)
            {
                return;
            }
            var dstIndex = QuickAccessCollection.Current.Items.IndexOf(dst);
            if (dstIndex < 0)
            {
                return;
            }
            QuickAccessCollection.Current.Items.Move(srcIndex, dstIndex);
        }

        public void SyncDirectory(string place)
        {
            _rootFolder.SyncDirectory(place);
        }

        public  void RefreshDirectory()
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
