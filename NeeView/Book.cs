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
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public struct PageValue
    {
        public int Value { get; set; }

        public int Index
        {
            get { return Value / 2; }
            set { Value = value * 2; }
        }

        public int Part
        {
            get { return Value % 2; }
            set { Value = Index * 2 + value; }
        }

        // constructor
        public PageValue(int index, int part)
        {
            Value = index * 2 + part;
        }

        //
        public override string ToString()
        {
            return "{" + Index.ToString() + "," + Part.ToString() + "}";
        }

        // add
        public static PageValue operator +(PageValue a, PageValue b)
        {
            return new PageValue() { Value = a.Value + b.Value };
        }

        public static PageValue operator +(PageValue a, int b)
        {
            return new PageValue() { Value = a.Value + b };
        }

        public static PageValue operator -(PageValue a, PageValue b)
        {
            return new PageValue() { Value = a.Value - b.Value };
        }

        public static PageValue operator -(PageValue a, int b)
        {
            return new PageValue() { Value = a.Value - b };
        }

        // compare
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is PageValue)) return false;
            return Value == ((PageValue)obj).Value;
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public static bool operator ==(PageValue a, PageValue b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(PageValue a, PageValue b)
        {
            return a.Value != b.Value;
        }

        public static bool operator <(PageValue a, PageValue b)
        {
            return a.Value < b.Value;
        }

        public static bool operator >(PageValue a, PageValue b)
        {
            return a.Value > b.Value;
        }

        public static bool operator <=(PageValue a, PageValue b)
        {
            return a.Value <= b.Value;
        }

        public static bool operator >=(PageValue a, PageValue b)
        {
            return a.Value >= b.Value;
        }
        // limit
        public PageValue Limit(PageValue min, PageValue max)
        {
            if (min.Value > max.Value) throw new ArgumentOutOfRangeException();

            int value = Value;
            if (value < min.Value) value = min.Value;
            if (value > max.Value) value = max.Value;

            return new PageValue() { Value = value };
        }
    }


    // ページ番号
    public class PageIndex
    {
        public int Index
        {
            get { return _Value / 2; }
            set { _Value = value * 2; }
        }

        public int IndexSize { get; set; }

        public int Part
        {
            get { return _Value % 2; }
            set { _Value = Index * 2 + value; }
        }

        int _Value;

        int _MaxValue => IndexSize * 2 - 1;

        public bool IsEqual(int index, int part)
        {
            return (index * 2 + part == _Value);
        }

        public int Direction(int index, int part)
        {
            return index * 2 + part < _Value ? -1 : 1;
        }

        public void Set(int index, int part)
        {
            _Value = index * 2 + part;
            if (_Value < 0) _Value = 0;
            if (_Value > _MaxValue) _Value = _MaxValue;
        }


        public void Move(int step)
        {
            _Value += step;
            if (_Value < 0) _Value = 0;
            if (_Value > _MaxValue) _Value = _MaxValue;
        }

        public void MoveToFirst()
        {
            _Value = 0;
        }

        public void MoveToLast()
        {
            _Value = _MaxValue;
        }
    }



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

                    // ここ前変更か？
                    //int start = (CurrentViewPageCount >= 2 && _ViewPageDirection < 0) ? Index - 1 : Index;
                    //Reflesh(start, 0, 1);
                    Reflesh(Index, 1); // 方向だけ修正
                }
            }
        }

        // 表示しているページ数
        //public int CurrentViewPageCount { get; private set; } = 1;


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
                    SetIndex(new PageValue(0, 0), 1);
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

        // ページ番号
        //private PageIndex _Index;

        // 現在のページ番号
        //private int _Index;
        private PageValue _Index;
        public PageValue Index
        {
            get { return _Index; }
            set
            {
                if (_Index == value) return;
                SetIndex(value, (_Index > value) ? -1 : 1);
            }
        }

        // 現在のページパーツ
        public int Part { get; set; }


        // 現在ページ番号設定
        public void SetIndex(PageValue value, int direction)
        {
            Debug.Assert(direction == 1 || direction == -1);

            if (Place == null) return;

            if (Pages.Count == 0)
            {
                ViewContentSources[0] = null;
                ViewContentSources[1] = null;
                ViewContentsChanged?.Invoke(this, null);
                return;
            }

            //value = value.Limit(new PageValue(0, 0), new PageValue(Pages.Count - 1, 1));

            // 現在ページ更新
            //_Index = value;
            //_Direction = direction;

            // 表示ページ数はページモードに依存
            //CurrentViewPageCount = PageMode;

            RequestViewPage(new SetPageCommand(this, value, direction, PageMode));
        }

        public void MoveIndex(int step)
        {
            if (Place == null) return;

            if (Pages.Count == 0)
            {
                ViewContentSources[0] = null;
                ViewContentSources[1] = null;
                ViewContentsChanged?.Invoke(this, null);
                return;
            }

            RequestViewPage(new MovePageCommand(this, step));
        }



        // 読み進めている方向
        private int _Direction = 1;

        // 現在ページ
        public Page CurrentPage
        {
            get { return Pages.Count > 0 ? Pages[Index.Index] : null; }
        }

        // 現在ページ+1
        public Page CurrentNextPage
        {
            get { return GetCurrentPage(1); }
        }

        // 現在ページ取得(オフセット指定)
        private Page GetCurrentPage(int offset)
        {
            int index = Index.Index + offset;
            return (0 <= index && index < Pages.Count) ? Pages[index] : null;
        }



        // 表示ページの番号
        private PageValue _ViewPageIndex;

        // 表示ページ
        public Page GetViewPage(int offset)
        {
            int index = _ViewPageIndex.Index + offset;
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
            //Page.ContentChanged += Page_ContentChanged;

            _PageMode = 1;
            _ViewPages = new List<Page>();
            for (int i = 0; i < 2; ++i)
            {
                _ViewPages.Add(null);
            }
            _ViewPageIndex = new PageValue(0, 0);

            ViewContentSources = new List<ViewContentSource>();
            for (int i = 0; i < 2; ++i)
            {
                ViewContentSources.Add(null);
            }

            _Context = new ViewPageContext();
            //_Context.ViewContents = ViewContentSources;

            StartUpdateViewPageTask();
        }


#if false
        // ページのコンテンツロード完了イベント処理
        // 注意：ジョブワーカーから呼ばれることがあるので別スレッドの可能性があります。
        private void Page_ContentChanged(object sender, EventArgs e)
        {
            return; // ##

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
                    //ValidateWidePage();
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
                _ViewPageDirection = _Direction; // 不要?

                if (_ViewPages[0] != GetViewPage(0))
                {
                    if (_ViewPages[0] != null && !_KeepPages.Contains(_ViewPages[0]))
                    {
                        _ViewPages[0].Close();
                    }

                    _ViewPages[0] = GetViewPage(0);
                    _ViewPages[0]?.Open(QueueElementPriority.Top);
                }

                // Sub ViewPage
                if (_PageMode >= 2 && _ViewPages[1] != GetViewPage(_ViewPageDirection))
                {
                    if (_ViewPages[1] != null && !_KeepPages.Contains(_ViewPages[1]))
                    {
                        _ViewPages[1].Close();
                    }

                    _ViewPages[1] = GetViewPage(_ViewPageDirection);
                    _ViewPages[1]?.Open(QueueElementPriority.Top);
                }

                _IsDartyViewPages = false;
            }
        }

        // 表示コンテンツの更新
        private void UpdateViewContents()
        {
            if (IsDartyViewContents())
            {
                //ValidateWidePage();
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



        private void SetViewContents(int id, PageValue v)
        {
            Page page = Pages[v.Index];
            if (page != null)
            {
                ViewContentSources[id] = new ViewContentSource(page, v.Index);
                ViewContentSources[id].Part = IsSupportedWidePage && page.IsWide ? (v.Part == 0 ? PagePart.Right : PagePart.Left) : PagePart.All;
            }
            else
            {
                ViewContentSources[id] = null;
            }
        }

        int contentPartSize;

        // 表示コンテンツの更新実行
        private void SetViewContents()
        {
            var varray = new List<PageValue>();
            var parray = new List<PagePart>();

            PageValue v = _ViewPageIndex;
            contentPartSize = 0;

            for (int id = 0; id < 2 && id < CurrentViewPageCount; ++id)
            {
                if (Pages[v.Index] == null) break;

                PagePart part = PagePart.All;
                if (IsSupportedWidePage && Pages[v.Index].IsWide)
                {
                    part = v.Part == 0 ? PagePart.Right : PagePart.Left;
                }
                else
                {
                    v.Part = 0;
                }

                //ViewContentSources[id] = new ViewContentSource(Pages[v.Index], v.Index);
                //ViewContentSources[id].Part = part;

                int partSize = part == PagePart.All ? 2 : 1;

                varray.Add(v);
                parray.Add(part);

                contentPartSize += partSize;

                v = v + ((_ViewPageDirection > 0) ? partSize : -1);
                if (v < new PageValue(0, 0) || v > new PageValue(Pages.Count - 1, 1)) break;
            }

            for (int id = 0; id < 2; ++id)
            {
                if (id < varray.Count)
                {
                    Page page = Pages[varray[id].Index];
                    ViewContentSources[id] = new ViewContentSource(page, varray[id].Index);
                    ViewContentSources[id].Part = parray[id];
                }
                else
                {
                    ViewContentSources[id] = null;
                }
            }

            // 現在ページID更新
            if (varray.Count > 0)
            {
                var newIndex = varray.Aggregate((value, e) => (value == null) ? e : (value == null) ? v : (e < value) ? e : value);
                _Index = newIndex;
            }

            //_IsDartyViewPages = true;
        }

#if false
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
#endif
#endif

        //
        ViewPageContext _Context;

        //
        public class ViewInfo
        {
            public PageValue Index;
            public int Size;
        }

        List<ViewInfo> _ViewInfoList;


        //
        public class ViewPageContext
        {
            public PageValue Index { get; set; }
            public int Direction { get; set; }
            public int Size { get; set; }

            //public List<ViewContentSource> ViewContents { get; set; }
        }

        public abstract class ViewPageCommand
        {
            protected Book _Book;
            public abstract ViewPageContext GetViewPageContext();
        }

        public class MovePageCommand : ViewPageCommand
        {
            public int Step { get; set; }
            public override ViewPageContext GetViewPageContext()
            {
                int size = 0;
                if (Step > 0)
                {
                    for (int i = 0; i < Step && i < _Book._ViewInfoList.Count; ++i)
                    {
                        size += _Book._ViewInfoList[i].Size;
                    }
                }
                else if (_Book._Context.Size == 2)
                {
                    size = Step + 1;
                }
                else
                {
                    size = Step;
                }

                return new ViewPageContext()
                {
                    Index = _Book._Context.Index + size,  //((Step < 0) ? -1 : size),
                    Direction = Step < 0 ? -1 : 1,
                    Size = _Book._Context.Size,
                };
            }
            public MovePageCommand(Book book, int step)
            {
                _Book = book;
                Step = step;
            }
        }

        public class SetPageCommand : ViewPageCommand
        {
            ViewPageContext Context { get; set; }
            public override ViewPageContext GetViewPageContext()
            {
                return Context;
            }

            public SetPageCommand(Book book, PageValue index, int direction, int size)
            {
                _Book = book;
                Context = new ViewPageContext()
                {
                    Index = index,
                    Direction = direction,
                    Size = size,
                };
            }
        }



        CancellationTokenSource _CancellationTokenSource;

        ViewPageCommand _request;

        public AutoResetEvent waitEvent { get; private set; } = new AutoResetEvent(false);

        private void RequestViewPage(ViewPageCommand command)
        {
            _request = command;
            waitEvent.Set();
        }

        private void StartUpdateViewPageTask()
        {
            _CancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => UpdateViewPageTask(), _CancellationTokenSource.Token);
        }

        private void BreakUpdateViewPageTask()
        {
            _CancellationTokenSource.Cancel();
            waitEvent.Set();
        }


        private async Task UpdateViewPageTask()
        {
            while (!_CancellationTokenSource.Token.IsCancellationRequested)
            {
                waitEvent.WaitOne();

                _CancellationTokenSource.Token.ThrowIfCancellationRequested();

                var request = _request;
                _request = null;

                await UpdateViewPageAsync(request);
            }

            Debug.WriteLine("BookTask.End");
        }


        private int ClampPageNumber(int index)
        {
            if (index < 0) index = 0;
            if (index > Pages.Count - 1) index = Pages.Count - 1;
            return index;
        }

        private bool IsValidIndex(PageValue index)
        {
            return (new PageValue(0, 0) <= index && index < new PageValue(Pages.Count, 0));
        }

        private async Task UpdateViewPageAsync(ViewPageCommand request)
        {
            var context = request.GetViewPageContext();

            if (!IsValidIndex(context.Index))
            {
                if (context.Index < new PageValue(0, 0))
                {
                    App.Current.Dispatcher.Invoke(() => PageTerminated?.Invoke(this, -1));
                }
                else
                {
                    App.Current.Dispatcher.Invoke(() => PageTerminated?.Invoke(this, +1));
                }
                return;
            }

            // view pages
            var viewPages = new List<Page>();
            for (int i = 0; i < context.Size; ++i)
            {
                var page = Pages[ClampPageNumber(context.Index.Index + context.Direction * i)];
                if (!viewPages.Contains(page))
                {
                    viewPages.Add(page);
                }
            }

            // load wait
            var tlist = new List<Task>();
            foreach (var page in viewPages)
            {
                tlist.Add(page.LoadAsync(QueueElementPriority.Top));
            }
            await Task.WhenAll(tlist);


            // update contents
            SetViewContentsEx(context);

            // cancel?
            _CancellationTokenSource.Token.ThrowIfCancellationRequested();

            // notice update
            App.Current.Dispatcher.Invoke(() => ViewContentsChanged?.Invoke(this, null));

            // PropertyChanged
            PageChanged?.Invoke(this, _Context.Index.Index);

            // cleanup pages
            _KeepPages.AddRange(viewPages.Where(e => !_KeepPages.Contains(e)));
            CleanupPages(_request == null);
        }


        // 表示コンテンツの更新実行
        private void SetViewContentsEx(ViewPageContext context)
        {
            //var varray = new List<PageValue>();
            //var parray = new List<PagePart>();

            var vinfo = new List<ViewInfo>();

            PageValue v = context.Index;
            //contentPartSize = 0;

            for (int id = 0; id < context.Size; ++id)
            {
                if (Pages[v.Index] == null) break; // 本当ならpageも渡したいよ？

                PagePart part = PagePart.All;
                if (IsSupportedWidePage && Pages[v.Index].IsWide) // TODO: IsSupportedWidePage
                {
                    part = v.Part == 0 ? PagePart.Right : PagePart.Left;
                }
                else
                {
                    v.Part = 0;
                }

                int partSize = part == PagePart.All ? 2 : 1;
                vinfo.Add(new ViewInfo() { Index = v, Size = partSize });

                //varray.Add(v);
                //parray.Add(part);

                //contentPartSize += partSize; // TODO: contentPartSize

                v = v + ((context.Direction > 0) ? partSize : -1);
                if (v < new PageValue(0, 0) || v > new PageValue(Pages.Count - 1, 1)) break;
            }

            for (int id = 0; id < 2; ++id)
            {
                if (id < vinfo.Count)
                {
                    Page page = Pages[vinfo[id].Index.Index];
                    ViewContentSources[id] = new ViewContentSource(page, vinfo[id].Index, vinfo[id].Size); //.Index, parray[id]);
                    //ViewContentSources[id].Part = parray[id];
                }
                else
                {
                    ViewContentSources[id] = null;
                }
            }


            // 並び順補正
            if (context.Direction < 0 && vinfo.Count >= 2)
            {
                ViewContentSources.Reverse();
                vinfo.Reverse();
            }


            // 単一ソース判定
            if (vinfo.Count == 2 && vinfo[0].Index.Index == vinfo[1].Index.Index)
            {
                //vinfo[0].Index.Part = 0; // = new PageValue(vinfo[0].Index, 0);
                //vinfo[0].Size = 2; // = PagePart.All;
                //vinfo.RemoveAt(1);

                var index = new PageValue(vinfo[0].Index.Index, 0);

                ViewContentSources[0] = new ViewContentSource(Pages[index.Index], index, 2);
                ViewContentSources[1] = null;
            }



            // 現在ページID更新
            //var newIndex = varray.Aggregate((value, e) => (value == null) ? e : (value == null) ? v : (e < value) ? e : value);
            context.Index = vinfo[0].Index;

            //context.ViewContents = ViewContentSources;

            // PropertyChanged

            _Context = context;
            _Index = context.Index;
            _Direction = context.Direction;

            //_IsDartyViewPages = true;

            _ViewInfoList = vinfo;
        }





        //List<Page> _ActivePages = new List<Page>();


        // ページコンテンツの準備
        // 先読み、不要ページコンテンツの削除を行う
        private void CleanupPages(bool preLoad)
        {
            // コンテンツを保持するページ収集
            var keepPages = new List<Page>();
            for (int offset = -_PageMode; offset <= _PageMode * 2 - 1; ++offset)
            {
                int index = Index.Index + offset;
                if (0 <= index && index < Pages.Count)
                {
                    keepPages.Add(Pages[index]);
                }
            }

            // 不要コンテンツ破棄
            foreach (var page in _KeepPages)
            {
                if (!_ViewPages.Contains(page) && !keepPages.Contains(page))
                {
                    page.Close();
                }
            }

            // 保持ページ更新
            _KeepPages = keepPages;


            if (preLoad)
            {
                // 先読み
                for (int offset = 1; offset <= _PageMode; ++offset)
                {
                    int index = (_Direction >= 0) ? Index.Index + (_PageMode - 1) + offset : Index.Index - offset;

                    if (0 <= index && index < Pages.Count)
                    {
                        // 念のため
                        Debug.Assert(_KeepPages.Contains(Pages[index]));

                        Pages[index].Open(QueueElementPriority.Default);
                    }
                }
            }

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
                if ((option & BookLoadOption.FirstPage) == BookLoadOption.FirstPage)
                {
                    _StartIndex = new PageValue(0, 0);
                    _Direction = 1;
                }
                else if ((option & BookLoadOption.LastPage) == BookLoadOption.LastPage)
                {
                    _StartIndex = new PageValue(Pages.Count - 1, 1);
                    _Direction = -1;
                }
                else
                {
                    int startIndex = (start != null) ? Pages.FindIndex(e => e.FullPath == start) : 0;
                    _StartIndex = startIndex >= 0 ? new PageValue(startIndex, 0) : new PageValue(0, 0); // TODO: Part
                    _Direction = 1;
                }
            });

            // 本有効化
            Place = archiver.FileName;
        }

        // 読み込み対象外サブフォルダ数。リカーシブ確認に使用します。
        public int SubFolderCount { get; private set; }

        // 開始ページ番号
        private PageValue _StartIndex;


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

            SetIndex(new PageValue(0, 0), 1);
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
        /*
        private bool IsStable()
        {
            return (_ViewPages[0] == CurrentPage && _ViewPages.All(e => e == null || e.Content != null));
        }
        */

        /*
    // 表示がワイドページ単独表示であるチェック
    private bool IsCurrentWidePage()
    {
        return (PageMode == 2 && CurrentViewPageCount == 1 && CurrentPage != null && CurrentPage.Width > CurrentPage.Height);
    }
    */



        // 前のページに戻る
        public void PrevPage(int step = 0)
        {
            //if (!IsStable()) return;

            // ページ移動量調整
            //step = (step == 0) ? CurrentViewPageCount : step;
            //step = 1;
            //SetIndex(Index - step, -1);

            MoveIndex((step == 0) ? -PageMode : -step);

#if false
            // ページ移動量調整
            step = (step == 0) ? CurrentViewPageCount : step;
            if (PageMode == 2)
            {
                if (_Direction > 0 && step > 0) step -= 1; // 読む方向が反転する場合、移動量-1
                if (step == 0 && IsCurrentWidePage()) step = 1; // ワイドページ補正
            }

            // 既に先頭ページ?
            if (Index - step < 0)
            {
                PageTerminated?.Invoke(this, -1);
            }
            else
            {
                SetIndex(Index - step, -1);
            }
#endif
        }


        // 次のページへ進む
        public void NextPage(int step = 0)
        {
            //if (!IsStable()) return;

            // ページ移動量調整
            //step = (step == 0) ? CurrentViewPageCount : step;
            // step = 1;
            //SetIndex(Index + step, +1);

            //MoveIndex(+1);
            MoveIndex((step == 0) ? PageMode : step);

#if false
            // ページ移動量調整
            step = (step == 0) ? CurrentViewPageCount : step;
            if (PageMode == 2)
            {
                if (_Direction < 0 && step > 0) step -= 1; // 読む方向が反転する場合、移動量-1
                if (step == 0 && IsCurrentWidePage()) step = 1; // ワイドページ補正
            }


            // 既に最終ページ?
            if (Index + step >= Pages.Count)
            {
                PageTerminated?.Invoke(this, +1);
            }
            else
            {
                SetIndex(Index + step, +1);
            }
#endif
        }


        // 最初のページに移動
        public void FirstPage()
        {
            if (Index > new PageValue(0, 0))
            {
                SetIndex(new PageValue(0, 0), 1);
            }
        }

        // 最後のページに移動
        public void LastPage()
        {
            if (Index < new PageValue(Pages.Count - 1, 1))
            {
                SetIndex(new PageValue(Pages.Count - 1, 1), -1);
            }
        }



        // ページの再読み込み
        public void Reflesh(PageValue index, int direction)
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
            //Page.ContentChanged -= Page_ContentChanged;
            BreakUpdateViewPageTask();

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
                IsSupportedWidePage = true;
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
