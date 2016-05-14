// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

// TODO: 高速切替でテンポラリが残るバグ
// ----------------------------
// TODO: フォルダサムネイル(非同期) 
// TODO: コマンド類の何時でも受付。ロード中だから弾く、ではない別の方法を。



namespace NeeView
{
    public class FolderListSyncArguments
    {
        public string Path { get; set; }
        public bool isKeepPlace { get; set; }
        public bool IsFocus { get; set; }
    }

    public enum BookMementoType
    {
        None,
        Bookmark,
        History,
    }

    //
    public class BookUnit : IDisposable
    {
        public Book Book { get; set; }

        public BookLoadOption LoadOptions { get; set; }
        public BookMementoUnit BookMementoUnit { get; set; }

        public BookUnit(Book book)
        {
            Book = book;
        }

        public void Dispose()
        {
            Book?.Dispose();
        }

        public BookMementoType BookMementoType
        {
            get
            {
                if (BookMementoUnit?.BookmarkNode != null)
                    return BookMementoType.Bookmark;
                else if (BookMementoUnit?.HistoryNode != null)
                    return BookMementoType.History;
                else
                    return BookMementoType.None;
            }
        }

        public bool IsKeepHistoryOrder
            => (LoadOptions & BookLoadOption.KeepHistoryOrder) == BookLoadOption.KeepHistoryOrder;

        public bool IsValid
            => Book?.Place != null;

        public string Address
            => Book?.Place;
    }

    /// <summary>
    /// 本の管理
    /// ロード、本の操作はここを通す
    /// </summary>
    public class BookHub : INotifyPropertyChanged
    {
        #region NormalizePathname

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetLongPathName(string shortPath, StringBuilder longPath, int longPathLength);

        // パス名の正規化
        private static string GetNormalizePathName(string source)
        {
            // 区切り文字修正
            source = new System.Text.RegularExpressions.Regex(@"[/\\]").Replace(source, "\\").TrimEnd('\\');

            // ドライブレター修正
            source = new System.Text.RegularExpressions.Regex(@"^[a-z]:").Replace(source, m => m.Value.ToUpper());
            source = new System.Text.RegularExpressions.Regex(@":$").Replace(source, ":\\");

            StringBuilder longPath = new StringBuilder(1024);
            if (0 == GetLongPathName(source, longPath, longPath.Capacity))
            {
                return source;
            }

            string dist = longPath.ToString();
            return longPath.ToString();
        }

        #endregion

        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        #region Events

        // 本の変更通知
        public event EventHandler<BookMementoType> BookChanged;

        // ページ番号の変更通知
        public event EventHandler<int> PageChanged;

        // 設定の変更通知
        public event EventHandler<string> SettingChanged;

        // ロード中通知
        public event EventHandler<string> Loading;

        // メッセージ通知
        public event EventHandler<string> InfoMessage;

        // ViewContentsの変更通知
        public event EventHandler<ViewSource> ViewContentsChanged;

        // スライドショーモード変更通知
        public event EventHandler<bool> SlideShowModeChanged;

        // 空ページメッセージ
        public event EventHandler<string> EmptyMessage;

        // フォルダ列更新要求
        public event EventHandler<FolderListSyncArguments> FolderListSync;

        // 履歴リスト更新要求
        public event EventHandler<string> HistoryListSync;

        // 履歴に追加、削除された
        public event EventHandler<BookMementoCollectionChangedArgs> HistoryChanged;

        // ブックマークにに追加、削除された
        public event EventHandler<BookMementoCollectionChangedArgs> BookmarkChanged;

        // アドレスが変更された
        public event EventHandler AddressChanged;

        // ページがソートされた
        public event EventHandler PagesSorted;

        // サムネイル変更
        public event EventHandler<Page> ThumbnailChanged;

        #endregion


        public void SetInfoMessage(string message)
        {
            InfoMessage?.Invoke(this, message);
        }


        // ロード中フラグ
        private bool _IsLoading;


        // アニメGIF 有効/無効
        #region Property: IsEnableAnimatedGif
        public bool IsEnableAnimatedGif
        {
            get { return Page.IsEnableAnimatedGif; }
            set
            {
                if (Page.IsEnableAnimatedGif != value)
                {
                    Page.IsEnableAnimatedGif = value;
                    CurrentBook?.RequestReflesh(true); // 表示更新
                }
            }
        }
        #endregion


        #region Property: IsEnableExif
        public bool IsEnableExif
        {
            get { return Page.IsEnableExif; }
            set
            {
                if (Page.IsEnableExif != value)
                {
                    Page.IsEnableExif = value;
                    CurrentBook?.RequestReflesh(true); // 表示更新
                }
            }
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
                    ReLoad();
                }
            }
        }

        // 履歴から設定を復元する
        public bool IsEnabledAutoNextFolder { get; set; } = false;

        // 再帰を確認する
        public bool IsConfirmRecursive { get; set; }


        // 7z.dll アクセスでファイルをロックする
        public bool IsSevenZipAccessLocked
        {
            get { return SevenZipSource.IsFileLocked; }
            set { SevenZipSource.IsFileLocked = value; }
        }


        // スライドショー再生フラグ
        private bool _IsEnableSlideShow;
        public bool IsEnableSlideShow
        {
            get
            {
                return _IsEnableSlideShow;
            }
            set
            {
                _IsEnableSlideShow = value;
                SlideShowModeChanged?.Invoke(this, _IsEnableSlideShow);
            }
        }

        // スライドショー設定：ループ再生
        private bool IsSlideShowByLoop { get; set; } = true;

        // スライドショー設定：切り替わる時間(秒)
        public double SlideShowInterval { get; set; } = 5.0;

        // スライドショー設定：マウス移動でキャンセル
        public bool IsCancelSlideByMouseMove { get; set; } = true;

        //「ファイルを開く」の初期フォルダを現在開いているフォルダ基準にする
        public bool IsEnarbleCurrentDirectory { get; set; }

        // 圧縮ファイルの有効/無効
        #region Property: IsSupportArchiveFile
        public bool IsSupportArchiveFile
        {
            get { return ModelContext.ArchiverManager.IsEnabled; }
            set { ModelContext.ArchiverManager.IsEnabled = value; }
        }
        #endregion

        // 先読み設定
        #region Property: AllowPagePreLoad
        private bool _AllowPagePreLoad = true;
        public bool AllowPagePreLoad
        {
            get { return _AllowPagePreLoad; }
            set
            {
                _AllowPagePreLoad = value;
                if (CurrentBook != null) CurrentBook.AllowPreLoad = _AllowPagePreLoad;
            }
        }
        #endregion


        // 現在の本
        public Book CurrentBook => Current?.Book;
        public BookUnit Current { get; private set; }

        // アドレス
        #region Property: Address
        private string _Address;
        public string Address
        {
            get { return _Address; }
            set { _Address = value; AddressChanged?.Invoke(this, null); }
        }
        #endregion



        // 本の設定、引き継ぎ用
        public Book.Memento BookMemento { get; set; } = new Book.Memento();

        // 本の設定、標準
        public Book.Memento BookMementoDefault { get; set; } = new Book.Memento();

        // 新しい本を開くときに標準設定にする？
        public bool IsUseBookMementoDefault { get; set; }

        // ページ番号のみ復元する？
        public bool IsRecoveryPageOnly { get; set; }

        // 外部アプリ設定
        public ExternalApplication ExternalApllication { get; set; } = new ExternalApplication();


        // ページ表示開始スレッドイベント
        private ManualResetEvent _ViewContentEvent = new ManualResetEvent(false);



        // コンストラクタ
        public BookHub()
        {
            ModelContext.BookHistory.HistoryChanged += (s, e) => HistoryChanged?.Invoke(s, e);
            ModelContext.Bookmarks.BookmarkChanged += (s, e) => BookmarkChanged?.Invoke(s, e);

            StartCommandWorker();
        }


        // コマンド基底
        private abstract class BookHubCommand
        {
            protected BookHub _BookHub;
            public abstract int Priority { get; }

            public BookHubCommand(BookHub bookHub)
            {
                _BookHub = bookHub;
            }

            public virtual async Task Execute() { await Task.Yield(); }
        }


        // ロードコマンド 引数
        public class LoadCommandArgs
        {
            public string Path { get; set; }
            public BookLoadOption Option { get; set; }
            public bool IsRefleshFolderList { get; set; }
        }

        // ロードコマンド
        private class LoadCommand : BookHubCommand
        {
            public override int Priority => 2;

            private LoadCommandArgs _Args;

            //
            public LoadCommand(BookHub bookHub, string path, BookLoadOption option, bool isRefleshFolderList) : base(bookHub)
            {
                _Args = new LoadCommandArgs()
                {
                    Path = path,
                    Option = option,
                    IsRefleshFolderList = isRefleshFolderList,
                };
            }

            //
            public LoadCommand(BookHub bookHub, LoadCommandArgs args) : base(bookHub)
            {
                _Args = args;
            }


            //
            public override async Task Execute()
            {
                await _BookHub.LoadAsync(_Args);
            }
        }


        // ロード
        public void RequestLoad(string path, BookLoadOption option, bool isRefleshFolderList)
        {
            if (path == null) return;
            path = GetNormalizePathName(path);

            if (CurrentBook?.Place == path && (option & BookLoadOption.SkipSamePlace) == BookLoadOption.SkipSamePlace) return;

            Address = path;

            RegistCommand(new LoadCommand(this, path, option, isRefleshFolderList));
        }

        // ワーカータスクのキャンセルトークン
        private CancellationTokenSource _CommandWorkerCancellationTokenSource;

        // 予約されているコマンド
        private BookHubCommand _ReadyCommand;

        // 予約コマンド存在イベント
        public AutoResetEvent _ReadyCommandEvent { get; private set; } = new AutoResetEvent(false);

        // 排他処理用ロックオブジェクト
        private object _Lock = new object();

        // コマンドの予約
        private void RegistCommand(BookHubCommand command)
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
                ////Debug.WriteLine("BookHubタスクの開始");
                while (!_CommandWorkerCancellationTokenSource.Token.IsCancellationRequested)
                {
                    await Task.Run(() => _ReadyCommandEvent.WaitOne());
                    _CommandWorkerCancellationTokenSource.Token.ThrowIfCancellationRequested();

                    BookHubCommand command;
                    lock (_Lock)
                    {
                        command = _ReadyCommand;
                        _ReadyCommand = null;
                    }

                    if (command != null)
                    {
                        ////Debug.WriteLine("CMD: " + command.ToString());
                        await command.Execute();
                        ////Debug.WriteLine("CMD: " + command.ToString() + " done.");

                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Action<Exception> action = (exception) => { throw new ApplicationException("BookHubタスク内部エラー", exception); };
                await App.Current.Dispatcher.BeginInvoke(action, e);
            }
            finally
            {
                ////Debug.WriteLine("Bookタスクの終了: " + Place);
            }
        }

        //設定の保存
        public void SaveBookMemento()
        {
            // 履歴の保存
            if (Current != null && Current.Book.Pages.Count > 0)
            {
                var memento = Current.Book.CreateMemento();

                // 履歴の保存
                ModelContext.BookHistory.Add(Current.BookMementoUnit, memento, Current.IsKeepHistoryOrder);

                // ブックマーク更新
                ModelContext.Bookmarks.Update(Current.BookMementoUnit, memento);
            }
        }

        /// <summary>
        /// 本の開放
        /// </summary>
        public void Unload(bool isClearViewContent)
        {
            // 履歴の保存
            SaveBookMemento();

            // 現在の本を開放
            Current?.Dispose();
            Current = null;

            // 現在表示されているコンテンツを無効
            if (isClearViewContent)
            {
                ViewContentsChanged?.Invoke(this, null);
            }
        }


        // 入力パスから場所を取得
        private string GetPlaceEx(string path, BookLoadOption option)
        {
            if (Directory.Exists(path))
            {
                return path;
            }

            if (File.Exists(path))
            {
                if (ModelContext.ArchiverManager.IsSupported(path))
                {
                    Archiver archiver = ModelContext.ArchiverManager.CreateArchiver(path, null);
                    if (archiver.IsSupported())
                    {
                        return path;
                    }
                }

                if (ModelContext.BitmapLoaderManager.IsSupported(path) || (option & BookLoadOption.SupportAllFile) == BookLoadOption.SupportAllFile)
                {
                    return Path.GetDirectoryName(path);
                }
                else
                {
                    //throw new FileFormatException($"\"{path}\" はサポート外ファイルです");
                    return path;
                }
            }

            //throw new FileNotFoundException($"\"{path}\" が見つかりません", path);
            return path;
        }



        // パスから対応するアーカイバを取得する
        private string GetPlace(string path, BookLoadOption option)
        {
            if (Directory.Exists(path))
            {
                return path;
            }

            if (File.Exists(path))
            {
                if (ModelContext.ArchiverManager.IsSupported(path))
                {
                    Archiver archiver = ModelContext.ArchiverManager.CreateArchiver(path, null);
                    if (archiver.IsSupported())
                    {
                        return path;
                    }
                }

                if (ModelContext.BitmapLoaderManager.IsSupported(path) || (option & BookLoadOption.SupportAllFile) == BookLoadOption.SupportAllFile)
                {
                    return Path.GetDirectoryName(path);
                }
                else
                {
                    throw new FileFormatException($"\"{path}\" はサポート外ファイルです");
                }
            }

            throw new FileNotFoundException($"\"{path}\" が見つかりません", path);
        }

        //
        private Book.Memento GetBookMementoDefault() => IsUseBookMementoDefault ? BookMementoDefault : BookMemento;

        // 設定をブックマーク、履歴から取得する
        private Book.Memento GetSetting(BookMementoUnit unit, string place)
        {
            if (unit != null)
            {
                // ブックマーク
                if (unit.BookmarkNode != null)
                {
                    return unit.Memento.Clone();
                }
                // 履歴
                else if (unit.HistoryNode != null && IsEnableHistory)
                {
                    if (IsRecoveryPageOnly)
                    {
                        var memento = GetBookMementoDefault().Clone();
                        memento.IsRecursiveFolder = unit.Memento.IsRecursiveFolder;
                        memento.BookMark = unit.Memento.BookMark;
                        return memento;
                    }
                    else
                    {
                        return unit.Memento.Clone();
                    }
                }
            }

            // 履歴なし
            {
                var memento = GetBookMementoDefault().Clone();
                memento.IsRecursiveFolder = false;
                memento.BookMark = null;
                return memento;
            }
        }

        // ロード中状態更新
        private void NotifyLoading(string path)
        {
            _IsLoading = (path != null);
            App.Current.Dispatcher.Invoke(() => Loading?.Invoke(this, path));
        }

        /// <summary>
        /// 本を読み込む
        /// </summary>
        /// <param name="path"></param>
        /// <param name="option"></param>
        /// <param name="isRefleshFolderList"></param>
        /// <returns></returns>
        private async Task LoadAsync(LoadCommandArgs args)
        {
            // 現在の本を開放
            Unload(false);

            try
            {
                // place
                string place = GetPlace(args.Path, args.Option);

                // start
                string startEntry = args.Path == place ? null : Path.GetFileName(args.Path);

                // Now Loading ON
                NotifyLoading(args.Path);

                // フォルダリスト更新
                if (args.IsRefleshFolderList)
                {
                    App.Current.Dispatcher.Invoke(() => FolderListSync?.Invoke(this, new FolderListSyncArguments() { Path = place, isKeepPlace = false }));
                }
                else if ((args.Option & BookLoadOption.SelectFoderListMaybe) != 0)
                {
                    App.Current.Dispatcher.Invoke(() => FolderListSync?.Invoke(this, new FolderListSyncArguments() { Path = place, isKeepPlace = true }));
                }

                // 履歴リスト更新
                if ((args.Option & BookLoadOption.SelectHistoryMaybe) != 0)
                {
                    App.Current.Dispatcher.Invoke(() => HistoryListSync?.Invoke(this, place));
                }

                // 本の設定
                var unit = ModelContext.BookMementoCollection.Find(place);
                var setting = GetSetting(unit, place);

                // Load本体
                await LoadAsyncCore(place, startEntry ?? setting.BookMark, args.Option, setting, unit);

                // ビュー初期化
                App.Current.Dispatcher.Invoke(() => ModelContext.CommandTable[CommandType.ViewReset].Execute(null));

                // 本の設定を退避
                App.Current.Dispatcher.Invoke(() => SettingChanged?.Invoke(this, null));


                // 本の変更通知
                App.Current.Dispatcher.Invoke(() => BookChanged?.Invoke(this, Current.BookMementoType));

                // ページがなかった時の処理
                if (CurrentBook.Pages.Count <= 0)
                {
                    App.Current.Dispatcher.Invoke(() => EmptyMessage?.Invoke(this, $"\"{CurrentBook.Place}\" には読み込めるファイルがありません"));

                    if (IsConfirmRecursive && (args.Option & BookLoadOption.ReLoad) == 0 && !CurrentBook.IsRecursiveFolder && CurrentBook.SubFolderCount > 0)
                    {
                        App.Current.Dispatcher.Invoke(() => ConfirmRecursive());
                    }
                }
            }
            catch (Exception e)
            {
                // 現在表示されているコンテンツを無効
                App.Current.Dispatcher.Invoke(() => ViewContentsChanged?.Invoke(this, null));

                // 本の変更通知
                App.Current.Dispatcher.Invoke(() => BookChanged?.Invoke(this, BookMementoType.None)); //  Current.BookMementoType));

                // ファイル読み込み失敗通知
                EmptyMessage?.Invoke(this, e.Message);

                // 履歴リスト更新
                if ((args.Option & BookLoadOption.SelectHistoryMaybe) != 0)
                {
                    App.Current.Dispatcher.Invoke(() => HistoryListSync?.Invoke(this, Address));
                }
            }
            finally
            {
                // Now Loading OFF
                NotifyLoading(null);
            }
        }


        // 再帰読み込み確認
        public void ConfirmRecursive()
        {
            // サブフォルダ確認
            var message = new MessageEventArgs("MessageBox");
            message.Parameter = new MessageBoxParams()
            {
                MessageBoxText = $"\"{CurrentBook.Place}\" には読み込めるファイルがありません。\n\nサブフォルダ(書庫)も読み込みますか？",
                Caption = "確認",
                Button = System.Windows.MessageBoxButton.YesNo,
                Icon = MessageBoxExImage.Question
            };
            Messenger.Send(this, message);

            if (message.Result == true)
            {
                RequestLoad(CurrentBook.Place, BookLoadOption.Recursive | BookLoadOption.ReLoad, true);
            }
        }


        /// <summary>
        /// 本を読み込む(本体)
        /// </summary>
        /// <param name="path">本のパス</param>
        /// <param name="startEntry">開始エントリ</param>
        /// <param name="option">読み込みオプション</param>
        private async Task LoadAsyncCore(string path, string startEntry, BookLoadOption option, Book.Memento setting, BookMementoUnit unit)
        {
            // 履歴に登録済の場合は履歴先頭に移動させる
            if (unit?.HistoryNode != null && (option & BookLoadOption.KeepHistoryOrder) == 0)
            {
                ModelContext.BookHistory.Add(unit, unit.Memento, false);
            }

            // 新しい本を作成
            var book = new Book();

            // 設定の復元
            if ((option & BookLoadOption.ReLoad) == BookLoadOption.ReLoad)
            {
                // リロード時は設定そのまま
                book.Restore(BookMemento);
            }
            else
            {
                book.Restore(setting);
            }

            // 先読み設定
            book.AllowPreLoad = this.AllowPagePreLoad;

            // 全種類ファイルサポート設定
            if (IsEnableNoSupportFile)
            {
                option |= BookLoadOption.SupportAllFile;
            }

            // リカーシブ設定
            if ((option & BookLoadOption.Recursive) == BookLoadOption.Recursive)
            {
                book.IsRecursiveFolder = true;
            }

            //
            try
            {
                // ロード。非同期で行う
                await book.Load(path, startEntry, option);

                // ロード後にイベント設定
                book.PageChanged += (s, e) => PageChanged?.Invoke(s, e);
                book.ViewContentsChanged += (s, e) => ViewContentsChanged?.Invoke(s, e);
                book.PageTerminated += OnPageTerminated;
                book.DartyBook += (s, e) => RequestLoad(Address, BookLoadOption.ReLoad, false);
                book.PagesSorted += (s, e) => PagesSorted?.Invoke(s, e);
                book.ThumbnailChanged += (s, e) => ThumbnailChanged?.Invoke(s, e);

                // 最初のコンテンツ表示待ち設定
                _ViewContentEvent.Reset();
                book.ViewContentsChanged += (s, e) => _ViewContentEvent.Set();

                // カレントを設定し、開始する
                Current = new BookUnit(book);
                Current.LoadOptions = option;
                Current.BookMementoUnit = unit;
                Current.Book.Start();

                // 最初のコンテンツ表示待ち
                await Task.Run(() => _ViewContentEvent.WaitOne());
            }
            catch (Exception e)
            {
                // 後始末
                Current?.Dispose();
                Current = null;

                // 履歴から消去
                ModelContext.BookHistory.Remove(path);
                Messenger.Send(this, "UpdateLastFiles");

                throw new ApplicationException($"{path} の読み込みに失敗しました。\n{e.Message}", e);
            }

            // 本の設定を退避
            BookMemento = CurrentBook.CreateMemento();
            BookMemento.ValidateForDefault();

            // 新規履歴
            if (Current.BookMementoUnit?.HistoryNode == null && CurrentBook.Pages.Count > 0 && !Current.IsKeepHistoryOrder)
            {
                Current.BookMementoUnit = ModelContext.BookHistory.Add(Current.BookMementoUnit, CurrentBook?.CreateMemento(), Current.IsKeepHistoryOrder);
            }

            Address = Current.Book.Place;
        }


        public bool CanReload()
        {
            return (!string.IsNullOrWhiteSpace(Address));
        }

        // 再読み込み
        public void ReLoad()
        {
            if (_IsLoading || Address == null) return;

            var options = Current != null ? (Current.LoadOptions & BookLoadOption.KeepHistoryOrder) : BookLoadOption.None;
            RequestLoad(Address, options, true);
        }

        // ページ終端を超えて移動しようとするときの処理
        private void OnPageTerminated(object sender, int e)
        {
            if (IsEnableSlideShow && IsSlideShowByLoop)
            {
                FirstPage();
            }

            else if (IsEnabledAutoNextFolder)
            {
                if (e < 0)
                {
                    PrevFolder(BookLoadOption.LastPage);
                }
                else
                {
                    NextFolder(BookLoadOption.FirstPage);
                }
            }
            else
            {
                if (IsEnableSlideShow)
                {
                    ToggleSlideShow(); // スライドショー解除
                }

                else if (e < 0)
                {
                    //FirstPage();
                    InfoMessage?.Invoke(this, "最初のページです");
                }
                else
                {
                    //LastPage();
                    InfoMessage?.Invoke(this, "最後のページです");
                }
            }
        }

        // 現在ページ番号取得
        public int GetPageIndex()
        {
            return CurrentBook == null ? 0 : CurrentBook.DisplayIndex; // GetPosition().Index;
        }

        // 現在ページ番号設定 (先読み無し)
        public void SetPageIndex(int index)
        {
            CurrentBook?.RequestSetPosition(new PagePosition(index, 0), 1, false);
        }

        // 総ページ数取得
        public int GetPageCount()
        {
            return CurrentBook == null ? 0 : CurrentBook.Pages.Count - 1;
        }

        // 履歴を戻ることができる？
        public bool CanPrevHistory()
        {
            var unit = ModelContext.BookHistory.Find(Address);
            // 履歴が存在するなら真
            if (unit == null && ModelContext.BookHistory.Count > 0) return true;
            // 現在の履歴位置より古いものがあれば真。リストと履歴の方向は逆
            return unit?.HistoryNode != null && unit.HistoryNode.Next != null;
        }

        // 履歴を戻る
        public void PrevHistory()
        {
            if (_IsLoading || ModelContext.BookHistory.Count <= 0) return;

            var unit = ModelContext.BookHistory.Find(Address);
            var previous = unit?.HistoryNode?.Next.Value; // リストと履歴の方向は逆

            if (unit == null)
            {
                RequestLoad(ModelContext.BookHistory.First?.Memento.Place, BookLoadOption.KeepHistoryOrder | BookLoadOption.SelectHistoryMaybe, false);
            }
            else if (previous != null)
            {
                RequestLoad(previous.Memento.Place, BookLoadOption.KeepHistoryOrder | BookLoadOption.SelectHistoryMaybe, false);
            }
            else
            {
                InfoMessage?.Invoke(this, "これより古い履歴はありません");
            }
        }

        // 履歴を進めることができる？
        public bool CanNextHistory()
        {
            var unit = ModelContext.BookHistory.Find(Address);
            return (unit?.HistoryNode != null && unit.HistoryNode.Previous != null); // リストと履歴の方向は逆
        }

        // 履歴を進める
        public void NextHistory()
        {
            if (_IsLoading) return;

            var unit = ModelContext.BookHistory.Find(Address);
            var next = unit?.HistoryNode?.Previous; // リストと履歴の方向は逆
            if (next != null)
            {
                RequestLoad(next.Value.Memento.Place, BookLoadOption.KeepHistoryOrder | BookLoadOption.SelectHistoryMaybe, false);
            }
            else
            {
                InfoMessage?.Invoke(this, "最新の履歴です");
            }
        }

        // 前のページに移動
        public void PrevPage()
        {
            CurrentBook?.PrevPage();
        }

        // 次のページに移動
        public void NextPage()
        {
            CurrentBook?.NextPage();
        }

        // 1ページ前に移動
        public void PrevOnePage()
        {
            CurrentBook?.PrevPage(1);
        }

        // 1ページ後に移動
        public void NextOnePage()
        {
            CurrentBook?.NextPage(1);
        }

        // 最初のページに移動
        public void FirstPage()
        {
            CurrentBook?.FirstPage();
        }

        // 最後のページに移動
        public void LastPage()
        {
            CurrentBook?.LastPage();
        }

        // 指定ページに移動
        public void JumpPage(Page page)
        {
            if (!_IsLoading && page != null) CurrentBook?.JumpPage(page);
        }

        // スライドショー用：次のページへ移動
        public void NextSlide()
        {
            if (IsEnableSlideShow) NextPage();
        }

        // 次のフォルダに移動
        public void NextFolder(BookLoadOption option = BookLoadOption.None)
        {
            var result = Messenger.Send(this, new MessageEventArgs("MoveFolder") { Parameter = new MoveFolderParams() { Distance = +1, BookLoadOption = option } });
            if (result != true)
            {
                InfoMessage?.Invoke(this, "次のフォルダはありません");
            }
        }

        // 前のフォルダに移動
        public void PrevFolder(BookLoadOption option = BookLoadOption.None)
        {
            var result = Messenger.Send(this, new MessageEventArgs("MoveFolder") { Parameter = new MoveFolderParams() { Distance = -1, BookLoadOption = option } });
            if (result != true)
            {
                InfoMessage?.Invoke(this, "前のフォルダはありません");
            }
        }


        // スライドショーON/OFF
        public void ToggleSlideShow()
        {
            IsEnableSlideShow = !IsEnableSlideShow;
            SettingChanged?.Invoke(this, null);
        }


        // 本来ここで実装すべきてはない
        #region FolderOrder

        // フォルダの並びの変更
        public void ToggleFolderOrder()
        {
            Messenger.Send(this, new MessageEventArgs("ToggleFolderOrder"));
        }

        // フォルダの並びの設定
        public void SetFolderOrder(FolderOrder order)
        {
            Messenger.Send(this, new MessageEventArgs("SetFolderOrder") { Parameter = new FolderOrderParams() { FolderOrder = order } });
        }

        // フォルダの並びの取得
        public FolderOrder GetFolderOrder()
        {
            var param = new FolderOrderParams();
            Messenger.Send(this, new MessageEventArgs("GetFolderOrder") { Parameter = param });
            return param.FolderOrder;
        }

        #endregion


        // 本の設定を更新
        private void RefleshBookSetting()
        {
            CurrentBook?.Restore(BookMemento);
            SettingChanged?.Invoke(this, null);
        }

        // ページモードごとの設定の可否
        public bool CanPageModeSubSetting(PageMode mode)
        {
            return !_IsLoading && BookMemento.PageMode == mode;
        }

        // 先頭ページの単ページ表示ON/OFF 
        public void ToggleIsSupportedSingleFirstPage()
        {
            if (_IsLoading) return;
            BookMemento.IsSupportedSingleFirstPage = !BookMemento.IsSupportedSingleFirstPage;
            RefleshBookSetting();
        }

        // 最終ページの単ページ表示ON/OFF 
        public void ToggleIsSupportedSingleLastPage()
        {
            if (_IsLoading) return;
            BookMemento.IsSupportedSingleLastPage = !BookMemento.IsSupportedSingleLastPage;
            RefleshBookSetting();
        }

        // 横長ページの分割ON/OFF
        public void ToggleIsSupportedDividePage()
        {
            if (_IsLoading) return;
            BookMemento.IsSupportedDividePage = !BookMemento.IsSupportedDividePage;
            RefleshBookSetting();
        }

        // 横長ページの見開き判定ON/OFF
        public void ToggleIsSupportedWidePage()
        {
            if (_IsLoading) return;
            BookMemento.IsSupportedWidePage = !BookMemento.IsSupportedWidePage;
            RefleshBookSetting();
        }

        // フォルダ再帰読み込みON/OFF
        public void ToggleIsRecursiveFolder()
        {
            if (_IsLoading) return;
            BookMemento.IsRecursiveFolder = !BookMemento.IsRecursiveFolder;
            RefleshBookSetting();
        }

        // 見開き方向設定
        public void SetBookReadOrder(PageReadOrder order)
        {
            BookMemento.BookReadOrder = order;
            RefleshBookSetting();
        }

        // 見開き方向変更
        public void ToggleBookReadOrder()
        {
            if (_IsLoading) return;
            BookMemento.BookReadOrder = BookMemento.BookReadOrder.GetToggle();
            RefleshBookSetting();
        }

        // ページモード設定
        public void SetPageMode(PageMode mode)
        {
            BookMemento.PageMode = mode;
            RefleshBookSetting();
        }


        // 単ページ/見開き表示トグル
        public void TogglePageMode()
        {
            if (_IsLoading) return;
            BookMemento.PageMode = BookMemento.PageMode.GetToggle();
            RefleshBookSetting();
        }

        // ページ並び変更
        public void ToggleSortMode()
        {
            if (_IsLoading) return;
            var mode = BookMemento.SortMode.GetToggle();
            CurrentBook?.SetSortMode(mode);
            BookMemento.SortMode = mode;
            RefleshBookSetting();
        }

        // ページ並び設定
        public void SetSortMode(PageSortMode mode)
        {
            CurrentBook?.SetSortMode(mode);
            BookMemento.SortMode = mode;
            RefleshBookSetting();
        }



        // 外部アプリで開く
        public void OpenApplication()
        {
            if (CanOpenFilePlace())
            {
                try
                {
                    ExternalApllication.Call(CurrentBook?.GetViewPages());
                }
                catch (Exception e)
                {
                    Messenger.MessageBox(this, $"外部アプリ実行に失敗しました\n\n原因: {e.Message}", "エラー", System.Windows.MessageBoxButton.OK, MessageBoxExImage.Error);
                }
            }
        }


        // ブックマーク登録可能？
        public bool CanBookmark()
        {
            return (CurrentBook != null);
        }

        // ブックマーク登録
        public void Bookmark()
        {
            if (CanBookmark())
            {
                if (Current.Book.Place.StartsWith(Temporary.TempDirectory))
                {
                    Messenger.MessageBox(this, $"一時フォルダーはブックマークできません", "エラー", System.Windows.MessageBoxButton.OK, MessageBoxExImage.Error);
                }
                else
                {
                    Current.BookMementoUnit = ModelContext.Bookmarks.Add(Current.BookMementoUnit, CurrentBook.CreateMemento());
                }
            }
        }

        // ブックマーク切り替え
        public void ToggleBookmark()
        {
            if (CanBookmark())
            {
                if (Current.Book.Place.StartsWith(Temporary.TempDirectory))
                {
                    Messenger.MessageBox(this, $"一時フォルダーはブックマークできません", "エラー", System.Windows.MessageBoxButton.OK, MessageBoxExImage.Error);
                }
                else
                {
                    Current.BookMementoUnit = ModelContext.Bookmarks.Toggle(Current.BookMementoUnit, CurrentBook.CreateMemento());
                }
            }
        }

        // ブックマーク判定
        public bool IsBookmark(string place)
        {
            if (Current?.BookMementoUnit != null && Current.BookMementoUnit.Memento.Place == (place ?? CurrentBook.Place))
            {
                return Current.BookMementoUnit.BookmarkNode != null;
            }
            else
            {
                return false;
            }
        }


        // ファイルの場所を開くことが可能？
        public bool CanOpenFilePlace()
        {
            return CurrentBook?.GetViewPage() != null;
        }

        // ファイルの場所を開く
        public void OpenFilePlace()
        {
            if (CanOpenFilePlace())
            {
                string place = CurrentBook.GetViewPage()?.GetFilePlace();
                if (place != null)
                {
                    System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + place + "\"");
                }
            }
        }

        // ファイルを開く基準となるフォルダを取得
        public string GetDefaultFolder()
        {
            // 既に開いている場合、その場所を起点とする
            if (IsEnarbleCurrentDirectory && CurrentBook != null)
            {
                return Path.GetDirectoryName(CurrentBook.Place);
            }
            else
            {
                return "";
            }
        }



        // ファイルに保存する
        public void Export()
        {
            if (CurrentBook != null && CanOpenFilePlace())
            {
                try
                {
                    var pages = CurrentBook.GetViewPages();
                    int index = CurrentBook.GetViewPageindex() + 1;
                    string name = $"{Path.GetFileNameWithoutExtension(CurrentBook.Place)}_{index:000}-{index + pages.Count - 1:000}.png";
                    var exporter = new Exporter();
                    exporter.Initialize(pages, CurrentBook.BookReadOrder, name);
                    if (Messenger.Send(this, new MessageEventArgs("Export") { Parameter = exporter }) == true)
                    {
                        try
                        {
                            exporter.Export();
                        }
                        catch (Exception e)
                        {
                            Messenger.MessageBox(this, $"ファイル保存に失敗しました\n\n原因: {e.Message}", "エラー", System.Windows.MessageBoxButton.OK, MessageBoxExImage.Error);
                        }
                    }
                }
                catch
                {
                    Messenger.MessageBox(this, "この画像は出力できません", "警告", System.Windows.MessageBoxButton.OK, MessageBoxExImage.Warning);
                    return;
                }
            }
        }


        // ファイル削除可能？
        public bool CanDeleteFile()
        {
            var page = CurrentBook?.GetViewPage();
            if (page == null) return false;
            if (!page.IsFile()) return false;
            return (File.Exists(page.GetFilePlace()));
        }

        // ファイルを削除する
        public void DeleteFile()
        {
            if (CanDeleteFile())
            {
                var page = CurrentBook?.GetViewPage();
                var path = page.GetFilePlace();

                // ビジュアル作成
                var stackPanel = new StackPanel();
                stackPanel.Orientation = Orientation.Horizontal;
                var thumbnail = new PageVisual(page).CreateVisualContent(new System.Windows.Size(100, 100), true);
                thumbnail.Margin = new System.Windows.Thickness(0, 0, 20, 0);
                stackPanel.Children.Add(thumbnail);
                var textblock = new TextBlock();
                textblock.Text = Path.GetFileName(path);
                stackPanel.Children.Add(textblock);

                // 削除実行
                bool isRemoved = ModelContext.RemoveFile(this, path, stackPanel);

                // ページを本から削除
                if (isRemoved)
                {
                    CurrentBook?.RequestRemove(page);
                }
            }
        }

        #region Memento

        /// <summary>
        /// BookHub Memento
        /// </summary>
        [DataContract]
        public class Memento : INotifyPropertyChanged
        {
            #region NotifyPropertyChanged
            public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
                }
            }
            #endregion


            [DataMember]
            public bool IsEnableAnimatedGif { get; set; }

            [DataMember(Order = 1)]
            public bool IsEnableExif { get; set; }

            [DataMember]
            public bool IsEnableHistory { get; set; }

            [DataMember]
            public bool IsEnableNoSupportFile { get; set; }

            [DataMember]
            public bool IsEnabledAutoNextFolder { get; set; }

            [DataMember]
            public bool IsSlideShowByLoop { get; set; }

            [DataMember]
            public double SlideShowInterval { get; set; }

            [DataMember(Order = 7)]
            public bool IsCancelSlideByMouseMove { get; set; }

            [DataMember]
            public Book.Memento BookMemento { get; set; }

            [DataMember(Order = 2)]
            public bool IsEnarbleCurrentDirectory { get; set; }

            [DataMember(Order = 4)]
            public bool IsSupportArchiveFile { get; set; }

            [DataMember(Order = 4)]
            public ExternalApplication ExternalApplication { get; set; }

            [DataMember(Order = 5)]
            public bool AllowPagePreLoad { get; set; }

            [DataMember(Order = 6)]
            public bool IsConfirmRecursive { get; set; }

            [DataMember(Order = 6)]
            public Book.Memento BookMementoDefault { get; set; }

            [DataMember(Order = 6)]
            public bool IsUseBookMementoDefault { get; set; }

            [DataMember(Order = 6)]
            public bool IsRecoveryPageOnly { get; set; }

            [DataMember(Order = 9)]
            public bool IsSevenZipAccessLocked { get; set; }

            #region Property: SlideShowIntervalIndex
            private static List<int> _SlideShowIntervalTable = new List<int>()
                { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20, 30, 45, 60, 90, 120, 180, 240, 300 };

            public int SlideShowIntervalIndexMax => _SlideShowIntervalTable.Count - 1;

            public int SlideShowIntervalIndex
            {
                get
                {
                    int index = _SlideShowIntervalTable.IndexOf((int)SlideShowInterval);
                    return (index < 0) ? 0 : index;
                }
                set
                {
                    int index = NVUtility.Clamp<int>(value, 0, _SlideShowIntervalTable.Count - 1);
                    SlideShowInterval = _SlideShowIntervalTable[index];
                    OnPropertyChanged(nameof(SlideShowInterval));
                }
            }
            #endregion

            

            //
            private void Constructor()
            {
                IsEnableHistory = true;
                IsEnableNoSupportFile = false;
                IsSlideShowByLoop = true;
                SlideShowInterval = 5.0;
                IsCancelSlideByMouseMove = true;
                IsSupportArchiveFile = true;
                BookMemento = new Book.Memento();
                ExternalApplication = new ExternalApplication();
                AllowPagePreLoad = true;
                BookMementoDefault = new Book.Memento();
                IsUseBookMementoDefault = false;
                IsRecoveryPageOnly = false;
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
        }

        // memento作成
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsEnableAnimatedGif = IsEnableAnimatedGif;
            memento.IsEnableExif = IsEnableExif;
            memento.IsEnableHistory = IsEnableHistory;
            memento.IsEnableNoSupportFile = IsEnableNoSupportFile;
            memento.IsEnabledAutoNextFolder = IsEnabledAutoNextFolder;
            memento.IsSlideShowByLoop = IsSlideShowByLoop;
            memento.SlideShowInterval = SlideShowInterval;
            memento.IsCancelSlideByMouseMove = IsCancelSlideByMouseMove;
            memento.BookMemento = BookMemento.Clone();
            memento.BookMemento.ValidateForDefault(); // 念のため
            memento.IsEnarbleCurrentDirectory = IsEnarbleCurrentDirectory;
            memento.IsSupportArchiveFile = IsSupportArchiveFile;
            memento.ExternalApplication = ExternalApllication.Clone();
            memento.AllowPagePreLoad = AllowPagePreLoad;
            memento.IsConfirmRecursive = IsConfirmRecursive;
            memento.BookMementoDefault = BookMementoDefault.Clone();
            memento.BookMementoDefault.ValidateForDefault(); // 念のため
            memento.IsUseBookMementoDefault = IsUseBookMementoDefault;
            memento.IsRecoveryPageOnly = IsRecoveryPageOnly;
            memento.IsSevenZipAccessLocked = IsSevenZipAccessLocked;


            return memento;
        }

        // memento反映
        public void Restore(Memento memento)
        {
            IsEnableAnimatedGif = memento.IsEnableAnimatedGif;
            IsEnableExif = memento.IsEnableExif;
            IsEnableHistory = memento.IsEnableHistory;
            IsEnableNoSupportFile = memento.IsEnableNoSupportFile;
            IsEnabledAutoNextFolder = memento.IsEnabledAutoNextFolder;
            IsSlideShowByLoop = memento.IsSlideShowByLoop;
            SlideShowInterval = memento.SlideShowInterval;
            IsCancelSlideByMouseMove = memento.IsCancelSlideByMouseMove;
            BookMemento = memento.BookMemento.Clone();
            IsEnarbleCurrentDirectory = memento.IsEnarbleCurrentDirectory;
            IsSupportArchiveFile = memento.IsSupportArchiveFile;
            ExternalApllication = memento.ExternalApplication.Clone();
            AllowPagePreLoad = memento.AllowPagePreLoad;
            IsConfirmRecursive = memento.IsConfirmRecursive;
            BookMementoDefault = memento.BookMementoDefault.Clone();
            IsUseBookMementoDefault = memento.IsUseBookMementoDefault;
            IsRecoveryPageOnly = memento.IsRecoveryPageOnly;
            IsSevenZipAccessLocked = memento.IsSevenZipAccessLocked;
        }

        #endregion
    }


}

