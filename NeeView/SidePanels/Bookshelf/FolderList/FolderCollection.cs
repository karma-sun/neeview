using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using NeeView.Collections.Generic;

namespace NeeView
{
    public class FolderCollectionChangedEventArgs : EventArgs
    {
        public FolderCollectionChangedEventArgs(CollectionChangeAction action, FolderItem item)
        {
            this.Action = action;
            this.Item = item;
        }

        public CollectionChangeAction Action { get; set; }
        public FolderItem Item { get; set; }
    }

    /// <summary>
    /// FolderItemコレクション
    /// </summary>
    public abstract class FolderCollection : IDisposable
    {
        #region Fields

        protected FolderItemFactory _folderItemFactory;
        protected bool _isOverlayEnabled;
        private object _lock = new object();

        #endregion

        #region Constructors

        protected FolderCollection(QueryPath path, bool isOverlayEnabled)
        {
            _folderItemFactory = new FolderItemFactory(path, isOverlayEnabled);

            this.Place = path;
            _isOverlayEnabled = isOverlayEnabled;

            // HACK: FullPathにする。過去のデータも修正が必要
            this.FolderParameter = new FolderParameter(Place.SimplePath);
            this.FolderParameter.PropertyChanged += (s, e) => ParameterChanged?.Invoke(s, null);
        }

        #endregion Constructors

        #region Events

        public event EventHandler<FolderCollectionChangedEventArgs> CollectionChanging;
        public event EventHandler<FolderCollectionChangedEventArgs> CollectionChanged;

        public event EventHandler ParameterChanged;

        #endregion

        #region Properties

        // indexer
        public FolderItem this[int index]
        {
            get { Debug.Assert(index >= 0 && index < Items.Count); return Items[index]; }
            private set { Items[index] = value; }
        }


        /// <summary>
        /// ソート適用の種類
        /// </summary>
        public abstract FolderOrderClass FolderOrderClass { get; }

        /// <summary>
        /// Folder Parameter
        /// </summary>
        public FolderParameter FolderParameter { get; private set; }

        /// <summary>
        /// Collection本体
        /// </summary>
        public ObservableCollection<FolderItem> Items { get; protected set; }

        /// <summary>
        /// フォルダーの場所(クエリ)
        /// </summary>
        public QueryPath Place { get; private set; }

        /// <summary>
        /// フォルダーの場所(表示用)
        /// </summary>
        public string PlaceDispString => Place.DispPath;

        /// <summary>
        /// フォルダーの場所(クエリー添付)
        /// </summary>
        public string QueryPath => Place.SimpleQuery;

        /// <summary>
        /// フォルダーの並び順
        /// </summary>
        public FolderOrder FolderOrder => FolderParameter.FolderOrder;

        /// <summary>
        /// シャッフル用ランダムシード
        /// </summary>
        private int RandomSeed => FolderParameter.RandomSeed;

        /// <summary>
        /// 有効判定
        /// </summary>
        public bool IsValid => Items != null;

        #endregion Properties

        #region Methods

        public virtual async Task InitializeItemsAsync(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public bool IsEmpty()
        {
            return Items == null
                || Items.Count == 0
                || Items.Count == 1 && Items[0].IsEmpty();
        }

        /// <summary>
        /// 更新が必要？
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public bool IsDarty(FolderParameter folder)
        {
            return (Place.SimplePath != folder.Path || FolderOrder != folder.FolderOrder || RandomSeed != folder.RandomSeed);
        }

        /// <summary>
        /// 更新が必要？
        /// </summary>
        /// <returns></returns>
        public bool IsDarty()
        {
            return IsDarty(new FolderParameter(Place.SimplePath));
        }


        /// <summary>
        /// パスから項目インデックス取得
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public int IndexOfPath(QueryPath path)
        {
            var item = Items.FirstOrDefault(e => e.TargetPath.Equals(path));
            return (item != null) ? Items.IndexOf(item) : -1;
        }


        public FolderItem FirstOrDefault(Func<FolderItem, bool> predicate)
        {
            return Items.FirstOrDefault(predicate);
        }

        /// <summary>
        /// 先頭項目を取得
        /// </summary>
        /// <returns></returns>
        public FolderItem FirstOrDefault()
        {
            return Items.FirstOrDefault();
        }

        /// <summary>
        /// 有効な先頭項目を取得
        /// </summary>
        public FolderItem FirstFolderOrDefault()
        {
            return Items.FirstOrDefault(e => !e.IsEmpty());
        }

        /// <summary>
        /// 有効な末端項目を取得
        /// </summary>
        public FolderItem LastFolderOrDefault()
        {
            return Items.LastOrDefault(e => !e.IsEmpty());
        }

        /// <summary>
        /// 親の場所を取得
        /// </summary>
        public virtual QueryPath GetParentQuery()
        {
            if (Place == null)
            {
                return null;
            }

            if (Place.Scheme == QueryScheme.Root)
            {
                return null;
            }

            return Place.GetParent() ?? new QueryPath(QueryScheme.Root, null);
        }

        /// <summary>
        /// パスがリストに含まれるか判定
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool Contains(QueryPath path)
        {
            return Items.Any(e => e.TargetPath.Equals(path));
        }

        /// <summary>
        /// 並び替え
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        protected IEnumerable<FolderItem> Sort(IEnumerable<FolderItem> source)
        {
            switch (FolderOrder)
            {
                default:
                case FolderOrder.FileName:
                    return source.OrderBy(e => e.Type).ThenBy(e => e, new ComparerFileName());
                case FolderOrder.FileNameDescending:
                    return source.OrderBy(e => e.Type).ThenByDescending(e => e, new ComparerFileName());
                case FolderOrder.Path:
                    return source.OrderBy(e => e.Type).ThenBy(e => e, new ComparerFullPath());
                case FolderOrder.PathDescending:
                    return source.OrderBy(e => e.Type).ThenByDescending(e => e, new ComparerFullPath());
                case FolderOrder.FileType:
                    return source.OrderBy(e => e.Type).ThenBy(e => e, new ComparerFileType());
                case FolderOrder.FileTypeDescending:
                    return source.OrderBy(e => e.Type).ThenByDescending(e => e, new ComparerFileType());
                case FolderOrder.TimeStamp:
                    return source.OrderBy(e => e.Type).ThenBy(e => e.LastWriteTime).ThenBy(e => e, new ComparerFileName());
                case FolderOrder.TimeStampDescending:
                    return source.OrderBy(e => e.Type).ThenByDescending(e => e.LastWriteTime).ThenBy(e => e, new ComparerFileName());
                case FolderOrder.EntryTime:
                    return source.OrderBy(e => e.Type).ThenBy(e => e.EntryTime).ThenBy(e => e, new ComparerFileName());
                case FolderOrder.EntryTimeDescending:
                    return source.OrderBy(e => e.Type).ThenByDescending(e => e.EntryTime).ThenBy(e => e, new ComparerFileName());
                case FolderOrder.Size:
                    return source.OrderBy(e => e.Type).ThenBy(e => e.Length).ThenBy(e => e, new ComparerFileName());
                case FolderOrder.SizeDescending:
                    return source.OrderBy(e => e.Type).ThenByDescending(e => e.Length).ThenBy(e => e, new ComparerFileName());
                case FolderOrder.Random:
                    var random = new Random(RandomSeed);
                    return source.OrderBy(e => e.Type).ThenBy(e => random.Next());
            }
        }

        /// <summary>
        /// ソート用：名前で比較(昇順)
        /// </summary>
        public class ComparerFileName : IComparer<FolderItem>
        {
            public int Compare(FolderItem x, FolderItem y)
            {
                return NativeMethods.StrCmpLogicalW(x.Name, y.Name);
            }
        }


        /// <summary>
        /// ソート用：フルパスで比較(昇順)
        /// </summary>
        public class ComparerFullPath : IComparer<FolderItem>
        {
            public int Compare(FolderItem x, FolderItem y)
            {
                return NativeMethods.StrCmpLogicalW(x.TargetPath.FullPath, y.TargetPath.FullPath);
            }
        }

        /// <summary>
        /// ソート用：ファイルの種類で比較(昇順)
        /// </summary>
        public class ComparerFileType : IComparer<FolderItem>
        {
            public int Compare(FolderItem x, FolderItem y)
            {
                // ディレクトリは種類判定なし
                if (x.IsDirectoryMaybe())
                {
                    return y.IsDirectoryMaybe() ? NativeMethods.StrCmpLogicalW(x.Name, y.Name) : 1;
                }
                if (y.IsDirectoryMaybe())
                {
                    return x.IsDirectoryMaybe() ? NativeMethods.StrCmpLogicalW(x.Name, y.Name) : -1;
                }

                var extX = LoosePath.GetExtension(x.Name);
                var extY = LoosePath.GetExtension(y.Name);
                if (extX != extY)
                {
                    return NativeMethods.StrCmpLogicalW(extX, extY);
                }
                else
                {
                    return NativeMethods.StrCmpLogicalW(x.Name, y.Name);
                }
            }
        }

        /// <summary>
        /// アイコンの表示更新
        /// </summary>
        /// <param name="path">指定パスの項目を更新。nullの場合全ての項目を更新</param>
        public void RefreshIcon(QueryPath path)
        {
            if (Items == null) return;

            if (path == null)
            {
                foreach (var item in Items)
                {
                    item.NotifyIconOverlayChanged();
                }
            }
            else
            {
                foreach (var item in Items.Where(e => e.EntityPath == path))
                {
                    item.NotifyIconOverlayChanged();
                }
            }
        }



        public virtual void RequestCreate(QueryPath path)
        {
            AddItem(path);
        }

        public virtual void RequestDelete(QueryPath path)
        {
            DeleteItem(path);
        }

        public virtual void RequestRename(QueryPath oldPath, QueryPath path)
        {
            RenameItem(oldPath, path);
        }


        //
        public void AddItem(QueryPath path)
        {
            FolderItem item;

            // FolderInfoを作成し、追加
            lock (_lock)
            {
                // 対象を検索
                item = this.Items.FirstOrDefault(i => i.TargetPath == path);
            }

            // 既に登録済みの場合は処理しない
            if (item != null) return;

            // TODO: ファイルシステムパス以外は不正処理になるので対処が必要
            item = _folderItemFactory.CreateFolderItem(path);

            AppDispatcher.Invoke(() =>
            {
                AddItem(item);
            });
        }

        //
        protected void AddItem(FolderItem item)
        {
            if (item == null) return;

            lock (_lock)
            {
                if (this.Items.Count == 1 && this.Items.First().Type == FolderItemType.Empty)
                {
                    this.Items.RemoveAt(0);
                    this.Items.Add(item);
                }
                else if (FolderOrder == FolderOrder.Random)
                {
                    this.Items.Add(item);
                }
                else if (BookshelfFolderList.Current.IsInsertItem)
                {
                    // 別にリストを作ってソートを実行し、それで挿入位置を決める
                    var list = Sort(this.Items.Concat(new List<FolderItem>() { item })).ToList();
                    var index = list.IndexOf(item);

                    if (index >= 0)
                    {
                        this.Items.Insert(index, item);
                    }
                    else
                    {
                        this.Items.Add(item);
                    }
                }
                else
                {
                    this.Items.Add(item);
                }
            }
        }


        // 対象を検索し、削除する
        public void DeleteItem(QueryPath path)
        {
            FolderItem item;

            lock (_lock)
            {
                item = this.Items.FirstOrDefault(i => i.TargetPath == path);
            }

            // 既に存在しない場合は処理しない
            if (item == null) return;

            AppDispatcher.Invoke(() =>
            {
                DeleteItem(item);
            });
        }

        //
        protected void DeleteItem(FolderItem item)
        {
            if (item == null) return;

            CollectionChanging?.Invoke(this, new FolderCollectionChangedEventArgs(CollectionChangeAction.Remove, item));

            lock (_lock)
            {
                this.Items.Remove(item);

                if (this.Items.Count == 0)
                {
                    this.Items.Add(_folderItemFactory.CreateFolderItemEmpty());
                }
            }

            CollectionChanged?.Invoke(this, new FolderCollectionChangedEventArgs(CollectionChangeAction.Remove, item));
        }


        //
        public void RenameItem(QueryPath oldPath, QueryPath path)
        {
            if (oldPath == path) return;

            FolderItem item;
            lock (_lock)
            {
                item = this.Items.FirstOrDefault(i => i.TargetPath == oldPath);
            }
            if (item == null)
            {
                // リストにない項目は追加を試みる
                AddItem(path);
                return;
            }

            // ディレクトリの名前が変更されていないかチェック
            if (LoosePath.GetDirectoryName(path.FullPath) != LoosePath.GetDirectoryName(item.TargetPath.FullPath))
            {
                throw new ArgumentException("remame exception: difference place");
            }

            RenameItem(item, path.FileName);
        }

        //
        protected void RenameItem(FolderItem item, string newName)
        {
            item.Name = newName;
        }


        #endregion Methods

        #region IDisposable Support
        private bool _disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (Items != null)
                    {
                        BindingOperations.DisableCollectionSynchronization(Items);
                        Items = null;
                    }
                }

                _disposedValue = true;
            }
        }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
        }
        #endregion
    }

}