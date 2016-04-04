using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NeeView
{
    [Flags]
    public enum FolderInfoAttribute
    {
        None = 0,
        Directory = (1 << 0),
        Drive = (1 << 1),
        Parent = (1 << 2),
        Empty = (1 << 3)
    }

    // フォルダ情報
    public class FolderInfo
    {
        public FolderInfoAttribute Attributes { get; set; }

        public string Path { get; set; }

        public string ParentPath => System.IO.Path.GetDirectoryName(Path);


        public bool IsDirectory => (Attributes & FolderInfoAttribute.Directory) == FolderInfoAttribute.Directory;
        public bool IsEmpty => (Attributes & FolderInfoAttribute.Empty) == FolderInfoAttribute.Empty;

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
                if ((Attributes & FolderInfoAttribute.Parent) == FolderInfoAttribute.Parent)
                {
                    return "..";
                }
                else if ((Attributes & FolderInfoAttribute.Drive) == FolderInfoAttribute.Drive)
                {
                    return Path;
                }
                else if (IsEmpty)
                {
                    return "項目はありません";
                }
                else
                {
                    return System.IO.Path.GetFileName(Path);
                }
            }
        }
    }

    public class FolderCollection
    {
        public event EventHandler ItemsChanged;
        public event EventHandler SelectedIndexChanged;

        // indexer
        public FolderInfo this[int index]
        {
            get { return Items[index]; }
            private set { Items[index] = value; }
        }

        #region Property: Items
        private List<FolderInfo> _Items;
        public List<FolderInfo> Items
        {
            get { return _Items; }
            private set
            {
                if (_Items != value)
                {
                    _Items = value;
                    ItemsChanged?.Invoke(this, null);
                }
            }
        }
        #endregion


        #region Property: SelectedBook
        private string _SelectedBook;
        public string SelectedBook
        {
            get { return _SelectedBook; }
            set
            {
                if (_SelectedBook != value)
                {
                    _SelectedBook = value;
                    Place = Path.GetDirectoryName(value);
                    //_IsDarty = true;
                }
            }
        }
        #endregion


        #region Property: Place
        private string _Place;
        public string Place
        {
            get { return _Place; }
            set
            {
                if (_Place != value)
                {
                    _Place = value;
                    if (_SelectedBook != null && Path.GetDirectoryName(_SelectedBook) != _Place)
                    {
                        _SelectedBook = null;
                    }
                    _IsDarty = true;
                }
            }
        }
        #endregion

        //
        public string ParentPlace
        {
            get { return Path.GetDirectoryName(_Place); }
        }

        #region Property: FolderOrder
        private FolderOrder _FolderOrder;
        public FolderOrder FolderOrder
        {
            get { return _FolderOrder; }
            set
            {
                if (_FolderOrder != value)
                {
                    _FolderOrder = value;
                    _IsDarty = true;
                }
            }
        }
        #endregion

        #region Property: RandomSeed
        private int _RandomSeed;
        public int RandomSeed
        {
            get { return _RandomSeed; }
            set
            {
                if (_RandomSeed != value)
                {
                    _RandomSeed = value;
                    if (FolderOrder == FolderOrder.Random) _IsDarty = true;
                }
            }
        }
        #endregion

        private bool _IsDarty;

        //
        public bool IsValid => _Items != null;

        //
        private string _CurrentPlace;

        //
        public int SelectedIndex => IndexOfPath(_CurrentPlace);

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
        public void Update(string path, bool isRefleshFolderList, bool isForce)
        {
            _CurrentPlace = path ?? _CurrentPlace;

            if (!_IsDarty && !isForce)
            {
                if (isRefleshFolderList)
                {
                    SelectedIndexChanged?.Invoke(this, null);
                }
                return;
            }

            _IsDarty = false;

            if (Place == null || !Directory.Exists(Place))
            {
                var drives = DriveInfo.GetDrives().Select(e => e.Name).ToList();
                Items = drives.Select(e => new FolderInfo() { Attributes = FolderInfoAttribute.Directory | FolderInfoAttribute.Drive, Path = e }).ToList();
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
            //directories.AddRange(archives);

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

            var list = directories.Select(e => new FolderInfo() { Path = e, Attributes = FolderInfoAttribute.Directory })
                .Concat(archives.Select(e => new FolderInfo() { Path = e, }))
                .ToList();
            //list.Insert(0, new FolderInfo() { Attributes = FolderInfoAttribute.Parent, Path = ParentPlace });

            if (list.Count <= 0)
            {
                list.Add(new FolderInfo() { Path = Place + "\\.", Attributes = FolderInfoAttribute.Empty });
            }

            Items = list;

            //if (isRefleshFolderList)
            //{
                SelectedIndexChanged?.Invoke(this, null);
            //}
        }
    }

}
