// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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

namespace NeeView
{
    /// <summary>
    /// FolderItemコレクション
    /// </summary>
    public abstract class FolderCollection : IDisposable
    {
        #region Fields

        /// <summary>
        /// Collection
        /// </summary>
        private ObservableCollection<FolderItem> _items;

        /// <summary>
        /// コマンド処理エンジン
        /// </summary>
        private Jobs.JobEngine _engine;

        /// <summary>
        /// 
        /// </summary>
        private object _lock = new object();

        #endregion

        #region Constructors

        protected FolderCollection(string place)
        {
            this.Place = place;

            this.FolderParameter = new FolderParameter(place);
            this.FolderParameter.PropertyChanged += (s, e) => ParameterChanged?.Invoke(s, null);

            _engine = new Jobs.JobEngine();
            _engine.Error += JobEngine_Error;
            _engine.IsEnabled = true;
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
        public ObservableCollection<FolderItem> Items
        {
            get { return _items; }
            protected set { _items = value; }
        }

        /// <summary>
        /// フォルダーの場所
        /// </summary>
        public string Place { get; private set; }

        /// <summary>
        /// フォルダーの場所(表示用)
        /// </summary>
        public string PlaceDispString => string.IsNullOrEmpty(Place) ? "このPC" : Place;

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

        #endregion

        #region Methods

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
                case FolderOrder.TimeStamp:
                    return source.OrderBy(e => e.Type).ThenBy(e => e, new ComparerTimeStamp());
                case FolderOrder.Size:
                    return source.OrderBy(e => e.Type).ThenBy(e => e, new ComparerSize());
                case FolderOrder.Random:
                    var random = new Random(RandomSeed);
                    return source.OrderBy(e => e.Type).ThenBy(e => random.Next());
                default:
                case FolderOrder.FileName:
                    return source.OrderBy(e => e.Type).ThenBy(e => e, new ComparerFileName());
            }
        }

        #region Comparer for Sort

        /// <summary>
        /// ソート用：名前で比較
        /// </summary>
        public class ComparerFileName : IComparer<FolderItem>
        {
            public int Compare(FolderItem x, FolderItem y)
            {
                return Win32Api.StrCmpLogicalW(x.Name, y.Name);
            }
        }

        /// <summary>
        /// ソート用：サイズで比較
        /// </summary>
        public class ComparerSize : IComparer<FolderItem>
        {
            public int Compare(FolderItem x, FolderItem y)
            {
                int diff = y.Length.CompareTo(x.Length);
                if (diff != 0)
                    return diff;
                else
                    return Win32Api.StrCmpLogicalW(x.Name, y.Name);
            }
        }

        /// <summary>
        /// ソート用：日時で比較
        /// </summary>
        public class ComparerTimeStamp : IComparer<FolderItem>
        {
            public int Compare(FolderItem x, FolderItem y)
            {
                int diff = y.LastWriteTime.CompareTo(x.LastWriteTime);
                if (diff != 0)
                    return diff;
                else
                    return Win32Api.StrCmpLogicalW(x.Name, y.Name);
            }
        }

        #endregion

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
        private void JobEngine_Error(object sender, ErrorEventArgs e)
        {
            Debug.WriteLine($"JobEngine Exception!: {e.GetException().Message}");
            throw e.GetException();
        }

        /// <summary>
        /// 項目追加
        /// </summary>
        /// <param name="path"></param>
        public void RequestCreate(string path)
        {
            _engine.Enqueue(new CreateJob(this, path, false));
        }

        /// <summary>
        /// 項目削除
        /// </summary>
        /// <param name="path"></param>
        public void RequestDelete(string path)
        {
            _engine.Enqueue(new DeleteJob(this, path, false));

        }

        /// <summary>
        /// 項目名変更
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="path"></param>
        public void RequestRename(string oldPath, string path)
        {
            _engine.Enqueue(new RenameJob(this, oldPath, path, false));
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
                Debug.WriteLine($"Create: {_path}");
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
            CreateItem(item);
        }

        //
        private void CreateItem(FolderItem item)
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
                Debug.WriteLine($"Delete: {_path}");
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

            App.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                Deleting?.Invoke(this, new FileSystemEventArgs(WatcherChangeTypes.Deleted, Path.GetDirectoryName(path), Path.GetFileName(path)));
                DeleteItem(item);
            }));
        }

        //
        private void DeleteItem(FolderItem item)
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
                Debug.WriteLine($"Rename: {_oldPath} => {_path}");
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

            item.Path = path;
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
                Path = Place + "\\.",
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
                if (Utility.FileShortcut.IsShortcut(path))
                {
                    var shortcut = new Utility.FileShortcut(file);
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
                    Path = e.Name,
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
                    Path = e.FullName,
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
                    Path = e.FullName,
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
        protected FolderItem CreateFolderItem(Utility.FileShortcut e)
        {
            FolderItem info = null;
            FolderItemType type = FolderItemType.FileShortcut;

            if (e != null && e.Source.Exists && (e.Source.Attributes & FileAttributes.Hidden) == 0)
            {
                if (e.DirectoryInfo.Exists)
                {
                    info = CreateFolderItem(e.DirectoryInfo);
                    type = FolderItemType.DirectoryShortcut;
                }
                else if (e.FileInfo.Exists)
                {
                    info = CreateFolderItem(e.FileInfo);
                    type = FolderItemType.FileShortcut;
                }
            }

            if (info != null)
            {
                info.Type = type;
                info.Path = e.Path;
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
