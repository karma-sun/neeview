﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
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

namespace NeeView
{
    /// <summary>
    /// FolderItemコレクション
    /// </summary>
    public class FolderCollection : IDisposable
    {
        public event EventHandler<FileSystemEventArgs> Deleting;

        public event EventHandler ParameterChanged;

        /// <summary>
        /// 追加されたファイルの挿入方法
        /// </summary>
        public static bool IsInsertAddFile => Preference.Current.folderlist_addfile_insert;

        /// <summary>
        /// FolderCollection Parameter
        /// </summary>
        public FolderCollectionParameter FolderCollectionParameter { get; private set; }

        // indexer
        public FolderItem this[int index]
        {
            get { Debug.Assert(index >= 0 && index < Items.Count); return Items[index]; }
            private set { Items[index] = value; }
        }

        /// <summary>
        /// Collection本体
        /// </summary>
        private ObservableCollection<FolderItem> _Items;
        public ObservableCollection<FolderItem> Items
        {
            get { return _Items; }
            private set { _Items = value; }
        }

        /// <summary>
        /// フォルダーの場所
        /// </summary>
        public string Place { get; private set; }


        /// <summary>
        /// フォルダーの並び順
        /// </summary>
        private FolderOrder FolderOrder => FolderCollectionParameter.FolderOrder;

        /// <summary>
        /// シャッフル用ランダムシード
        /// </summary>
        private int RandomSeed => FolderCollectionParameter.RandomSeed;

        /// <summary>
        /// 有効判定
        /// </summary>
        public bool IsValid => Items != null;

        /// <summary>
        /// 更新が必要？
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public bool IsDarty(FolderCollectionParameter folder)
        {
            return (Place != folder.Path || FolderOrder != folder.FolderOrder || RandomSeed != folder.RandomSeed);
        }

        /// <summary>
        /// 更新が必要？
        /// </summary>
        /// <returns></returns>
        public bool IsDarty()
        {
            return IsDarty(new FolderCollectionParameter(Place));
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
            this.Place = place;

            this.FolderCollectionParameter = new FolderCollectionParameter(place);
            this.FolderCollectionParameter.PropertyChanged += (s, e) => ParameterChanged?.Invoke(s, null);
        }

        /// <summary>
        /// リスト生成
        /// </summary>
        public void Initialize()
        {
            if (Items != null)
            {
                BindingOperations.DisableCollectionSynchronization(this.Items);
            }

            if (string.IsNullOrWhiteSpace(Place))
            {
                Items = new ObservableCollection<FolderItem>(DriveInfo.GetDrives().Select(e => CreateFolderItem(e)));
            }
            else
            {
                var directory = new DirectoryInfo(Place);

                if (!directory.Exists)
                {
                    var items = new ObservableCollection<FolderItem>();
                    items.Add(new FolderItem() { Path = Place + "\\.", Attributes = FolderItemAttribute.Empty | FolderItemAttribute.DirectoryNoFound });
                    Items = items;
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
                        .Where(e => e.Exists && ModelContext.ArchiverManager.IsSupported(e.FullName) && (e.Attributes & FileAttributes.Hidden) == 0)
                        .Select(e => CreateFolderItem(e))
                        .ToList();

                    var archiveShortcuts = shortcuts
                        .Where(e => e.FileInfo.Exists && ModelContext.ArchiverManager.IsSupported(e.TargetPath))
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

                    Items = new ObservableCollection<FolderItem>(list);
                }
            }

            BindingOperations.EnableCollectionSynchronization(this.Items, new object());

            if (Place != null)
            {
                InitializeWatcher(Place);
                StartWatch();
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
            if (this.Items.Count == 1 && this.Items.First().Type == FolderItemType.Empty)
            {
                this.Items.RemoveAt(0);
                this.Items.Add(item);
            }
            else if (FolderOrder == FolderOrder.Random)
            {
                this.Items.Add(item);
            }
            else if (IsInsertAddFile)
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

        /// <summary>
        /// ファイル削除イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            // 対象を検索し、削除する
            var item = this.Items.FirstOrDefault(i => i.Path == e.FullPath);
            if (item != null)
            {
                Deleting?.Invoke(sender, e);
                Watcher_Deleted(item);
            }
        }

        private void Watcher_Deleted(FolderItem item)
        {
            Items.Remove(item);

            if (this.Items.Count == 0)
            {
                this.Items.Add(CreateFolderItemEmpty());
            }
        }


        /// <summary>
        /// ファイル名変更イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            var item = this.Items.FirstOrDefault(i => i.Path == e.OldFullPath);
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
            if (e != null && e.Exists && ModelContext.ArchiverManager.IsSupported(e.FullName) && (e.Attributes & FileAttributes.Hidden) == 0)
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
    }

}