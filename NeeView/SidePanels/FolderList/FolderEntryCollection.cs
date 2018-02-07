// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php


using NeeView.IO;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// フォルダーエントリコレクション
    /// </summary>
    public class FolderEntryCollection : FolderCollection, IDisposable
    {
        #region Fields
        
        // ファイルシステム監視
        private FileSystemWatcher _fileSystemWatcher;

        #endregion

        #region Constructors

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="place"></param>
        public FolderEntryCollection(string place) : base(place)
        {
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
                    items.Add(new FolderItem() { Path = Place + "\\.", Attributes = FolderItemAttribute.Empty });
                    this.Items = items;
                }
                else
                {
                    try
                    {
                        var fileSystemInfos = directory.GetFileSystemInfos();

                        var fileInfos = fileSystemInfos.OfType<FileInfo>();
                        var directoryInfos = fileSystemInfos.OfType<DirectoryInfo>();

                        var shortcuts = fileInfos
                            .Where(e => FileShortcut.IsShortcut(e.FullName) && (e.Attributes & FileAttributes.Hidden) == 0)
                            .Select(e => new FileShortcut(e))
                            .ToList();

                        var directories = directoryInfos
                            .Where(e => (e.Attributes & FileAttributes.Hidden) == 0)
                            .Select(e => CreateFolderItem(e))
                            .ToList();

                        var directoryShortcuts = shortcuts
                            .Where(e => e.Target.Exists && (e.Target.Attributes & FileAttributes.Directory) != 0)
                            .Select(e => CreateFolderItem(e))
                            .ToList();

                        var archives = fileInfos
                            .Where(e => ArchiverManager.Current.IsSupported(e.FullName) && (e.Attributes & FileAttributes.Hidden) == 0)
                            .Select(e => CreateFolderItem(e))
                            .ToList();

                        var archiveShortcuts = shortcuts
                            .Where(e => e.Target.Exists && (e.Target.Attributes & FileAttributes.Directory) == 0 && ArchiverManager.Current.IsSupported(e.TargetPath))
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
                    catch(Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        this.Items = new ObservableCollection<FolderItem>() { CreateFolderItemEmpty() };
                        return;
                    }
                }
            }

            BindingOperations.EnableCollectionSynchronization(this.Items, new object());

            // フォルダー監視
            InitializeWatcher(Place);
            StartWatch();
        }

        #endregion

        #region Methods

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
                _fileSystemWatcher.Error += Watcher_Error;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                _fileSystemWatcher.Dispose();
                _fileSystemWatcher = null;
            }
        }

        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            var ex = e.GetException();
            Debug.WriteLine($"FileSystemWatcher Error!! : {ex.ToString()} : {ex.Message}");

            // recoverty...
            ////var path = _fileSystemWatcher.Path;
            ////TerminateWatcher();
            ////InitializeWatcher(path);
        }

        /// <summary>
        /// ファイルシステム監視終了
        /// </summary>
        private void TerminateWatcher()
        {
            if (_fileSystemWatcher != null)
            {
                _fileSystemWatcher.EnableRaisingEvents = false;
                _fileSystemWatcher.Error -= Watcher_Error;
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
            RequestCreate(e.FullPath);
        }

        /// <summary>
        /// ファイル削除イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            RequestDelete(e.FullPath);
        }
        
        /// <summary>
        /// ファイル名変更イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            RequestRename(e.OldFullPath, e.FullPath);
        }

        #endregion

        #region IDisposable Support

        private bool _disposedValue = false; // 重複する呼び出しを検出するには

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    TerminateWatcher();
                }

                _disposedValue = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
