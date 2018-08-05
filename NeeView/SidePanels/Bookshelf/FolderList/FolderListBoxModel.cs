using NeeLaboratory.ComponentModel;
using NeeView.Collections;
using NeeView.Collections.Generic;
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
        /// ロード時にフォーカスを要求するフラグ 
        /// </summary>
        public bool IsFocusOnLoad { get; set; }


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
                if (item != null && item.Path == pos.Path)
                {
                    return item;
                }
            }

            if (pos.TargetPath != null)
            {
                return this.FolderCollection.Items.FirstOrDefault(e => e.Path == pos.Path && e.TargetPath == pos.TargetPath) ?? this.FolderCollection.FirstOrDefault();
            }
            else
            {
                return this.FolderCollection.Items.FirstOrDefault(e => e.Path == pos.Path) ?? this.FolderCollection.FirstOrDefault();
            }
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

            var query = item.TargetPath;
            if (query.Path == null)
            {
                return;
            }

            BookHub.Current.RequestLoad(query.SimplePath, null, option | BookLoadOption.IsBook, false);
        }

        /// <summary>
        /// フォルダーの並びを設定
        /// </summary>
        public void SetFolderOrder(FolderOrder folderOrder)
        {
            if (FolderCollection == null) return;
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
            this.FolderCollection.FolderParameter.FolderOrder.GetToggle();
            RaisePropertyChanged(nameof(FolderOrder));
        }


        //
        public void ToggleFolderRecursive_Executed()
        {
            this.FolderCollection.FolderParameter.IsFolderRecursive = !this.FolderCollection.FolderParameter.IsFolderRecursive;
        }

        public void NewFolder()
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
            var place = BookHub.Current.Book?.Place;
            if (place == null)
            {
                return false;
            }

            return AddBookmark(new QueryPath(place), true);
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
                        var toast = new Toast(string.Format(Properties.Resources.DialogPagemarkFolderDelete, count), Properties.Resources.WordRestore, () => BookmarkCollection.Current.Restore(memento));
                        ToastService.Current.Show("BookmarkList", toast);
                    }
                }
            }

            return isRemoved;
        }

        public void MoveToHome()
        {
            FolderList.Current.MoveToHome();
        }

        public void MoveToUp()
        {
            FolderList.Current.MoveToParent();
        }

        /// <summary>
        /// 可能な場合のみ、フォルダー移動
        /// </summary>
        /// <param name="item"></param>
        public void MoveToSafety(FolderItem item)
        {
            if (item != null && item.CanOpenFolder())
            {
                FolderList.Current.MoveTo(item.TargetPath);
            }
        }

        public void MoveToPrevious()
        {
            FolderList.Current.MoveToPrevious();
        }

        public void MoveToNext()
        {
            FolderList.Current.MoveToNext();
        }
    }

}
