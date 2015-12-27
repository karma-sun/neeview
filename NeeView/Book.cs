using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NeeView
{
    public enum PageStretchMode
    {
        None, // もとの大きさ
        Inside,  // もとの大きさ、大きい場合はウィンドウサイズに合わせる
        Outside, // もとの大きさ、小さい場合はウィンドウサイズに合わせる
        Uniform, // ウィンドウサイズに合わせる
        UniformToFill, // ウィンドウいっぱいに広げる
    }

    public static class PageStretchModeExtension
    {
        public static PageStretchMode GetToggle(this PageStretchMode mode)
        {
            return (PageStretchMode)(((int)mode + 1) % Enum.GetNames(typeof(PageStretchMode)).Length);
        }
    }


    public enum BookSortMode
    {
        FileName,
        FileNameDictionary,
        TimeStamp,
        Random,
    }

    public static class BookSortModeExtension
    {
        public static BookSortMode GetToggle(this BookSortMode mode)
        {
            return (BookSortMode)(((int)mode + 1) % Enum.GetNames(typeof(BookSortMode)).Length);
        }
    }


    // 本単位の保存される設定
    [DataContract]
    public class BookParamSetting
    {
        [DataMember]
        public string Place { get; set; }

        [DataMember]
        public string BookMark;

        [DataMember]
        public int PageMode;

        [DataMember]
        public BookReadOrder ReadOrder;

        [DataMember]
        public bool IsSupportedTitlePage;

        [DataMember]
        public bool IsSupportedWidePage;

        [DataMember]
        public bool IsRecursiveFolder;

        [DataMember]
        public BookSortMode SortMode;

        [DataMember]
        public bool IsReverseSort;


        //
        private void Constructor()
        {
            PageMode = 1;
        }

        public BookParamSetting()
        {
            Constructor();
        }

        [OnDeserializing]
        private void Deserializing(StreamingContext c)
        {
            Constructor();
        }

        //
        public void Store(Book book)
        {
            Place = book.Place;
            BookMark = book.CurrentPage?.FullPath;

            PageMode = book.PageMode;
            ReadOrder = book.BookReadOrder;
            IsSupportedTitlePage = book.IsSupportedTitlePage;
            IsSupportedWidePage = book.IsSupportedWidePage;
            IsRecursiveFolder = book.IsRecursiveFolder;
            SortMode = book.SortMode;
            IsReverseSort = book.IsReverseSort;
        }

        public void Restore(Book book)
        {
            // Place, BookMark は復元しない

            book.PageMode = PageMode;
            book.BookReadOrder = ReadOrder;
            book.IsSupportedTitlePage = IsSupportedTitlePage;
            book.IsSupportedWidePage = IsSupportedWidePage;
            book.IsRecursiveFolder = IsRecursiveFolder;
            book.SortMode = SortMode;
            book.IsReverseSort = book.IsReverseSort;
        }
    }

    [DataContract]
    public class BookSetting
    {
        // 本単位の設定
        [DataMember]
        public BookParamSetting BookParamSetting;


        // すべての本に共通の設定

        [DataMember]
        public bool IsEnableAnimatedGif { get; set; }

        [DataMember]
        public bool IsEnableHistory { get; set; }

        [DataMember]
        public bool IsEnableNoSupportFile { get; set; }

        //
        private void Constructor()
        {
            BookParamSetting = new BookParamSetting();

            IsEnableHistory = true;
            IsEnableNoSupportFile = false;
        }

        public BookSetting()
        {
            Constructor();
        }

        [OnDeserializing]
        private void Deserializing(StreamingContext c)
        {
            Constructor();
        }


        public void Store(Book book)
        {
            BookParamSetting.Store(book);

            IsEnableAnimatedGif = book.IsEnableAnimatedGif;
            IsEnableHistory = book.IsEnableHistory;
            IsEnableNoSupportFile = book.IsEnableNoSupportFile;
        }

        public void Restore(Book book)
        {
            BookParamSetting.Restore(book);

            book.IsEnableAnimatedGif = IsEnableAnimatedGif;
            book.IsEnableHistory = IsEnableHistory;
            book.IsEnableNoSupportFile = IsEnableNoSupportFile;

            book.Reflesh();
        }
    }


    public enum BackgroundStyle
    {
        Black,
        White,
        Auto,
        Check
    };
    
    public enum BookReadOrder
    {
        RightToLeft,
        LeftToRight,
    }

    public static class BookReadOrderExtension
    {
        public static BookReadOrder GetToggle(this BookReadOrder mode)
        {
            return (BookReadOrder)(((int)mode + 1) % Enum.GetNames(typeof(BookReadOrder)).Length);
        }
    }


    public class ViewContent
    {
        public object Content { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public Color Color { get; set; }

        public ViewContent(Page page)
        {
            Content = page.Content;
            Width = page.Width;
            Height = page.Height;
            Color = page.Color;
        }
    }




    /// <summary>
    /// 
    /// </summary>
    public class Book
    {
        public List<IDisposable> TrashBox { get; private set; } = new List<IDisposable>();


        public event EventHandler BookChanged;
        public event EventHandler<int> PageChanged;
        public event EventHandler ModeChanged;
        public event EventHandler<string> PropertyChanged;
        public event EventHandler<bool> Loaded;
        public event EventHandler<string> InfoMessage;
        public event EventHandler NowPagesChanged;


        #region Property: IsEnableAnimatedGif
        private bool _IsEnableAnimatedGif;
        public bool IsEnableAnimatedGif
        {
            get { return _IsEnableAnimatedGif; }
            set { _IsEnableAnimatedGif = value; Page.IsEnableAnimatedGif = value; }
        }
#endregion

        // 履歴から設定を復元する
        public bool IsEnableHistory { get; set; } = true;

        // 非対応拡張子ファイルを読み込む
        private bool _IsEnableNoSupportFile;
        public bool IsEnableNoSupportFile
        {
            get { return _IsEnableNoSupportFile; }
            set
            {
                if (_IsEnableNoSupportFile != value)
                {
                    _IsEnableNoSupportFile = value;
                    if (_Place != null)
                    {
                        this.Load(_Place); // 再読み込み
                    }
                }
            }
        }

        // 最初のページはタイトル
        private bool _IsSupportedTitlePage;
        public bool IsSupportedTitlePage
        {
            get { return _IsSupportedTitlePage; }
            set
            {
                if (_IsSupportedTitlePage != value)
                {
                    _IsSupportedTitlePage = value;
                    if (_Place != null)
                    {
                        PropertyChanged?.Invoke(this, null);
                    }
                }
            }
        }

        // 横長ページは２ページとみなす
        private bool _IsSupportedWidePage;
        public bool IsSupportedWidePage
        {
            get { return _IsSupportedWidePage; }
            set
            {
                if (_IsSupportedWidePage != value)
                {
                    _IsSupportedWidePage = value;
                    if (_Place != null)
                    {
                        PropertyChanged?.Invoke(this, null);
                    }
                }
            }
        }

        // 右開き、左開き
        private BookReadOrder _ReadOrder;
        public BookReadOrder BookReadOrder
        {
            get { return _ReadOrder; }
            set
            {
                if (_ReadOrder != value)
                {
                    _ReadOrder = value;
                    if (_Place != null)
                    {
                        PropertyChanged?.Invoke(this, nameof(BookReadOrder));
                    }
                }
            }
        }

        // サブフォルダ読み込み
        private bool _IsRecursiveFolder;
        public bool IsRecursiveFolder
        {
            get { return _IsRecursiveFolder; }
            set
            {
                if (_IsRecursiveFolder != value)
                {
                    _IsRecursiveFolder = value;
                    if (_Place != null)
                    {
                        Load(_Place);
                        PropertyChanged?.Invoke(this, nameof(IsRecursiveFolder));
                    }
                }
            }
        }

        // ページモード
        private int _PageMode;
        public int PageMode
        {
            get { return _PageMode; }
            set
            {
                Debug.Assert(value == 1 || value == 2);
                if (_PageMode != value)
                {
                    _PageMode = value;
                    _CurrentViewPageCount = _PageMode;
                    if (Place != null)
                    {
                        ResetViewPages();
                    }
                }
            }
        }


        public List<Page> Pages { get; private set; }

        private int _Index;
        public int Index
        {
            get { return _Index; }
            set
            {
                if (_Index == value) return;
                SetIndex(value, (_Index <= value) ? 1 : -1);
            }
        }

        public void SetIndex(int value, int direction)
        {
            if (value > Pages.Count - _PageMode) value = Pages.Count - _PageMode;
            if (value < 0) value = 0;

            _Index = value;
            _Direction = direction;

            if (IsSupportedTitlePage)
            {
                _CurrentViewPageCount = _Index == 0 ? 1 : PageMode;
            }
            else
            {
                _CurrentViewPageCount = PageMode;
            }

            UpdateActivePages();
            lock (_Lock)
            {
                UpdateViewPage();
                UpdateNowPages();
            }
            PageChanged?.Invoke(this, _Index);
        }

        // 読む方向
        private int _Direction;

        public Page CurrentPage
        {
            get { return Pages.Count > 0 ? Pages[Index] : null; }
        }

        private Page GetOffsetPage(int offset)
        {
            int index = Index + offset;
            return (0 <= index && index < Pages.Count) ? Pages[index] : null;
        }

        private int _ViewPageIndex;
        public Page ViewPage(int offset)
        {
            int index = _ViewPageIndex + offset;
            return (0 <= index && index < Pages.Count) ? Pages[index] : null;
        }



        private List<Page> _KeepPages; // リソースを開放しないページ
        private volatile List<Page> _ViewPages; // 表示するべきページ
        public volatile List<ViewContent> NowPages; // 実際に表示されているページ


        private object _Lock = new object();


        //
        public Book()
        {
            Pages = new List<Page>();

            _KeepPages = new List<Page>();
            Page.ContentChanged += Page_ContentChanged;

            _PageMode = 1;
            _ViewPages = new List<Page>();
            for (int i = 0; i < 2; ++i)
            {
                _ViewPages.Add(null);
            }
            _ViewPageIndex = 0;

            NowPages = new List<ViewContent>();
            for (int i = 0; i < 2; ++i)
            {
                NowPages.Add(null);
            }
        }

        //
        public void Reflesh()
        {
            lock (_Lock)
            {
                _KeepPages.ForEach(e => e.Close());
                UpdateActivePages();
            }
        }


        // 注意：ジョブワーカーから呼ばれることがあるので別スレッドの可能性があります。
        private void Page_ContentChanged(object sender, EventArgs e)
        {
            bool isDartyNowPages = false;

            lock (_Lock)
            {
                if (sender == CurrentPage)
                {
                    UpdateViewPage();
                    isDartyNowPages = IsDartyNowPages();
                }
                else if (_ViewPages.Contains(sender))
                {
                    isDartyNowPages = IsDartyNowPages();
                }

                if (isDartyNowPages)
                {
                    ValidateWidePage();
                    //ValidateReadOrder();
                    ForceUpdateNowPages();
                }
            }

            if (isDartyNowPages && NowPagesChanged != null)
            {
                App.Current.Dispatcher.Invoke(() => NowPagesChanged(this, null));
            }
        }

        //bool _IsDartyVewPages;

        private bool IsDartyViewPages()
        {
            if (_ViewPages[0] == null) return true;

            if (_ViewPages[0] == CurrentPage) return false;

            if (CurrentPage.Content != null) return true;

            foreach (var page in _ViewPages)
            {
                if (page != null && page.Content == null) return false;
            }
            return true;
        }

        private void UpdateViewPage()
        {
            if (IsDartyViewPages())
            {
                _ViewPageIndex = Index;

                if (_ViewPages[0] != ViewPage(0))
                {
                    if (_ViewPages[0] != null && !_KeepPages.Contains(_ViewPages[0]))
                    {
                        _ViewPages[0].Close();
                    }

                    _ViewPages[0] = ViewPage(0);
                    _ViewPages[0]?.Open(JobPriority.Hi);
                }
            }

            if (_PageMode >= 2 && _ViewPages[1] != ViewPage(1))
            {
                if (_ViewPages[1] != null && !_KeepPages.Contains(_ViewPages[1]))
                {
                    _ViewPages[1].Close();
                }

                _ViewPages[1] = ViewPage(1);
                _ViewPages[1]?.Open(JobPriority.Hi);
            }

            if (_PageMode <= 1 && _ViewPages[1] != null)
            {
                _ViewPages[1].Close();
                _ViewPages[1] = null;
            }
        }


        private void UpdateNowPages()
        {
            if (IsDartyNowPages())
            {
                ValidateWidePage();
                ForceUpdateNowPages();
                NowPagesChanged(this, null);
            }
        }

        private bool IsDartyNowPages()
        {
            if (_ViewPages[0] == null) return true;

            return (_ViewPages.All(e => e == null || e.Content != null));
        }

        private void ForceUpdateNowPages()
        {
            for (int i = 0; i < NowPages.Count; ++i)
            {
                if (i < _CurrentViewPageCount)
                {
                    int cid = (BookReadOrder == BookReadOrder.RightToLeft) ? i : _CurrentViewPageCount - 1 - i;
                    NowPages[i] = _ViewPages[i] != null ? new ViewContent(_ViewPages[cid]) : null;
                }
                else
                {
                    NowPages[i] = null;
                }
            }
        }

        // ワイドページ処理
        private void ValidateWidePage()
        {
            // IsDartyNowPageが確定してから処理される
            // すなわち１度だけの処理

            // ワイドページ非対応、もしくはもともと1ページ表示であるなら処理不要
            if (!IsSupportedWidePage || _CurrentViewPageCount <= 1)
            {
                return;
            }

            // どちらかのページが横長ならば、１ページ表示にする
            if ((_ViewPages[0] != null && _ViewPages[0].Width > _ViewPages[0].Height) ||
                (_ViewPages[1] != null && _ViewPages[1].Width > _ViewPages[1].Height))
            {
                _CurrentViewPageCount = 1;

                // 進行方向がマイナスの場合、ページの計算からやりなおし？
                if (_Direction < 0)
                {
                    _Index = _Index + 1;

                    // swap ... ン？
                    // TODO: ViewPagesの順番を入れ替えることに疑問。他の処理で戻ってしまわないのか？
                    var temp = _ViewPages[0];
                    _ViewPages[0] = _ViewPages[1];
                    _ViewPages[1] = temp;
                }
            }
        }

        private void UpdateActivePages()
        {
            // カレントページ収集
            var currentPages = new List<Page>();
            for (int offset = 0; offset < _PageMode; ++offset)
            {
                int index = Index + offset;
                if (0 <= index && index < Pages.Count && !_ViewPages.Contains(Pages[index]))
                {
                    currentPages.Add(Pages[index]);
                }
            }

            // 先読みページ収集
            var preLoadPages = new List<Page>();
            for (int offset = 1; offset <= _PageMode; ++offset)
            {
                int index = (_Direction >= 0) ? Index + (_PageMode - 1) + offset : Index - offset;

                if (0 <= index && index < Pages.Count && !_ViewPages.Contains(Pages[index]))
                {
                    preLoadPages.Add(Pages[index]);
                }
            }

            // コンテンツを保持するページ収集
            var keepPages = new List<Page>();
            for (int offset = -_PageMode; offset <= _PageMode * 2 - 1; ++offset)
            {
                int index = Index + offset;
                if (0 <= index && index < Pages.Count)
                {
                    keepPages.Add(Pages[index]);
                }
            }

            // 一応テストしておく
            currentPages.ForEach(e => Debug.Assert(keepPages.Contains(e)));
            preLoadPages.ForEach(e => Debug.Assert(keepPages.Contains(e)));

            // 不要コンテンツ破棄
            foreach (var page in _KeepPages)
            {
                if (!_ViewPages.Contains(page) && !keepPages.Contains(page))
                {
                    page.Close();
                }
            }

            // 読み込み設定
            foreach (var page in _ViewPages)
            {
                page?.Open(JobPriority.Top);
            }
            foreach (var page in currentPages)
            {
                page.Open(JobPriority.Hi);
            }
            foreach (var page in preLoadPages)
            {
                page.Open(JobPriority.Default);
            }

            // 保持ページ更新
            _KeepPages = keepPages;
        }

        // 保持ページ開放
        public void FreeKeepPage()
        {
            foreach (var page in _KeepPages)
            {
                if (!_ViewPages.Contains(page))
                {
                    page.Close();
                }
            }
        }


        //
        List<Archiver> _Archivers = new List<Archiver>();
        List<string> _TempArchives = new List<string>();

        // Book初期化
        // 全ページクリア
        private void ClearPages()
        {
            lock (_Lock)
            {
                Pages.ForEach(e => e?.Close());
                Pages.Clear();

                for (int i = 0; i < _ViewPages.Count; ++i)
                {
                    _ViewPages[i] = null;
                }
                _ViewPageIndex = 0;

                _KeepPages.Clear();



                _Archivers.ForEach(e => e.Dispose());
                _Archivers.Clear();

                _TempArchives.ForEach(e => File.Delete(e));
                _TempArchives.Clear();

                TrashBox.ForEach(e => e.Dispose());
                TrashBox.Clear();
            }
        }


        // ファイル読み込み
        public void Load(string path, LoadFolderOption option = LoadFolderOption.None)
        {
            // 履歴保存
            if (Place != null)
            {
                ModelContext.BookHistory.Add(this);
            }

            Archiver archiver = null;
            string start = null;

            // アーカイバの選択
#region SelectArchiver
            if (Directory.Exists(path))
            {
                archiver = ModelContext.ArchiverManager.CreateArchiver(path);
            }
            else if (File.Exists(path))
            {
                if (ModelContext.ArchiverManager.IsSupported(path))
                {
                    archiver = ModelContext.ArchiverManager.CreateArchiver(path);
                    option |= LoadFolderOption.Recursive; // 圧縮ファイルはリカーシブ標準
                }
                else
                {
                    archiver = ModelContext.ArchiverManager.CreateArchiver(Path.GetDirectoryName(path));
                    start = Path.GetFileName(path);
                }
            }
            else
            {
                throw new FileNotFoundException("ファイルが見つかりません", path);
            }
#endregion

            // 本無効化
            _Place = null;

            // 設定の復元
            if (IsEnableHistory)
            {
                var setting = ModelContext.BookHistory.Find(archiver.Path);
                if (setting != null)
                {
                    setting.Restore(this);
                    start = start ?? setting.BookMark;
                }
            }

            if (IsRecursiveFolder)
            {
                option |= LoadFolderOption.Recursive;
            }

            LoadArchive(archiver, start, option);
        }


        // アーカイブ読み込み
        private async void LoadArchive(Archiver archiver, string start, LoadFolderOption option)
        {
            try
            {
                ClearPages(); // ページクリア。タイミングどうなの？

                // 読み込み。非同期で行う。
                Loaded?.Invoke(this, true);

                await Task.Run(() =>
                    {
                        _Archivers.Add(archiver);
                        ReadArchive(archiver, "", (option & LoadFolderOption.Recursive) == LoadFolderOption.Recursive);
                    });

                Loaded?.Invoke(this, false);

                // 初期ソート
                Sort();

                // スタートページ取得
                int startIndex = (start != null) ? Pages.FindIndex(e => e.FullPath == start) : 0;
                if ((option & LoadFolderOption.FirstPage) == LoadFolderOption.FirstPage)
                {
                    startIndex = 0;
                }
                else if ((option & LoadFolderOption.LastPage) == LoadFolderOption.LastPage)
                {
                    startIndex = Pages.Count - 1;
                }

                // 本有効化
                _Place = archiver.Path;

                // イベント発行
                PropertyChanged?.Invoke(this, null);
                BookChanged?.Invoke(this, null);

                SetIndex(startIndex, 1);

                ResetViewPages();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }

        private void ReadArchive(Archiver archiver, string place, bool isRecursive)
        {
            List<PageFileInfo> entries = null;

            try
            {
                entries = archiver.GetEntries();
            }
            catch
            {
                Debug.WriteLine($"{archiver.Path} の展開に失敗しました");
                return;
            }

            foreach (var entry in entries)
            {
                // 再帰設定、もしくは単一ファイルの場合、再帰を行う
                if ((isRecursive || entries.Count == 1) && ModelContext.ArchiverManager.IsSupported(entry.Path))
                {
                    if (archiver is FolderFiles)
                    {
                        var ff = (FolderFiles)archiver;
                        ReadArchive(ModelContext.ArchiverManager.CreateArchiver(ff.GetFullPath(entry.Path)), LoosePath.Combine(place, entry.Path), isRecursive);
                    }
                    else
                    {
                        // テンポラリにアーカイブを解凍する
                        string tempFileName = Temporary.CreateTempFileName(Path.GetFileName(entry.Path));
                        archiver.ExtractToFile(entry.Path, tempFileName);
                        _TempArchives.Add(tempFileName);
                        ReadArchive(ModelContext.ArchiverManager.CreateArchiver(tempFileName), LoosePath.Combine(place, entry.Path), isRecursive);
                    }
                }
                else
                {
                    if (ModelContext.BitmapLoaderManager.IsSupported(entry.Path))
                    {
                        var page = new BitmapPage(entry, archiver, place);
                        Pages.Add(page);
                    }
                    else if (IsEnableNoSupportFile)
                    {
                        switch (ModelContext.ArchiverManager.GetSupportedType(entry.Path))
                        {
                            case ArchiverType.None:
                                Pages.Add(new FilePage(entry, FilePageIcon.File, place));
                                break;
                            case ArchiverType.FolderFiles:
                                Pages.Add(new FilePage(entry, FilePageIcon.Folder, place));
                                break;
                            default:
                                Pages.Add(new FilePage(entry, FilePageIcon.Archive, place));
                                break;
                        }
                    }
                }
            }
        }



        private BookSortMode _SortMode = BookSortMode.FileName;
        public BookSortMode SortMode
        {
            get { return _SortMode; }
            set
            {
                if (_SortMode == value && value != BookSortMode.Random) return;
                _SortMode = value;
                if (_Place != null)
                {
                    Sort();
                    SetIndex(0, 1);
                    ModeChanged?.Invoke(this, null); // モード変更通知
                }
            }
        }

        private bool _IsReverseSort;
        public bool IsReverseSort
        {
            get { return _IsReverseSort; }
            set
            {
                if (_IsReverseSort == value) return;
                _IsReverseSort = value;
                //Page current = CurrentPage;
                if (_SortMode == BookSortMode.Random)
                {
                    Pages.Reverse();
                }
                else
                {
                    Sort();
                }
                //_IsDartyVewPages = true;
                //SetIndex(Pages.IndexOf(current), _Direction); // ページ番号更新
                SetIndex(0, 1);
                ModeChanged?.Invoke(this, null); // モード変更通知
            }
        }

        // ファイル名：辞書順ソート
        void Sort()
        {
            if (Pages.Count <= 0) return;

            switch (SortMode)
            {
                case BookSortMode.FileName:
                    //Pages.Sort((a, b) => StrCmpLogicalW(a.FullPath, b.FullPath));
                    Pages.Sort((a, b) => ComparePath(a.FullPath, b.FullPath, StrCmpLogicalW));
                    break;
                case BookSortMode.FileNameDictionary:
                    //Pages = Pages.OrderBy(e => e.FullPath).ToList();
                    Pages.Sort((a, b) => ComparePath(a.FullPath, b.FullPath, string.Compare));
                    break;
                case BookSortMode.TimeStamp:
                    Pages = Pages.OrderBy(e => e.UpdateTime).ToList();
                    break;
                case BookSortMode.Random:
                    Pages = Pages.OrderBy(e => Guid.NewGuid()).ToList();
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (IsReverseSort)
            {
                Pages.Reverse();
            }

            //Debug.WriteLine("--");
            //Pages.ForEach(e => Debug.WriteLine(e.FullPath));
        }

        // 参考：自然順ソート？
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern int StrCmpLogicalW(string psz1, string psz2);

        private int ComparePath(string s1, string s2, Func<string, string, int> compare)
        {
            string d1 = LoosePath.GetDirectoryName(s1);
            string d2 = LoosePath.GetDirectoryName(s2);

            if (d1 == d2)
                return compare(s1, s2);
            else
                return compare(d1, d2);
        }



        // 表示の安定状態チェック
        private bool IsStable()
        {
            return (_Place != null && _ViewPages[0] == CurrentPage && _ViewPages.All(e => e == null || e.Content != null));
        }

        // 前のページに戻る
        public void PrevPage()
        {
            // if (Pages.Count <= 0) return;
            if (!IsStable()) return;

            int index = Index - _PageMode;
            if (index < 0) index = 0;
            if (Index == index)
            {
                PrevFolder(LoadFolderOption.LastPage);
            }
            else
            {
                Index = index;
            }
        }

        private int _CurrentViewPageCount = 1;

        // 次のページへ進む
        public void NextPage()
        {
            //if (Pages.Count <= 0) return;
            if (!IsStable()) return;

            int index = Index + _CurrentViewPageCount; // _PageMode;
            if (index > Pages.Count - _PageMode)
            {
                index = Pages.Count - _PageMode;
                if (index < 0) index = 0;
            }
            if (Index == index)
            {
                NextFolder(LoadFolderOption.FirstPage);
            }
            else
            {
                Index = index;
            }
        }

        //
        public void FirstPage()
        {
            Index = 0;
        }

        //
        public void LastPage()
        {
            Index = Pages.Count - 1;
        }

        // 表示数変更によるViewPages作り直し
        private void ResetViewPages()
        {
            lock (_Lock)
            {
                foreach (var page in _ViewPages)
                {
                    if (page != null && !_KeepPages.Contains(page)) page.Close();
                }

                //_ViewPages.Clear();
                for (int i = 0; i < _ViewPages.Count; ++i)
                {
                    _ViewPages[i] = null; //.Add(null);
                }

                // 余分なコンテンツは破棄
                for (int i = _PageMode; i < 2; ++i)
                {
                    NowPages[i] = null;
                }
            }

            ModelContext.JobEngine.ChangeWorkerSize(_PageMode);

            SetIndex(Index, _Direction);
        }


        string _Place;
        public string Place => _Place;

        [Flags]
        public enum LoadFolderOption
        {
            None = 0,
            Recursive = (1 << 0),
            FirstPage = (1 << 1),
            LastPage = (1 << 2)
        };

        // 次のフォルダに移動
        public bool MoveFolder(int direction, LoadFolderOption option)
        {
            if (_Place == null) return false;

            string place = File.Exists(_Place) ? Path.GetDirectoryName(_Place) : _Place;

            if (Directory.Exists(place))
            {
                var entries = Directory.GetFileSystemEntries(Path.GetDirectoryName(_Place)).ToList();

                // ディレクトリ、アーカイブ以外は除外
                var directories = entries.Where(e => Directory.Exists(e) || ModelContext.ArchiverManager.IsSupported(e)).ToList();

                // TODO: ディレクトリの並び順ソート

                int index = directories.IndexOf(_Place);
                if (index < 0) return false;

                int next = index + direction;
                if (next < 0 || next >= directories.Count) return false;

                Load(directories[next], option);
            }

            return true;
        }

        public void NextFolder(LoadFolderOption option = LoadFolderOption.None)
        {
            bool result = MoveFolder(+1, option);
            if (!result)
            {
                InfoMessage?.Invoke(this, "次のフォルダはありません");
            }
        }

        public void PrevFolder(LoadFolderOption option = LoadFolderOption.None)
        {
            bool result = MoveFolder(-1, option);
            if (!result)
            {
                InfoMessage?.Invoke(this, "前のフォルダはありません");
            }
        }
    }



    public static class Temporary
    {
        public static string TempDirectory { get; private set; } = Path.Combine(Path.GetTempPath(), "neeLaboratory.ImageViewer");

        public static string CreateTempFileName(string name)
        {
            // 専用フォルダ作成
            Directory.CreateDirectory(TempDirectory);

            // ファイル名作成
            string tempFileName = Path.Combine(TempDirectory, name);
            int count = 1;
            while (File.Exists(tempFileName) || Directory.Exists(tempFileName))
            {
                tempFileName = Path.Combine(TempDirectory, Path.GetFileNameWithoutExtension(name) + $"-{count++}" + Path.GetExtension(name));
            }

            return tempFileName;
        }

        public static void RemoveTempFolder()
        {
            try
            {
                Directory.Delete(TempDirectory, true);
            }
            catch
            {
                // 例外スルー
            }
        }
    }
}
