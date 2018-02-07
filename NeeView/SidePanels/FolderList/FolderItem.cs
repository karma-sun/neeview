// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
        ArchiveEntry = (1<<4),
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
    public class FolderItem : BindableBase, IHasPage
    {
        public FolderItemAttribute Attributes { get; set; }

        /// <summary>
        /// 種類。ソート用
        /// </summary>
        public FolderItemType Type { get; set; }


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
        /// ArchiveEntry property.
        /// </summary>
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

        public string ParentPath => System.IO.Path.GetDirectoryName(Path);

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

        public bool IsReady { get; set; }


        /// <summary>
        /// IsRecursived property.
        /// フォルダーリストのコンテキストメニュー用
        /// </summary>
        private bool _isRecursived;
        public bool IsRecursived
        {
            get { return _isRecursived; }
            set { if (_isRecursived != value) { _isRecursived = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// IsRecursived 更新
        /// </summary>
        /// <param name="isDefaultRecursive"></param>
        public void UpdateIsRecursived(bool isDefaultRecursive)
        {
            var option = isDefaultRecursive ? BookLoadOption.DefaultRecursive : BookLoadOption.None;
            var memento = BookHub.Current.GetLastestBookMemento(this.TargetPath, option);
            this.IsRecursived = memento.IsRecursiveFolder;
        }



        public static bool IsVisibleHistoryMark { get; set; } = true;
        public static bool IsVisibleBookmarkMark { get; set; } = true;

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

        private void UpdateOverlay()
        {
            var unit = BookMementoCollection.Current.Find(TargetPath);

            if (IsVisibleBookmarkMark && unit?.BookmarkNode != null)
                _iconOverlay = FolderItemIconOverlay.Star;
            else if (IsVisibleHistoryMark && unit?.HistoryNode != null)
                _iconOverlay = FolderItemIconOverlay.Checked;
            else
                _iconOverlay = FolderItemIconOverlay.None;
        }

        public bool IsOverlayStar => IconOverlay == FolderItemIconOverlay.Star;
        public bool IsOverlayChecked => IconOverlay == FolderItemIconOverlay.Checked;

        // アイコンオーバーレイの変更を通知
        public void NotifyIconOverlayChanged()
        {
            UpdateOverlay();
            RaisePropertyChanged("");
        }

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

        public string Name
        {
            get
            {
                if ((Attributes & FolderItemAttribute.Drive) == FolderItemAttribute.Drive)
                {
                    return Path;
                }
                else if (IsEmpty)
                {
                    return "表示できるファイルはありません"; //// IsDirectoryNotFound ? "フォルダーが存在しません" : "表示できるファイルはありません";
                }
                else
                {
                    return IsShortcut ? System.IO.Path.GetFileNameWithoutExtension(Path) : LoosePath.GetFileName(Path);
                }
            }
        }

        // サムネイル用
        #region Property: ArchivePage
        private ArchivePage _archivePage;
        public ArchivePage ArchivePage
        {
            get
            {
                if (_archivePage == null && !IsDrive && !IsEmpty)
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
                return _archivePage;
            }
            set { _archivePage = value; RaisePropertyChanged(); }
        }

        //
        private void Thumbnail_Touched(object sender, EventArgs e)
        {
            var thumbnail = (Thumbnail)sender;
            BookThumbnailPool.Current.Add(thumbnail);
        }
        #endregion

        #region IHasPage Supprt

        /// <summary>
        /// ページ取得
        /// </summary>
        /// <returns></returns>
        public Page GetPage()
        {
            return ArchivePage;
        }

        #endregion

        //
        public override string ToString()
        {
            return $"FolderItem: {Path}";
        }
    }

}
