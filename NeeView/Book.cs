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
        //[DataMember]
        //public string Place;

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
            //Place = book.Place;
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

        //[DataMember]
        //public int PageMode;

        //[DataMember]
        //public bool IsViewStartPositionCenter;



        // View?

        [DataMember]
        public PageStretchMode StretchMode;

        [DataMember]
        public BackgroundStyle Background { get; set; }

        // Book

        [DataMember]
        public bool IsEnableAnimatedGif { get; set; }

        [DataMember]
        public bool IsEnableSusie { get; set; }

        [DataMember]
        public string SusiePluginPath { get; set; }

        [DataMember]
        public bool IsFirstOrderSusieImage { get; set; }

        [DataMember]
        public bool IsFirstOrderSusieArchive { get; set; }



        //
        private void Constructor()
        {
            BookParamSetting = new BookParamSetting();

            StretchMode = PageStretchMode.Uniform;
            //PageMode = 1;

            IsEnableSusie = false;
            SusiePluginPath = null;
            IsFirstOrderSusieImage = false;
            IsFirstOrderSusieArchive = false;
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

            //PageMode = book.PageMode;
            //IsViewStartPositionCenter = book.IsViewStartPositionCenter;

            StretchMode = book.StretchMode;
            Background = book.Background;
            IsEnableAnimatedGif = book.IsEnableAnimatedGif;

            IsEnableSusie = book.IsEnableSusie;
            SusiePluginPath = book.SusiePluginPath;
            IsFirstOrderSusieImage = book.IsFirstOrderSusieImage;
            IsFirstOrderSusieArchive = book.IsFirstOrderSusieArchive;
        }

        public void Restore(Book book)
        {
            BookParamSetting.Restore(book);

            //book.PageMode = PageMode;
            //book.IsViewStartPositionCenter = IsViewStartPositionCenter;

            book.StretchMode = StretchMode;
            book.Background = Background;
            book.IsEnableAnimatedGif = IsEnableAnimatedGif;

            RestoreSusieSetting(book);

            book.Reflesh();
        }

        public void RestoreSusieSetting(Book book)
        {
            book.IsEnableSusie = IsEnableSusie;
            book.SusiePluginPath = SusiePluginPath;
            book.IsFirstOrderSusieImage = IsFirstOrderSusieImage;
            book.IsFirstOrderSusieArchive = IsFirstOrderSusieArchive;
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
        public static Book Current { get; private set; }

        public static JobEngine JobEngine { get; private set; }

        public static Susie.Susie Susie { get; private set; }

        public List<IDisposable> TrashBox { get; private set; } = new List<IDisposable>();


        public event EventHandler BookChanged;
        public event EventHandler<int> PageChanged;
        public event EventHandler ModeChanged;
        public event EventHandler BackgroundChanged;
        public event EventHandler<string> PropertyChanged;
        public event EventHandler<bool> Loaded;
        public event EventHandler<string> InfoMessage;

        public bool _IsEnableSusie;
        public bool IsEnableSusie
        {
            get { return _IsEnableSusie; }
            set
            {
                if (_IsEnableSusie == value) return;
                _IsEnableSusie = value;
                SusieBitmapLoader.IsEnable = _IsEnableSusie;
            }
        }

        public string _SusiePluginPath;
        public string SusiePluginPath
        {
            get { return _SusiePluginPath; }
            set
            {
                if (_SusiePluginPath == value) return;
                _SusiePluginPath = value;
                // Susie 再起動
                Susie = new Susie.Susie();
                Susie.SearchPath.Add(_SusiePluginPath);
                Susie.Initialize();
                // Susie対応拡張子更新
                _ArchiverManager.UpdateSusieSupprtedFileTypes(Susie);
            }
        }

        public bool _IsFirstOrderSusieImage = (Page.LoaderOrder == BitmapLoaderType.Susie);
        public bool IsFirstOrderSusieImage
        {
            get { return _IsFirstOrderSusieImage; }
            set
            {
                if (_IsFirstOrderSusieImage == value) return;
                _IsFirstOrderSusieImage = value;
                Page.LoaderOrder = _IsFirstOrderSusieImage ? BitmapLoaderType.Susie : BitmapLoaderType.Default;
            }
        }

        public bool _IsFirstOrderSusieArchive;
        public bool IsFirstOrderSusieArchive
        {
            get { return _IsFirstOrderSusieArchive; }
            set
            {
                if (_IsFirstOrderSusieArchive == value) return;
                _IsFirstOrderSusieArchive = value;
                ArchiverManager.OrderType = _IsFirstOrderSusieArchive ? ArchiverType.SusieArchiver : ArchiverType.DefaultArchiver;
            }
        }


        #region Property: Background
        private BackgroundStyle _Background;
        public BackgroundStyle Background
        {
            get { return _Background; }
            set { _Background = value; BackgroundChanged?.Invoke(this, null); }
        }
        #endregion

        #region Property: IsEnableAnimatedGif
        private bool _IsEnableAnimatedGif;
        public bool IsEnableAnimatedGif
        {
            get { return _IsEnableAnimatedGif; }
            set { _IsEnableAnimatedGif = value; Page.IsEnableAnimatedGif = value; }
        }
        #endregion

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


        // 拡縮設定
        private PageStretchMode _StretchMode = PageStretchMode.Uniform;
        public PageStretchMode StretchMode
        {
            get { return _StretchMode; }
            set { if (_StretchMode != value) { _StretchMode = value; ModeChanged?.Invoke(this, null); } }
        }

#if false
        // 拡大はしない
        private bool _IsIgnoreScaleUp;
        public bool IsIgnoreScaleUp
        {
            get { return _IsIgnoreScaleUp; }
            set { if (_IsIgnoreScaleUp != value) { _IsIgnoreScaleUp = value; ModeChanged?.Invoke(this, null); } }
        }
#endif


        private readonly int _MaxPageMode = 2;

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



        private List<Page> _KeepPages; // リソースを開放しないページ
        private volatile List<Page> _ViewPages; // 表示するべきページ
        public volatile List<ViewContent> NowPages; // 実際に表示されているページ

        public event EventHandler NowPagesChanged;

        private object _Lock = new object();


        private ArchiverManager _ArchiverManager = new ArchiverManager();
        public ArchiverManager ArchiverManager => _ArchiverManager;

        //
        public Book()
        {
            Current = this;

            JobEngine = new JobEngine();
            JobEngine.Start();

            //global::Susie.SusiePluginApi.TemporaryFolderPath = Temporary.TempDirectory;
            Susie = new Susie.Susie();
            //Susie.SearchPath.Add(@"E:\Bin\susie347b");
            Susie.Initialize();

            //SusieArchiver.UpdateSupportExtensions(Susie);
            _ArchiverManager.UpdateSusieSupprtedFileTypes(Susie);

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
            for (int i = 0; i < _MaxPageMode; ++i)
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
#if false
                for (int i = 0; i < _PageMode; ++i)
                {
                    var page = GetOffsetPage(i);
                    if (_ViewPages[i] == page) continue;

                    if (_ViewPages[i] != null && !_KeepPages.Contains(_ViewPages[i]))
                    {
                        _ViewPages[i].Close();
                    }

                    _ViewPages[i] = page;
                    _ViewPages[i].Open(JobPriority.Hi);
                }
#endif
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
                //ValidateReadOrder();
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

#if false
        void ValidateReadOrder()
        { 
            // 2ページ左開きの場合、左右を入れ替える
            if (_CurrentViewPageCount == 2 && BookReadOrder == BookReadOrder.LeftToRight && _ViewPages[0] != null)
            {
                var temp = _ViewPages[0];
                _ViewPages[0] = _ViewPages[1];
                _ViewPages[1] = temp;
            }
        }
#endif

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

            if (Directory.Exists(path))
            {
                archiver = _ArchiverManager.CreateArchiver(path);
            }
            else if (File.Exists(path))
            {
                if (_ArchiverManager.IsSupported(path))
                {
                    archiver = _ArchiverManager.CreateArchiver(path);
                    option |= LoadFolderOption.Recursive; // 圧縮ファイルはリカーシブ標準
                }
                else
                {
                    archiver = _ArchiverManager.CreateArchiver(Path.GetDirectoryName(path));
                    start = Path.GetFileName(path);
                }
            }
            else
            {
                throw new FileNotFoundException("ファイルが見つかりません", path);
            }

            if (IsRecursiveFolder)
            {
                option |= LoadFolderOption.Recursive;
            }

            //
            _Place = null;

            // 設定の復元
            var setting = ModelContext.BookHistory.Search(archiver.Path);
            if (setting != null)
            {
                setting.Restore(this);
                start = start ?? setting.BookMark;
            }

            LoadArchive(archiver, start, option);
        }


        // アーカイブ読み込み
        private async void LoadArchive(Archiver archiver, string start, LoadFolderOption option)
        {
            try
            {
                Debug.WriteLine("LoadArchive: " + archiver.Path);

                ClearPages();

                Debug.WriteLine("LoadArchive.CleaPages-Done.");


                Loaded?.Invoke(this, true);

                await Task.Run(() =>
                    {
                        _Archivers.Add(archiver);
                        ReadArchive(archiver, "", (option & LoadFolderOption.Recursive) == LoadFolderOption.Recursive);
                    });

                Loaded?.Invoke(this, false);

                Sort();


                int startIndex = (start != null) ? Pages.FindIndex(e => e.FullPath == start) : 0;
                if ((option & LoadFolderOption.FirstPage) == LoadFolderOption.FirstPage)
                {
                    startIndex = 0;
                }
                else if ((option & LoadFolderOption.LastPage) == LoadFolderOption.LastPage)
                {
                    startIndex = Pages.Count - 1;
                }

                _Place = archiver.Path;

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
                if ((isRecursive || entries.Count == 1) && _ArchiverManager.IsSupported(entry.Path))
                {
                    if (archiver is FolderFiles)
                    {
                        var ff = (FolderFiles)archiver;
                        ReadArchive(_ArchiverManager.CreateArchiver(ff.GetFullPath(entry.Path)), LoosePath.Combine(place, entry.Path), isRecursive);
                    }
                    else
                    {
                        // テンポラリにアーカイブを解凍する
                        string tempFileName = Temporary.CreateTempFileName(Path.GetFileName(entry.Path));
                        archiver.ExtractToFile(entry.Path, tempFileName);
                        _TempArchives.Add(tempFileName);
                        ReadArchive(_ArchiverManager.CreateArchiver(tempFileName), LoosePath.Combine(place, entry.Path), isRecursive);
                    }
                }
                else
                {
                    var page = new BitmapPage(entry, archiver, place); // ここで place を活用すべき
                    //Debug.WriteLine("> " + page.FullPath);
                    Pages.Add(page);
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
                    Pages.Sort((a, b) => StrCmpLogicalW(a.FullPath, b.FullPath));
                    break;
                case BookSortMode.FileNameDictionary:
                    Pages = Pages.OrderBy(e => e.FullPath).ToList();
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
        }

        // 参考：自然順ソート？
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern int StrCmpLogicalW(string psz1, string psz2);


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
                for (int i = _PageMode; i < _MaxPageMode; ++i)
                {
                    NowPages[i] = null;
                }
            }

            Book.JobEngine.ChangeWorkerSize(_PageMode);

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
                var directories = entries.Where(e => Directory.Exists(e) || _ArchiverManager.IsSupported(e)).ToList();

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
