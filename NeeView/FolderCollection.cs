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
    }

    public enum FolderInfoIconOverlay
    {
        None,
        Disable,
        Checked,
        Star,
    }

    // フォルダ情報
    public class FolderInfo : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        public FolderInfoAttribute Attributes { get; set; }

        public string Path { get; set; }

        public string ParentPath => System.IO.Path.GetDirectoryName(Path);

        public bool IsDirectory => (Attributes & FolderInfoAttribute.Directory) == FolderInfoAttribute.Directory;
        public bool IsEmpty => (Attributes & FolderInfoAttribute.Empty) == FolderInfoAttribute.Empty;

        public bool IsReady { get; set; }

        public static bool IsVisibleHistoryMark { get; set; } = true;

        // アイコンオーバーレイの種類を返す
        public FolderInfoIconOverlay IconOverlay
        {
            get
            {
                var unit = ModelContext.BookMementoCollection.Find(Path);

                if (IsVisibleHistoryMark && unit?.BookmarkNode != null)
                    return FolderInfoIconOverlay.Star;
                if (IsVisibleHistoryMark && unit?.HistoryNode != null)
                    return FolderInfoIconOverlay.Checked;
                else if (IsDirectory && !IsReady)
                    return FolderInfoIconOverlay.Disable;
                else
                    return FolderInfoIconOverlay.None;
            }
        }

        // アイコンオーバーレイの変更を通知
        public void NotifyIconOverlayChanged()
        {
            OnPropertyChanged(nameof(IconOverlay));
        }


        private BitmapSource _Icon;
        public BitmapSource Icon
        {
            get
            {
                if (_Icon == null && !IsEmpty)
                {
                    _Icon = Utility.FileInfo.GetTypeIconSource(Path, Utility.FileInfo.IconSize.Normal);
                }
                return _Icon;
            }
        }

        private BitmapSource _IconSmall;
        public BitmapSource IconSmall
        {
            get
            {
                if (_IconSmall == null && !IsEmpty)
                {
                    _IconSmall = Utility.FileInfo.GetTypeIconSource(Path, Utility.FileInfo.IconSize.Small);
                }
                return _IconSmall;
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
                    return "表示できるファイルはありません";
                }
                else
                {
                    return System.IO.Path.GetFileName(Path);
                }
            }
        }
    }

    //
    public class FolderCollection
    {
        // indexer
        public FolderInfo this[int index]
        {
            get { return Items[index]; }
            private set { Items[index] = value; }
        }

        public List<FolderInfo> Items { get; private set; }

        public string Place { get; set; }

        public string ParentPlace => Path.GetDirectoryName(Place);

        public FolderOrder FolderOrder { get; set; }

        public int RandomSeed { get; set; }

        //
        public bool IsValid => Items != null;

        //
        private string _CurrentPlace;

        //
        public int SelectedIndex => IndexOfPath(_CurrentPlace);

        //
        public string SelectedPath => _CurrentPlace;

        //
        public int IndexOfPath(string path)
        {
            return Items.FindIndex(e => e.Path == path);
        }

        //
        public bool Contains(string path)
        {
            return Items.Any(e => e.Path == path);
        }

        //
        public void Update(string path)
        {
            _CurrentPlace = path ?? _CurrentPlace;

            if (Place == null || !Directory.Exists(Place))
            {
                var items = new List<FolderInfo>();
                foreach (var drive in DriveInfo.GetDrives())
                {
                    var folderInfo = new FolderInfo();
                    folderInfo.Attributes = FolderInfoAttribute.Directory | FolderInfoAttribute.Drive;
                    folderInfo.IsReady = drive.IsReady;
                    folderInfo.Path = drive.Name;
                    items.Add(folderInfo);
                }
                Items = items;

                return;
            }

            var entries = Directory.GetFileSystemEntries(Place);

            // ディレクトリ、アーカイブ以外は除外
            var directories = entries.Where(e => Directory.Exists(e) && (new DirectoryInfo(e).Attributes & FileAttributes.Hidden) == 0).ToList();
            if (FolderOrder == FolderOrder.TimeStamp)
            {
                directories = directories.OrderBy((e) => Directory.GetLastWriteTime(e)).ToList();
            }
            else
            {
                directories.Sort((a, b) => Win32Api.StrCmpLogicalW(a, b));
            }
            var archives = entries.Where(e => File.Exists(e) && ModelContext.ArchiverManager.IsSupported(e)).ToList();
            if (FolderOrder == FolderOrder.TimeStamp)
            {
                archives = archives.OrderBy((e) => File.GetLastWriteTime(e)).ToList();
            }
            else
            {
                archives.Sort((a, b) => Win32Api.StrCmpLogicalW(a, b));
            }

            // 日付順は逆順にする (エクスプローラー標準にあわせる)
            if (FolderOrder == FolderOrder.TimeStamp)
            {
                directories.Reverse();
                archives.Reverse();
            }
            // ランダムに並べる
            else if (FolderOrder == FolderOrder.Random)
            {
                var random = new Random(RandomSeed);
                directories = directories.OrderBy(e => random.Next()).ToList();
                archives = archives.OrderBy(e => random.Next()).ToList();
            }

            var list = directories.Select(e => new FolderInfo() { Path = e, Attributes = FolderInfoAttribute.Directory, IsReady = true })
                .Concat(archives.Select(e => new FolderInfo() { Path = e, IsReady = true }))
                .ToList();

            if (list.Count <= 0)
            {
                list.Add(new FolderInfo() { Path = Place + "\\.", Attributes = FolderInfoAttribute.Empty });
            }

            Items = list;
        }

        // アイコンの表示更新
        public void RefleshIcon(string path)
        {
            if (path == null)
            {
                Items.ForEach(e => e.NotifyIconOverlayChanged());
            }
            else
            {
                var item = Items.Find(e => e.Path == path);
                if (item != null)
                    item.NotifyIconOverlayChanged();
            }
        }
    }

}
