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
    //
    public class RemoveFileParams
    {
        public string Path { get; set; }
        public System.Windows.FrameworkElement Visual { get; set; }
    }

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

    public enum PageEndAction
    {
        None,
        NextFolder,
        Loop,
    }

    /// <summary>
    /// 先読みモード
    /// </summary>
    public enum PreLoadMode
    {
        None, // 先読み無し
        AutoPreLoad, // 自動先読み
        PreLoad, // 固定先読み
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

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
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

        // ページマークにに追加、削除された
        public event EventHandler<PagemarkChangedEventArgs> PagemarkChanged;

        // アドレスが変更された
        public event EventHandler AddressChanged;

        // ページがソートされた
        public event EventHandler PagesSorted;

        // サムネイル変更
        public event EventHandler<Page> ThumbnailChanged;

        // ページが削除された
        public event EventHandler<Page> PageRemoved;

        // マーカーが変更された
        //public event EventHandler<PagemarkChangedEventArgs> PagemarkChanged;

        #endregion


        public void SetInfoMessage(string message)
        {
            InfoMessage?.Invoke(this, message);
        }


        // ロード中フラグ
        private bool _isLoading;


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


        // 非対応拡張子ファイルを読み込む
        private bool _isEnableNoSupportFile;
        public bool IsEnableNoSupportFile
        {
            get { return _isEnableNoSupportFile; }
            set
            {
                if (_isEnableNoSupportFile != value)
                {
                    _isEnableNoSupportFile = value;
                    ReLoad();
                }
            }
        }

        // ページ終端でのアクション
        public PageEndAction PageEndAction { get; set; }

        // 再帰を確認する
        public bool IsConfirmRecursive { get; set; }

        // 自動再帰
        #region Property: IsAutoRecursive
        private bool _isAutoRecursive = true;
        public bool IsAutoRecursive
        {
            get { return _isAutoRecursive; }
            set
            {
                _isAutoRecursive = value;
                ArchivePage.IsAutoRecursive = _isAutoRecursive;
            }
        }
        #endregion


        // スライドショー再生フラグ
        private bool _isEnableSlideShow;
        public bool IsEnableSlideShow
        {
            get
            {
                return _isEnableSlideShow;
            }
            set
            {
                _isEnableSlideShow = value;
                SlideShowModeChanged?.Invoke(this, _isEnableSlideShow);
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


        /// <summary>
        /// PreLoadMode property.
        /// </summary>
        private PreLoadMode _preLoadMode;
        public PreLoadMode PreLoadMode
        {
            get { return _preLoadMode; }
            set { if (_preLoadMode != value) { _preLoadMode = value; RaisePropertyChanged(); } }
        }



        // 現在の本
        public Book CurrentBook => Current?.Book;
        public BookUnit Current { get; private set; }

        // アドレス
        #region Property: Address
        private string _address;
        public string Address
        {
            get { return _address; }
            set { _address = value; AddressChanged?.Invoke(this, null); }
        }
        #endregion



        // 本の設定、引き継ぎ用
        public Book.Memento BookMemento { get; set; } = new Book.Memento();

        // 本の設定、標準
        public Book.Memento BookMementoDefault { get; set; } = new Book.Memento();

        // 履歴から復元する設定のフィルタ
        public BookMementoFilter HistoryMementoFilter { get; set; }

        // 新しい本を開くときに標準設定にする？
        public bool IsUseBookMementoDefault { get; set; }

        // 外部アプリ設定
        public ExternalApplication ExternalApllication { get; set; } = new ExternalApplication();

        // クリップボード設定
        public ClipboardUtility ClipboardUtility { get; set; } = new ClipboardUtility();

        // ページ表示開始スレッドイベント
        private ManualResetEvent _viewContentEvent = new ManualResetEvent(false);



        // コンストラクタ
        public BookHub()
        {
            ModelContext.BookHistory.HistoryChanged += (s, e) => HistoryChanged?.Invoke(s, e);
            ModelContext.Bookmarks.BookmarkChanged += (s, e) => BookmarkChanged?.Invoke(s, e);
            //ModelContext.Pagemarks.PagemarkChanged += (s, e) => PagemarkChanged?.Invoke(s, e);

            // messenger
            Messenger.AddReciever("RemoveFile", CallRemoveFile);

            StartCommandWorker();
        }


        // コマンド基底
        private abstract class BookHubCommand
        {
            protected BookHub _bookHub;
            public abstract int Priority { get; }

            public BookHubCommand(BookHub bookHub)
            {
                _bookHub = bookHub;
            }

            public virtual async Task Execute() { await Task.Yield(); }
        }


        // ロードコマンド 引数
        public class LoadCommandArgs
        {
            public string Path { get; set; }
            public string StartEntry { get; set; }
            public BookLoadOption Option { get; set; }
            public bool IsRefleshFolderList { get; set; }
        }

        // ロードコマンド
        private class LoadCommand : BookHubCommand
        {
            public override int Priority => 2;

            private LoadCommandArgs _args;

            //
            public LoadCommand(BookHub bookHub, string path, string start, BookLoadOption option, bool isRefleshFolderList) : base(bookHub)
            {
                _args = new LoadCommandArgs()
                {
                    Path = path,
                    StartEntry = start,
                    Option = option,
                    IsRefleshFolderList = isRefleshFolderList,
                };
            }

            //
            public LoadCommand(BookHub bookHub, LoadCommandArgs args) : base(bookHub)
            {
                _args = args;
            }


            //
            public override async Task Execute()
            {
                await _bookHub.LoadAsync(_args);
            }
        }


        // ロード
        public void RequestLoad(string path, string start, BookLoadOption option, bool isRefleshFolderList)
        {
            if (path == null) return;
            path = GetNormalizePathName(path);

            if (CurrentBook?.Place == path && (option & BookLoadOption.SkipSamePlace) == BookLoadOption.SkipSamePlace) return;

            Address = path;

            RegistCommand(new LoadCommand(this, path, start, option, isRefleshFolderList));
        }

        // ワーカータスクのキャンセルトークン
        private CancellationTokenSource _commandWorkerCancellationTokenSource;

        // 予約されているコマンド
        private BookHubCommand _readyCommand;

        // 予約コマンド存在イベント
        public AutoResetEvent _readyCommandEvent { get; private set; } = new AutoResetEvent(false);

        // 排他処理用ロックオブジェクト
        private object _lock = new object();

        // コマンドの予約
        private void RegistCommand(BookHubCommand command)
        {
            lock (_lock)
            {
                if (_readyCommand == null || _readyCommand.Priority <= command.Priority)
                {
                    _readyCommand = command;
                }
            }
            _readyCommandEvent.Set();
        }

        // ワーカータスクの起動
        private void StartCommandWorker()
        {
            _commandWorkerCancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => CommandWorker(), _commandWorkerCancellationTokenSource.Token);
        }

        // ワーカータスクの終了
        private void BreakCommandWorker()
        {
            _commandWorkerCancellationTokenSource.Cancel();
            _readyCommandEvent.Set();
        }

        // ワーカータスク
        private async void CommandWorker()
        {
            try
            {
                ////Debug.WriteLine("BookHubタスクの開始");
                while (!_commandWorkerCancellationTokenSource.Token.IsCancellationRequested)
                {
                    await Task.Run(() => _readyCommandEvent.WaitOne());
                    _commandWorkerCancellationTokenSource.Token.ThrowIfCancellationRequested();

                    BookHubCommand command;
                    lock (_lock)
                    {
                        command = _readyCommand;
                        _readyCommand = null;
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

                // ページマーク更新
                ModelContext.Pagemarks.Update(Current.BookMementoUnit, memento);
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
        private Book.Memento GetSetting(BookMementoUnit unit, string place, BookLoadOption option)
        {
            // 既定の設定
            var memento = GetBookMementoDefault().Clone();
            memento.IsRecursiveFolder = false;
            memento.Page = null;

            if (unit != null)
            {
                Book.Memento memory = null;

                // ブックマーク
                if (unit.BookmarkNode != null)
                {
                    memory = unit.Memento.Clone();
                }
                // 履歴
                else if (unit.HistoryNode != null)
                {
                    memory = unit.Memento.Clone();
                }

                if (memory != null)
                {
                    if ((option & BookLoadOption.Resume) == BookLoadOption.Resume)
                    {
                        memento = memory;
                    }
                    else
                    {
                        memento.Write(HistoryMementoFilter, memory);
                    }

                    return memento;
                }
            }

            // 履歴なし
            return memento;
        }

        // ロード中状態更新
        private void NotifyLoading(string path)
        {
            _isLoading = (path != null);
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
                string startEntry = args.Path == place ? args.StartEntry : Path.GetFileName(args.Path);

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
                var setting = GetSetting(unit, place, args.Option);

                // Load本体
                await LoadAsyncCore(place, startEntry ?? setting.Page, args.Option, setting, unit);

                // ビュー初期化
                App.Current.Dispatcher.Invoke(() => ModelContext.CommandTable[CommandType.ViewReset].Execute(this, null));

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
                RequestLoad(CurrentBook.Place, CurrentBook.StartEntry, BookLoadOption.Recursive | BookLoadOption.ReLoad, true);
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
            book.PreLoadMode = this.PreLoadMode;

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

            // 最初の自動再帰設定
            if (IsAutoRecursive)
            {
                option |= BookLoadOption.AutoRecursive;
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
                book.DartyBook += (s, e) => RequestLoad(Address, null, BookLoadOption.ReLoad, false);
                book.PagesSorted += (s, e) => PagesSorted?.Invoke(s, e);
                book.ThumbnailChanged += (s, e) => ThumbnailChanged?.Invoke(s, e);
                book.PageRemoved += OnPageRemoved;

                // 最初のコンテンツ表示待ち設定
                _viewContentEvent.Reset();
                book.ViewContentsChanged += (s, e) => _viewContentEvent.Set();

                // カレントを設定し、開始する
                Current = new BookUnit(book);
                Current.LoadOptions = option;
                Current.BookMementoUnit = unit;
                Current.Book.Start();

                // マーカー復元
                UpdatePagemark();

                // 最初のコンテンツ表示待ち
                await Task.Run(() => _viewContentEvent.WaitOne());
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
            if (_isLoading || Address == null) return;

            var options = Current != null ? (Current.LoadOptions & BookLoadOption.KeepHistoryOrder) | BookLoadOption.Resume : BookLoadOption.None;
            RequestLoad(Address, null, options, true);
        }

        // ページ終端を超えて移動しようとするときの処理
        private void OnPageTerminated(object sender, int e)
        {
            if (IsEnableSlideShow && IsSlideShowByLoop)
            {
                FirstPage();
            }

            else if (PageEndAction == PageEndAction.Loop)
            {
                if (e < 0)
                {
                    LastPage();
                }
                else
                {
                    FirstPage();
                }
            }
            else if (PageEndAction == PageEndAction.NextFolder)
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
                    InfoMessage?.Invoke(this, "最初のページです");
                }
                else
                {
                    InfoMessage?.Invoke(this, "最後のページです");
                }
            }
        }

        private void OnPageRemoved(object sender, Page e)
        {
            PageRemoved?.Invoke(sender, e);

            // ページマーカーから削除
            RemovePagemark(new Pagemark(CurrentBook.Place, e.FullPath));
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
            var count = CurrentBook == null ? 0 : CurrentBook.Pages.Count - 1;
            if (count < 0) count = 0;
            return count;
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
            if (_isLoading || ModelContext.BookHistory.Count <= 0) return;

            var unit = ModelContext.BookHistory.Find(Address);
            var previous = unit?.HistoryNode?.Next.Value; // リストと履歴の方向は逆

            if (unit == null)
            {
                RequestLoad(ModelContext.BookHistory.First?.Memento.Place, null, BookLoadOption.KeepHistoryOrder | BookLoadOption.SelectHistoryMaybe, false);
            }
            else if (previous != null)
            {
                RequestLoad(previous.Memento.Place, null, BookLoadOption.KeepHistoryOrder | BookLoadOption.SelectHistoryMaybe, false);
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
            if (_isLoading) return;

            var unit = ModelContext.BookHistory.Find(Address);
            var next = unit?.HistoryNode?.Previous; // リストと履歴の方向は逆
            if (next != null)
            {
                RequestLoad(next.Value.Memento.Place, null, BookLoadOption.KeepHistoryOrder | BookLoadOption.SelectHistoryMaybe, false);
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

        // 指定ページ数前に移動
        public void PrevSizePage(int size)
        {
            CurrentBook?.PrevPage(size);
        }

        // 指定ページ数後に移動
        public void NextSizePage(int size)
        {
            CurrentBook?.NextPage(size);
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
            if (!_isLoading && page != null) CurrentBook?.JumpPage(page);
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
            return !_isLoading && BookMemento.PageMode == mode;
        }

        // 先頭ページの単ページ表示ON/OFF 
        public void ToggleIsSupportedSingleFirstPage()
        {
            if (_isLoading) return;
            BookMemento.IsSupportedSingleFirstPage = !BookMemento.IsSupportedSingleFirstPage;
            RefleshBookSetting();
        }

        // 最終ページの単ページ表示ON/OFF 
        public void ToggleIsSupportedSingleLastPage()
        {
            if (_isLoading) return;
            BookMemento.IsSupportedSingleLastPage = !BookMemento.IsSupportedSingleLastPage;
            RefleshBookSetting();
        }

        // 横長ページの分割ON/OFF
        public void ToggleIsSupportedDividePage()
        {
            if (_isLoading) return;
            BookMemento.IsSupportedDividePage = !BookMemento.IsSupportedDividePage;
            RefleshBookSetting();
        }

        // 横長ページの見開き判定ON/OFF
        public void ToggleIsSupportedWidePage()
        {
            if (_isLoading) return;
            BookMemento.IsSupportedWidePage = !BookMemento.IsSupportedWidePage;
            RefleshBookSetting();
        }

        // フォルダ再帰読み込みON/OFF
        public void ToggleIsRecursiveFolder()
        {
            if (_isLoading) return;
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
            if (_isLoading) return;
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
            if (_isLoading) return;
            BookMemento.PageMode = BookMemento.PageMode.GetToggle();
            RefleshBookSetting();
        }

        // ページ並び変更
        public void ToggleSortMode()
        {
            if (_isLoading) return;
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

        // 既定設定を適用
        public void SetDefaultPageSetting()
        {
            BookMemento = BookMementoDefault.Clone();
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


        // クリップボードにコピー
        public void CopyToClipboard()
        {
            if (CanOpenFilePlace())
            {
                try
                {
                    ClipboardUtility.Copy(CurrentBook?.GetViewPages());
                }
                catch (Exception e)
                {
                    Messenger.MessageBox(this, $"コピーに失敗しました\n\n原因: {e.Message}", "エラー", System.Windows.MessageBoxButton.OK, MessageBoxExImage.Error);
                }
            }
        }


        // ブックマーク登録可能？
        public bool CanBookmark()
        {
            return (CurrentBook != null);
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

        // ブックマークを戻る
        public void PrevBookmark()
        {
            if (_isLoading) return;

            if (!ModelContext.Bookmarks.CanMoveSelected(-1))
            {
                InfoMessage?.Invoke(this, "前のブックマークはありません");
                return;
            }

            var unit = ModelContext.Bookmarks.MoveSelected(-1);
            if (unit != null)
            {
                RequestLoad(unit.Value.Memento.Place, null, BookLoadOption.SkipSamePlace, false);
            }
        }


        // ブックマークを進む
        public void NextBookmark()
        {
            if (_isLoading) return;

            if (!ModelContext.Bookmarks.CanMoveSelected(+1))
            {
                InfoMessage?.Invoke(this, "次のブックマークはありません");
                return;
            }

            var unit = ModelContext.Bookmarks.MoveSelected(+1);
            if (unit != null)
            {
                RequestLoad(unit.Value.Memento.Place, null, BookLoadOption.SkipSamePlace, false);
            }
        }


        // ページマーク登録可能？
        public bool CanPagemark()
        {
            return (CurrentBook != null);
        }

        // マーカー切り替え
        public void TogglePagemark()
        {
            if (_isLoading || CurrentBook == null) return;

            if (Current.Book.Place.StartsWith(Temporary.TempDirectory))
            {
                Messenger.MessageBox(this, $"一時フォルダーはページマークできません", "エラー", System.Windows.MessageBoxButton.OK, MessageBoxExImage.Error);
            }

            // マーク登録/解除
            ModelContext.Pagemarks.Toggle(new Pagemark(CurrentBook.Place, CurrentBook.GetViewPage().FullPath));

            // 更新
            UpdatePagemark();
        }

        // マーカー削除
        public void RemovePagemark(Pagemark mark)
        {
            ModelContext.Pagemarks.Remove(mark);
            UpdatePagemark(mark);
        }


        // 表示ページのマーク判定
        public bool IsMarked()
        {
            return CurrentBook != null ? CurrentBook.IsMarked(Current.Book.GetViewPage()) : false;
        }

        /// <summary>
        /// マーカー表示更新
        /// </summary>
        /// <param name="mark">変更や削除されたマーカー</param>
        public void UpdatePagemark(Pagemark mark)
        {
            // 現在ブックに影響のある場合のみ更新
            if (CurrentBook?.Place == mark.Place)
            {
                UpdatePagemark();
            }
        }


        // マーカー表示更新
        private void UpdatePagemark()
        {
            // 本にマーカを設定
            CurrentBook?.SetMarkers(ModelContext.Pagemarks.Collect(CurrentBook.Place).Select(e => e.EntryName));

            // 表示更新
            this.PagemarkChanged?.Invoke(this, null);
        }

        public bool CanPrevPagemarkInPlace(MovePagemarkCommandParameter param)
        {
            return (CurrentBook?.Markers != null && Current.Book.Markers.Count > 0) || param.IsIncludeTerminal;
        }

        public bool CanNextPagemarkInPlace(MovePagemarkCommandParameter param)
        {
            return (CurrentBook?.Markers != null && Current.Book.Markers.Count > 0) || param.IsIncludeTerminal;
        }

        // ページマークに移動
        public void PrevPagemarkInPlace(MovePagemarkCommandParameter param)
        {
            if (_isLoading || CurrentBook == null) return;
            var result = CurrentBook.RequestJumpToMarker(-1, param.IsLoop, param.IsIncludeTerminal);
            if (!result)
            {
                InfoMessage?.Invoke(this, "現在ページより前のページマークはありません");
            }
        }

        public void NextPagemarkInPlace(MovePagemarkCommandParameter param)
        {
            if (_isLoading || CurrentBook == null) return;
            var result = CurrentBook.RequestJumpToMarker(+1, param.IsLoop, param.IsIncludeTerminal);
            if (!result)
            {
                InfoMessage?.Invoke(this, "現在ページより後のページマークはありません");
            }
        }

        public void RequestLoad(Pagemark mark)
        {
            if (mark == null) return;


            if (mark.Place == CurrentBook?.Place)
            {
                Page page = CurrentBook.GetPage(mark.EntryName);
                if (page != null) JumpPage(page);
            }
            else
            {
                RequestLoad(mark.Place, mark.EntryName, BookLoadOption.None, false);
            }
        }


        public void PrevPagemark()
        {
            if (_isLoading) return;

            if (!ModelContext.Pagemarks.CanMoveSelected(-1))
            {
                InfoMessage?.Invoke(this, "前のページマークはありません");
                return;
            }

            Pagemark mark = ModelContext.Pagemarks.MoveSelected(-1);
            RequestLoad(mark);
        }

        public void NextPagemark()
        {
            if (_isLoading) return;

            if (!ModelContext.Pagemarks.CanMoveSelected(+1))
            {
                InfoMessage?.Invoke(this, "次のページマークはありません");
                return;
            }

            Pagemark mark = ModelContext.Pagemarks.MoveSelected(+1);
            RequestLoad(mark);
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
            return CanRemoveFile(CurrentBook?.GetViewPage());
        }

        // ファイルを削除する
        public async void DeleteFile()
        {
            if (CanDeleteFile())
            {
                await RemoveFile(CurrentBook?.GetViewPage());
            }
        }

        // ファイル削除可能？
        public bool CanRemoveFile(Page page)
        {
            if (page == null) return false;
            if (!page.IsFile()) return false;
            return (File.Exists(page.GetFilePlace()));
        }

        // ファイルを削除する
        public async Task RemoveFile(Page page)
        {
            if (page == null) return;

            var path = page.GetFilePlace();

            // ビジュアル作成
            var stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            var thumbnail = await new PageVisual(page).CreateVisualContentAsync(new System.Windows.Size(100, 100), true);
            if (thumbnail != null)
            {
                thumbnail.Margin = new System.Windows.Thickness(0, 0, 20, 0);
                stackPanel.Children.Add(thumbnail);
            }
            var textblock = new TextBlock();
            textblock.Text = Path.GetFileName(path);
            stackPanel.Children.Add(textblock);

            // 削除実行
            var isRemoved = Messenger.Send(this, new MessageEventArgs("RemoveFile") { Parameter = new RemoveFileParams() { Path = path, Visual = stackPanel } });

            // ページを本から削除
            if (isRemoved == true && CurrentBook != null)
            {
                CurrentBook.RequestRemove(page);
            }
        }



        // ファイルを削除する
        // RemoveFileメッセージ処理
        public void CallRemoveFile(object sender, MessageEventArgs e)
        {
            var removeParam = (RemoveFileParams)e.Parameter;
            var path = removeParam.Path;
            var visual = removeParam.Visual;

            if (visual == null)
            {
                var textblock = new System.Windows.Controls.TextBlock();
                textblock.Text = Path.GetFileName(path);

                visual = textblock;
            }

            bool isDirectory = System.IO.Directory.Exists(path);
            string itemType = isDirectory ? "フォルダ" : "ファイル";

            // 削除確認
            var param = new MessageBoxParams()
            {
                Caption = "削除の確認",
                MessageBoxText = "この" + itemType + "をごみ箱に移動しますか？",
                Button = System.Windows.MessageBoxButton.OKCancel,
                Icon = MessageBoxExImage.RecycleBin,
                VisualContent = visual,
            };
            var result = Messenger.Send(sender, new MessageEventArgs("MessageBox") { Parameter = param });

            // 削除する
            if (result == true)
            {
                try
                {
                    // 開いている本を閉じる
                    if (this.Address == path)
                    {
                        Unload(true);
                    }

                    // ゴミ箱に捨てる
                    if (isDirectory)
                    {
                        Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(path, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                    }
                    else
                    {
                        Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(path, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                    }
                }
                catch (Exception ex)
                {
                    Messenger.MessageBox(sender, $"{itemType}削除に失敗しました\n\n原因: {ex.Message}", "エラー", System.Windows.MessageBoxButton.OK, MessageBoxExImage.Error);
                }
            }

            e.Result = result == true;
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

            protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
            {
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
            #endregion

            [DataMember]
            public int _Version { get; set; }

            [DataMember]
            public bool IsEnableAnimatedGif { get; set; }

            [DataMember(Order = 1)]
            public bool IsEnableExif { get; set; }

            [DataMember]
            public bool IsEnableNoSupportFile { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public bool IsEnabledAutoNextFolder { get; set; } // no used

            [DataMember(Order = 19)]
            public PageEndAction PageEndAction { get; set; }

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

            [DataMember(Order = 5, EmitDefaultValue = false)]
            public bool AllowPagePreLoad { get; set; } // no used

            [DataMember(Order = 6)]
            public bool IsConfirmRecursive { get; set; }

            [DataMember(Order = 6)]
            public Book.Memento BookMementoDefault { get; set; }

            [DataMember(Order = 6)]
            public bool IsUseBookMementoDefault { get; set; }

            [DataMember(Order = 10)]
            public ClipboardUtility ClipboardUtility { get; set; }

            [DataMember(Order = 10)]
            public bool IsAutoRecursive { get; set; }

            [DataMember(Order = 19)]
            public BookMementoFilter HistoryMementoFilter { get; set; }

            [DataMember(Order = 19)]
            public PreLoadMode PreLoadMode { get; set; }

            //
            private void Constructor()
            {
                IsEnableNoSupportFile = false;
                IsSlideShowByLoop = true;
                SlideShowInterval = 5.0;
                IsCancelSlideByMouseMove = true;
                IsSupportArchiveFile = true;
                BookMemento = new Book.Memento();
                ExternalApplication = new ExternalApplication();
                BookMementoDefault = new Book.Memento();
                IsUseBookMementoDefault = false;
                ClipboardUtility = new ClipboardUtility();
                IsAutoRecursive = true;
                HistoryMementoFilter = new BookMementoFilter(true);
                PreLoadMode = PreLoadMode.AutoPreLoad;
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


            [OnDeserialized]
            private void Deserialized(StreamingContext c)
            {
                if (_Version < Config.GenerateProductVersionNumber(1, 19, 0))
                {
                    PageEndAction = IsEnabledAutoNextFolder ? PageEndAction.NextFolder : PageEndAction.None;
                    PreLoadMode = AllowPagePreLoad ? PreLoadMode.AutoPreLoad : PreLoadMode.None;
                }
                IsEnabledAutoNextFolder = false;
                AllowPagePreLoad = false;
            }
        }

        // memento作成
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento._Version = App.Config.ProductVersionNumber;
            memento.IsEnableAnimatedGif = IsEnableAnimatedGif;
            memento.IsEnableExif = IsEnableExif;
            memento.IsEnableNoSupportFile = IsEnableNoSupportFile;
            memento.PageEndAction = PageEndAction;
            memento.IsSlideShowByLoop = IsSlideShowByLoop;
            memento.SlideShowInterval = SlideShowInterval;
            memento.IsCancelSlideByMouseMove = IsCancelSlideByMouseMove;
            memento.BookMemento = BookMemento.Clone();
            memento.BookMemento.ValidateForDefault(); // 念のため
            memento.IsEnarbleCurrentDirectory = IsEnarbleCurrentDirectory;
            memento.IsSupportArchiveFile = IsSupportArchiveFile;
            memento.ExternalApplication = ExternalApllication.Clone();
            memento.IsConfirmRecursive = IsConfirmRecursive;
            memento.BookMementoDefault = BookMementoDefault.Clone();
            memento.BookMementoDefault.ValidateForDefault(); // 念のため
            memento.IsUseBookMementoDefault = IsUseBookMementoDefault;
            memento.ClipboardUtility = ClipboardUtility.Clone();
            memento.IsAutoRecursive = IsAutoRecursive;
            memento.HistoryMementoFilter = HistoryMementoFilter;
            memento.PreLoadMode = PreLoadMode;

            return memento;
        }

        // memento反映
        public void Restore(Memento memento)
        {
            IsEnableAnimatedGif = memento.IsEnableAnimatedGif;
            IsEnableExif = memento.IsEnableExif;
            IsEnableNoSupportFile = memento.IsEnableNoSupportFile;
            PageEndAction = memento.PageEndAction;
            IsSlideShowByLoop = memento.IsSlideShowByLoop;
            SlideShowInterval = memento.SlideShowInterval;
            IsCancelSlideByMouseMove = memento.IsCancelSlideByMouseMove;
            BookMemento = memento.BookMemento.Clone();
            IsEnarbleCurrentDirectory = memento.IsEnarbleCurrentDirectory;
            IsSupportArchiveFile = memento.IsSupportArchiveFile;
            ExternalApllication = memento.ExternalApplication.Clone();
            IsConfirmRecursive = memento.IsConfirmRecursive;
            BookMementoDefault = memento.BookMementoDefault.Clone();
            IsUseBookMementoDefault = memento.IsUseBookMementoDefault;
            ClipboardUtility = memento.ClipboardUtility.Clone();
            IsAutoRecursive = memento.IsAutoRecursive;
            HistoryMementoFilter = memento.HistoryMementoFilter;
            PreLoadMode = memento.PreLoadMode;
        }

        #endregion
    }
}

