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

        private static Dictionary<PageStretchMode, string> _DispStrings = new Dictionary<PageStretchMode, string>
        {
            [PageStretchMode.None] = "オリジナルサイズ",
            [PageStretchMode.Inside] = "大きい場合、ウィンドウサイズに合わせる",
            [PageStretchMode.Outside] = "小さい場合、ウィンドウサイズに合わせる",
            [PageStretchMode.Uniform] = "ウィンドウサイズに合わせる",
            [PageStretchMode.UniformToFill] = "ウィンドウいっぱいに広げる",
        };

        public static string ToDispString(this PageStretchMode mode)
        {
            return _DispStrings[mode];
        }
    }


    public enum BookSortMode
    {
        FileName,
        TimeStamp,
        Random,
    }

    public static class BookSortModeExtension
    {
        public static BookSortMode GetToggle(this BookSortMode mode)
        {
            return (BookSortMode)(((int)mode + 1) % Enum.GetNames(typeof(BookSortMode)).Length);
        }

        public static string ToDispString(this BookSortMode mode)
        {
            switch (mode)
            {
                case BookSortMode.FileName: return "ファイル名順";
                case BookSortMode.TimeStamp: return "日付順";
                case BookSortMode.Random: return "ランダムに並べる";
                default:
                    throw new NotSupportedException();
            }
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

        public static string ToDispString(this BookReadOrder mode)
        {
            switch (mode)
            {
                case BookReadOrder.RightToLeft: return "右開き";
                case BookReadOrder.LeftToRight: return "左開き";
                default:
                    throw new NotSupportedException();
            }
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
    public class Book : IDisposable
    {
        public List<IDisposable> _TrashBox { get; private set; } = new List<IDisposable>();

        // 現在ページ変更(ページ番号)
        // タイトル、スライダーの更新を要求
        public event EventHandler<int> PageChanged;

        // 表示コンテンツ変更
        // 表示の更新を要求
        public event EventHandler ViewContentsChanged;

        // ページ終端を超えて移動しようとした
        // 次の本への移動を要求
        public event EventHandler<int> PageTerminated;

        // 再読み込みを要求
        public event EventHandler DartyBook;



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
                    if (Place != null)
                    {
                        ResetViewPages();
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
                    if (Place != null)
                    {
                        ResetViewPages();
                    }
                }
            }
        }

        // 右開き、左開き
        private BookReadOrder _BookReadOrder;
        public BookReadOrder BookReadOrder
        {
            get { return _BookReadOrder; }
            set
            {
                if (_BookReadOrder != value)
                {
                    _BookReadOrder = value;
                    if (Place != null)
                    {
                        ViewContentsChanged?.Invoke(this, null);
                        // TODO: もっと必要
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
                    DartyBook?.Invoke(this, null);
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
                        //ReloadViewPage();
                        ResetViewPages();
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
                if (_SortMode != value)
                {
                    _SortMode = value;
                    Sort();
                }
            }
        }

        // ランダムソートの場合はソートを必ず実行する
        public void SetSortMode(BookSortMode mode)
        {
            if (_SortMode != mode || mode == BookSortMode.Random)
            {
                _SortMode = mode;
                Sort();
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
                if (_SortMode == BookSortMode.Random)
                {
                    Pages.Reverse();
                    SetIndex(0, 1);
                }
                else
                {
                    Sort();
                }
            }
        }




        public List<Page> Pages { get; private set; }

        private int _OldIndex;
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
            if (Place == null) return;
            if (value > Pages.Count - 1) value = Pages.Count - 1;
            if (value < 0) value = 0;

            // 2ページ表示最終ページの場合の補正
            if (_PageMode == 2 && value == Pages.Count - 1)
            {
                if (direction < 0) value = Pages.Count - _PageMode;
            }

            if (_Index != value)
            {
                _OldIndex = _Index;
            }
            _Index = value;
            _Direction = direction;

            //if (IsSupportedTitlePage)
            //{
            //    _CurrentViewPageCount = _Index == 0 ? 1 : PageMode;
            //}
            //else
            {
                _CurrentViewPageCount = PageMode;
            }

            UpdateActivePages();
            lock (_Lock)
            {
                UpdateViewPage();
                UpdateNowPages();
                //UpdateViewPage();
            }
            PageChanged?.Invoke(this, _Index);
        }

        // この本の場所
        public string Place { get; private set; }

        [Flags]
        public enum LoadFolderOption
        {
            None = 0,
            Recursive = (1 << 0),
            SupportAllFile = (1 << 1),
            FirstPage = (1 << 2),
            LastPage = (1 << 3),
            ReLoad = (1 << 4),
        };


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
                    ForceUpdateNowPages();
                }

                //UpdateViewPage();
            }

            if (isDartyNowPages && ViewContentsChanged != null)
            {
                App.Current.Dispatcher.Invoke(() => ViewContentsChanged(this, null));
            }
        }

        private bool IsDartyViewPages()
        {
            if (_ViewPages[0] == null) return true;

            if (_ViewPages[0] == CurrentPage) return false;

            if (CurrentPage.Content != null) return true;

            // ViewPageが表示済であるならtrue
            //foreach (var page in _ViewPages)
            //{
            //    if (page != null && page.Content == null) return false;
            //}

            for (int i = 0; i < 2; i++)
            {
                if (_ViewPages[i] == null) continue;
                if (_ViewPages[i].Content == null || _ViewPages[i].Content != NowPages[i]?.Content) return false;
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
                    _ViewPages[0]?.Open(JobPriority.Top);
                }
            }

            if (_PageMode >= 2 && _ViewPages[1] != ViewPage(1))
            {
                if (_ViewPages[1] != null && !_KeepPages.Contains(_ViewPages[1]))
                {
                    _ViewPages[1].Close();
                }

                _ViewPages[1] = ViewPage(1);
                _ViewPages[1]?.Open(JobPriority.Top);
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
                ViewContentsChanged?.Invoke(this, null);
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
                    //int cid = (BookReadOrder == BookReadOrder.RightToLeft) ? i : _CurrentViewPageCount - 1 - i;
                    //NowPages[i] = _ViewPages[i] != null ? new ViewContent(_ViewPages[cid]) : null;
                    NowPages[i] = _ViewPages[i] != null ? new ViewContent(_ViewPages[i]) : null;
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

            // もともと単独ページであれば処理不要
            if (_CurrentViewPageCount <= 1) return;

            // 先頭ページは強制単独ページ処理
            if (IsSupportedTitlePage && _ViewPageIndex == 0)
            {
                ToSingleViewPage();
                return;
            }

            // ワイドページ非対応なら処理不要
            if (!IsSupportedWidePage)
            {
                return;
            }

            // どちらかのページが横長ならば、１ページ表示にする
            if ((_ViewPages[0] != null && _ViewPages[0].Width > _ViewPages[0].Height) ||
                (_ViewPages[1] != null && _ViewPages[1].Width > _ViewPages[1].Height))
            {
                ToSingleViewPage();
            }
        }

        private void ToSingleViewPage()
        {
            _CurrentViewPageCount = 1;

            // 進行方向がマイナスの場合、ページの計算からやりなおし？
            if (_Direction < 0 && _Index + 1 != _OldIndex)
            {
                _Index = _Index + 1;

                if (_ViewPages[0] != null && !_KeepPages.Contains(_ViewPages[0]))
                {
                    _ViewPages[0].Close();
                }
                _ViewPages[0] = _ViewPages[1];
                _ViewPages[1] = null;

                PageChanged?.Invoke(this, _Index);
            }

            // swap ... ン？
            // TODO: ViewPagesの順番を入れ替えることに疑問。他の処理で戻ってしまわないのか？
            // var temp = _ViewPages[0];
            //_ViewPages[0] = _ViewPages[1];
            //_ViewPages[1] = temp;
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


        // ファイル読み込み
        public async Task Load(string path, string start = null, LoadFolderOption option = LoadFolderOption.None)
        {
            Debug.Assert(Place == null);

            // ランダムソートの場合はブックマーク無効
            if (SortMode == BookSortMode.Random)
            {
                start = null;
            }

            // リカーシブフラグ
            if (IsRecursiveFolder)
            {
                option |= LoadFolderOption.Recursive;
            }

            Archiver archiver = null;

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
                else if (ModelContext.BitmapLoaderManager.IsSupported(path) || (option & LoadFolderOption.SupportAllFile) == LoadFolderOption.SupportAllFile)
                {
                    archiver = ModelContext.ArchiverManager.CreateArchiver(Path.GetDirectoryName(path));
                    start = Path.GetFileName(path);
                }
                else
                {
                    throw new FileFormatException("サポート外ファイルです");
                }
            }
            else
            {
                throw new FileNotFoundException("ファイルが見つかりません", path);
            }
            #endregion

            await LoadArchive(archiver, start, option);
        }


        // アーカイブ読み込み
        private async Task LoadArchive(Archiver archiver, string start, LoadFolderOption option)
        {
            try
            {
                await Task.Run(() =>
                    {
                        _Archivers.Add(archiver);
                        ReadArchive(archiver, "", option); // (option & LoadFolderOption.Recursive) == LoadFolderOption.Recursive);

                        // 初期ソート
                        Sort();

                        // スタートページ取得
                        int startIndex = (start != null) ? Pages.FindIndex(e => e.FullPath == start) : 0;
                        if ((option & LoadFolderOption.FirstPage) == LoadFolderOption.FirstPage)
                        {
                            startIndex = 0;
                            _Direction = 1;
                        }
                        else if ((option & LoadFolderOption.LastPage) == LoadFolderOption.LastPage)
                        {
                            startIndex = Pages.Count - 1;
                            _Direction = -1;
                        }

                        _StartIndex = startIndex;
                    });

                // 本有効化
                Place = archiver.Path;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }

        public int SubFolderCount;
        private int _StartIndex;

        public void Start()
        {
            Debug.Assert(Place != null);

            SetIndex(_StartIndex, _Direction);
        }

        private void ReadArchive(Archiver archiver, string place, LoadFolderOption option)
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
                bool isRecursive = (option & LoadFolderOption.Recursive) == LoadFolderOption.Recursive;
                if ((isRecursive || entries.Count == 1) && ModelContext.ArchiverManager.IsSupported(entry.Path))
                {
                    if (archiver is FolderFiles)
                    {
                        var ff = (FolderFiles)archiver;
                        ReadArchive(ModelContext.ArchiverManager.CreateArchiver(ff.GetFullPath(entry.Path)), LoosePath.Combine(place, entry.Path), option);
                    }
                    else
                    {
                        // テンポラリにアーカイブを解凍する
                        string tempFileName = Temporary.CreateTempFileName(Path.GetFileName(entry.Path));
                        archiver.ExtractToFile(entry.Path, tempFileName);
                        _TempArchives.Add(tempFileName);
                        ReadArchive(ModelContext.ArchiverManager.CreateArchiver(tempFileName), LoosePath.Combine(place, entry.Path), option);
                    }
                }
                else
                {
                    if (ModelContext.BitmapLoaderManager.IsSupported(entry.Path))
                    {
                        var page = new BitmapPage(entry, archiver, place);
                        Pages.Add(page);
                    }
                    else
                    {
                        var type = ModelContext.ArchiverManager.GetSupportedType(entry.Path);
                        bool isSupportAllFile = (option & LoadFolderOption.SupportAllFile) == LoadFolderOption.SupportAllFile;
                        if (isSupportAllFile)
                        {
                            switch (type)
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
                        else if (type != ArchiverType.None)
                        {
                            SubFolderCount++;
                        }
                    }
                }
            }
        }


        // ページの並び替え
        public void Sort()
        {
            if (Pages.Count <= 0) return;

            switch (SortMode)
            {
                case BookSortMode.FileName:
                    Pages.Sort((a, b) => ComparePath(a.FullPath, b.FullPath, Win32Api.StrCmpLogicalW));
                    break;
                //case BookSortMode.FileNameDictionary:
                //    Pages.Sort((a, b) => ComparePath(a.FullPath, b.FullPath, string.Compare));
                //    break;
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

            SetIndex(0, 1);

            //Debug.WriteLine("--");
            //Pages.ForEach(e => Debug.WriteLine(e.FullPath));
        }

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
            return (_ViewPages[0] == CurrentPage && _ViewPages.All(e => e == null || e.Content != null));
        }

        // 前のページに戻る
        public void PrevPage(int step = 0)
        {
            if (!IsStable()) return;

            //step = (step != 0) ? step : ((_IsSupportedTitlePage && Index <= 2) ? 1 : _PageMode);
            step = (step != 0) ? step : _PageMode;

            int index = Index - step;
            if (index < 0) index = 0;
            if (Index == index)
            {
                PageTerminated?.Invoke(this, -1);
            }
            else
            {
                Index = index;
            }
        }

        private int _CurrentViewPageCount = 1;

        // 次のページへ進む
        public void NextPage(int step = 0)
        {
            if (!IsStable()) return;

            // 既に最終ページ?
            if (Index + _CurrentViewPageCount >= Pages.Count)
            {
                PageTerminated?.Invoke(this, +1);
                return;
            }

            int index = Index + ((step == 0) ? _CurrentViewPageCount : step);
            if (index > Pages.Count - 1)
            {
                index = Pages.Count - 1;
                if (index < 0) index = 0;
            }
            if (Index == index)
            {
                PageTerminated?.Invoke(this, +1);
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


        // ページの再読み込み
        private void ReloadViewPage()
        {
            if (Place == null) return;

            lock (_Lock)
            {
                if (_ViewPages[0] != null)
                {
                    _ViewPages[0].Close();
                    _ViewPages[0].Open(JobPriority.Top);
                }
            }
        }

        // 表示数変更によるViewPages作り直し
        private void ResetViewPages()
        {
            if (Place == null) return;

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

            //ModelContext.JobEngine.ChangeWorkerSize(_PageMode);

            SetIndex(Index, _Direction);
        }



        // 全ページクリア
        private void Close()
        {
            lock (_Lock)
            {
                Pages.ForEach(e => e?.Close());
                Pages.Clear();

                /*
                for (int i = 0; i < _ViewPages.Count; ++i)
                {
                    _ViewPages[i] = null;
                }
                _ViewPageIndex = 0;

                _KeepPages.Clear();
                */

                _Archivers.ForEach(e => e.Dispose());
                _Archivers.Clear();

                foreach (var temp in _TempArchives)
                {
                    try
                    {
                        File.Delete(temp);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                    }
                }
                _TempArchives.Clear();

                _TrashBox.ForEach(e => e.Dispose());
                _TrashBox.Clear();
            }
        }



        public void Dispose()
        {
            Page.ContentChanged -= Page_ContentChanged;
            Close();
        }



        // 本単位の保存される設定
        [DataContract]
        public class Memento
        {
            [DataMember]
            public string Place { get; set; }

            [DataMember]
            public string BookMark { get; set; }

            [DataMember]
            public int PageMode { get; set; }

            [DataMember]
            public BookReadOrder BookReadOrder { get; set; }

            [DataMember]
            public bool IsSupportedTitlePage { get; set; }

            [DataMember]
            public bool IsSupportedWidePage { get; set; }

            [DataMember]
            public bool IsRecursiveFolder { get; set; }

            [DataMember]
            public BookSortMode SortMode { get; set; }

            [DataMember]
            public bool IsReverseSort { get; set; }




            //
            private void Constructor()
            {
                PageMode = 1;
            }

            public Memento()
            {
                Constructor();
            }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
            }

            public Memento Clone()
            {
                return (Memento)this.MemberwiseClone();
            }
        }

        // bookの設定を取得する
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.Place = Place;

            memento.BookMark = CurrentPage?.FullPath;

            memento.PageMode = PageMode;
            memento.BookReadOrder = BookReadOrder;
            memento.IsSupportedTitlePage = IsSupportedTitlePage;
            memento.IsSupportedWidePage = IsSupportedWidePage;
            memento.IsRecursiveFolder = IsRecursiveFolder;
            memento.SortMode = SortMode;
            memento.IsReverseSort = IsReverseSort;

            return memento;
        }

        // bookに設定を反映させる
        public void Restore(Memento memento)
        {
            PageMode = memento.PageMode;
            BookReadOrder = memento.BookReadOrder;
            IsSupportedTitlePage = memento.IsSupportedTitlePage;
            IsSupportedWidePage = memento.IsSupportedWidePage;
            IsRecursiveFolder = memento.IsRecursiveFolder;
            SortMode = memento.SortMode;
            IsReverseSort = memento.IsReverseSort;
        }

        /*
        public static BookSetting Store(BookHub book)
        {
            if (BookHub.Current != null)
            {
                book.BookSetting.Store(BookHub.Current);
            }
            return book.BookSetting.Clone();
        }

        public void Restore(BookHub book)
        {
            book.BookSetting = this.Clone();
            if (BookHub.Current != null)
            {
                book.BookSetting.Restore(BookHub.Current);
            }
        }
        */
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
