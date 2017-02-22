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
    [Flags]
    public enum FolderInfoAttribute
    {
        None = 0,
        Directory = (1 << 0),
        Drive = (1 << 1),
        DriveNotReady = (1 << 2),
        Empty = (1 << 3),
        DirectoryNoFound = (1 << 4),
        Shortcut = (1 << 5),
    }

    public enum FolderInfoIconOverlay
    {
        Uninitialized,
        None,
        Disable,
        Checked,
        Star,
        Pagemark,
    }

    // フォルダ情報
    public class FolderInfo : INotifyPropertyChanged, IHasPage
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        public FolderInfoAttribute Attributes { get; set; }


        /// <summary>
        /// Path property.
        /// </summary>
        private string _Path;
        public string Path
        {
            get { return _Path; }
            set { if (_Path != value) { _Path = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(Name)); } }
        }

        /// <summary>
        /// TargetPath property.
        /// 実体へのパス。nullの場合はパスと同じ
        /// </summary>
        private string _targetPath;
        public string TargetPath
        {
            get { return _targetPath ?? Path; }
            set { if (_targetPath != value) { _targetPath = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// 最終更新日。ソート用
        /// </summary>
        public DateTime LastWriteTime { get; set; }

        public string ParentPath => System.IO.Path.GetDirectoryName(Path);

        public bool IsDrive => (Attributes & FolderInfoAttribute.Drive) == FolderInfoAttribute.Drive;
        public bool IsDirectory => (Attributes & FolderInfoAttribute.Directory) == FolderInfoAttribute.Directory;
        public bool IsEmpty => (Attributes & FolderInfoAttribute.Empty) == FolderInfoAttribute.Empty;
        public bool IsDirectoryNotFound => (Attributes & FolderInfoAttribute.DirectoryNoFound) == FolderInfoAttribute.DirectoryNoFound;
        public bool IsShortcut => (Attributes & FolderInfoAttribute.Shortcut) == FolderInfoAttribute.Shortcut;

        public bool IsReady { get; set; }

        public static bool IsVisibleHistoryMark { get; set; } = true;
        public static bool IsVisibleBookmarkMark { get; set; } = true;

        // パスの存在チェック
        public bool IsExist()
        {
            return IsDirectory ? Directory.Exists(Path) : File.Exists(Path);
        }

        // アイコンオーバーレイの種類を返す
        private FolderInfoIconOverlay _iconOverlay = FolderInfoIconOverlay.Uninitialized;
        public FolderInfoIconOverlay IconOverlay
        {
            get
            {
                if (_iconOverlay == FolderInfoIconOverlay.Uninitialized)
                {
                    UpdateOverlay();
                }
                return _iconOverlay;
            }
        }

        private void UpdateOverlay()
        {
            var unit = ModelContext.BookMementoCollection.Find(TargetPath);

            //if (IsVisibleBookmarkMark && unit?.PagemarkNode != null)
            //    IconOverlay = FolderInfoIconOverlay.Pagemark;
            if (IsVisibleBookmarkMark && unit?.BookmarkNode != null)
                _iconOverlay = FolderInfoIconOverlay.Star;
            else if (IsVisibleHistoryMark && unit?.HistoryNode != null)
                _iconOverlay = FolderInfoIconOverlay.Checked;
            else if (IsDirectory && !IsReady)
                _iconOverlay = FolderInfoIconOverlay.Disable;
            else
                _iconOverlay = FolderInfoIconOverlay.None;
        }

        public bool IsOverlayStar => IconOverlay == FolderInfoIconOverlay.Star;
        public bool IsOverlayChecked => IconOverlay == FolderInfoIconOverlay.Checked;
        public bool IsOverlayDisable => IconOverlay == FolderInfoIconOverlay.Disable;

        // アイコンオーバーレイの変更を通知
        public void NotifyIconOverlayChanged()
        {
            UpdateOverlay();

            RaisePropertyChanged(nameof(IconOverlay));

            RaisePropertyChanged(nameof(IsOverlayStar));
            RaisePropertyChanged(nameof(IsOverlayChecked));
            RaisePropertyChanged(nameof(IsOverlayDisable));
        }

        private BitmapSource _icon;
        public BitmapSource Icon
        {
            get
            {
                if (_icon == null && !IsEmpty)
                {
                    _icon = Utility.FileInfo.GetTypeIconSource(TargetPath, Utility.FileInfo.IconSize.Normal);
                }
                return _icon;
            }
        }

        private BitmapSource _iconSmall;
        public BitmapSource IconSmall
        {
            get
            {
                if (_iconSmall == null && !IsEmpty)
                {
                    _iconSmall = Utility.FileInfo.GetTypeIconSource(TargetPath, Utility.FileInfo.IconSize.Small);
                }
                return _iconSmall;
            }
        }

        public string Name
        {
            get
            {
                if ((Attributes & FolderInfoAttribute.Drive) == FolderInfoAttribute.Drive)
                {
                    return Path;
                }
                else if (IsEmpty)
                {
                    return IsDirectoryNotFound ? "フォルダが存在しません" : "表示できるファイルはありません";
                }
                else
                {
                    return IsShortcut ? System.IO.Path.GetFileNameWithoutExtension(Path) : System.IO.Path.GetFileName(Path);
                }
            }
        }

        public static event EventHandler<Page> ThumbnailChanged;

        // サムネイル用
        #region Property: ArchivePage
        private ArchivePage _archivePage;
        public ArchivePage ArchivePage
        {
            get
            {
                if (_archivePage == null && !IsDrive && !IsEmpty)
                {
                    _archivePage = new ArchivePage(TargetPath);
                    _archivePage.ThumbnailChanged += (s, e) => ThumbnailChanged?.Invoke(this, _archivePage);
                }
                return _archivePage;
            }
            set { _archivePage = value; RaisePropertyChanged(); }
        }
        #endregion

        public Page GetPage()
        {
            return ArchivePage;
        }
    }


    /// <summary>
    /// フォルダー情報
    /// </summary>
    public class Folder : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
        #endregion

        public string Path { get; set; }


        #region Property: FolderOrder
        private FolderOrder _folderOrder;
        public FolderOrder FolderOrder
        {
            get { return _folderOrder; }
            set { _folderOrder = value; Save(); s_randomSeed = new Random().Next(); RaisePropertyChanged(); }
        }
        #endregion

        public int RandomSeed { get; set; }

        private static int s_randomSeed;

        static Folder()
        {
            s_randomSeed = new Random().Next();
        }


        public Folder(string path)
        {
            Path = path;
            Load();
            RandomSeed = s_randomSeed;
        }

        public void Save()
        {
            ModelContext.BookHistory.SetFolderOrder(Path, _folderOrder);
        }

        private void Load()
        {
            _folderOrder = ModelContext.BookHistory.GetFolderOrder(Path);
        }

        public Folder Clone()
        {
            return (Folder)this.MemberwiseClone();
        }
    }



    /// <summary>
    /// 
    /// </summary>
    public class FolderCollection : IDisposable
    {
        public event EventHandler<FileSystemEventArgs> Changing;
        public event EventHandler<FileSystemEventArgs> Changed;

        //
        private Folder _folder;
        public Folder Folder
        {
            get { return _folder; }
            set
            {
                _folder = value;
                Folder.PropertyChanged += (s, e) => Changed?.Invoke(s, null);
            }
        }

        // indexer
        public FolderInfo this[int index]
        {
            get { Debug.Assert(index >= 0 && index < Items.Count); return Items[index]; }
            private set { Items[index] = value; }
        }

        public ObservableCollection<FolderInfo> Items { get; private set; }

        public string Place { get; set; }

        public string ParentPlace => Path.GetDirectoryName(Place);

        private FolderOrder FolderOrder => Folder.FolderOrder;

        private int RandomSeed => Folder.RandomSeed;

        //
        public bool IsValid => Items != null;


        //
        public bool IsDarty(Folder folder)
        {
            return (Place != folder.Path || FolderOrder != folder.FolderOrder || RandomSeed != folder.RandomSeed);
        }

        //
        public bool IsDarty()
        {
            return IsDarty(new Folder(Place));
        }


        //
        private string _currentPlace;

        //
        public int SelectedIndex => IndexOfPath(_currentPlace);

        //
        public string SelectedPath => _currentPlace;

        //
        public int IndexOfPath(string path)
        {
            var item = Items.FirstOrDefault(e => e.Path == path);
            return (item != null) ? Items.IndexOf(item) : -1;
        }

        //
        public bool Contains(string path)
        {
            return Items.Any(e => e.Path == path);
        }

        //
        public void Update(string path)
        {
            if (Items != null)
            {
                BindingOperations.DisableCollectionSynchronization(this.Items);
            }

            _currentPlace = path ?? _currentPlace;

            if (string.IsNullOrWhiteSpace(Place))
            {
                Items = new ObservableCollection<FolderInfo>(DriveInfo.GetDrives().Select(e => CreateFolderInfo(e)));
            }
            else
            {
                var directory = new DirectoryInfo(Place);

                if (!directory.Exists)
                {
                    var items = new ObservableCollection<FolderInfo>();
                    items.Add(new FolderInfo() { Path = Place + "\\.", Attributes = FolderInfoAttribute.Empty | FolderInfoAttribute.DirectoryNoFound });
                    Items = items;
                }
                else
                {
                    var shortcuts = directory.EnumerateFiles()
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
                        list.Add(new FolderInfo() { Path = Place + "\\.", Attributes = FolderInfoAttribute.Empty });
                    }

                    Items = new ObservableCollection<FolderInfo>(list);
                }
            }


            BindingOperations.EnableCollectionSynchronization(this.Items, new object());

            if (Place != null)
            {
                InitializeWatcher(Place);
                StartWatch();
            }
        }

        // アイコンの表示更新
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

        // 廃棄処理
        public void Dispose()
        {
            TerminateWatcher();
        }


        #region FileSystemWatcher

        // ファイル変更監視
        private FileSystemWatcher _fileSystemWatcher;

        //
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



        //
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

        // フォルダ監視開始
        private void StartWatch()
        {
            if (_fileSystemWatcher != null)
            {
                _fileSystemWatcher.EnableRaisingEvents = true;
            }
        }


        private void Watcher_Creaded(object sender, FileSystemEventArgs e)
        {
            // FolderInfoを作成し、追加
            var item = CreateFolderInfo(e.FullPath);
            if (item != null)
            {
                this.Items.Add(item);
            }
        }

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




        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            // 対象を検索
            var item = this.Items.FirstOrDefault(i => i.Path == e.OldFullPath);
            if (item != null)
            {
                item.Path = e.FullPath;
            }
        }



        //
        private FolderInfo CreateFolderInfo(string path)
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


        private FolderInfo CreateFolderInfo(DriveInfo e)
        {
            if (e != null)
            {
                return new FolderInfo()
                {
                    Path = e.Name,
                    Attributes = FolderInfoAttribute.Directory | FolderInfoAttribute.Drive,
                    IsReady = e.IsReady,
                };
            }
            else
            {
                return null;
            }
        }

        //
        private FolderInfo CreateFolderInfo(DirectoryInfo e)
        {
            if (e != null && e.Exists && (e.Attributes & FileAttributes.Hidden) == 0)
            {
                return new FolderInfo()
                {
                    Path = e.FullName,
                    LastWriteTime = e.LastWriteTime,
                    Attributes = FolderInfoAttribute.Directory,
                    IsReady = true
                };
            }
            else
            {
                return null;
            }
        }

        //
        private FolderInfo CreateFolderInfo(FileInfo e)
        {
            if (e != null && e.Exists && ModelContext.ArchiverManager.IsSupported(e.FullName) && (e.Attributes & FileAttributes.Hidden) == 0)
            {
                return new FolderInfo()
                {
                    Path = e.FullName,
                    LastWriteTime = e.LastWriteTime,
                    IsReady = true
                };
            }
            else
            {
                return null;
            }
        }

        //
        private FolderInfo CreateFolderInfo(Utility.FileShortcut e)
        {
            FolderInfo info = null;

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
                info.Attributes = info.Attributes | FolderInfoAttribute.Shortcut;
            }

            return info;
        }

    }

    #endregion
}
