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


        // 横長ページを分割する
        private bool _IsSupportedDividePage;
        public bool IsSupportedDividePage
        {
            get { return _IsSupportedDividePage; }
            set
            {
                if (_IsSupportedDividePage != value)
                {
                    _IsSupportedDividePage = value;
                    RequestReflesh();
                }
            }
        }

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

        // 表示されるページ番号(スライダー用)
        public int DisplayIndex { get; set; }

        // 表示ページコンテキスト
        ViewPageContext _ViewContext = new ViewPageContext();

        // 先頭ページの場所
        PagePosition _FirstPosition => new PagePosition(0, 0);

        // 最終ページの場所
        PagePosition _LastPosition => Pages.Count > 0 ? new PagePosition(Pages.Count - 1, 1) : _FirstPosition;

        // リソースを保持しておくページ
        private List<Page> _KeepPages = new List<Page>();


        // 排他処理用ロックオブジェクト
        private object _Lock = new object();


        // 本の読み込み
        #region LoadBook

        // 読み込み対象外サブフォルダ数。リカーシブ確認に使用します。
        public int SubFolderCount { get; private set; }

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


            PagePosition position = _FirstPosition;
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
                    position = _FirstPosition;
                    direction = 1;
                }
                else if ((option & BookLoadOption.LastPage) == BookLoadOption.LastPage)
                {
                    position = _LastPosition;
                    direction = -1;
                }
                else
                {
                    int index = (start != null) ? Pages.FindIndex(e => e.FullPath == start) : 0;
                    position = index >= 0 ? new PagePosition(index, 0) : _FirstPosition;
                    direction = 1;
                }
            });

            // 有効化
            Place = archiver.FileName;

            // 初期ページ設定
            RequestSetPosition(position, direction);
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

        // 開始
        // ページ設定を行うとコンテンツ読み込みが始まるため、ロードと分離した
        public void Start()
        {
            Debug.Assert(Place != null);
            StartCommandWorker();
        }

        #endregion


        // 廃棄処理
        public void Dispose()
        {
            RequestDispose();
        }

        // 前のページに戻る
        public void PrevPage(int step = 0)
        {
            var s = (step == 0) ? PageMode.Size() : step;
            RequestMovePosition(-s);
        }

        // 次のページへ進む
        public void NextPage(int step = 0)
        {
            var s = (step == 0) ? PageMode.Size() : step;
            RequestMovePosition(+s);
        }

        // 最初のページに移動
        public void FirstPage()
        {
            RequestSetPosition(_FirstPosition, 1);
        }

        // 最後のページに移動
        public void LastPage()
        {
            RequestSetPosition(_LastPosition, -1);
        }




        // ページ指定移動
        public void RequestSetPosition(PagePosition position, int direction)
        {
            Debug.Assert(direction == 1 || direction == -1);

            if (Place == null) return;

            DisplayIndex = position.Index;
            RegistCommand(new SetPageCommand(this, position, direction, PageMode.Size()));
        }

        // ページ相対移動
        public void RequestMovePosition(int step)
        {
            if (Place == null) return;
            RegistCommand(new MovePageCommand(this, step));
        }

        // リフレッシュ
        public void RequestReflesh()
        {
            if (Place == null) return;
            RegistCommand(new RefleshCommand(this));
        }

        // ソート
        public void RequestSort()
        {
            if (Place == null) return;
            RegistCommand(new SortCommand(this));
        }

        // 終了処理
        public void RequestDispose()
        {
            if (Place == null) return;
            RegistCommand(new DisposeCommand(this));
        }




        // 表示ページ情報
        public class PageInfo
        {
            public PagePosition Position;
            public int Size;
        }

        // 表示ページコンテキスト
        public class ViewPageContext
        {
            // 基準となるページの場所
            public PagePosition Position { get; set; }

            // 進行方向
            public int Direction { get; set; } = 1;

            // 表示ページ数
            public int Size { get; set; }

            // 表示ページ情報
            public List<PageInfo> Infos { get; set; }

            // 表示ページコンテンツソース
            public List<ViewContentSource> ViewContentsSource { get; set; }
        }

        // 表示ページコンテキストのソース
        public class ViewPageContextSource
        {
            public PagePosition Position { get; set; }
            public int Direction { get; set; } = 1;
            public int Size { get; set; }
        }


        // コマンド基底
        private abstract class ViewPageCommand
        {
            protected Book _Book;
            public abstract int Priority { get; }

            public ViewPageCommand(Book book)
            {
                _Book = book;
            }

            public virtual async Task Execute() { await Task.Yield(); }
        }

        // 廃棄処理コマンド
        private class DisposeCommand : ViewPageCommand
        {
            public override int Priority => 3;

            public DisposeCommand(Book book) : base(book)
            {
            }

            public override async Task Execute()
            {
                _Book.Terminate();
                _Book.BreakCommandWorker();
                await Task.Yield();
            }
        }

        // ソートコマンド
        private class SortCommand : ViewPageCommand
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

        // リフレッシュコマンド
        private class RefleshCommand : ViewPageCommand
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

        // ページ指定移動コマンド
        private class SetPageCommand : ViewPageCommand
        {
            public override int Priority => 0;

            ViewPageContextSource _Source { get; set; }

            public SetPageCommand(Book book, PagePosition position, int direction, int size) : base(book)
            {
                _Source = new ViewPageContextSource()
                {
                    Position = position,
                    Direction = direction,
                    Size = size,
                };
            }

            public override async Task Execute()
            {
                await _Book.UpdateViewPageAsync(_Source);
            }
        }

        // ページ相対移動コマンド
        private class MovePageCommand : ViewPageCommand
        {
            public override int Priority => 0;

            public int Step { get; set; }


            public MovePageCommand(Book book, int step) : base(book)
            {
                Step = step;
            }

            private ViewPageContextSource GetViewPageContextSource()
            {
                int step = 0;

                if (_Book.Pages.Count == 0)
                {
                    step = Step < 0 ? -1 : 1;
                }
                else if (Step > 0)
                {
                    for (int i = 0; i < Step && i < _Book._ViewContext.Infos.Count; ++i)
                    {
                        step += _Book._ViewContext.Infos[i].Size;
                    }
                }
                else if (_Book._ViewContext.Size == 2)
                {
                    step = Step + 1;
                }
                else
                {
                    step = Step;
                }

                return new ViewPageContextSource()
                {
                    Position = _Book._ViewContext.Position + step,
                    Direction = Step < 0 ? -1 : 1,
                    Size = _Book.PageMode.Size(),
                };
            }

            public override async Task Execute()
            {
                await _Book.UpdateViewPageAsync(GetViewPageContextSource());
                _Book.DisplayIndex = _Book._ViewContext.Position.Index;
            }
        }


        // ワーカータスクのキャンセルトークン
        private CancellationTokenSource _CommandWorkerCancellationTokenSource;

        // 予約されているコマンド
        private ViewPageCommand _ReadyCommand;

        // 予約コマンド存在イベント
        public AutoResetEvent _ReadyCommandEvent { get; private set; } = new AutoResetEvent(false);


        // コマンドの予約
        private void RegistCommand(ViewPageCommand command)
        {
            lock (_Lock)
            {
                if (_ReadyCommand == null || _ReadyCommand.Priority <= command.Priority)
                {
                    _ReadyCommand = command;
                }
            }
            _ReadyCommandEvent.Set();
        }

        // ワーカータスクの起動
        private void StartCommandWorker()
        {
            _CommandWorkerCancellationTokenSource = new CancellationTokenSource();
            Task.Run( () => CommandWorker(), _CommandWorkerCancellationTokenSource.Token);
        }

        // ワーカータスクの終了
        private void BreakCommandWorker()
        {
            _CommandWorkerCancellationTokenSource.Cancel();
            _ReadyCommandEvent.Set();
        }

        // ワーカータスク
        private async void CommandWorker()
        {
            try
            {
                ////Debug.WriteLine("Bookタスクの開始");
                while (!_CommandWorkerCancellationTokenSource.Token.IsCancellationRequested)
                {
                    await Task.Run(() => _ReadyCommandEvent.WaitOne());
                    _CommandWorkerCancellationTokenSource.Token.ThrowIfCancellationRequested();

                    ViewPageCommand command;
                    lock (_Lock)
                    {
                        command = _ReadyCommand;
                        _ReadyCommand = null;
                    }

                    if (command != null)
                    {
                        await command.Execute();
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
                ////Debug.WriteLine("Bookタスクの終了: " + Place);
            }
        }


        // ページ番号のクランプ
        private int ClampPageNumber(int index)
        {
            if (index > Pages.Count - 1) index = Pages.Count - 1;
            if (index < 0) index = 0;
            return index;
        }

        // ページ場所の有効判定
        private bool IsValidPosition(PagePosition position)
        {
            return (_FirstPosition <= position && position <= _LastPosition);
        }

        // 表示ページ更新
        private async Task UpdateViewPageAsync(ViewPageContextSource source)
        {
            // ページ終端を越えたか判定
            if (source.Position < _FirstPosition)
            {
                App.Current.Dispatcher.Invoke(() => PageTerminated?.Invoke(this, -1));
                return;
            }
            else if (source.Position > _LastPosition)
            {
                App.Current.Dispatcher.Invoke(() => PageTerminated?.Invoke(this, +1));
                return;
            }

            // ページ数０の場合は表示コンテンツなし
            if (Pages.Count == 0)
            {
                App.Current.Dispatcher.Invoke(() => ViewContentsChanged?.Invoke(this, null));
                return;
            }

            // view pages
            var viewPages = new List<Page>();
            for (int i = 0; i < source.Size; ++i)
            {
                var page = Pages[ClampPageNumber(source.Position.Index + source.Direction * i)];
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
            _ViewContext = CreateViewPageContext(source);

            // task cancel?
            _CommandWorkerCancellationTokenSource.Token.ThrowIfCancellationRequested();

            // notice ViewContentsChanged
            App.Current.Dispatcher.Invoke(() => ViewContentsChanged?.Invoke(this, _ViewContext.ViewContentsSource));

            // notice PropertyChanged
            PageChanged?.Invoke(this, _ViewContext.Position.Index);

            // cleanup pages
            _KeepPages.AddRange(viewPages.Where(e => !_KeepPages.Contains(e)));
            CleanupPages();

            // pre load
            if (_ReadyCommand == null) PreLoad();
        }

        // 見開きモードでも単独表示するべきか判定
        private bool IsSoloPage(int index)
        {
            if (IsSupportedWidePage && Pages[index].IsWide) return true;
            if (IsSupportedSingleFirstPage && index == 0) return true;
            if (IsSupportedSingleLastPage && index == Pages.Count - 1) return true;
            return false;
        }

        // 分割モード有効判定
        private bool IsEnableDividePage()
        {
            return IsSupportedDividePage && PageMode == PageMode.SinglePage && !Page.IsEnableAnimatedGif;
        }

        // 表示コンテンツソースと、それに対応したコンテキスト作成
        private ViewPageContext CreateViewPageContext(ViewPageContextSource source)
        {
            var infos = new List<PageInfo>();

            {
                PagePosition position = source.Position;

                for (int id = 0; id < source.Size; ++id)
                {
                    if (!IsValidPosition(position) || Pages[position.Index] == null) break;

                    int size = 2;
                    if (IsEnableDividePage() && Pages[position.Index].IsWide)
                    {
                        size = 1;
                    }
                    else
                    {
                        position.Part = 0;
                    }

                    infos.Add(new PageInfo() { Position = position, Size = size });

                    position = position + ((source.Direction > 0) ? size : -1);
                }
            }

            // 見開き補正
            if (PageMode == PageMode.WidePage && infos.Count >= 2)
            {
                if (IsSoloPage(infos[0].Position.Index) || IsSoloPage(infos[1].Position.Index))
                {
                    infos = infos.GetRange(0, 1);
                }
            }

            // コンテンツソース作成
            var contentsSource = new List<ViewContentSource>();
            foreach (var v in infos)
            {
                contentsSource.Add(new ViewContentSource(Pages[v.Position.Index], v.Position, v.Size, BookReadOrder));
            }

            // 並び順補正
            if (source.Direction < 0 && infos.Count >= 2)
            {
                contentsSource.Reverse();
                infos.Reverse();
            }

            // 左開き
            if (BookReadOrder == PageReadOrder.LeftToRight)
            {
                contentsSource.Reverse();
            }

            // 単一ソースならコンテンツは１つにまとめる
            if (infos.Count == 2 && infos[0].Position.Index == infos[1].Position.Index)
            {
                var position = new PagePosition(infos[0].Position.Index, 0);
                contentsSource.Clear();
                contentsSource.Add(new ViewContentSource(Pages[position.Index], position, 2, BookReadOrder));
            }

            // 新しいコンテキスト
            var context = new ViewPageContext();
            context.Position = infos[0].Position;
            context.Size = infos.Count;
            context.Direction = source.Direction;
            context.Infos = infos;
            context.ViewContentsSource = contentsSource;

            return context;
        }


        // 不要ページコンテンツの削除を行う
        private void CleanupPages()
        {
            // コンテンツを保持するページ収集
            var keepPages = new List<Page>();
            for (int offset = -_PageMode.Size(); offset <= _PageMode.Size() * 2 - 1; ++offset)
            {
                int index = _ViewContext.Position.Index + offset;
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
                int index = (_ViewContext.Direction >= 0) ? _ViewContext.Position.Index + (_PageMode.Size() - 1) + offset : _ViewContext.Position.Index - offset;

                if (0 <= index && index < Pages.Count)
                {
                    // 念のため
                    Debug.Assert(_KeepPages.Contains(Pages[index]));

                    Pages[index].Open(QueueElementPriority.Default);
                }
            }
        }

        // ページの並び替え
        private void Sort()
        {
            if (Pages.Count <= 0) return;

            switch (SortMode)
            {
                case PageSortMode.FileName:
                    Pages.Sort((a, b) => ComparePath(a.FullPath, b.FullPath, Win32Api.StrCmpLogicalW));
                    break;
                case PageSortMode.FileNameDescending:
                    Pages.Sort((a, b) => ComparePath(a.FullPath, b.FullPath, Win32Api.StrCmpLogicalW));
                    Pages.Reverse();
                    break;
                case PageSortMode.TimeStamp:
                    Pages = Pages.OrderBy(e => e.UpdateTime).ToList();
                    break;
                case PageSortMode.TimeStampDescending:
                    Pages = Pages.OrderBy(e => e.UpdateTime).ToList();
                    Pages.Reverse();
                    break;
                case PageSortMode.Random:
                    var random = new Random();
                    Pages = Pages.OrderBy(e => random.Next()).ToList();
                    break;
                default:
                    throw new NotImplementedException();
            }

            RequestSetPosition(_FirstPosition, 1);
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


        // 表示の再構築
        private void Reflesh()
        {
            if (Place == null) return;

            lock (_Lock)
            {
                _KeepPages.ForEach(e => e?.Close());
            }

            RequestSetPosition(_ViewContext.Position, 1);
        }


        // 廃棄処理
        private void Terminate()
        {
            Pages.ForEach(e => e?.Close());
            Pages.Clear();

            _Archivers.ForEach(e => e.Dispose());
            _Archivers.Clear();

            _TrashBox.Clear();
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
            public bool IsSupportedDividePage { get; set; }

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
            memento.BookMark = Pages.Count > 0 ? Pages[_ViewContext.Position.Index].FullPath : null;

            memento.PageMode = PageMode;
            memento.BookReadOrder = BookReadOrder;
            memento.IsSupportedDividePage = IsSupportedDividePage;
            memento.IsSupportedSingleFirstPage = IsSupportedSingleFirstPage;
            memento.IsSupportedSingleLastPage = IsSupportedSingleLastPage;
            memento.IsSupportedWidePage = IsSupportedWidePage;
            memento.IsRecursiveFolder = IsRecursiveFolder;
            memento.SortMode = SortMode;

            return memento;
        }

        // bookに設定を反映させる
        public void Restore(Memento memento)
        {
            PageMode = memento.PageMode;
            BookReadOrder = memento.BookReadOrder;
            IsSupportedDividePage = memento.IsSupportedDividePage;
            IsSupportedSingleFirstPage = memento.IsSupportedSingleFirstPage;
            IsSupportedSingleLastPage = memento.IsSupportedSingleLastPage;
            IsSupportedWidePage = memento.IsSupportedWidePage;
            IsRecursiveFolder = memento.IsRecursiveFolder;
            SortMode = memento.SortMode;
        }
    }

    #endregion
}
