// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        KeepHistoryOrder = (1 << 5), // 履歴の順番を変更しない
        SelectFoderListMaybe = (1 << 6), // 可能ならばフォルダリストで選択する
        SelectHistoryMaybe = (1 << 7), // 可能ならば履歴リストで選択する
        SkipSamePlace = (1 << 8), // 同じフォルダならば読み込まない
        AutoRecursive = (1 << 9), // 自動再帰
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
        public event EventHandler<ViewSource> ViewContentsChanged;

        // ページ終端を超えて移動しようとした
        // 次の本への移動を要求
        public event EventHandler<int> PageTerminated;

        // 再読み込みを要求
        public event EventHandler DartyBook;

        // ソートされた
        public event EventHandler PagesSorted;

        // サムネイル更新
        public event EventHandler<Page> ThumbnailChanged;

        // 先読み許可フラグ
        public bool AllowPreLoad { get; set; } = true;

        // ファイル削除された
        public event EventHandler<Page> PageRemoved;


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
                    RequestReflesh(false);
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
                    RequestReflesh(false);
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
                    RequestReflesh(false);
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
                    RequestReflesh(false);
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
                    RequestReflesh(false);
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
                    RequestReflesh(false);
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

        // 表示されるページ番号(スライダー用)
        public int DisplayIndex { get; set; }

        // 表示ページコンテキスト
        ViewPageContext _ViewContext = new ViewPageContext();

        // 表示ページ番号
        public int GetViewPageindex() => _ViewContext.Position.Index;

        // 表示ページ
        public Page GetViewPage() => GetPage(_ViewContext.Position.Index);

        // 表示ページ群
        public List<Page> GetViewPages()
        {
            var pages = new List<Page>();
            for (int i = 0; i < _ViewContext.Size; ++i)
            {
                pages.Add(GetPage(_ViewContext.Position.Index + i));
            }
            return pages;
        }

        // ページ
        public Page GetPage(int index) => Pages.Count > 0 ? Pages[ClampPageNumber(index)] : null;

        // ページ番号
        public int GetIndex(Page page) => Pages.IndexOf(page);

        // 先頭ページの場所
        PagePosition _FirstPosition => new PagePosition(0, 0);

        // 最終ページの場所
        PagePosition _LastPosition => Pages.Count > 0 ? new PagePosition(Pages.Count - 1, 1) : _FirstPosition;

        // リソースを保持しておくページ
        private List<Page> _KeepPages = new List<Page>();

        // マーカー
        public List<Page> Markers = new List<Page>();

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

            // リカーシブフラグ
            if (IsRecursiveFolder)
            {
                option |= BookLoadOption.Recursive;
            }

            // アーカイバの選択
            Archiver archiver = ModelContext.ArchiverManager.CreateArchiver(path, null);
            if (archiver.IsFileSystem)
            {
                // 入力ファイルを最初のページにする
                if (path != archiver.GetPlace())
                {
                    start = Path.GetFileName(path);
                }
            }
            else
            {
                // 圧縮ファイルは再帰させる
                option |= BookLoadOption.Recursive;
            }

            PagePosition position = _FirstPosition;
            int direction = 1;

            // 読み込み(非同期タスク)
            await Task.Run(() =>
            {
                ReadArchive(archiver, "", option);

                // Pages Prefix
                var prefix = GetPagesPrefix();
                foreach (var page in Pages)
                {
                    page.Prefix = prefix;
                }

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
            RequestSetPosition(position, direction, true);
        }


        // アーカイブからページ作成(再帰)
        private bool ReadArchive(Archiver archiver, string place, BookLoadOption option)
        {
            List<ArchiveEntry> entries = null;

            try
            {
                if (!archiver.IsSupported())
                {
                    archiver.Dispose();
                    return false;
                }
                entries = archiver.GetEntries();
            }
            catch (Exception e)
            {
                archiver.Dispose();
                throw new ApplicationException(e.Message);
            }

            //
            _Archivers.Add(archiver);

            //
            foreach (var entry in entries)
            {
                // 再帰設定、もしくは単一ファイルの場合、再帰を行う
                bool isRecursive = (option & BookLoadOption.Recursive) == BookLoadOption.Recursive;
                bool isAutoRecursive = (option & BookLoadOption.AutoRecursive) == BookLoadOption.AutoRecursive && entries.Count == 1 && archiver.IsFileSystem;
                if ((isRecursive || isAutoRecursive) && ModelContext.ArchiverManager.IsSupported(entry.EntryName))
                {
                    bool result = false;
                    if (archiver.IsFileSystem)
                    {
                        result = ReadArchive(ModelContext.ArchiverManager.CreateArchiver(archiver.GetFileSystemPath(entry), archiver), LoosePath.Combine(place, entry.EntryName), option);
                    }
                    else
                    {
                        // テンポラリにアーカイブを解凍する
                        string tempFileName = Temporary.CreateTempFileName(Path.GetFileName(entry.EntryName));
                        try
                        {
                            archiver.ExtractToFile(entry, tempFileName, false);
                            _TrashBox.Add(new TrashFile(tempFileName));

                            result = ReadArchive(ModelContext.ArchiverManager.CreateArchiver(tempFileName, archiver), LoosePath.Combine(place, entry.EntryName), option);
                            if (!result)
                            {
                                AddPage(archiver, entry, place, option);
                            }
                        }
                        catch (Exception e)
                        {
                            // 展開に失敗した場合、エラーページとして登録する
                            Pages.Add(new FilePage(archiver, entry, place, FilePageIcon.Alart) { Text = "ファイルの抽出に失敗しました\n" + e.Message });
                        }
                    }
                }
                else
                {
                    // ファイルとして展開
                    AddPage(archiver, entry, place, option);
                }
            }
            return true;
        }


        // ページを追加する
        private void AddPage(Archiver archiver, ArchiveEntry entry, string place, BookLoadOption option)
        {
            Page page = null;

            if (ModelContext.BitmapLoaderManager.IsSupported(entry.EntryName))
            {
                page = new BitmapPage(archiver, entry, place);
            }
            else
            {
                var type = ModelContext.ArchiverManager.GetSupportedType(entry.EntryName);
                bool isSupportAllFile = (option & BookLoadOption.SupportAllFile) == BookLoadOption.SupportAllFile;
                if (isSupportAllFile)
                {
                    switch (type)
                    {
                        case ArchiverType.None:
                            page = new FilePage(archiver, entry, place, FilePageIcon.File);
                            break;
                        case ArchiverType.FolderFiles:
                            page = new FilePage(archiver, entry, place, FilePageIcon.Folder);
                            break;
                        default:
                            page = new FilePage(archiver, entry, place, FilePageIcon.Archive);
                            break;
                    }
                }
                else if (type != ArchiverType.None)
                {
                    SubFolderCount++;
                }
            }

            if (page != null)
            {
                page.ThumbnailChanged += Page_ThumbnailChanged;
                Pages.Add(page);
            }
        }

        //
        private void Page_ThumbnailChanged(object sender, System.Windows.Media.Imaging.BitmapSource e)
        {
            var page = sender as Page;
            if (page != null && e != null)
            {
                ThumbnailChanged?.Invoke(this, page);
            }
        }


        // 名前の最長一致文字列取得
        private string GetPagesPrefix()
        {
            if (Pages == null || Pages.Count == 0) return "";

            string s = Pages[0].FullPath;
            foreach (var page in Pages)
            {
                s = GetStartsWith(s, page.FullPath);
                if (string.IsNullOrEmpty(s)) break;
            }

            // 最初の区切り記号
            for (int i = s.Length - 1; i >= 0; --i)
            {
                if (s[i] == '\\' || s[i] == '/')
                {
                    return s.Substring(0, i + 1);
                }
            }

            // ヘッダとして認識できなかった
            return "";
        }

        //
        private string GetStartsWith(string s0, string s1)
        {
            if (s0 == null || s1 == null) return "";

            if (s0.Length > s1.Length)
            {
                var temp = s0;
                s0 = s1;
                s1 = temp;
            }

            for (int i = 0; i < s0.Length; ++i)
            {
                char a0 = s0[i];
                char a1 = s1[i];
                if (s0[i] != s1[i])
                {
                    return i > 0 ? s0.Substring(0, i) : "";
                }
            }

            return s0;
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
            RequestSetPosition(_FirstPosition, 1, true);
        }

        // 最後のページに移動
        public void LastPage()
        {
            RequestSetPosition(_LastPosition, -1, true);
        }

        // 指定ページに移動
        public void JumpPage(Page page)
        {
            int index = Pages.IndexOf(page);
            if (index >= 0)
            {
                var position = new PagePosition(index, 0);
                RequestSetPosition(position, 1, false);
            }
        }



        // ページ指定移動
        public void RequestSetPosition(PagePosition position, int direction, bool isPreLoad)
        {
            Debug.Assert(direction == 1 || direction == -1);

            if (Place == null) return;

            DisplayIndex = position.Index;
            RegistCommand(new SetPageCommand(this, position, direction, PageMode.Size(), isPreLoad));
        }

        // ページ相対移動
        public void RequestMovePosition(int step)
        {
            if (Place == null) return;
            RegistCommand(new MovePageCommand(this, step));
        }

        // リフレッシュ
        public void RequestReflesh(bool isClear)
        {
            if (Place == null) return;
            RegistCommand(new RefleshCommand(this, isClear));
        }

        // ソート
        public void RequestSort()
        {
            if (Place == null) return;
            RegistCommand(new SortCommand(this));
        }

        // ページ削除
        public void RequestRemove(Page page)
        {
            if (Place == null) return;
            RegistCommand(new RemoveCommand(this, page));
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
            public override int Priority => 4;

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

        // 削除コマンド
        private class RemoveCommand : ViewPageCommand
        {
            public override int Priority => 3;

            private Page _Page;

            public RemoveCommand(Book book, Page page) : base(book)
            {
                _Page = page;
            }

            public override async Task Execute()
            {
                _Book.Remove(_Page);
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
                _Book.RequestSetPosition(_Book._FirstPosition, 1, true);
                await Task.Yield();
            }
        }

        // リフレッシュコマンド
        private class RefleshCommand : ViewPageCommand
        {
            public override int Priority => 1;

            private bool _IsClear;

            public RefleshCommand(Book book, bool isClear) : base(book)
            {
                _IsClear = isClear;
            }

            public override async Task Execute()
            {
                _Book.Reflesh(_IsClear);
                await Task.Yield();
            }
        }


        // ページ指定移動コマンド
        private class SetPageCommand : ViewPageCommand
        {
            public override int Priority => 0;

            bool _IsPreLoad;

            ViewPageContextSource _Source { get; set; }

            public SetPageCommand(Book book, PagePosition position, int direction, int size, bool isPreLoad) : base(book)
            {
                _IsPreLoad = isPreLoad;

                _Source = new ViewPageContextSource()
                {
                    Position = position,
                    Direction = direction,
                    Size = size,
                };
            }

            public override async Task Execute()
            {
                await _Book.UpdateViewPageAsync(_Source, _IsPreLoad);
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
                await _Book.UpdateViewPageAsync(GetViewPageContextSource(), true);
            }
        }


        // マーカー用ロック
        private object _MarkerLock = new object();

        // マーカー更新イベント
        public event EventHandler MarkerChanged;

        // マーカーコマンド
        private class MarkerCommand : ViewPageCommand
        {
            public override int Priority => 0;

            private bool _IsMark;

            public MarkerCommand(Book book, bool isMark) : base(book)
            {
                _IsMark = isMark;
            }

            public override async Task Execute()
            {
                _Book.ToggleMarker();
                await Task.Yield();
            }
        }

        //
        private void ToggleMarker()
        {
            lock (_MarkerLock)
            {
                var page = GetViewPage();
                if (!Markers.Contains(page))
                {
                    Markers.Add(page);
                    MarkerChanged?.Invoke(this, null);
                }
                else
                {
                    Markers.Remove(page);
                    MarkerChanged?.Invoke(this, null);
                }
            }
        }

        //
        public bool IsMarked(Page page)
        {
            lock (_MarkerLock)
            {
                return Markers.Contains(page);
            }
        }

        // マーカー設定
        public void RequestMarker()
        {
            if (Place == null) return;
            RegistCommand(new MarkerCommand(this, true));
        }

        private class Pair<T1, T2>
        {
            public T1 Key;
            public T2 Value;

            public Pair() { }
            public Pair(T1 key, T2 value)
            {
                Key = key;
                Value = value;
            }
        }

        // マーカー移動動
        public bool RequestModeMarkerPosition(int direction, bool isLoop)
        {
            if (Place == null) return false;
            Debug.Assert(direction == 1 || direction == -1);

            // こういう処理を処理スレッドでやるんじゃないのか？
            // ロックだけする？
            // マーク実行もロックだけでいいかも
            // あーロックだめだ。処理がタスクになってる。マーカーロック？

            int id = 0;

            lock (_MarkerLock)
            {
                if (Markers.Count == 0) return false;
                var list = Markers.Select(e => new Pair<int, Page>(GetIndex(e), e)).OrderBy(e => e.Key); // このためだけに型つくるとかないわー
                var index = GetViewPageindex();

                //
                var next =
                    direction > 0
                    ? list.FirstOrDefault(e => e.Key > index) ?? (isLoop ? list.First() : null)
                    : list.LastOrDefault(e => e.Key < index) ?? (isLoop ? list.Last() : null);

                if (next == null) return false;
                id = next.Key;
            }

            RequestSetPosition(new PagePosition(id, 0), direction, false);
            return true;
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
            Task.Run(() => CommandWorker(), _CommandWorkerCancellationTokenSource.Token);
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
        private async Task UpdateViewPageAsync(ViewPageContextSource source, bool isPreLoad)
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

            // cleanup pages
            _KeepPages.AddRange(viewPages.Where(e => !_KeepPages.Contains(e)));
            CleanupPages(source);

            // start load
            var tlist = new List<Task>();
            foreach (var page in viewPages)
            {
                tlist.Add(page.LoadAsync(QueueElementPriority.Top));
            }

            // pre load
            if (isPreLoad) PreLoad(source);

            // wait load
            await Task.WhenAll(tlist);

            // update contents
            _ViewContext = CreateViewPageContext(source);

            // task cancel?
            _CommandWorkerCancellationTokenSource.Token.ThrowIfCancellationRequested();

            // notice ViewContentsChanged
            App.Current.Dispatcher.Invoke(() => ViewContentsChanged?.Invoke(this, new ViewSource() { Sources = _ViewContext.ViewContentsSource, Direction = _ViewContext.Direction }));

            // change page
            DisplayIndex = _ViewContext.Position.Index;

            // notice PropertyChanged
            PageChanged?.Invoke(this, _ViewContext.Position.Index);

            // ページ破棄
            if (!AllowPreLoad) ClearAllPages();
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
        private bool IsEnableDividePage(int index)
        {
            return (PageMode == PageMode.SinglePage && IsSupportedDividePage && Pages[index].IsWide);
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
                    if (IsEnableDividePage(position.Index))
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
        private void CleanupPages(ViewPageContextSource source)
        {
            int keepPageSize = 3;

            // コンテンツを保持するページ収集
            var keepPages = new List<Page>();
            for (int offset = -keepPageSize; offset <= keepPageSize; ++offset)
            {
                int index = source.Position.Index + offset;
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


        // 全ページコンテンツの削除を行う
        private void ClearAllPages()
        {
            foreach (var page in _KeepPages)
            {
                page.Close();
            }

            // 保持ページ更新
            _KeepPages = new List<Page>();
        }


        // 先読み
        private void PreLoad(ViewPageContextSource source)
        {
            if (!AllowPreLoad) return;

            var preLoadPages = new List<Page>();

            for (int offset = 0; offset < 4; ++offset)
            {
                int index = source.Position.Index + (source.Direction < 0 ? -offset : offset);
                if (0 <= index && index < Pages.Count)
                {
                    Debug.Assert(_KeepPages.Contains(Pages[index])); // 念のため
                    Pages[index].Open(QueueElementPriority.Default, Page.OpenOption.WeakPriority);

                    if (!_KeepPages.Contains(Pages[index]))
                    {
                        _KeepPages.Add(Pages[index]);
                    }
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
                    Pages.Sort((a, b) => CompareFileNameOrder(a, b, Win32Api.StrCmpLogicalW));
                    break;
                case PageSortMode.FileNameDescending:
                    Pages.Sort((a, b) => CompareFileNameOrder(b, a, Win32Api.StrCmpLogicalW));
                    break;
                case PageSortMode.TimeStamp:
                    Pages.Sort((a, b) => CompareDateTimeOrder(a, b, Win32Api.StrCmpLogicalW));
                    break;
                case PageSortMode.TimeStampDescending:
                    Pages.Sort((a, b) => CompareDateTimeOrder(b, a, Win32Api.StrCmpLogicalW));
                    break;
                case PageSortMode.Random:
                    var random = new Random();
                    Pages = Pages.OrderBy(e => random.Next()).ToList();
                    break;
                default:
                    throw new NotImplementedException();
            }

            for (int i = 0; i < Pages.Count; ++i) Pages[i].Index = i;

            PagesSorted?.Invoke(this, null);
        }

        //
        public static void SortPages(List<Page> pages, PageSortMode sortMode)
        {
            if (pages == null || pages.Count <= 0) return;

            switch (sortMode)
            {
                case PageSortMode.FileName:
                    pages.Sort((a, b) => CompareFileNameOrder(a, b, Win32Api.StrCmpLogicalW));
                    break;
                case PageSortMode.FileNameDescending:
                    pages.Sort((a, b) => CompareFileNameOrder(b, a, Win32Api.StrCmpLogicalW));
                    break;
                case PageSortMode.TimeStamp:
                    pages.Sort((a, b) => CompareDateTimeOrder(a, b, Win32Api.StrCmpLogicalW));
                    break;
                case PageSortMode.TimeStampDescending:
                    pages.Sort((a, b) => CompareDateTimeOrder(b, a, Win32Api.StrCmpLogicalW));
                    break;
                case PageSortMode.Random:
                    var random = new Random();
                    pages = pages.OrderBy(e => random.Next()).ToList();
                    break;
                default:
                    throw new NotImplementedException();
            }

            for (int i = 0; i < pages.Count; ++i) pages[i].Index = i;
        }

        // ファイル名, 日付, ID の順で比較
        private static int CompareFileNameOrder(Page p1, Page p2, Func<string, string, int> compare)
        {
            if (p1.FullPath != p2.FullPath)
                return CompareFileName(p1.FullPath, p2.FullPath, compare);
            else if (p1.Entry.LastWriteTime != p2.Entry.LastWriteTime)
                return CompareDateTime(p1.Entry.LastWriteTime, p2.Entry.LastWriteTime);
            else
                return p1.Entry.Id - p2.Entry.Id;
        }

        // 日付, ファイル名, ID の順で比較
        private static int CompareDateTimeOrder(Page p1, Page p2, Func<string, string, int> compare)
        {
            if (p1.Entry.LastWriteTime != p2.Entry.LastWriteTime)
                return CompareDateTime(p1.Entry.LastWriteTime, p2.Entry.LastWriteTime);
            else if (p1.FullPath != p2.FullPath)
                return CompareFileName(p1.FullPath, p2.FullPath, compare);
            else
                return p1.Entry.Id - p2.Entry.Id;
        }

        // ファイル名比較. ディレクトリを優先する
        private static int CompareFileName(string s1, string s2, Func<string, string, int> compare)
        {
            string d1 = LoosePath.GetDirectoryName(s1);
            string d2 = LoosePath.GetDirectoryName(s2);

            if (d1 == d2)
                return compare(s1, s2);
            else
                return compare(d1, d2);
        }

        // 日付比較。null対応
        private static int CompareDateTime(DateTime? _t1, DateTime? _t2)
        {
            DateTime t1 = _t1 ?? DateTime.MinValue;
            DateTime t2 = _t2 ?? DateTime.MinValue;
            return (t1.Ticks - t2.Ticks < 0) ? -1 : 1;
        }



        // ページの削除
        private void Remove(Page page)
        {
            if (Pages.Count <= 0) return;

            int index = Pages.IndexOf(page);
            if (index < 0) return;

            Pages.RemoveAt(index);
            PageRemoved?.Invoke(this, page);

            index = ClampPageNumber(index);
            RequestSetPosition(new PagePosition(index, 0), 1, true);
        }


        // 表示の再構築
        private void Reflesh(bool clear)
        {
            if (Place == null) return;

            if (clear)
            {
                _KeepPages.ForEach(e => e?.Close());
            }

            RequestSetPosition(_ViewContext.Position, 1, true);
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
            [DataMember(EmitDefaultValue = false)]
            public string Place { get; set; }

            public string Name => Place.EndsWith(@":\") ? Place : System.IO.Path.GetFileName(Place);

            [DataMember(EmitDefaultValue = false)]
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

            [DataMember(Order = 12, EmitDefaultValue = false)]
            public DateTime LastAccessTime { get; set; }

            [DataMember(Order = 14, EmitDefaultValue = false)]
            public List<string> Markers { get; set; }

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
                var clone = (Memento)this.MemberwiseClone();
                clone.Markers = this.Markers != null ? new List<string>(this.Markers) : null;
                return clone;
            }

            //
            public Memento CloneWithoutMarkers()
            {
                var clone = (Memento)this.MemberwiseClone();
                clone.Markers = null;
                return clone;
            }


            // 保存用バリデート
            // このmementoは履歴とデフォルト設定の２つに使われるが、デフォルト設定には本の場所やページは不要
            public void ValidateForDefault()
            {
                Place = null;
                BookMark = null;
                Markers = null;
            }
        }

        // 重複チェック用
        public class MementoPlaceCompare : IEqualityComparer<Memento>
        {
            public bool Equals(Memento m1, Memento m2)
            {
                if (m1 == null && m2 == null)
                    return true;
                else if (m1 == null | m2 == null)
                    return false;
                else if (m1.Place == m2.Place)
                    return true;
                else
                    return false;
            }

            public int GetHashCode(Memento m)
            {
                return m.Place.GetHashCode();
            }
        }


        // bookの設定を取得する
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.Place = Place;
            memento.BookMark = SortMode != PageSortMode.Random ? GetViewPage()?.FullPath : null;

            memento.PageMode = PageMode;
            memento.BookReadOrder = BookReadOrder;
            memento.IsSupportedDividePage = IsSupportedDividePage;
            memento.IsSupportedSingleFirstPage = IsSupportedSingleFirstPage;
            memento.IsSupportedSingleLastPage = IsSupportedSingleLastPage;
            memento.IsSupportedWidePage = IsSupportedWidePage;
            memento.IsRecursiveFolder = IsRecursiveFolder;
            memento.SortMode = SortMode;
            //memento.LastAccessTime = DateTime.Now;

            memento.Markers = (this.Markers.Count > 0) ? this.Markers.Select(e => e.FileName).ToList() : null;

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

        // マーカー復元
        public void RestoreMarker(Memento memento)
        {
            if (memento.Markers != null)
            {
                this.Markers = memento.Markers.Select(e => Pages.FirstOrDefault(page => page.FileName == e)).Where(e => e != null).ToList();
            }
        }
    }

    #endregion
}
