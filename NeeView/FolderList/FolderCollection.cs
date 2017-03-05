// Copyright (c) 2016 Mitsuhiro Ito (nee)
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
        public event EventHandler<FileSystemEventArgs> Changing;

        public event EventHandler ParameterChanged;

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
        public ObservableCollection<FolderItem> Items { get; private set; }

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
                Items = new ObservableCollection<FolderItem>(DriveInfo.GetDrives().Select(e => CreateFolderInfo(e)));
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
                    var shortcuts = directory.EnumerateFiles() // TODO: 極端に重い時がある
                        .Where(e => e.Exists && Utility.FileShortcut.IsShortcut(e.FullName) && (e.Attributes & FileAttributes.Hidden) == 0)
                        .Select(e => new Utility.FileShortcut(e))
                        .ToList();

                    var directories = directory.EnumerateDirectories()
                        .Where(e => e.Exists && (e.Attributes & FileAttributes.Hidden) == 0)
                        .Select(e => CreateFolderInfo(e))
                        .ToList();

                    var directoryShortcuts = shortcuts
                        .Where(e => e.DirectoryInfo.Exists)
                        .Select(e => CreateFolderInfo(e))
                        .ToList();

                    directories = directories.Concat(directoryShortcuts).Where(e => e != null).ToList();


                    var archives = directory.EnumerateFiles()
                        .Where(e => e.Exists && ModelContext.ArchiverManager.IsSupported(e.FullName) && (e.Attributes & FileAttributes.Hidden) == 0)
                        .Select(e => CreateFolderInfo(e))
                        .ToList();

                    var archiveShortcuts = shortcuts
                        .Where(e => e.FileInfo.Exists && ModelContext.ArchiverManager.IsSupported(e.TargetPath))
                        .Select(e => CreateFolderInfo(e))
                        .ToList();

                    archives = archives.Concat(archiveShortcuts).Where(e => e != null).ToList();


                    if (FolderOrder == FolderOrder.TimeStamp)
                    {
                        directories = directories.OrderByDescending((e) => e.LastWriteTime).ToList();
                        archives = archives.OrderByDescending((e) => e.LastWriteTime).ToList();
                    }
                    else if (FolderOrder == FolderOrder.Random)
                    {
                        var random = new Random(RandomSeed);
                        directories = directories.OrderBy(e => random.Next()).ToList();
                        archives = archives.OrderBy(e => random.Next()).ToList();
                    }
                    else
                    {
                        directories.Sort((a, b) => Win32Api.StrCmpLogicalW(a.Name, b.Name));
                        archives.Sort((a, b) => Win32Api.StrCmpLogicalW(a.Name, b.Name));
                    }

                    var list = directories.Concat(archives).ToList();

                    if (list.Count <= 0)
                    {
                        list.Add(new FolderItem() { Path = Place + "\\.", Attributes = FolderItemAttribute.Empty });
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


        /// <summary>
        /// ファイルシステム監視
        /// </summary>
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
            var item = CreateFolderInfo(e.FullPath);
            if (item != null)
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
                Changing?.Invoke(sender, e);
                Items.Remove(item);
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
        }

        /// <summary>
        /// パスからFolderItemを作成
        /// </summary>
        /// <param name="path">パス</param>
        /// <returns>FolderItem。生成できなかった場合はnull</returns>
        private FolderItem CreateFolderInfo(string path)
        {
            // directory
            var directory = new DirectoryInfo(path);
            if (directory.Exists)
            {
                return CreateFolderInfo(directory);
            }

            // file
            var file = new FileInfo(path);
            if (file.Exists)
            {
                // .lnk
                if (Utility.FileShortcut.IsShortcut(path))
                {
                    var shortcut = new Utility.FileShortcut(file);
                    return CreateFolderInfo(shortcut);
                }
                else
                {
                    return CreateFolderInfo(file);
                }
            }

            return null;
        }


        /// <summary>
        /// DriveInfoからFodlerItem作成
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private FolderItem CreateFolderInfo(DriveInfo e)
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
        private FolderItem CreateFolderInfo(DirectoryInfo e)
        {
            if (e != null && e.Exists && (e.Attributes & FileAttributes.Hidden) == 0)
            {
                return new FolderItem()
                {
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
        private FolderItem CreateFolderInfo(FileInfo e)
        {
            if (e != null && e.Exists && ModelContext.ArchiverManager.IsSupported(e.FullName) && (e.Attributes & FileAttributes.Hidden) == 0)
            {
                return new FolderItem()
                {
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
        private FolderItem CreateFolderInfo(Utility.FileShortcut e)
        {
            FolderItem info = null;

            if (e != null && e.Source.Exists && (e.Source.Attributes & FileAttributes.Hidden) == 0)
            {
                if (e.DirectoryInfo.Exists)
                {
                    info = CreateFolderInfo(e.DirectoryInfo);
                }
                else if (e.FileInfo.Exists)
                {
                    info = CreateFolderInfo(e.FileInfo);
                }
            }

            if (info != null)
            {
                info.Path = e.Path;
                info.TargetPath = e.TargetPath;
                info.Attributes = info.Attributes | FolderItemAttribute.Shortcut;
            }

            return info;
        }
    }

    #endregion
}
