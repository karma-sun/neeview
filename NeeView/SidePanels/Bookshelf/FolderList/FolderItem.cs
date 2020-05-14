using NeeLaboratory.ComponentModel;
using NeeLaboratory.IO;
using NeeView.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    [Flags]
    public enum FolderItemAttribute
    {
        None = 0,
        Directory = (1 << 0),
        Drive = (1 << 1),
        Empty = (1 << 2),
        Shortcut = (1 << 3),
        ArchiveEntry = (1 << 4),
        Bookmark = (1 << 5),
        QuickAccess = (1 << 6),
        System = (1 << 7),
        ReadOnly = (1 << 8),
        Playlist = (1 << 9),
        PlaylistMember = (1 << 10),
    }

    /// <summary>
    /// FolderItemAttribute メソッド拡張
    /// </summary>
    public static class FolderItemAttributeExtensions
    {
        /// <summary>
        /// いずれかのフラグのONをチェック
        /// </summary>
        public static bool AnyFlag(this FolderItemAttribute self, FolderItemAttribute value)
        {
            return (self & value) != 0;
        }
    }



    /// <summary>
    /// アイコンオーバーレイ
    /// </summary>
    public enum FolderItemIconOverlay
    {
        Uninitialized,
        None,
        Checked,
        Star,
        Disable,
    }

    /// <summary>
    /// FolderItemの種類。ソート用
    /// </summary>
    public enum FolderItemType
    {
        Empty,
        Directory,
        DirectoryShortcut,
        Playlist,
        PlaylistShortcut,
        File,
        FileShortcut,
    }

    /// <summary>
    /// フォルダー情報
    /// フォルダーリストの１項目の情報 
    /// </summary>
    public abstract class FolderItem : BindableBase, IHasPage, IHasName
    {
        private bool _isOverlayEnabled;

        private QueryPath _place;
        private string _name;
        private string _dispName;
        private QueryPath _targetPath;
        private QueryPath _entityPath;
        private bool _isReady;
        private bool _isRecursived;
        private FolderItemIconOverlay _iconOverlay = FolderItemIconOverlay.Uninitialized;
        private bool _isVisibled;


        public FolderItem(bool isOverlayEnabled)
        {
            _isOverlayEnabled = isOverlayEnabled;
        }

        #region Properties

        /// <summary>
        /// このFolderItemと関係のある情報
        /// </summary>
        public object Source { get; set; }

        // 属性
        public FolderItemAttribute Attributes { get; set; }

        public bool IsDirectory => (Attributes & FolderItemAttribute.Directory) == FolderItemAttribute.Directory;
        public bool IsShortcut => (Attributes & FolderItemAttribute.Shortcut) == FolderItemAttribute.Shortcut;
        public bool IsPlaylist => (Attributes & FolderItemAttribute.Playlist) == FolderItemAttribute.Playlist;

        // 種類。ソート用
        public FolderItemType Type { get; set; }

        // このアイテムが存在しているディレクトリ。ほぼ未使用
        public QueryPath Place
        {
            get { return _place; }
            set { SetProperty(ref _place, value); }
        }

        // アイテム名
        public string Name
        {
            get { return _name; }
            set
            {
                if (SetProperty(ref _name, value))
                {
                    RaisePropertyChanged(nameof(DispName));
                    RaisePropertyChanged(nameof(Detail));
                }
            }
        }

        // 表示名
        public string DispName
        {
            get { return _dispName ?? (IsHideExtension() ? System.IO.Path.GetFileNameWithoutExtension(_name) : _name); }
            set { SetProperty(ref _dispName, value); }
        }

        /// <summary>
        /// 実体へのパス。ショートカットはそのまま
        /// </summary>
        public QueryPath TargetPath
        {
            get { return _targetPath; }
            set { if (_targetPath != value) { _targetPath = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// ショートカット先を反映した実体パス
        /// </summary>
        public QueryPath EntityPath
        {
            get { return _entityPath ?? (_entityPath = TargetPath?.ToEntityPath()); }
        }


        // 最終更新日
        public DateTime LastWriteTime { get; set; }

        // 登録日時
        public DateTime EntryTime { get; set; }

        // ファイルサイズ
        public long Length { get; set; }

        /// <summary>
        /// 編集可能
        /// </summary>
        public bool IsEditable => (this.Attributes & (FolderItemAttribute.Empty | FolderItemAttribute.Drive | FolderItemAttribute.ArchiveEntry | FolderItemAttribute.ReadOnly | FolderItemAttribute.System)) == 0;

        /// <summary>
        /// アクセス可能？(ドライブの準備ができているか)
        /// </summary>
        public bool IsReady
        {
            get { return _isReady; }
            set
            {
                if (SetProperty(ref _isReady, value))
                {
                    UpdateOverlay();
                    RaisePropertyChanged(nameof(IconOverlay));
                }
            }
        }


        /// <summary>
        /// フォルダーリストのコンテキストメニュー用
        /// </summary>
        public bool IsRecursived
        {
            get { return _isRecursived; }
            set { if (_isRecursived != value) { _isRecursived = value; RaisePropertyChanged(); } }
        }


        // アイコンオーバーレイの種類を返す
        public FolderItemIconOverlay IconOverlay
        {
            get
            {
                if (_iconOverlay == FolderItemIconOverlay.Uninitialized)
                {
                    UpdateOverlay();
                }
                return _iconOverlay;
            }
        }

        public virtual string Detail => Name;

        public abstract IThumbnail Thumbnail { get; }

        /// <summary>
        /// 現在ブック表示用
        /// </summary>
        public bool IsVisibled
        {
            get { return _isVisibled; }
            set { SetProperty(ref _isVisibled, value); }
        }

        #endregion Properties

        #region Methods

        public virtual Page GetPage() => null;

        public bool IsDrive() => (Attributes & FolderItemAttribute.Drive) == FolderItemAttribute.Drive;
        public bool IsEmpty() => (Attributes & FolderItemAttribute.Empty) == FolderItemAttribute.Empty;
        public bool IsDisable() => IsDirectory && !IsReady;
        public bool IsBookmark() => (Attributes & FolderItemAttribute.Bookmark) == FolderItemAttribute.Bookmark;
        public bool IsFileSystem() => (Attributes & (FolderItemAttribute.System | FolderItemAttribute.Bookmark | FolderItemAttribute.QuickAccess | FolderItemAttribute.Empty | FolderItemAttribute.None)) == 0;

        // FolderCollection上のパス
        public QueryPath GetFolderCollectionPath() => _place.ReplacePath(LoosePath.Combine(_place.Path, _name));

        // 推定ディレクトリ
        public bool IsDirectoryMaybe() => IsDirectory || IsPlaylist || Length == -1;

        // 拡張子の非表示
        public bool IsHideExtension() => IsShortcut || IsPlaylist;

        /// <summary>
        /// IsRecursived 更新
        /// </summary>
        public void UpdateIsRecursived(bool isDefaultRecursive)
        {
            var option = isDefaultRecursive ? BookLoadOption.DefaultRecursive : BookLoadOption.None;
            var memento = BookHub.Current.GetLastestBookMemento(EntityPath.SimplePath, option);
            this.IsRecursived = memento.IsRecursiveFolder;
        }

        private void UpdateOverlay()
        {
            if (_isOverlayEnabled)
            {
                if (IsDisable())
                    _iconOverlay = FolderItemIconOverlay.Disable;
                else if (Config.Current.Bookshelf.IsVisibleBookmarkMark && BookmarkCollection.Current.Contains(EntityPath.SimplePath))
                    _iconOverlay = FolderItemIconOverlay.Star;
                else if (Config.Current.Bookshelf.IsVisibleHistoryMark && BookHistoryCollection.Current.Contains(EntityPath.SimplePath))
                    _iconOverlay = FolderItemIconOverlay.Checked;
                else
                    _iconOverlay = FolderItemIconOverlay.None;
            }
            else
            {
                _iconOverlay = FolderItemIconOverlay.None;
            }
        }

        // アイコンオーバーレイの変更を通知
        public void NotifyIconOverlayChanged()
        {
            UpdateOverlay();
            RaisePropertyChanged(nameof(IconOverlay));
        }

        /// <summary>
        /// フォルダーとして展開可能？
        /// </summary>
        public bool CanOpenFolder()
        {
            if (IsDirectory || IsPlaylist)
            {
                return true;
            }

            var archiveType = ArchiverManager.Current.GetSupportedType(EntityPath.SimplePath, false);
            if (IsFileSystem() && Config.Current.System.ArchiveRecursiveMode != ArchiveEntryCollectionMode.IncludeSubArchives && archiveType.IsRecursiveSupported())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// ファイルシステムから削除可能？
        /// </summary>
        /// <returns></returns>
        public bool CanRemove()
        {
            if (!IsEditable)
            {
                return false;
            }
            else if (IsFileSystem())
            {
                return Config.Current.System.IsFileWriteAccessEnabled;
            }
            else if (Attributes.HasFlag(FolderItemAttribute.Bookmark))
            {
                return true;
            }

            return false;
        }

        public virtual string GetNote(FolderOrder order)
        {
            return null;
        }

        public override string ToString()
        {
            return $"FolderItem: {Name}, Place={Place}, TargetPath={TargetPath}";
        }

        #endregion
    }

    /// <summary>
    /// 標準 FolderItem
    /// </summary>
    public class FileFolderItem : FolderItem, IDisposable
    {
        private Page _archivePage;


        public FileFolderItem(bool isOverlayEnabled) : base(isOverlayEnabled)
        {
        }


        /// <summary>
        /// サムネイルロード完了時のイベント (開発用)
        /// </summary>
        public event EventHandler ThumbnailLoaded;


        /// <summary>
        /// サムネイル.
        /// アクセスすることで自動でサムネイル読み込み処理が開始される
        /// </summary>
        public override IThumbnail Thumbnail => GetArchivePage()?.Thumbnail;

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_archivePage != null)
                    {
                        _archivePage.Dispose();
                    }
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion


        public override Page GetPage()
        {
            return GetArchivePage();
        }

        private Page GetArchivePage()
        {
            if (_archivePage == null)
            {
                _archivePage = new Page("", new ArchiveContent(TargetPath.SimplePath));
                _archivePage.Thumbnail.IsCacheEnabled = true;
                _archivePage.Thumbnail.Touched += Thumbnail_Touched;
            }
            return _archivePage;
        }

        private void Thumbnail_Touched(object sender, EventArgs e)
        {
            var thumbnail = (Thumbnail)sender;
            BookThumbnailPool.Current.Add(thumbnail);
            ThumbnailLoaded?.Invoke(sender, e);
        }

        public override string GetNote(FolderOrder order)
        {
            if (!IsFileSystem() && IsDirectory) return null;

            string GetLastWriteTimeString() => (LastWriteTime != default ? $"{LastWriteTime:yyyy/MM/dd HH:mm:ss}   " : "");

            switch (order)
            {
                default:
                    return GetLastWriteTimeString() + (Length > 0 ? FileSizeToStringConverter.ByteToDispString(Length) : "");

                case FolderOrder.FileType:
                case FolderOrder.FileTypeDescending:
                    return GetLastWriteTimeString() + (IsDirectoryMaybe() ? Properties.Resources.WordFolder : LoosePath.GetExtension(Name));

                case FolderOrder.Path:
                case FolderOrder.PathDescending:
                    return SidePanelProfile.Current.GetDecoratePlaceName(LoosePath.GetDirectoryName(TargetPath.SimplePath));
            }
        }
    }

    /// <summary>
    /// Drive FolderItem
    /// </summary>
    public class DriveFolderItem : FolderItem
    {
        private IThumbnail _thumbnail;

        public DriveFolderItem(DriveInfo driveInfo, bool isOverlayEnabled) : base(isOverlayEnabled)
        {
            _thumbnail = new DriveThumbnail(driveInfo.Name);
        }

        public override IThumbnail Thumbnail => _thumbnail;
    }

    /// <summary>
    /// 固定表示用FolderItem.
    /// </summary>
    public class ConstFolderItem : FolderItem
    {
        private IThumbnail _thumbnail;

        public ConstFolderItem(IThumbnail thumbnail, bool isOverlayEnabled) : base(isOverlayEnabled)
        {
            _thumbnail = thumbnail;
        }

        public override IThumbnail Thumbnail => _thumbnail;
    }
}
