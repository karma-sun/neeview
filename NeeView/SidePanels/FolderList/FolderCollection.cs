// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Collections.Specialized;

namespace NeeView
{
    /// <summary>
    /// FolderItemコレクションの種類
    /// </summary>
    public enum FolderCollectionMode
    {
        /// <summary>
        /// ファイルリスト
        /// </summary>
        Entry,

        /// <summary>
        /// 検索結果
        /// </summary>
        Search,
    }

    /// <summary>
    /// FolderItemコレクション
    /// </summary>
    public class FolderCollection : IDisposable
    {
        private object _lock = new object();

        public event EventHandler<FileSystemEventArgs> Deleting;

        public event EventHandler ParameterChanged;

        /// <summary>
        /// Folder Parameter
        /// </summary>
        public FolderParameter FolderParameter { get; private set; }

        // indexer
        public FolderItem this[int index]
        {
            get { Debug.Assert(index >= 0 && index < Items.Count); return Items[index]; }
            private set { Items[index] = value; }
        }

        /// <summary>
        /// Collection本体
        /// </summary>
        private ObservableCollection<FolderItem> _items;
        public ObservableCollection<FolderItem> Items
        {
            get { return _items; }
            private set { _items = value; }
        }


        /// <summary>
        /// 検索結果
        /// </summary>
        private NeeLaboratory.IO.Search.SearchResultWatcher _searchResult;


        /// <summary>
        /// 検索キーワード
        /// </summary>
        public string SearchKeyword => _searchResult?.Keyword;



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

        /// <summary>
        /// Mode property.
        /// </summary>
        public FolderCollectionMode Mode { get; private set; }


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
        /// constructor
        /// </summary>
        /// <param name="place"></param>
        public FolderCollection(string place)
        {
            Initialize(place);
            InitializeFolder();
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="place"></param>
        public FolderCollection(string place, NeeLaboratory.IO.Search.SearchResultWatcher searchResult)
        {
            Initialize(place);
            InitializeSearch(searchResult);
        }

        //
        private void Initialize(string place)
        {
            this.Place = place;

            this.FolderParameter = new FolderParameter(place);
            this.FolderParameter.PropertyChanged += (s, e) => ParameterChanged?.Invoke(s, null);
        }



        /// <summary>
        /// リスト生成
        /// </summary>
        private void InitializeFolder()
        {
            this.Mode = FolderCollectionMode.Entry;

            if (string.IsNullOrWhiteSpace(Place))
            {
                this.Items = new ObservableCollection<FolderItem>(DriveInfo.GetDrives().Select(e => CreateFolderItem(e)));
                return;
            }
            else
            {
                var directory = new DirectoryInfo(Place);

                if (!directory.Exists)
                {
                    var items = new ObservableCollection<FolderItem>();
                    items.Add(new FolderItem() { Path = Place + "\\.", Attributes = FolderItemAttribute.Empty | FolderItemAttribute.DirectoryNoFound });
                    this.Items = items;
                }
                else
                {
                    var fileInfos = directory.GetFiles();

                    var shortcuts = fileInfos
                        .Where(e => e.Exists && Utility.FileShortcut.IsShortcut(e.FullName) && (e.Attributes & FileAttributes.Hidden) == 0)
                        .Select(e => new Utility.FileShortcut(e))
                        .ToList();

                    var directoryInfos = directory.GetDirectories();

                    var directories = directoryInfos
                        .Where(e => e.Exists && (e.Attributes & FileAttributes.Hidden) == 0)
                        .Select(e => CreateFolderItem(e))
                        .ToList();

                    var directoryShortcuts = shortcuts
                        .Where(e => e.DirectoryInfo.Exists)
                        .Select(e => CreateFolderItem(e))
                        .ToList();

                    var archives = fileInfos
                        .Where(e => e.Exists && ArchiverManager.Current.IsSupported(e.FullName) && (e.Attributes & FileAttributes.Hidden) == 0)
                        .Select(e => CreateFolderItem(e))
                        .ToList();

                    var archiveShortcuts = shortcuts
                        .Where(e => e.FileInfo.Exists && ArchiverManager.Current.IsSupported(e.TargetPath))
                        .Select(e => CreateFolderItem(e))
                        .ToList();


                    var items = directories
                        .Concat(directoryShortcuts)
                        .Concat(archives)
                        .Concat(archiveShortcuts)
                        .Where(e => e != null);


                    var list = Sort(items).ToList();

                    if (!list.Any())
                    {
                        list.Add(CreateFolderItemEmpty());
                    }

                    this.Items = new ObservableCollection<FolderItem>(list);
                }
            }

            BindingOperations.EnableCollectionSynchronization(this.Items, new object());

            InitializeWatcher(Place);
            StartWatch();
        }


        /// <summary>
        /// 検索結果からリスト生成
        /// </summary>
        private void InitializeSearch(NeeLaboratory.IO.Search.SearchResultWatcher searchResult)
        {
            this.Mode = FolderCollectionMode.Search;

            var items = searchResult.Items
                .Select(e => CreateFolderItem(e))
                .Where(e => e != null)
                .ToList();

            var list = Sort(items).ToList();

            if (!list.Any())
            {
                list.Add(CreateFolderItemEmpty());
            }

            this.Items = new ObservableCollection<FolderItem>(list);
            BindingOperations.EnableCollectionSynchronization(this.Items, new object());

            _searchResult = searchResult;
            _searchResult.SearchResultChanged += SearchResult_NodeChanged;
        }


        /// <summary>
        /// 検索結果変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchResult_NodeChanged(object sender, NeeLaboratory.IO.Search.SearchResultChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NeeLaboratory.IO.Search.NodeChangedAction.Add:
                    {
                        Watcher_Creaded(CreateFolderItem(e.Content));
                    }
                    break;
                case NeeLaboratory.IO.Search.NodeChangedAction.Remove:
                    {
                        var item = this.Items.FirstOrDefault(i => i.Path == e.Content.Path);
                        if (item != null)
                        {
                            App.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                Deleting?.Invoke(sender, new FileSystemEventArgs(WatcherChangeTypes.Deleted, Path.GetDirectoryName(e.Content.Path), Path.GetFileName(e.Content.Path)));
                                Watcher_Deleted(item);
                            }));
                        }
                    }
                    break;
                case NeeLaboratory.IO.Search.NodeChangedAction.Rename:
                    {
                        FolderItem item;
                        lock (_lock)
                        {
                            item = this.Items.FirstOrDefault(i => i.Path == e.OldPath);
                        }
                        if (item != null)
                        {
                            item.Path = e.Content.Path;
                        }
                        else
                        {
                            // リストにない項目は追加を試みる
                            Watcher_Creaded(CreateFolderItem(e.Content));
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// 並び替え
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private IEnumerable<FolderItem> Sort(IEnumerable<FolderItem> source)
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




        /// <summary>
        /// アイコンの表示更新
        /// </summary>
        /// <param name="path">指定パスの項目を更新。nullの場合全ての項目を更新</param>
        public void RefleshIcon(string path)
        {
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
        /// 廃棄処理
        /// </summary>
        public void Dispose()
        {
            TerminateWatcher();

            if (_searchResult != null)
            {
                _searchResult.Dispose();
                _searchResult = null;
            }

            if (Items != null)
            {
                BindingOperations.DisableCollectionSynchronization(Items);
                Items = null;
            }
        }


        #region FileSystemWatcher

        // ファイルシステム監視
        private FileSystemWatcher _fileSystemWatcher;

        /// <summary>
        /// ファイルシステム監視初期化
        /// </summary>
        /// <param name="path"></param>
        private void InitializeWatcher(string path)
        {
            _fileSystemWatcher = new FileSystemWatcher();

            try
            {
                _fileSystemWatcher.Path = path;
                _fileSystemWatcher.IncludeSubdirectories = false;
                _fileSystemWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName;
                _fileSystemWatcher.Created += Watcher_Creaded;
                _fileSystemWatcher.Deleted += Watcher_Deleted;
                _fileSystemWatcher.Renamed += Watcher_Renamed;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                _fileSystemWatcher.Dispose();
                _fileSystemWatcher = null;
            }
        }

        /// <summary>
        /// ファイルシステム監視終了
        /// </summary>
        private void TerminateWatcher()
        {
            if (_fileSystemWatcher != null)
            {
                _fileSystemWatcher.EnableRaisingEvents = false;
                _fileSystemWatcher.Created -= Watcher_Creaded;
                _fileSystemWatcher.Deleted -= Watcher_Deleted;
                _fileSystemWatcher.Renamed -= Watcher_Renamed;
                _fileSystemWatcher.Dispose();
                _fileSystemWatcher = null;
            }
        }

        /// <summary>
        /// ファイルシステム監視開始
        /// </summary>
        private void StartWatch()
        {
            if (_fileSystemWatcher != null)
            {
                _fileSystemWatcher.EnableRaisingEvents = true;
            }
        }


        /// <summary>
        /// ファイル生成イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Watcher_Creaded(object sender, FileSystemEventArgs e)
        {
            // FolderInfoを作成し、追加
            var item = CreateFolderItem(e.FullPath);
            if (item != null)
            {
                Watcher_Creaded(item);
            }
        }

        //
        private void Watcher_Creaded(FolderItem item)
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

        /// <summary>
        /// ファイル削除イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            FolderItem item;

            lock (_lock)
            {
                // 対象を検索し、削除する
                item = this.Items.FirstOrDefault(i => i.Path == e.FullPath);
            }

            if (item != null)
            {
                App.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    Deleting?.Invoke(sender, e);
                    Watcher_Deleted(item);
                }));
            }
        }

        //
        private void Watcher_Deleted(FolderItem item)
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


        /// <summary>
        /// ファイル名変更イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            FolderItem item;
            lock (_lock)
            {
                item = this.Items.FirstOrDefault(i => i.Path == e.OldFullPath);
            }
            if (item != null)
            {
                item.Path = e.FullPath;
            }
            else
            {
                // リストにない項目は追加を試みる
                Watcher_Creaded(sender, e);
            }
        }

        #endregion


        /// <summary>
        /// 空のFolderItemを作成
        /// </summary>
        /// <returns></returns>
        private FolderItem CreateFolderItemEmpty()
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
        private FolderItem CreateFolderItem(string path)
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
        private FolderItem CreateFolderItem(DriveInfo e)
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
        private FolderItem CreateFolderItem(DirectoryInfo e)
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
        private FolderItem CreateFolderItem(FileInfo e)
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
        private FolderItem CreateFolderItem(Utility.FileShortcut e)
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

        /// <summary>
        /// 検索結果からFolderItem作成
        /// </summary>
        /// <param name="nodeContent"></param>
        /// <returns></returns>
        public FolderItem CreateFolderItem(NeeLaboratory.IO.Search.NodeContent nodeContent)
        {
            // TODO: ショートカット対応

            if (nodeContent.FileInfo.IsDirectory)
            {
                return new FolderItem()
                {
                    Type = FolderItemType.Directory, // TODO
                    Path = nodeContent.Path,
                    LastWriteTime = nodeContent.FileInfo.LastWriteTime,
                    Length = -1,
                    Attributes = FolderItemAttribute.Directory,
                    IsReady = true
                };
            }
            else
            {
                if (Utility.FileShortcut.IsShortcut(nodeContent.Path))
                {
                    var shortcut = new Utility.FileShortcut(nodeContent.Path);
                    if (shortcut.DirectoryInfo.Exists || (shortcut.FileInfo.Exists && ArchiverManager.Current.IsSupported(shortcut.TargetPath)))
                    {
                        return CreateFolderItem(shortcut);
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (ArchiverManager.Current.IsSupported(nodeContent.Path))
                {
                    return new FolderItem()
                    {
                        Type = FolderItemType.File,
                        Path = nodeContent.Path,
                        LastWriteTime = nodeContent.FileInfo.LastWriteTime,
                        Length = nodeContent.FileInfo.Size,
                        IsReady = true
                    };
                }
                else
                {
                    return null;
                }
            }
        }
    }

}
