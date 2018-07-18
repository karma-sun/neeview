using NeeLaboratory.ComponentModel;
using NeeLaboratory.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        Pagemark = (1 << 6),
    }

    /// <summary>
    /// FolderItemAttribute メソッド拡張
    /// </summary>
    public static class FolderItemAttributeExtensions
    {
        /// <summary>
        /// いずれかのフラグのONをチェック
        /// </summary>
        /// <param name="self"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool AnyFlag(this FolderItemAttribute self, FolderItemAttribute value)
        {
            return (self & value) != 0;
        }
    }

    public enum FolderItemIconOverlay
    {
        Uninitialized,
        None,
        Checked,
        Star,
    }

    //
    public enum FolderItemType
    {
        Empty,
        Directory,
        DirectoryShortcut,
        File,
        FileShortcut,
        ArchiveEntry,
    }


    /// <summary>
    /// フォルダー情報
    /// フォルダーリストの１項目の情報 
    /// </summary>
    public class FolderItem : BindableBase, IBookListItem
    {
        // TODO: プロパティ多すぎ！

        #region Properties

        public object Source { get; set; }

        public FolderItemAttribute Attributes { get; set; }

        // 種類。ソート用
        public FolderItemType Type { get; set; }

        private string _place;
        public string Place
        {
            get { return _place; }
            set
            {
                if (SetProperty(ref _place, value))
                {
                    RaisePropertyChanged(nameof(Path));
                }
            }
        }

        private string _name;
        public virtual string Name
        {
            get
            {
                if ((Attributes & FolderItemAttribute.Drive) == FolderItemAttribute.Drive)
                {
                    return Path;
                }
                else if (IsEmpty)
                {
                    return Properties.Resources.NotifyNoFiles;
                }
                else
                {
                    return IsShortcut ? System.IO.Path.GetFileNameWithoutExtension(_name) : _name;
                }
            }
            set
            {
                if (SetProperty(ref _name, value))
                {
                    RaisePropertyChanged(nameof(Path));
                }
            }
        }

        // パス
        public string Path => LoosePath.Combine(_place, _name);

        // 実体へのパス。nullの場合は Path と同じ
        private string _targetPath;
        public string TargetPath
        {
            get { return _targetPath ?? Path; }
            set { if (_targetPath != value) { _targetPath = value; RaisePropertyChanged(); } }
        }

        private ArchiveEntry _archiveEntry;
        public ArchiveEntry ArchiveEntry
        {
            get { return _archiveEntry; }
            set { if (_archiveEntry != value) { _archiveEntry = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// 最終更新日
        /// </summary>
        public DateTime LastWriteTime { get; set; }

        /// <summary>
        /// ファイルサイズ
        /// </summary>
        public long Length { get; set; }

        public bool IsDrive => (Attributes & FolderItemAttribute.Drive) == FolderItemAttribute.Drive;
        public bool IsDirectory => (Attributes & FolderItemAttribute.Directory) == FolderItemAttribute.Directory;
        public bool IsFile => !IsDirectory && !IsEmpty;
        public bool IsEmpty => (Attributes & FolderItemAttribute.Empty) == FolderItemAttribute.Empty;
        public bool IsShortcut => (Attributes & FolderItemAttribute.Shortcut) == FolderItemAttribute.Shortcut;
        public bool IsDisable => IsDirectory && !IsReady;

        /// <summary>
        /// 編集可能
        /// </summary>
        public bool IsEditable => (this.Attributes & (FolderItemAttribute.Empty | FolderItemAttribute.Drive | FolderItemAttribute.ArchiveEntry)) == 0;

        /// <summary>
        /// アクセス可能？(ドライブの準備ができているか)
        /// </summary>
        public bool IsReady { get; set; }

        /// <summary>
        /// フォルダーリストのコンテキストメニュー用
        /// </summary>
        private bool _isRecursived;
        public bool IsRecursived
        {
            get { return _isRecursived; }
            set { if (_isRecursived != value) { _isRecursived = value; RaisePropertyChanged(); } }
        }

        public static bool IsVisibleHistoryMark { get; set; } = true;
        public static bool IsVisibleBookmarkMark { get; set; } = true;

        // アイコンオーバーレイの種類を返す
        private FolderItemIconOverlay _iconOverlay = FolderItemIconOverlay.Uninitialized;
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

        public bool IsOverlayStar => IconOverlay == FolderItemIconOverlay.Star;
        public bool IsOverlayChecked => IconOverlay == FolderItemIconOverlay.Checked;

        private BitmapSource _icon;
        public BitmapSource Icon
        {
            get
            {
                if (_icon == null && !IsEmpty)
                {
                    _icon = FileSystem.GetTypeIconSource(TargetPath, FileSystem.IconSize.Normal);
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
                    _iconSmall = FileSystem.GetTypeIconSource(TargetPath, FileSystem.IconSize.Small);
                }
                return _iconSmall;
            }
        }

        // サムネイル用
        private Page _archivePage;
        public Page ArchivePage
        {
            get
            {
                if (_archivePage == null)
                {
                    if (Attributes.AnyFlag(FolderItemAttribute.Bookmark | FolderItemAttribute.Pagemark) && Attributes.HasFlag(FolderItemAttribute.Directory))
                    {
                        _archivePage = new ConstPage(ThumbnailType.Folder);
                    }
                    else if (!IsDrive && !IsEmpty)
                    {
                        var entry = this.ArchiveEntry ?? new ArchiveEntry()
                        {
                            RawEntryName = TargetPath,
                            Length = this.Length,
                            LastWriteTime = this.LastWriteTime,
                        };
                        _archivePage = new ArchivePage(entry);
                        _archivePage.Thumbnail.IsCacheEnabled = true;
                        _archivePage.Thumbnail.Touched += Thumbnail_Touched;
                    }
                }
                return _archivePage;
            }
            set { _archivePage = value; RaisePropertyChanged(); }
        }




        #endregion

        #region IBookListItem Supprt

        public string Note => IsFileSystem() || !IsDirectory ? ArchivePage?.Note : null;

        public string Detail => Name;

        public Thumbnail Thumbnail => ArchivePage?.Thumbnail;

        public Page GetPage()
        {
            return ArchivePage;
        }

        #endregion

        #region Methods

        public bool IsBookmark()
        {
            return (Attributes & FolderItemAttribute.Bookmark) != 0;
        }

        public bool IsFileSystem()
        {
            return (Attributes & (FolderItemAttribute.Bookmark | FolderItemAttribute.Pagemark | FolderItemAttribute.Empty | FolderItemAttribute.None)) == 0;
        }

        /// <summary>
        /// IsRecursived 更新
        /// </summary>
        public void UpdateIsRecursived(bool isDefaultRecursive)
        {
            var option = isDefaultRecursive ? BookLoadOption.DefaultRecursive : BookLoadOption.None;
            var memento = BookHub.Current.GetLastestBookMemento(this.TargetPath, option);
            this.IsRecursived = memento.IsRecursiveFolder;
        }

        // エクスプローラーへのドラッグオブジェクト
        public DataObject GetFileDragData()
        {
            return new DataObject(DataFormats.FileDrop, new string[] { this.Path });
        }

        // パスの存在チェック
        public bool IsExist()
        {
            return IsDirectory ? Directory.Exists(Path) : File.Exists(Path);
        }


        private void UpdateOverlay()
        {
            if (IsVisibleBookmarkMark && BookmarkCollection.Current.Contains(TargetPath))
                _iconOverlay = FolderItemIconOverlay.Star;
            else if (IsVisibleHistoryMark && BookHistoryCollection.Current.Contains(TargetPath))
                _iconOverlay = FolderItemIconOverlay.Checked;
            else
                _iconOverlay = FolderItemIconOverlay.None;
        }


        // アイコンオーバーレイの変更を通知
        public void NotifyIconOverlayChanged()
        {
            UpdateOverlay();
            RaisePropertyChanged("");
        }

        //
        private void Thumbnail_Touched(object sender, EventArgs e)
        {
            var thumbnail = (Thumbnail)sender;
            BookThumbnailPool.Current.Add(thumbnail);
        }

        /// <summary>
        /// 有効なページを持っているか判定
        /// </summary>
        /// <returns></returns>
        public async Task<bool> HasAnyPageAsync()
        {
            // TODO: 実装。フォルダーの場合はページの有無をチェック、アーカイブの場合は判定せずtrue。
            await Task.Yield();

            if (this.IsEmpty) return false;

            return true;
        }


        /// <summary>
        /// フォルダーとして展開可能？
        /// </summary>
        public bool CanOpenFolder()
        {
            if (IsReady)
            {
                if (IsDirectory)
                {
                    return true;
                }

                var archiveType = ArchiverManager.Current.GetSupportedType(TargetPath, false);
                if (IsFileSystem() && !BookHub.Current.IsArchiveRecursive && archiveType.IsRecursiveSupported())
                {
                    return true;
                }
            }

            return false;
        }

        //
        public override string ToString()
        {
            return $"FolderItem: {Path}";
        }

        #endregion
    }

}
