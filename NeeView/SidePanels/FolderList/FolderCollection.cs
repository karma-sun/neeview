using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

using Jobs = NeeLaboratory.Threading.Jobs;
using NeeView.IO;
using System.Runtime.InteropServices;

namespace NeeView
{
    /// <summary>
    /// FolderItemコレクション
    /// </summary>
    public abstract class FolderCollection : IDisposable
    {
        #region Fields

        private Jobs.SingleJobEngine _engine;

        private object _lock = new object();

        #endregion

        #region Constructors

        protected FolderCollection(string place, bool isStartEngine)
        {
            this.Place = place;

            this.FolderParameter = new FolderParameter(place);
            this.FolderParameter.PropertyChanged += (s, e) => ParameterChanged?.Invoke(s, null);

            if (isStartEngine)
            {
                _engine = new Jobs.SingleJobEngine();
                _engine.JobError += JobEngine_Error;
                _engine.StartEngine();
            }
        }

        #endregion

        #region Events

        public event EventHandler<FileSystemEventArgs> Deleting;

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
        /// Folder Parameter
        /// </summary>
        public FolderParameter FolderParameter { get; private set; }

        /// <summary>
        /// Collection本体
        /// </summary>
        public ObservableCollection<FolderItem> Items { get; protected set; }

        /// <summary>
        /// フォルダーの場所
        /// </summary>
        public string Place { get; private set; }

        /// <summary>
        /// フォルダーの場所(表示用)
        /// </summary>
        public string PlaceDispString => string.IsNullOrEmpty(Place) ? "PC" : Place;

        /// <summary>
        /// フォルダーの並び順
        /// </summary>
        private FolderOrder FolderOrder => FolderParameter.FolderOrder;

        /// <summary>
        /// シャッフル用ランダムシード
        /// </summary>
        private int RandomSeed => FolderParameter.RandomSeed;

        /// <summary>
        /// 有効判定
        /// </summary>
        public bool IsValid => Items != null;

        /// <summary>
        /// 場所の補足情報.
        /// FolderSearchCollectionでは検索キーワードが設定される
        /// </summary>
        public virtual string Meta { get; }

        #endregion

        #region Methods

        internal static partial class NativeMethods
        {
            [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
            public static extern int StrCmpLogicalW(string psz1, string psz2);
        }

        /// <summary>
        /// 一致判定
        /// </summary>
        public bool IsSame(string place, string meta)
        {
            return Place == place && Meta == meta;
        }

        /// <summary>
        /// 更新が必要？
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public bool IsDarty(FolderParameter folder)
        {
            return (Place != folder.Path || FolderOrder != folder.FolderOrder || RandomSeed != folder.RandomSeed);
        }

        /// <summary>
        /// 更新が必要？
        /// </summary>
        /// <returns></returns>
        public bool IsDarty()
        {
            return IsDarty(new FolderParameter(Place));
        }


        /// <summary>
        /// パスから項目インデックス取得
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public int IndexOfPath(string path)
        {
            var item = Items.FirstOrDefault(e => e.Path == path);
            return (item != null) ? Items.IndexOf(item) : -1;
        }


        /// <summary>
        /// パスから項目取得
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public FolderItem FirstOrDefault(string path)
        {
            return Items.FirstOrDefault(e => e.Path == path);
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
            return Items.FirstOrDefault(e => !e.IsEmpty);
        }

        /// <summary>
        /// 有効な末端項目を取得
        /// </summary>
        public FolderItem LastFolderOrDefault()
        {
            return Items.LastOrDefault(e => !e.IsEmpty);
        }

        /// <summary>
        /// 親の場所を取得
        /// </summary>
        public virtual string GetParentPlace()
        {
            if (Place == null) return null;
            return Path.GetDirectoryName(Place);
        }

        /// <summary>
        /// 前の項目を取得
        /// </summary>
        /// <param name="item">基準となる項目</param>
        /// <returns>前の項目。存在しない場合はnull</returns>
        public FolderItem GetPrevious(FolderItem item)
        {
            var index = IndexOfPath(item.Path);
            return (index > 0) ? Items[index - 1] : null;
        }

        /// <summary>
        /// 次の項目を取得
        /// </summary>
        /// <param name="item">基準となる項目</param>
        /// <returns>次の項目。存在しない場合はnull</returns>
        public FolderItem GetNext(FolderItem item)
        {
            var index = IndexOfPath(item.Path);
            return (index >= 0 && index < Items.Count - 1) ? Items[index + 1] : null;
        }


        /// <summary>
        /// パスがリストに含まれるか判定
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool Contains(string path)
        {
            return Items.Any(e => e.Path == path);
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
                case FolderOrder.TimeStamp:
                    return source.OrderBy(e => e.Type).ThenBy(e => e.LastWriteTime).ThenBy(e => e, new ComparerFileName());
                case FolderOrder.TimeStampDescending:
                    return source.OrderBy(e => e.Type).ThenByDescending(e => e.LastWriteTime).ThenBy(e => e, new ComparerFileName());
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
        /// アイコンの表示更新
        /// </summary>
        /// <param name="path">指定パスの項目を更新。nullの場合全ての項目を更新</param>
        public void RefleshIcon(string path)
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
                foreach (var item in Items.Where(e => e.TargetPath == path))
                {
                    item.NotifyIconOverlayChanged();
                }
            }
        }


        /// <summary>
        /// JobEngineで例外発生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void JobEngine_Error(object sender, Jobs.JobErrorEventArgs e)
        {
            Debug.WriteLine($"FolderCollection JOB Exception!: {e.Job}: {e.GetException().Message}");
            e.Handled = true;
        }

        /// <summary>
        /// 項目追加
        /// </summary>
        /// <param name="path"></param>
        public void RequestCreate(string path)
        {
            _engine?.Enqueue(new CreateJob(this, path, false));
        }

        /// <summary>
        /// 項目削除
        /// </summary>
        /// <param name="path"></param>
        public void RequestDelete(string path)
        {
            _engine?.Enqueue(new DeleteJob(this, path, false));
        }

        /// <summary>
        /// 項目名変更
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="path"></param>
        public void RequestRename(string oldPath, string path)
        {
            if (oldPath == path || path == null)
            {
                return;
            }

            _engine?.Enqueue(new RenameJob(this, oldPath, path, false));
        }

        #region Job.Create

        public class CreateJob : Jobs.IJob
        {
            private FolderCollection _target;
            private string _path;
            private bool _verify;

            public CreateJob(FolderCollection target, string path, bool verify)
            {
                _target = target;
                _path = path;
                _verify = verify;
            }

#pragma warning disable 1998
            public async Task ExecuteAsync()
            {
                ////Debug.WriteLine($"Create: {_path}");
                _target.CreateItem(_path);
            }
#pragma warning restore 1998
        }

        //
        private void CreateItem(string path)
        {
            FolderItem item;

            // FolderInfoを作成し、追加
            lock (_lock)
            {
                // 対象を検索
                item = this.Items.FirstOrDefault(i => i.Path == path);
            }

            // 既に登録済みの場合は処理しない
            if (item != null) return;

            item = CreateFolderItem(path);

            App.Current.Dispatcher.Invoke(() =>
            {
                CreateItem(item);
            });
        }

        //
        protected void CreateItem(FolderItem item)
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
                else if (FolderList.Current.IsInsertItem)
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

        #endregion

        #region Job.Delete

        public class DeleteJob : Jobs.IJob
        {
            private FolderCollection _target;
            private string _path;
            private bool _verify;

            public DeleteJob(FolderCollection target, string path, bool verify)
            {
                _target = target;
                _path = path;
                _verify = verify;
            }

#pragma warning disable 1998
            public async Task ExecuteAsync()
            {
                ////Debug.WriteLine($"Delete: {_path}");
                _target.DeleteItem(_path);
            }
#pragma warning restore 1998
        }

        // 対象を検索し、削除する
        private void DeleteItem(string path)
        {
            FolderItem item;

            lock (_lock)
            {
                item = this.Items.FirstOrDefault(i => i.Path == path);
            }

            // 既に存在しない場合は処理しない
            if (item == null) return;

            App.Current.Dispatcher.Invoke(() =>
            {
                Deleting?.Invoke(this, new FileSystemEventArgs(WatcherChangeTypes.Deleted, Path.GetDirectoryName(path), Path.GetFileName(path)));
                DeleteItem(item);
            });
        }

        //
        protected void DeleteItem(FolderItem item)
        {
            if (item == null) return;

            lock (_lock)
            {
                this.Items.Remove(item);

                if (this.Items.Count == 0)
                {
                    this.Items.Add(CreateFolderItemEmpty());
                }
            }
        }

        #endregion

        #region Job.Rename

        public class RenameJob : Jobs.IJob
        {
            private FolderCollection _target;
            private string _oldPath;
            private string _path;
            private bool _verify;

            public RenameJob(FolderCollection target, string oldPath, string path, bool verify)
            {
                _target = target;
                _oldPath = oldPath;
                _path = path;
                _verify = verify;
            }

#pragma warning disable 1998
            public async Task ExecuteAsync()
            {
                ////Debug.WriteLine($"Rename: {_oldPath} => {_path}");
                _target.RenameItem(_oldPath, _path);
            }
#pragma warning restore 1998
        }

        //
        private void RenameItem(string oldPath, string path)
        {
            if (oldPath == path) return;

            FolderItem item;
            lock (_lock)
            {
                item = this.Items.FirstOrDefault(i => i.Path == oldPath);
            }
            if (item == null)
            {
                // リストにない項目は追加を試みる
                CreateItem(path);
                return;
            }

            // 名前部分
            if (string.Compare(path, 0, item.Place, 0, item.Place.Length) != 0) throw new ArgumentException("remame exception: difference place");

            RenameItem(item, LoosePath.GetFileName(path, item.Place));
        }

        //
        protected void RenameItem(FolderItem item, string newName)
        {
            item.Name = newName;
        }

        #endregion

        #region Methods.CreateFilderItems

        /// <summary>
        /// 空のFolderItemを作成
        /// </summary>
        /// <returns></returns>
        protected FolderItem CreateFolderItemEmpty()
        {
            return new FolderItem()
            {
                Type = FolderItemType.Empty,
                Place = Place,
                Name = ".",
                Attributes = FolderItemAttribute.Empty,
            };
        }

        /// <summary>
        /// パスからFolderItemを作成
        /// </summary>
        /// <param name="path">パス</param>
        /// <returns>FolderItem。生成できなかった場合はnull</returns>
        protected FolderItem CreateFolderItem(string path)
        {
            // directory
            var directory = new DirectoryInfo(path);
            if (directory.Exists)
            {
                return CreateFolderItem(directory);
            }

            // file
            var file = new FileInfo(path);
            if (file.Exists)
            {
                // .lnk
                if (FileShortcut.IsShortcut(path))
                {
                    var shortcut = new FileShortcut(file);
                    return CreateFolderItem(shortcut);
                }
                else
                {
                    return CreateFolderItem(file);
                }
            }

            return null;
        }

        /// <summary>
        /// DriveInfoからFodlerItem作成
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected FolderItem CreateFolderItem(DriveInfo e)
        {
            if (e != null)
            {
                return new FolderItem()
                {
                    Place = null,
                    Name = e.Name,
                    Attributes = FolderItemAttribute.Directory | FolderItemAttribute.Drive,
                    IsReady = e.IsReady,
                };
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// DirectoryInfoからFolderItem作成
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected FolderItem CreateFolderItem(DirectoryInfo e)
        {
            if (e != null && e.Exists && (e.Attributes & FileAttributes.Hidden) == 0)
            {
                return new FolderItem()
                {
                    Type = FolderItemType.Directory,
                    Place = Place,
                    Name = e.Name,
                    LastWriteTime = e.LastWriteTime,
                    Length = -1,
                    Attributes = FolderItemAttribute.Directory,
                    IsReady = true
                };
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// FileInfoからFolderItem作成
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected FolderItem CreateFolderItem(FileInfo e)
        {
            if (e != null && e.Exists && ArchiverManager.Current.IsSupported(e.FullName) && (e.Attributes & FileAttributes.Hidden) == 0)
            {
                return new FolderItem()
                {
                    Type = FolderItemType.File,
                    Place = Place,
                    Name = e.Name,
                    LastWriteTime = e.LastWriteTime,
                    Length = e.Length,
                    IsReady = true
                };
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// FileShortcutからFolderItem作成
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected FolderItem CreateFolderItem(FileShortcut e)
        {
            FolderItem info = null;
            FolderItemType type = FolderItemType.FileShortcut;

            if (e != null && e.Source.Exists && (e.Source.Attributes & FileAttributes.Hidden) == 0 && e.Target.Exists)
            {
                if (e.Target.Attributes.HasFlag(FileAttributes.Directory))
                {
                    info = CreateFolderItem((DirectoryInfo)e.Target);
                    type = FolderItemType.DirectoryShortcut;
                }
                else
                {
                    info = CreateFolderItem((FileInfo)e.Target);
                    type = FolderItemType.FileShortcut;
                }
            }

            if (info != null)
            {
                info.Type = type;
                info.Place = Path.GetDirectoryName(e.SourcePath);
                info.Name = Path.GetFileName(e.SourcePath);
                info.TargetPath = e.TargetPath;
                info.Attributes = info.Attributes | FolderItemAttribute.Shortcut;
            }

            return info;
        }

        #endregion CreateFolderItems

        #endregion Methods

        #region IDisposable Support
        private bool _disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_engine != null)
                    {
                        _engine.Dispose();
                        _engine = null;
                    }

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
