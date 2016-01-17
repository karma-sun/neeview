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

/*
    TODO:

    - [v] 新しいフラグ：「横長ページを分割」
    - [v] 左開きのときのページ順。分割での順番をVMで調整
    - [v] タイトル表示。11.5 .... > L とか？
    - [v] 不要パラメータ削除
    - [v] まだまだ削れる、ViewContentSource とかローカルにできそう
    - [v] 現在状態をまとめられないか
    - [v] タスクキャンセルの出力メッセージの抑制
    - [v] 見開きページを２ページ分とみなす、のあつかい
    - [v] 最初、最後のページ単独表示
    - [v] 名前どうにか、ViewPage:Index とか、 Contextとか、おかしいでしょ。 -- Contextはそのまま
    - [v] Sort, Reflesh, Dispose のコマンド化
    - [v] ワイド分割はページモード０にする？
    - [v] PageModeのenum化
    - [v] PagePart を数値にする。0(R),1(L) と サイズ(1-2)で領域を表す

    - やっぱページモードは1と2だけにして、ページ分割はモード1のときの拡張設定でいいんじゃね？
    - Bookコマンドの整備。名前とか範囲とか。定義場所とか。

    - 操作不能になるバグを再現させる
*/

namespace NeeView
{
    public struct PagePosition
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
        public PagePosition(int index, int part)
        {
            Value = index * 2 + part;
        }

        //
        public override string ToString()
        {
            return Index.ToString() + (Part == 1 ? ".5" : "");
        }

        // add
        public static PagePosition operator +(PagePosition a, PagePosition b)
        {
            return new PagePosition() { Value = a.Value + b.Value };
        }

        public static PagePosition operator +(PagePosition a, int b)
        {
            return new PagePosition() { Value = a.Value + b };
        }

        public static PagePosition operator -(PagePosition a, PagePosition b)
        {
            return new PagePosition() { Value = a.Value - b.Value };
        }

        public static PagePosition operator -(PagePosition a, int b)
        {
            return new PagePosition() { Value = a.Value - b };
        }

        // compare
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is PagePosition)) return false;
            return Value == ((PagePosition)obj).Value;
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public static bool operator ==(PagePosition a, PagePosition b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(PagePosition a, PagePosition b)
        {
            return a.Value != b.Value;
        }

        public static bool operator <(PagePosition a, PagePosition b)
        {
            return a.Value < b.Value;
        }

        public static bool operator >(PagePosition a, PagePosition b)
        {
            return a.Value > b.Value;
        }

        public static bool operator <=(PagePosition a, PagePosition b)
        {
            return a.Value <= b.Value;
        }

        public static bool operator >=(PagePosition a, PagePosition b)
        {
            return a.Value >= b.Value;
        }

        // clamp
        public PagePosition Clamp(PagePosition min, PagePosition max)
        {
            if (min.Value > max.Value) throw new ArgumentOutOfRangeException();

            int value = Value;
            if (value < min.Value) value = min.Value;
            if (value > max.Value) value = max.Value;

            return new PagePosition() { Value = value };
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
        public event EventHandler<IEnumerable<ViewContentSource>> ViewContentsChanged;

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
                    RequestReflesh();
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
                    RequestReflesh();
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
                    RequestReflesh();
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
                    RequestReflesh();
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
        private PageMode _PageMode = PageMode.SinglePage;
        public PageMode PageMode
        {
            get { return _PageMode; }
            set
            {
                if (_PageMode != value)
                {
                    _PageMode = value;
                    RequestReflesh();
                }
            }
        }

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
                    RequestSort();
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
                RequestSort();
            }
        }

        // ページ列を逆順にする
        private bool _IsReverseSort;
        public bool IsReverseSort
        {
            get { return _IsReverseSort; }
            set
            {
                if (_IsReverseSort != value)
                {
                    _IsReverseSort = value;
                    RequestSort();
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
        public List<Page> Pages { get; private set; } = new List<Page>();

        // 先読み有効
        // ページ切替時に自動で有効に戻される
        public bool IsEnablePreLoad { get; set; } = true;

        // 予約されているページ番号
        public int OrderIndex { get; set; }

        // ページコンテキスト
        ViewPageContext _Context = new ViewPageContext();

        // 在ページ番号取得
        /*
        public PagePosition GetPosition()
        {
            return _Context.Position;
        }
        */


        // リソースを保持しておくページ
        private List<Page> _KeepPages = new List<Page>();


        // 排他処理用ロックオブジェクト
        private object _Lock = new object();



        // ページ指定移動
        public void SetPosition(PagePosition position, int direction)
        {
            Debug.Assert(direction == 1 || direction == -1);

            if (Place == null) return;

            OrderIndex = position.Index;
            RequestViewPage(new SetPageCommand(this, position, direction, PageMode.Size()));
        }

        // ページ相対移動
        public void MovePosition(int step)
        {
            if (Place == null) return;
            RequestViewPage(new MovePageCommand(this, step));
        }

        // リフレッシュ
        public void RequestReflesh()
        {
            if (Place == null) return;
            RequestViewPage(new RefleshCommand(this));
        }

        // ソート
        public void RequestSort()
        {
            if (Place == null) return;
            RequestViewPage(new SortCommand(this));
        }

        // 終了
        public void RequestDispose()
        {
            if (Place == null) return;
            RequestViewPage(new DisposeCommand(this));
        }



        // 現在ページ
        public Page CurrentPage
        {
            get { return Pages.Count > 0 ? Pages[_Context.Position.Index] : null; }
        }




        //
        public class ViewInfo
        {
            public PagePosition Position;
            public int Size;
        }

        //
        public class ViewPageContext
        {
            public PagePosition Position { get; set; }
            public int Direction { get; set; } = 1;
            public int Size { get; set; }

            public List<ViewInfo> ViewInfoList { get; set; }
            public List<ViewContentSource> ViewContentsSource { get; set; }
        }

        //
        public abstract class ViewPageCommand
        {
            protected Book _Book;
            public abstract int Priority { get; }

            public ViewPageCommand(Book book)
            {
                _Book = book;
            }

            public virtual async Task Execute() { await Task.Yield(); }
        }

        //
        public class DisposeCommand : ViewPageCommand
        {
            public override int Priority => 3;

            public DisposeCommand(Book book) : base(book)
            {
            }

            public override async Task Execute()
            {
                _Book.Terminate();
                _Book._CancellationTokenSource.Cancel(); // task cancel
                await Task.Yield();
            }
        }

        //
        public class SortCommand : ViewPageCommand
        {
            public override int Priority => 2;

            public SortCommand(Book book) : base(book)
            {
            }

            public override async Task Execute()
            {
                _Book.Sort();
                await Task.Yield();
            }
        }

        //
        public class RefleshCommand : ViewPageCommand
        {
            public override int Priority => 1;

            public RefleshCommand(Book book) : base(book)
            {
            }

            public override async Task Execute()
            {
                _Book.Reflesh();
                await Task.Yield();
            }
        }


        //
        public interface ISetPageCommand
        {
            ViewPageContext GetViewPageContext();
        }


        //
        public class SetPageCommand : ViewPageCommand, ISetPageCommand
        {
            public override int Priority => 0;

            ViewPageContext Context { get; set; }

            public SetPageCommand(Book book, PagePosition position, int direction, int size) : base(book)
            {
                Context = new ViewPageContext()
                {
                    Position = position,
                    Direction = direction,
                    Size = size,
                };
            }

            public ViewPageContext GetViewPageContext()
            {
                return Context;
            }

            public override async Task Execute()
            {
                await _Book.UpdateViewPageAsync(GetViewPageContext());
            }
        }


        //
        public class MovePageCommand : ViewPageCommand, ISetPageCommand
        {
            public override int Priority => 0;

            public int Step { get; set; }


            public MovePageCommand(Book book, int step) : base(book)
            {
                Step = step;
            }

            public ViewPageContext GetViewPageContext()
            {
                int size = 0;
                if (Step > 0)
                {
                    for (int i = 0; i < Step && i < _Book._Context.ViewInfoList.Count; ++i)
                    {
                        size += _Book._Context.ViewInfoList[i].Size;
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
                    Position = _Book._Context.Position + size,
                    Direction = Step < 0 ? -1 : 1,
                    Size = _Book._Context.Size,
                };
            }

            public override async Task Execute()
            {
                await _Book.UpdateViewPageAsync(GetViewPageContext());
                _Book.OrderIndex = _Book._Context.Position.Index;
            }

        }




        CancellationTokenSource _CancellationTokenSource;

        ViewPageCommand _Command;

        public AutoResetEvent _WaitEvent { get; private set; } = new AutoResetEvent(false);

        private void RequestViewPage(ViewPageCommand command)
        {
            lock (_Lock)
            {
                if (_Command == null || _Command.Priority <= command.Priority)
                {
                    _Command = command;
                }
            }
            _WaitEvent.Set();
        }

        private void StartUpdateViewPageTask()
        {
            _CancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => UpdateViewPageTask(), _CancellationTokenSource.Token);
        }

        private void BreakUpdateViewPageTask()
        {
            _CancellationTokenSource.Cancel();
            _WaitEvent.Set();
        }


        private async Task UpdateViewPageTask()
        {
            try
            {
                Debug.WriteLine("Bookタスクの開始");
                while (!_CancellationTokenSource.Token.IsCancellationRequested)
                {
                    await Task.Run(() => _WaitEvent.WaitOne());
                    _CancellationTokenSource.Token.ThrowIfCancellationRequested();

                    ViewPageCommand command;
                    lock (_Lock)
                    {
                        command = _Command;
                        _Command = null;
                    }

                    if (command != null)
                    {
                        await command.Execute();
                        //await UpdateViewPageAsync(command);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Action<Exception> action = (exception) => { throw new ApplicationException("Bookタスク内部エラー", exception); };
                await App.Current.Dispatcher.BeginInvoke(action, e);
            }
            finally
            {
                Debug.WriteLine("Bookタスクの終了: " + Place);
            }
        }

        private int ClampPageNumber(int index)
        {
            if (index < 0) index = 0;
            if (index > Pages.Count - 1) index = Pages.Count - 1;
            return index;
        }

        private bool IsValidPosition(PagePosition position)
        {
            return (new PagePosition(0, 0) <= position && position < new PagePosition(Pages.Count, 0));
        }


        private async void UpdateViewPage(ViewPageContext context)
        {
            await UpdateViewPageAsync(context);
        }


        private async Task UpdateViewPageAsync(ViewPageContext context)
        {
            if (Pages.Count == 0)
            {
                App.Current.Dispatcher.Invoke(() => ViewContentsChanged?.Invoke(this, null));
                return;
            }

            //
            //if (command is ISetPageCommand)
            {
                //var context = ((ISetPageCommand)command).GetViewPageContext();

                if (!IsValidPosition(context.Position))
                {
                    if (context.Position < new PagePosition(0, 0))
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
                    var page = Pages[ClampPageNumber(context.Position.Index + context.Direction * i)];
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
                _Context = CreateFixedContext(context);

                // cancel?
                _CancellationTokenSource.Token.ThrowIfCancellationRequested();

                // notice update
                App.Current.Dispatcher.Invoke(() => ViewContentsChanged?.Invoke(this, _Context.ViewContentsSource));

                // PropertyChanged
                PageChanged?.Invoke(this, _Context.Position.Index);

                // cleanup pages
                _KeepPages.AddRange(viewPages.Where(e => !_KeepPages.Contains(e)));
                CleanupPages();

                // pre load
                if (_Command == null) PreLoad();
            }
        }

        private bool IsSoloPage(int index)
        {
            if (IsSupportedWidePage && Pages[index].IsWide) return true;
            if (IsSupportedSingleFirstPage && index == 0) return true;
            if (IsSupportedSingleLastPage && index == Pages.Count - 1) return true;
            return false;
        }

        // 表示コンテンツソースと、それに対応したコンテキスト作成
        // 入力と出力の関係が・・
        private ViewPageContext CreateFixedContext(ViewPageContext context)
        {
            var vinfo = new List<ViewInfo>();

            {
                PagePosition position = context.Position;

                for (int id = 0; id < context.Size; ++id)
                {
                    if (position < new PagePosition(0, 0) || position > new PagePosition(Pages.Count - 1, 1)) break;
                    if (Pages[position.Index] == null) break; // 本当ならpageも渡したいよ？

                    int size = 2;
                    if (PageMode == 0 && Pages[position.Index].IsWide && !Page.IsEnableAnimatedGif)
                    {
                        size = 1;
                    }
                    else
                    {
                        position.Part = 0;
                    }

                    vinfo.Add(new ViewInfo() { Position = position, Size = size });

                    position = position + ((context.Direction > 0) ? size : -1);
                }
            }

            // 見開き補正
            if (PageMode == PageMode.WidePage && vinfo.Count >= 2)
            {
                if (IsSoloPage(vinfo[0].Position.Index) || IsSoloPage(vinfo[1].Position.Index))
                {
                    vinfo.RemoveAt(1);
                }
            }


            // create contents source
            var contentsSource = new List<ViewContentSource>();
            foreach (var v in vinfo)
            {
                contentsSource.Add(new ViewContentSource(Pages[v.Position.Index], v.Position, v.Size, BookReadOrder));
            }

            // 並び順補正
            if (context.Direction < 0 && vinfo.Count >= 2)
            {
                contentsSource.Reverse();
                vinfo.Reverse();
            }

            // 左開き
            if (BookReadOrder == PageReadOrder.LeftToRight)
            {
                contentsSource.Reverse();
            }

            // 単一ソースならコンテンツは１つにまとめる
            if (vinfo.Count == 2 && vinfo[0].Position.Index == vinfo[1].Position.Index)
            {
                var position = new PagePosition(vinfo[0].Position.Index, 0);
                var source = new ViewContentSource(Pages[position.Index], position, 2, BookReadOrder);

                contentsSource.Clear();
                contentsSource.Add(source);
            }

            // 新しいコンテキスト
            context.Position = vinfo[0].Position;
            context.ViewInfoList = vinfo;
            context.ViewContentsSource = contentsSource;

            return context;
        }


        // ページコンテンツの準備
        // 先読み、不要ページコンテンツの削除を行う
        private void CleanupPages()
        {
            // コンテンツを保持するページ収集
            var keepPages = new List<Page>();
            for (int offset = -_PageMode.Size(); offset <= _PageMode.Size() * 2 - 1; ++offset)
            {
                int index = _Context.Position.Index + offset;
                if (0 <= index && index < Pages.Count)
                {
                    keepPages.Add(Pages[index]);
                }
            }

            // 不要コンテンツ破棄
            foreach (var page in _KeepPages)
            {
                if (!keepPages.Contains(page))
                {
                    page.Close();
                }
            }

            // 保持ページ更新
            _KeepPages = keepPages;
        }






        // 先読み
        private void PreLoad()
        {
            for (int offset = 1; offset <= _PageMode.Size(); ++offset)
            {
                int index = (_Context.Direction >= 0) ? _Context.Position.Index + (_PageMode.Size() - 1) + offset : _Context.Position.Index - offset;

                if (0 <= index && index < Pages.Count)
                {
                    // 念のため
                    Debug.Assert(_KeepPages.Contains(Pages[index]));

                    Pages[index].Open(QueueElementPriority.Default);
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


            PagePosition position = new PagePosition(0, 0);
            int direction = 1;

            // 読み込み(非同期タスク)
            await Task.Run(() =>
            {
                ReadArchive(archiver, "", option);

                // 初期ソート
                Sort();

                // スタートページ取得
                if ((option & BookLoadOption.FirstPage) == BookLoadOption.FirstPage)
                {
                    position = new PagePosition(0, 0);
                    direction = 1;
                }
                else if ((option & BookLoadOption.LastPage) == BookLoadOption.LastPage)
                {
                    position = new PagePosition(Pages.Count - 1, 1);
                    direction = -1;
                }
                else
                {
                    int index = (start != null) ? Pages.FindIndex(e => e.FullPath == start) : 0;
                    position = index >= 0 ? new PagePosition(index, 0) : new PagePosition(0, 0);
                    direction = 1;
                }
            });

            // 有効化
            Place = archiver.FileName;

            // 初期ページ設定
            SetPosition(position, direction);
        }

        // 読み込み対象外サブフォルダ数。リカーシブ確認に使用します。
        public int SubFolderCount { get; private set; }


        // 開始
        // ページ設定を行うとコンテンツ読み込みが始まるため、ロードと分離した
        public void Start()
        {
            Debug.Assert(Place != null);
            StartUpdateViewPageTask();
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
        // あー、これ、コマンドにしないと。
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
                    var random = new Random();
                    Pages = Pages.OrderBy(e => random.Next()).ToList();
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (IsReverseSort)
            {
                Pages.Reverse();
            }

            SetPosition(new PagePosition(0, 0), 1);
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


        // 前のページに戻る
        public void PrevPage(int step = 0)
        {
            var s = (step == 0) ? PageMode.Size() : step;
            MovePosition(-s);
        }


        // 次のページへ進む
        public void NextPage(int step = 0)
        {
            var s = (step == 0) ? PageMode.Size() : step;
            MovePosition(+s);
        }


        // 最初のページに移動
        public void FirstPage()
        {
            SetPosition(new PagePosition(0, 0), 1);
        }

        // 最後のページに移動
        public void LastPage()
        {
            SetPosition(new PagePosition(Pages.Count - 1, 1), -1);
        }


        // ページの再読み込み
        // あー、これ、コマンドにしないと。
        public void Reflesh(PagePosition position, int direction)
        {
            if (Place == null) return;

            lock (_Lock)
            {
                _KeepPages.ForEach(e => e?.Close());
            }

            SetPosition(position, direction);
        }


        // ページの再読み込み
        public void Reflesh()
        {
            Reflesh(_Context.Position, 1);
        }


        // 廃棄処理
        // んー、これもコマンドにしてしまうか？
        // そうすれば排他の問題はなくなる
        public void Dispose()
        {
            RequestDispose();
        }

        private void Terminate()
        { 
            //BreakUpdateViewPageTask();

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

            [DataMember(Name = "PageModeV2")]
            public PageMode PageMode { get; set; }

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
                PageMode = PageMode.SinglePage;
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
