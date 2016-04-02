using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NeeView
{
    // フォルダ情報
    public class FolderInfo
    {
        public string Path { get; set; }

        private BitmapSource _Icon;
        public BitmapSource Icon
        {
            get
            {
                if (_Icon == null)
                {
                    _Icon = Utility.FileInfo.GetTypeIconSource(Path, Utility.FileInfo.IconSize.Normal);
                }
                return _Icon;
            }
        }

        public string Name { get { return System.IO.Path.GetFileName(Path); } }
    }

    public class FolderColelction
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

        #region Property: Place
        private string _Place;
        public string Place
        {
            get { return _Place; }
            set
            {
                var path = Path.GetDirectoryName(value);
                if (_Place != path)
                {
                    _Place = path;
                    _IsDarty = true;
                }
            }
        }
        #endregion

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
                Items = null;
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
            var archives = entries.Where(e => ModelContext.ArchiverManager.IsSupported(e)).ToList();
            if (FolderOrder == FolderOrder.TimeStamp)
            {
                archives = archives.OrderBy((e) => File.GetLastWriteTime(e)).ToList();
            }
            else
            {
                archives.Sort((a, b) => Win32Api.StrCmpLogicalW(a, b));
            }
            directories.AddRange(archives);

            // 日付順は逆順にする (エクスプローラー標準にあわせる)
            if (FolderOrder == FolderOrder.TimeStamp)
            {
                directories.Reverse();
            }
            // ランダムに並べる
            else if (FolderOrder == FolderOrder.Random)
            {
                var random = new Random(RandomSeed);
                directories = directories.OrderBy(e => random.Next()).ToList();
            }

            Items = directories.Select(e => new FolderInfo() { Path = e }).ToList();

            if (isRefleshFolderList)
            {
                SelectedIndexChanged?.Invoke(this, null);
            }
        }
    }

}
