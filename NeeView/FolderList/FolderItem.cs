﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NeeView
{
    [Flags]
    public enum FolderItemAttribute
    {
        None = 0,
        Directory = (1 << 0),
        Drive = (1 << 1),
        DriveNotReady = (1 << 2),
        Empty = (1 << 3),
        DirectoryNoFound = (1 << 4),
        Shortcut = (1 << 5),
    }

    public enum FolderItemIconOverlay
    {
        Uninitialized,
        None,
        Disable,
        Checked,
        Star,
        Pagemark,
    }

    /// <summary>
    /// フォルダ情報
    /// フォルダリストの１項目の情報 
    /// </summary>
    public class FolderItem : INotifyPropertyChanged, IHasPage
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

        public FolderItemAttribute Attributes { get; set; }


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

        public bool IsDrive => (Attributes & FolderItemAttribute.Drive) == FolderItemAttribute.Drive;
        public bool IsDirectory => (Attributes & FolderItemAttribute.Directory) == FolderItemAttribute.Directory;
        public bool IsEmpty => (Attributes & FolderItemAttribute.Empty) == FolderItemAttribute.Empty;
        public bool IsDirectoryNotFound => (Attributes & FolderItemAttribute.DirectoryNoFound) == FolderItemAttribute.DirectoryNoFound;
        public bool IsShortcut => (Attributes & FolderItemAttribute.Shortcut) == FolderItemAttribute.Shortcut;

        public bool IsReady { get; set; }

        public static bool IsVisibleHistoryMark { get; set; } = true;
        public static bool IsVisibleBookmarkMark { get; set; } = true;

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
            var unit = ModelContext.BookMementoCollection.Find(TargetPath);

            //if (IsVisibleBookmarkMark && unit?.PagemarkNode != null)
            //    IconOverlay = FolderInfoIconOverlay.Pagemark;
            if (IsVisibleBookmarkMark && unit?.BookmarkNode != null)
                _iconOverlay = FolderItemIconOverlay.Star;
            else if (IsVisibleHistoryMark && unit?.HistoryNode != null)
                _iconOverlay = FolderItemIconOverlay.Checked;
            else if (IsDirectory && !IsReady)
                _iconOverlay = FolderItemIconOverlay.Disable;
            else
                _iconOverlay = FolderItemIconOverlay.None;
        }

        public bool IsOverlayStar => IconOverlay == FolderItemIconOverlay.Star;
        public bool IsOverlayChecked => IconOverlay == FolderItemIconOverlay.Checked;
        public bool IsOverlayDisable => IconOverlay == FolderItemIconOverlay.Disable;

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
                if ((Attributes & FolderItemAttribute.Drive) == FolderItemAttribute.Drive)
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
}