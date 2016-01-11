// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace NeeView
{

    /// <summary>
    /// ロードオプションフラグ
    /// </summary>
    [Flags]
    public enum BookLoadOption
    {
        None = 0,
        Recursive = (1 << 0), // 再帰
        SupportAllFile = (1 << 1), // すべてのファイルをページとみなす
        FirstPage = (1 << 2), // 初期ページを先頭ページにする
        LastPage = (1 << 3), // 初期ページを最終ページにする
        ReLoad = (1 << 4), // 再読み込みフラグ(BookHubで使用)
    };


    /// <summary>
    /// 本
    /// </summary>
    public class Book : IDisposable
    {
        // テンポラリコンテンツ用ゴミ箱
        public TrashBox _TrashBox { get; private set; } = new TrashBox();


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


        // 最初のページは単独表示
        private bool _IsSupportedSingleFirstPage;
        public bool IsSupportedSingleFirstPage
        {
            get { return _IsSupportedSingleFirstPage; }
            set
            {
                if (_IsSupportedSingleFirstPage != value)
                {
                    _IsSupportedSingleFirstPage = value;
                    Reflesh();
                }
            }
        }

        // 最後のページは単独表示
        private bool _IsSupportedSingleLastPage;
        public bool IsSupportedSingleLastPage
        {
            get { return _IsSupportedSingleLastPage; }
            set
            {
                if (_IsSupportedSingleLastPage != value)
                {
                    _IsSupportedSingleLastPage = value;
                    Reflesh();
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
                    Reflesh();
                }
            }
        }

        // 右開き、左開き
        private PageReadOrder _BookReadOrder = PageReadOrder.RightToLeft;
        public PageReadOrder BookReadOrder
        {
            get { return _BookReadOrder; }
            set
            {
                if (_BookReadOrder != value)
                {
                    _BookReadOrder = value;
                    ViewContentsChanged?.Invoke(this, null);
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

        // 単ページ/見開き
        private int _PageMode = 1;
        public int PageMode
        {
            get { return _PageMode; }
            set
            {
                Debug.Assert(value == 1 || value == 2);
                if (_PageMode != value)
                {
                    _PageMode = value;

                    int start = (CurrentViewPageCount >= 2 && _ViewPageDirection < 0) ? Index - 1 : Index;
                    Reflesh(start, 1);
                }
            }
        }

        // 表示しているページ数
        public int CurrentViewPageCount { get; private set; } = 1;


        // ページ列
        private PageSortMode _SortMode = PageSortMode.FileName;
        public PageSortMode SortMode
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

        // ページ列を設定
        // プロパティと異なり、ランダムソートの場合はソートを再実行する
        public void SetSortMode(PageSortMode mode)
        {
            if (_SortMode != mode || mode == PageSortMode.Random)
            {
                _SortMode = mode;
                Sort();
            }
        }

        // ページ列を逆順にする
        private bool _IsReverseSort;
        public bool IsReverseSort
        {
            get { return _IsReverseSort; }
            set
            {
                if (_IsReverseSort == value) return;
                _IsReverseSort = value;
                if (_SortMode == PageSortMode.Random)
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


        // この本の場所
        // nullの場合、この本は無効
        public string Place { get; private set; }

        // アーカイバコレクション
        // Dispose処理のために保持
        List<Archiver> _Archivers = new List<Archiver>();

        // ページ コレクション
        public List<Page> Pages { get; private set; }

        // 先読み有効
        // ページ切替時に自動で有効に戻される
        public bool IsEnablePreLoad { get; set; } = true;

        // 現在のページ番号
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

        // 現在ページ番号設定
        public void SetIndex(int value, int direction)
        {
            Debug.Assert(direction == 1 || direction == -1);

            if (Place == null) return;
            if (value > Pages.Count - 1) value = Pages.Count - 1;
            if (value < 0) value = 0;

            // 現在ページ更新
            _Index = value;
            _Direction = direction;

            // 表示ページ数はページモードに依存
            CurrentViewPageCount = PageMode;

            // ページ状態更新
            lock (_Lock)
            {
                UpdateActivePages(IsEnablePreLoad);
                IsEnablePreLoad = true; // 通常、先読み有効

                UpdateViewPage();
                UpdateViewContents();
            }
            PageChanged?.Invoke(this, _Index);
        }




        // 読み進めている方向
        private int _Direction = 1;

        // 現在ページ
        public Page CurrentPage
        {
            get { return Pages.Count > 0 ? Pages[Index] : null; }
        }

        // 現在ページ+1
        public Page CurrentNextPage
        {
            get { return GetCurrentPage(1); }
        }

        // 現在ページ取得(オフセット指定)
        private Page GetCurrentPage(int offset)
        {
            int index = Index + offset;
            return (0 <= index && index < Pages.Count) ? Pages[index] : null;
        }



        // 表示ページの番号
        private int _ViewPageIndex;

        // 表示ページ
        public Page GetViewPage(int offset)
        {
            int index = _ViewPageIndex + offset;
            return (0 <= index && index < Pages.Count) ? Pages[index] : null;
        }

        // リソースを保持しておくページ
        private List<Page> _KeepPages;

        // 表示するべきページ
        private volatile List<Page> _ViewPages;

        // 表示されているコンテンツ
        public volatile List<ViewContentSource> ViewContentSources;


        // 排他処理用ロックオブジェクト
        private object _Lock = new object();


        // コンストラクタ
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

            ViewContentSources = new List<ViewContentSource>();
            for (int i = 0; i < 2; ++i)
            {
                ViewContentSources.Add(null);
            }
        }



        // ページのコンテンツロード完了イベント処理
        // 注意：ジョブワーカーから呼ばれることがあるので別スレッドの可能性があります。
        private void Page_ContentChanged(object sender, EventArgs e)
        {
            bool isDartyNowPages = false;

            lock (_Lock)
            {
                if (sender == CurrentPage)
                {
                    UpdateViewPage();
                    isDartyNowPages = IsDartyViewContents();
                }
                else if (_ViewPages.Contains(sender))
                {
                    isDartyNowPages = IsDartyViewContents();
                }

                if (isDartyNowPages)
                {
                    ValidateWidePage();
                    SetViewContents();
                    UpdateViewPage();
                }
            }

            if (isDartyNowPages && ViewContentsChanged != null)
            {
                App.Current.Dispatcher.Invoke(() => ViewContentsChanged(this, null));
            }
        }

        // ViewContentSourceに反映されたらONになる
        bool _IsDartyViewPages = false;

        // 表示ページの更新が必要かチェックをする
        // 表示ページが実際に表示されていれば更新を許可する
        private bool IsDartyViewPages()
        {
            if (_ViewPages[0] == null) return true;

            if (_Direction != _ViewPageDirection) return true;

            if (_ViewPages[0] == CurrentPage) return false;

            return _IsDartyViewPages;
        }


        int _ViewPageDirection = 1;

        // 表示ページの更新
        // 表示ページコンテンツは最優先でロードを行う
        private void UpdateViewPage()
        {
            // over viewpage
            if (_PageMode <= 1 && _ViewPages[1] != null)
            {
                _ViewPages[1].Close();
                _ViewPages[1] = null;
            }

            // Main ViewPage
            if (IsDartyViewPages())
            {
                _ViewPageIndex = Index;
                _ViewPageDirection = _Direction;

                if (_ViewPages[0] != GetViewPage(0))
                {
                    if (_ViewPages[0] != null && !_KeepPages.Contains(_ViewPages[0]))
                    {
                        _ViewPages[0].Close();
                    }

                    _ViewPages[0] = GetViewPage(0);
                    _ViewPages[0]?.Open(QueueElementPriority.Top);
                }

                _IsDartyViewPages = false;
            }

            // Sub ViewPage
            if (_PageMode >= 2 && _ViewPages[0] != null && _ViewPages[1] != GetViewPage(_ViewPageDirection))
            {
                if (_ViewPages[1] != null && !_KeepPages.Contains(_ViewPages[1]))
                {
                    _ViewPages[1].Close();
                }

                _ViewPages[1] = GetViewPage(_ViewPageDirection);
                _ViewPages[1]?.Open(QueueElementPriority.Top);
            }
        }

        // 表示コンテンツの更新
        private void UpdateViewContents()
        {
            if (IsDartyViewContents())
            {
                ValidateWidePage();
                SetViewContents();
                ViewContentsChanged?.Invoke(this, null);
                UpdateViewPage();
            }
        }

        // 表示コンテンツの更新チェック
        // 表示ページの準備ができていれば更新
        private bool IsDartyViewContents()
        {
            if (_ViewPages[0] == null) return true;

            return (_ViewPages.All(e => e == null || e.Content != null));
        }

        // 表示コンテンツの更新実行
        private void SetViewContents()
        {
            for (int i = 0; i < ViewContentSources.Count; ++i)
            {
                if (i < CurrentViewPageCount && _ViewPages[i] != null)
                {
                    ViewContentSources[i] = new ViewContentSource(_ViewPages[i], _ViewPageIndex + _ViewPageDirection * i);
                    ////if (_ViewPages[i] != null) Debug.WriteLine($"View: {_ViewPages[i].FileName}");
                }
                else
                {
                    ViewContentSources[i] = null;
                }
            }

            // ２ページ表示で方向が逆なら入れ替える
            if (_ViewPageDirection < 0 && ViewContentSources.All(e => e != null))
            {
                ViewContentSources.Reverse();
            }

            _IsDartyViewPages = true;
        }

        // ワイドページ処理
        private void ValidateWidePage()
        {
            // IsDartyViewContentsが確定してから処理される
            // すなわち１度だけの処理

            // もともと単独ページであれば処理不要
            if (CurrentViewPageCount <= 1) return;

            // 先頭ページの強制単独ページ処理
            if (IsSupportedSingleFirstPage && (_ViewPageIndex == 0 || _ViewPageIndex + _ViewPageDirection == 0))
            {
                ToSingleViewPage();
                return;
            }

            // 最終ページの強制単独ページ処理
            if (IsSupportedSingleLastPage && (_ViewPageIndex == Pages.Count - 1 || _ViewPageIndex + _ViewPageDirection == Pages.Count - 1))
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

        // 見開きページから単ページ表示に補正する処理
        private void ToSingleViewPage()
        {
            CurrentViewPageCount = 1;
        }

        // ページコンテンツの準備
        // 先読み、不要ページコンテンツの削除を行う
        private void UpdateActivePages(bool preLoad)
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
                page?.Open(QueueElementPriority.Top);
            }

            // 先読み
            if (preLoad)
            {
                foreach (var page in currentPages)
                {
                    page.Open(QueueElementPriority.Hi);
                }
                foreach (var page in preLoadPages)
                {
                    page.Open(QueueElementPriority.Default);
                }
            }

            // 保持ページ更新
            _KeepPages = keepPages;
        }




        // 本読み込み
        public async Task Load(string path, string start = null, BookLoadOption option = BookLoadOption.None)
        {
            Debug.Assert(Place == null);

            // ランダムソートの場合はブックマーク無効
            if (SortMode == PageSortMode.Random)
            {
                start = null;
            }

            // リカーシブフラグ
            if (IsRecursiveFolder)
            {
                option |= BookLoadOption.Recursive;
            }

            // アーカイバの選択
            #region SelectArchiver
            Archiver archiver = null;
            if (Directory.Exists(path))
            {
                archiver = ModelContext.ArchiverManager.CreateArchiver(path);
            }
            else if (File.Exists(path))
            {
                if (ModelContext.ArchiverManager.IsSupported(path))
                {
                    archiver = ModelContext.ArchiverManager.CreateArchiver(path);
                    option |= BookLoadOption.Recursive; // 圧縮ファイルはリカーシブ標準
                }
                else if (ModelContext.BitmapLoaderManager.IsSupported(path) || (option & BookLoadOption.SupportAllFile) == BookLoadOption.SupportAllFile)
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

            // 読み込み(非同期タスク)
            await Task.Run(() =>
            {
                ReadArchive(archiver, "", option);

                // 初期ソート
                Sort();

                // スタートページ取得
                int startIndex = (start != null) ? Pages.FindIndex(e => e.FullPath == start) : 0;
                if ((option & BookLoadOption.FirstPage) == BookLoadOption.FirstPage)
                {
                    startIndex = 0;
                    _Direction = 1;
                }
                else if ((option & BookLoadOption.LastPage) == BookLoadOption.LastPage)
                {
                    startIndex = Pages.Count - 1;
                    _Direction = -1;
                }

                _StartIndex = startIndex;
            });

            // 本有効化
            Place = archiver.FileName;
        }

        // 読み込み対象外サブフォルダ数。リカーシブ確認に使用します。
        public int SubFolderCount { get; private set; }

        // 開始ページ番号
        private int _StartIndex;

        // 開始
        // ページ設定を行うとコンテンツ読み込みが始まるため、ロードと分離した
        public void Start()
        {
            Debug.Assert(Place != null);

            SetIndex(_StartIndex, _Direction);
        }


        // アーカイブからページ作成(再帰)
        private void ReadArchive(Archiver archiver, string place, BookLoadOption option)
        {
            List<ArchiveEntry> entries = null;

            _Archivers.Add(archiver);

            try
            {
                entries = archiver.GetEntries();
            }
            catch
            {
                Debug.WriteLine($"{archiver.FileName} の展開に失敗しました");
                return;
            }

            foreach (var entry in entries)
            {
                // 再帰設定、もしくは単一ファイルの場合、再帰を行う
                bool isRecursive = (option & BookLoadOption.Recursive) == BookLoadOption.Recursive;
                if ((isRecursive || entries.Count == 1) && ModelContext.ArchiverManager.IsSupported(entry.FileName))
                {
                    if (archiver is FolderFiles)
                    {
                        var ff = (FolderFiles)archiver;
                        ReadArchive(ModelContext.ArchiverManager.CreateArchiver(ff.GetFullPath(entry.FileName)), LoosePath.Combine(place, entry.FileName), option);
                    }
                    else
                    {
                        // テンポラリにアーカイブを解凍する
                        string tempFileName = Temporary.CreateTempFileName(Path.GetFileName(entry.FileName));
                        archiver.ExtractToFile(entry.FileName, tempFileName);
                        _TrashBox.Add(new TrashFile(tempFileName));
                        ReadArchive(ModelContext.ArchiverManager.CreateArchiver(tempFileName), LoosePath.Combine(place, entry.FileName), option);
                    }
                }
                else
                {
                    if (ModelContext.BitmapLoaderManager.IsSupported(entry.FileName))
                    {
                        var page = new BitmapPage(archiver, entry, place);
                        Pages.Add(page);
                    }
                    else
                    {
                        var type = ModelContext.ArchiverManager.GetSupportedType(entry.FileName);
                        bool isSupportAllFile = (option & BookLoadOption.SupportAllFile) == BookLoadOption.SupportAllFile;
                        if (isSupportAllFile)
                        {
                            switch (type)
                            {
                                case ArchiverType.None:
                                    Pages.Add(new FilePage(archiver, entry, place, FilePageIcon.File));
                                    break;
                                case ArchiverType.FolderFiles:
                                    Pages.Add(new FilePage(archiver, entry, place, FilePageIcon.Folder));
                                    break;
                                default:
                                    Pages.Add(new FilePage(archiver, entry, place, FilePageIcon.Archive));
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
                case PageSortMode.FileName:
                    Pages.Sort((a, b) => ComparePath(a.FullPath, b.FullPath, Win32Api.StrCmpLogicalW));
                    break;
                case PageSortMode.TimeStamp:
                    Pages = Pages.OrderBy(e => e.UpdateTime).ToList();
                    break;
                case PageSortMode.Random:
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
        }


        // ファイル名比較
        // ディレクトリを優先する
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

            // ページ移動量調整
            step = (step == 0) ? CurrentViewPageCount : step;
            if (_Direction > 0) step = 1; // 読む方向が反転する場合、移動量は1

            // 既に先頭ページ?
            if (Index - step < 0)
            {
                PageTerminated?.Invoke(this, -1);
                //if (Index > 0) FirstPage();
                return;
            }

            //
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


        // 次のページへ進む
        public void NextPage(int step = 0)
        {
            if (!IsStable()) return;

            // ページ移動量調整
            step = (step == 0) ? CurrentViewPageCount : step;
            if (_Direction < 0) step = 1; // 読む方向が反転する場合、移動量は1

            // 既に最終ページ?
            if (Index + step >= Pages.Count)
            {
                PageTerminated?.Invoke(this, +1);
                //if (Index < Pages.Count - 1) LastPage();
                return;
            }

            //
            int index = Index + step;
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


        // 最初のページに移動
        public void FirstPage()
        {
            if (Index > 0)
            {
                SetIndex(0, +1);
            }
        }

        // 最後のページに移動
        public void LastPage()
        {
            if (Index < Pages.Count - 1)
            {
                SetIndex(Pages.Count - 1, -1);
            }
        }



        // ページの再読み込み
        public void Reflesh(int index, int direction)
        {
            if (Place == null) return;

            lock (_Lock)
            {
                _KeepPages.ForEach(e => e?.Close());
                _ViewPages.ForEach(e => e?.Close());
            }

            SetIndex(index, direction);
        }


        // ページの再読み込み
        public void Reflesh()
        {
            Reflesh(Index, _Direction);
        }


        // 廃棄処理
        public void Dispose()
        {
            Page.ContentChanged -= Page_ContentChanged;

            lock (_Lock)
            {
                Pages.ForEach(e => e?.Close());
                Pages.Clear();

                _Archivers.ForEach(e => e.Dispose());
                _Archivers.Clear();

                _TrashBox.Clear();
            }
        }


        #region Memento

        /// <summary>
        /// 保存設定
        /// </summary>
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
            public PageReadOrder BookReadOrder { get; set; }

            [DataMember]
            public bool IsSupportedSingleFirstPage { get; set; }

            [DataMember]
            public bool IsSupportedSingleLastPage { get; set; }

            [DataMember]
            public bool IsSupportedWidePage { get; set; }

            [DataMember]
            public bool IsRecursiveFolder { get; set; }

            [DataMember]
            public PageSortMode SortMode { get; set; }

            [DataMember]
            public bool IsReverseSort { get; set; }


            //
            private void Constructor()
            {
                PageMode = 1;
            }

            //
            public Memento()
            {
                Constructor();
            }

            //
            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
            }

            //
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
            memento.IsSupportedSingleFirstPage = IsSupportedSingleFirstPage;
            memento.IsSupportedSingleLastPage = IsSupportedSingleLastPage;
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
            IsSupportedSingleFirstPage = memento.IsSupportedSingleFirstPage;
            IsSupportedSingleLastPage = memento.IsSupportedSingleLastPage;
            IsSupportedWidePage = memento.IsSupportedWidePage;
            IsRecursiveFolder = memento.IsRecursiveFolder;
            SortMode = memento.SortMode;
            IsReverseSort = memento.IsReverseSort;
        }
    }

    #endregion
}
