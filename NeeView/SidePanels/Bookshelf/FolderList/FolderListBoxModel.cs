using NeeLaboratory.ComponentModel;
using NeeView.Collections;
using NeeView.Collections.Generic;
using NeeView.Properties;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    public class SelectedChangedEventArgs : EventArgs
    {
        public bool IsFocus { get; set; }
        public bool IsNewFolder { get; set; }
    }

    /// <summary>
    /// FolderListBox用 Model
    /// </summary>
    public class FolderListBoxModel : BindableBase
    {
        public FolderListBoxModel(FolderCollection folderCollection)
        {
            _folderCollection = folderCollection;
        }

        public event EventHandler<SelectedChangedEventArgs> SelectedChanging;
        public event EventHandler<SelectedChangedEventArgs> SelectedChanged;

        private FolderCollection _folderCollection;
        public FolderCollection FolderCollection
        {
            get { return _folderCollection; }
            set { SetProperty(ref _folderCollection, value); }
        }

        private FolderItem _selectedItem;
        public FolderItem SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        /// <summary>
        /// リスト自体のコンテキストメニュー表示が有効？
        /// </summary>
        public bool IsContextMenuEnabled => FolderCollection is BookmarkFolderCollection;

        /// <summary>
        /// フォーカス要求
        /// </summary>
        public bool IsFocusAtOnce { get; set; }

        /// <summary>
        /// 本を読み込むときに本棚の更新を要求する
        /// </summary>
        private bool _isSyncBookshelfEnabled;
        public bool IsSyncBookshelfEnabled
        {
            get { return _isSyncBookshelfEnabled; }
            set { SetProperty(ref _isSyncBookshelfEnabled, value); }
        }


        public void Loaded()
        {
            if (_folderCollection != null)
            {
                _folderCollection.CollectionChanging += FolderCollection_CollectionChanging;
                _folderCollection.CollectionChanged += FolderCollection_CollectionChanged;
            }
        }

        public void Unloaded()
        {
            if (_folderCollection != null)
            {
                _folderCollection.CollectionChanging -= FolderCollection_CollectionChanging;
                _folderCollection.CollectionChanged -= FolderCollection_CollectionChanged;
            }
        }

        public void SetSelectedItem(FolderItemPosition select, bool isFocus)
        {
            RaiseSelectedItemChanging();
            this.SelectedItem = FixedItem(select);
            RaiseSelectedItemChanged(isFocus);
        }


        /// <summary>
        /// ふさわしい選択項目インデックスを取得
        /// </summary>
        /// <param name="path">選択したいパス</param>
        /// <returns></returns>
        internal int FixedIndexOfPath(QueryPath path)
        {
            var index = this.FolderCollection.IndexOfPath(path);
            return index < 0 ? 0 : index;
        }

        /// <summary>
        /// 選択項目の復元
        /// </summary>
        internal FolderItem FixedItem(FolderItemPosition pos)
        {
            if (pos == null)
            {
                return this.FolderCollection.FirstOrDefault();
            }

            if (pos.Index >= 0)
            {
                var item = this.FolderCollection.Items.ElementAtOrDefault(pos.Index);
                if (item != null && item.TargetPath == pos.Path)
                {
                    return item;
                }
            }

            // アーカイブ内のパスの場合、有効な項目になるまで場所を遡る
            var path = pos.Path;
            do
            {
                var select = this.FolderCollection.Items.FirstOrDefault(e => e.TargetPath == path);
                if (select != null)
                {
                    return select;
                }
                path = path.GetParent();
            }
            while (path != null && path.FullPath.Length > this.FolderCollection.Place.FullPath.Length);
            return this.FolderCollection.FirstOrDefault();
        }

        /// <summary>
        /// 項目変更前通知
        /// </summary>
        public void RaiseSelectedItemChanging()
        {
            SelectedChanging?.Invoke(this, null);
        }

        /// <summary>
        /// 項目変更後通知
        /// </summary>
        /// <param name="isFocus"></param>
        public void RaiseSelectedItemChanged(bool isFocus = false)
        {
            SelectedChanged?.Invoke(this, new SelectedChangedEventArgs() { IsFocus = isFocus });
        }


        // となりを取得
        public FolderItem GetNeighbor(FolderItem item)
        {
            var items = this.FolderCollection?.Items;
            if (items == null || items.Count <= 0) return null;

            int index = items.IndexOf(item);
            if (index < 0) return items[0];

            if (index + 1 < items.Count)
            {
                return items[index + 1];
            }
            else if (index > 0)
            {
                return items[index - 1];
            }
            else
            {
                return item;
            }
        }

        private void FolderCollection_CollectionChanging(object sender, FolderCollectionChangedEventArgs e)
        {
            if (e.Action == CollectionChangeAction.Remove)
            {
                SelectedChanging?.Invoke(this, new SelectedChangedEventArgs());
                if (SelectedItem == e.Item)
                {
                    SelectedItem = GetNeighbor(SelectedItem);
                }
            }
        }

        private void FolderCollection_CollectionChanged(object sender, FolderCollectionChangedEventArgs e)
        {
            if (e.Action == CollectionChangeAction.Remove)
            {
                if (SelectedItem == null)
                {
                    SelectedItem = FolderCollection.Items?.FirstOrDefault();
                }
                SelectedChanged?.Invoke(this, new SelectedChangedEventArgs());
            }
        }

        /// <summary>
        /// 選択項目を基準とした項目取得
        /// </summary>
        /// <param name="offset">選択項目から前後した項目を指定</param>
        /// <returns></returns>
        internal FolderItem GetFolderItem(FolderItem item, int offset)
        {
            if (this.FolderCollection?.Items == null) return null;

            int index = this.FolderCollection.Items.IndexOf(item);
            if (index < 0) return null;

            int next = (this.FolderCollection.FolderParameter.FolderOrder == FolderOrder.Random)
                ? (index + this.FolderCollection.Items.Count + offset) % this.FolderCollection.Items.Count
                : index + offset;

            if (next < 0 || next >= this.FolderCollection.Items.Count) return null;

            return this.FolderCollection[next];
        }

        internal int GetFolderItemIndex(FolderItem item)
        {
            if (this.FolderCollection?.Items == null) return -1;

            return this.FolderCollection.Items.IndexOf(item);
        }


        /// <summary>
        /// フォルダーアイコンの表示更新
        /// </summary>
        /// <param name="path">更新するパス。nullならば全て更新</param>
        public void RefreshIcon(QueryPath path)
        {
            this.FolderCollection?.RefreshIcon(path);
        }

        // ブックの読み込み
        public void LoadBook(FolderItem item)
        {
            if (item == null) return;

            BookLoadOption option = BookLoadOption.SkipSamePlace | (this.FolderCollection.FolderParameter.IsFolderRecursive ? BookLoadOption.DefaultRecursive : BookLoadOption.None);
            LoadBook(item, option);
        }

        // ブックの読み込み
        public void LoadBook(FolderItem item, BookLoadOption option)
        {
            if (item.Attributes.HasFlag(FolderItemAttribute.System))
            {
                return;
            }

            // ブックマークフォルダーは本として開けないようにする
            if (item.Attributes.HasFlag(FolderItemAttribute.Directory | FolderItemAttribute.Bookmark))
            {
                return;
            }

            var query = item.TargetPath;
            if (query.Scheme != QueryScheme.Pagemark && query.Path == null)
            {
                return;
            }

            var additionalOption = BookLoadOption.IsBook | (item.CanRemove() ? BookLoadOption.None : BookLoadOption.Undeliteable);
            BookHub.Current.RequestLoad(query.SimplePath, null, option | additionalOption, _isSyncBookshelfEnabled);
        }

        /// <summary>
        /// フォルダーの並びを設定
        /// </summary>
        public void SetFolderOrder(FolderOrder folderOrder)
        {
            if (FolderCollection == null) return;
            if (!FolderCollection.FolderOrderClass.GetFolderOrderMap().ContainsKey(folderOrder)) return;

            this.FolderCollection.FolderParameter.FolderOrder = folderOrder;
            RaisePropertyChanged(nameof(FolderOrder));
        }

        /// <summary>
        /// フォルダーの並びを取得
        /// </summary>
        public FolderOrder GetFolderOrder()
        {
            if (this.FolderCollection == null) return default(FolderOrder);
            return this.FolderCollection.FolderParameter.FolderOrder;
        }

        /// <summary>
        /// フォルダーの並びを順番に切り替える
        /// </summary>
        public void ToggleFolderOrder()
        {
            if (this.FolderCollection == null) return;
            SetFolderOrder(GetNextFolderOrder());
            RaisePropertyChanged(nameof(FolderOrder));
        }

        public FolderOrder GetNextFolderOrder()
        {
            if (this.FolderCollection == null) return default;

            var orders = FolderCollection.FolderOrderClass.GetFolderOrderMap().Keys;
            var now = this.FolderCollection.FolderParameter.FolderOrder;
            var index = orders.IndexOf(now);
            return orders.ElementAt((index + 1) % orders.Count);
        }


        //
        public void ToggleFolderRecursive_Executed()
        {
            this.FolderCollection.FolderParameter.IsFolderRecursive = !this.FolderCollection.FolderParameter.IsFolderRecursive;
        }


        public void NewFolder()
        {
            if (FolderCollection is BookmarkFolderCollection)
            {
                NewBookmarkFolder();
            }
        }


        public void NewBookmarkFolder()
        {
            if (FolderCollection is BookmarkFolderCollection bookmarkFolderCollection)
            {
                var node = BookmarkCollection.Current.AddNewFolder(bookmarkFolderCollection.BookmarkPlace);

                var item = bookmarkFolderCollection.FirstOrDefault(e => e.Attributes.HasFlag(FolderItemAttribute.Directory) && e.Name == node.Value.Name);

                if (item != null)
                {
                    SelectedItem = item;
                    SelectedChanged?.Invoke(this, new SelectedChangedEventArgs() { IsFocus = true, IsNewFolder = true });
                }
            }
        }

        public void SelectBookmark(TreeListNode<IBookmarkEntry> node, bool isFocus)
        {
            if (!(FolderCollection is BookmarkFolderCollection bookmarkFolderCollection))
            {
                return;
            }

            var item = bookmarkFolderCollection.FirstOrDefault(e => node == (e.Source as TreeListNode<IBookmarkEntry>));
            if (item != null)
            {
                SelectedItem = item;
                SelectedChanged?.Invoke(this, new SelectedChangedEventArgs() { IsFocus = isFocus });
            }
        }

        public bool AddBookmark()
        {
            var address = BookHub.Current.Book?.Address;
            if (address == null)
            {
                return false;
            }

            return AddBookmark(new QueryPath(address), true);
        }

        public bool AddBookmark(QueryPath path, bool isFocus)
        {
            if (!(FolderCollection is BookmarkFolderCollection bookmarkFolderCollection))
            {
                return false;
            }

            var node = BookmarkCollectionService.AddToChild(bookmarkFolderCollection.BookmarkPlace, path);
            if (node != null)
            {
                var item = bookmarkFolderCollection.FirstOrDefault(e => node == (e.Source as TreeListNode<IBookmarkEntry>));
                if (item != null)
                {
                    SelectedItem = item;
                    SelectedChanged?.Invoke(this, new SelectedChangedEventArgs() { IsFocus = isFocus });
                }
            }

            return true;
        }

        public bool RemoveBookmark(FolderItem item)
        {
            var node = item.Source as TreeListNode<IBookmarkEntry>;
            if (node == null)
            {
                return false;
            }

            var memento = new TreeListNodeMemento<IBookmarkEntry>(node);

            bool isRemoved = BookmarkCollection.Current.Remove(node);
            if (isRemoved)
            {
                if (node.Value is BookmarkFolder)
                {
                    var count = node.Count(e => e.Value is Bookmark);
                    if (count > 0)
                    {
                        var toast = new Toast(string.Format(Properties.Resources.DialogPagemarkFolderDelete, count), null, ToastIcon.Information, Properties.Resources.WordRestore, () => BookmarkCollection.Current.Restore(memento));
                        ToastService.Current.Show("BookmarkList", toast);
                    }
                }
            }

            return isRemoved;
        }

        public FolderItem FindFolderItem(string address)
        {
            var path = new QueryPath(address);
            var select = this.FolderCollection.Items.FirstOrDefault(e => e.TargetPath == path);

            return select;
        }

        public async Task RemoveAsync(FolderItem item)
        {
            if (item == null) return;

            var index = item == SelectedItem ? GetFolderItemIndex(item) : -1;
            bool isCurrentBook = BookHub.Current.Address == item.TargetPath.SimplePath;

            if (item.Attributes.HasFlag(FolderItemAttribute.Bookmark))
            {
                RemoveBookmark(item);
            }
            else if (item.IsFileSystem())
            {
                var removed = await FileIO.Current.RemoveAsync(item.TargetPath.SimplePath, Properties.Resources.DialogFileDeleteBookTitle);
                if (removed)
                {
                    FolderCollection?.RequestDelete(item.TargetPath);
                    if (isCurrentBook && Config.Current.Bookshelf.IsOpenNextBookWhenRemove)
                    {
                        var next = NeeLaboratory.MathUtility.Clamp(index, -1, this.FolderCollection.Items.Count - 1);
                        if (!FolderCollection.IsEmpty() && next >= 0)
                        {
                            SelectedItem = this.FolderCollection[next];
                            LoadBook(SelectedItem);
                        }
                    }
                }
            }
        }

    }
}
