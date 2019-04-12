using NeeView.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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

        public FolderEntryCollection(QueryPath path, bool isActive, bool isOverlayEnabled) : base(path, isActive, isOverlayEnabled)
        {
        }

        public override async Task InitializeItemsAsync(CancellationToken token)
        {
            await Task.Run(() => InitializeItems());
        }

        private void InitializeItems()
        { 
            if (string.IsNullOrWhiteSpace(Place.SimplePath))
            {
                this.Items = new ObservableCollection<FolderItem>(DriveInfo.GetDrives().Select(e => CreateFolderItem(e)));
                return;
            }
            else
            {
                var directory = new DirectoryInfo(Place.SimplePath);

                if (!directory.Exists)
                {
                    var items = new ObservableCollection<FolderItem>();
                    items.Add(CreateFolderItemEmpty());
                    this.Items = items;
                }
                else
                {
                    try
                    {
                        var fileSystemInfos = directory.GetFileSystemInfos();

                        directory.EnumerateFileSystemInfos();

                        var fileInfos = fileSystemInfos.OfType<FileInfo>();
                        var directoryInfos = fileSystemInfos.OfType<DirectoryInfo>();

                        var shortcuts = fileInfos
                            .Where(e => FileShortcut.IsShortcut(e.FullName) && (e.Attributes & FileAttributes.Hidden) == 0)
                            .Select(e => new FileShortcut(e))
                            .Where(e => e.IsValid)
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

                        // RAR連番フィルター
                        if (BookshelfFolderList.Current.IsMultipleRarFilterEnabled)
                        {
                            var groups = archives.Select(e => new MultipleArchive(e)).GroupBy(e => e.Key);
                            var part0 = groups.Where(e => e.Key == null).Cast<IEnumerable<MultipleArchive>>().FirstOrDefault() ?? Enumerable.Empty<MultipleArchive>();
                            var part1 = groups.Where(e => e.Key != null).Select(g => g.OrderBy(e => e.PartNumber).First());
                            archives = part0.Concat(part1).Select(e => e.FolderItem).ToList();
                        }

                        var items = directories
                            .Concat(directoryShortcuts)
                            .Concat(archives)
                            .Concat(archiveShortcuts)
                            .Where(e => e != null);

                        // 除外フィルター
                        if (BookshelfFolderList.Current.ExcludeRegex != null)
                        {
                            items = items.Where(e => !BookshelfFolderList.Current.ExcludeRegex.IsMatch(e.Name));
                        }

                        var list = Sort(items).ToList();

                        if (!list.Any())
                        {
                            list.Add(CreateFolderItemEmpty());
                        }

                        this.Items = new ObservableCollection<FolderItem>(list);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        this.Items = new ObservableCollection<FolderItem>() { CreateFolderItemEmpty() };
                        return;
                    }
                }
            }

            BindingOperations.EnableCollectionSynchronization(this.Items, new object());

            if (_isStartEngine)
            {
                // フォルダー監視
                InitializeWatcher(Place.SimplePath);
                StartWatch();
            }
        }

        #endregion

        #region Properties

        public override FolderOrderClass FolderOrderClass => FolderOrderClass.Normal;

        #endregion Properties

        #region Methods

        // 分割アーカイブフィルタ用
        private class MultipleArchive
        {
            // .partXX.rar のみ対応
            private static Regex _regex = new Regex(@"^(.+)\.part(\d+)\.rar$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            public FolderItem FolderItem { get; set; }
            public string Key { get; set; }
            public int PartNumber { get; set; }

            public bool IsPart => Key != null;

            public MultipleArchive(FolderItem folderItem)
            {
                FolderItem = folderItem;
                var match = _regex.Match(folderItem.Name);
                if (match.Success)
                {
                    Key = match.Groups[1].Value;
                    PartNumber = int.Parse(match.Groups[2].Value);
                }
            }
        }

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
            RequestCreate(new QueryPath(e.FullPath));
        }

        /// <summary>
        /// ファイル削除イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            RequestDelete(new QueryPath(e.FullPath));
        }

        /// <summary>
        /// ファイル名変更イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            RequestRename(new QueryPath(e.OldFullPath), new QueryPath(e.FullPath));
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
