using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Diagnostics;
using NeeView.IO;
using NeeView.Threading.Tasks;
using NeeView.Windows.Property;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// TODO: コマンド類の何時でも受付。ロード中だから弾く、ではない別の方法を。

namespace NeeView
{
    #region Enums

    public enum BookMementoType
    {
        None,
        Bookmark,
        History,
    }

    /// <summary>
    /// ページが終わったときのアクション
    /// </summary>
    public enum PageEndAction
    {
        [AliasName("@EnumPageEndActionNone")]
        None,

        [AliasName("@EnumPageEndActionNextFolder")]
        NextFolder,

        [AliasName("@EnumPageEndActionLoop")]
        Loop,
    }


    /// <summary>
    /// 先読みモード
    /// </summary>
    [Obsolete]
    public enum PreLoadMode
    {
        [AliasName("@EnumPreLoadModeNone")]
        None,

        [AliasName("@EnumPreLoadModeAutoPreLoad")]
        AutoPreLoad,

        [AliasName("@EnumPreLoadModePreLoad")]
        PreLoad,

        [AliasName("@EnumPreLoadModePreLoadNoUnload")]
        PreLoadNoUnload,
    }

    #endregion Enums

    #region EventArgs

    /// <summary>
    /// フォルダーリスト更新イベントパラメーター
    /// </summary>
    public class FolderListSyncEventArgs : EventArgs
    {
        /// <summary>
        /// フォルダーリストで選択されて欲しい項目のパス
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// フォルダーリストの場所。アーカイブパス用。
        /// nullの場合Pathから求められる。
        /// </summary>
        public string Parent { get; set; }

        /// <summary>
        /// なるべくリストの選択項目を変更しないようにする
        /// </summary>
        public bool isKeepPlace { get; set; }
    }

    public class BookChangedEventArgs : EventArgs
    {
        public BookChangedEventArgs(BookMementoType type)
        {
            BookMementoType = type;
        }

        public BookMementoType BookMementoType { get; set; }
    }

    public class BookHubPathEventArgs : EventArgs
    {
        public BookHubPathEventArgs(string path)
        {
            Path = path;
        }

        public string Path { get; set; }
    }

    public class BookHubMessageEventArgs : EventArgs
    {
        public BookHubMessageEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }


    #endregion EventArgs

    /// <summary>
    /// Bookとその付属情報の管理
    /// </summary>
    public class BookUnit
    {
        public BookUnit(Book book, BookAddress bookAddress, BookLoadOption loadOptions)
        {
            Book = book;
            BookAddress = bookAddress;
            LoadOptions = loadOptions;
        }

        public Book Book { get; private set; }
        public BookAddress BookAddress { get; private set; }
        public BookLoadOption LoadOptions { get; private set; }

        public bool IsKeepHistoryOrder
            => (LoadOptions & BookLoadOption.KeepHistoryOrder) == BookLoadOption.KeepHistoryOrder;

        public bool IsValid
            => Book?.Address != null;

        public BookMementoType GetBookMementoType()
        {
            if (BookmarkCollection.Current.Contains(Book.Address))
            {
                return BookMementoType.Bookmark;
            }
            else if (BookHistoryCollection.Current.Contains(Book.Address))
            {
                return BookMementoType.History;
            }
            else
            {
                return BookMementoType.None;
            }
        }
    }


    /// <summary>
    /// 本の管理
    /// ロード、本の操作はここを通す
    /// </summary>
    public sealed class BookHub : BindableBase, IDisposable
    {
        // Singleton
        static BookHub() => Current = new BookHub();
        public static BookHub Current { get; }

        #region NormalizePathName

        internal static class NativeMethods
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern int GetLongPathName(string shortPath, StringBuilder longPath, int longPathLength);
        }

        // パス名の正規化
        private static string GetNormalizePathName(string source)
        {
            // 区切り文字修正
            source = new System.Text.RegularExpressions.Regex(@"[/\\]").Replace(source, "\\").TrimEnd('\\');

            // ドライブレター修正
            source = new System.Text.RegularExpressions.Regex(@"^[a-z]:").Replace(source, m => m.Value.ToUpper());
            source = new System.Text.RegularExpressions.Regex(@":$").Replace(source, ":\\");

            StringBuilder longPath = new StringBuilder(1024);
            if (0 == NativeMethods.GetLongPathName(source, longPath, longPath.Capacity))
            {
                return source;
            }

            string dist = longPath.ToString();
            return longPath.ToString();
        }

        #endregion

        #region Fields

        private Toast _bookHubToast;
        private bool _isLoading;
        private bool _isAutoRecursive = false;
        private ArchiveEntryCollectionMode _archiveRecursiveMode = ArchiveEntryCollectionMode.IncludeSubArchives;
        private BookUnit _bookUnit;
        private string _address;
        private BookHubCommandEngine _commandEngine;
        private bool _historyEntry;
        private bool _historyRemoved;
        private volatile int _requestLoadCount;
        private object _lock = new object();

        #endregion Fields

        #region Constructors

        private BookHub()
        {
            this.BookChanged +=
                (s, e) =>
                {
                    if (this.Book?.NotFoundStartPage != null && this.Book.Pages.Count > 0)
                    {
                        InfoMessage.Current.SetMessage(InfoMessageType.BookName, string.Format(Properties.Resources.NotifyCannotOpen, LoosePath.GetFileName(this.Book.NotFoundStartPage)), null, 2.0);
                    }
                    else
                    {
                        InfoMessage.Current.SetMessage(InfoMessageType.BookName, LoosePath.GetFileName(Address), null, 2.0, e.BookMementoType);
                    }
                };

            BookHistoryCollection.Current.HistoryChanged += BookHistoryCollection_HistoryChanged;

            BookmarkCollection.Current.BookmarkChanged += (s, e) => BookmarkChanged?.Invoke(s, e);

            // command engine
            _commandEngine = new BookHubCommandEngine();
            _commandEngine.Name = "BookHubJobEngine";
            _commandEngine.Log = new Log(nameof(BookHubCommandEngine), 0);

            Start();

            // アプリ終了前の開放予約
            ApplicationDisposer.Current.Add(this);
        }

        #endregion Constructors

        #region Events

        // 本の変更通知
        public event EventHandler BookChanging;
        public event EventHandler<BookChangedEventArgs> BookChanged;

        // 新しいロードリクエスト
        public event EventHandler<BookHubPathEventArgs> LoadRequested;

        // ロード中通知
        public event EventHandler<BookHubPathEventArgs> Loading;

        // ViewContentsの変更通知
        public event EventHandler<ViewPageCollectionChangedEventArgs> ViewContentsChanged;

        // NextContentsの変更通知
        public event EventHandler<ViewPageCollectionChangedEventArgs> NextContentsChanged;

        // 空ページメッセージ
        public event EventHandler<BookHubMessageEventArgs> EmptyMessage;

        // フォルダー列更新要求
        public event EventHandler<FolderListSyncEventArgs> FolderListSync;

        // 履歴リスト更新要求
        public event EventHandler<BookHubPathEventArgs> HistoryListSync;

        // 履歴に追加、削除された
        public event EventHandler<BookMementoCollectionChangedArgs> HistoryChanged;

        // ブックマークにに追加、削除された
        public event EventHandler<BookmarkCollectionChangedEventArgs> BookmarkChanged;

        // アドレスが変更された
        public event EventHandler AddressChanged;

        #endregion

        #region Properties

        // 再帰を確認する
        [PropertyMember("@ParamIsConfirmRecursive", Tips = "@ParamIsConfirmRecursiveTips")]
        public bool IsConfirmRecursive { get; set; }

        // 自動再帰
        [PropertyMember("@ParamIsAutoRecursive")]
        public bool IsAutoRecursive
        {
            get { return _isAutoRecursive; }
            set { SetProperty(ref _isAutoRecursive, value); }
        }

        /// <summary>
        /// アーカイブの展開モード
        /// </summary>
        [PropertyMember("@ParamArchiveRecursiveMode", Tips = "@ParamArchiveRecursiveModeTips")]
        public ArchiveEntryCollectionMode ArchiveRecursiveMode
        {
            get { return _archiveRecursiveMode; }
            set { SetProperty(ref _archiveRecursiveMode, value); }
        }

        /// <summary>
        /// アーカイブ内アーカイブの履歴保存
        /// </summary>
        [PropertyMember("@ParamIsInnerArchiveHistoryEnabled")]
        public bool IsInnerArchiveHistoryEnabled { get; set; } = true;

        /// <summary>
        /// UNCパスの履歴保存
        /// </summary>
        [PropertyMember("@ParamIsUncHistoryEnabled", Tips = "@ParamIsUncHistoryEnabledTips")]
        public bool IsUncHistoryEnabled { get; set; } = true;

        /// <summary>
        /// 履歴閲覧でも履歴登録日を更新する
        /// </summary>
        [PropertyMember("@ParamIsForceUpdateHistory")]
        public bool IsForceUpdateHistory { get; set; }

        /// <summary>
        /// 何回ページを切り替えたら履歴登録するか
        /// </summary>
        [PropertyMember("@ParamHistoryEntryPageCount", Tips = "@ParamHistoryEntryPageCountTips")]
        public int HistoryEntryPageCount { get; set; } = 0;

        /// <summary>
        /// 現在の本
        /// </summary>
        public BookUnit BookUnit
        {
            get { return _bookUnit; }
            set { SetProperty(ref _bookUnit, value); }
        }

        // 現在の本
        public Book Book => BookUnit?.Book;

        // アドレス
        public string Address
        {
            get { return _address; }
            set
            {
                _address = value;
                AddressChanged?.Invoke(this, null);
            }
        }

        /// <summary>
        /// ロード可能フラグ
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// ロード中フラグ
        /// </summary>
        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

        public bool IsBusy => _commandEngine.Count > 0;

        /// <summary>
        /// ロードリクエストカウント.
        /// 名前変更時の再読込判定に使用される
        /// </summary>
        public int RequestLoadCount => _requestLoadCount;

        #endregion Properties

        #region Callback Methods

        private void BookHistoryCollection_HistoryChanged(object sender, BookMementoCollectionChangedArgs e)
        {
            HistoryChanged?.Invoke(sender, e);

            lock (_lock)
            {
                if (this.BookUnit == null) return;

                // 履歴削除されたものを履歴登録しないようにする
                if (e.HistoryChangedType == BookMementoCollectionChangedType.Remove && this.BookUnit.Book.Address == e.Key)
                {
                    _historyRemoved = true;
                }
            }
        }

        private void OnViewContentsChanged(object sender, ViewPageCollectionChangedEventArgs e)
        {
            bool allowUpdateHistory;
            lock (_lock)
            {
                if (BookUnit == null) return;
                allowUpdateHistory = !BookUnit.IsKeepHistoryOrder || IsForceUpdateHistory;
            }

            // 履歴更新
            if (allowUpdateHistory && !_historyEntry && CanHistory())
            {
                _historyEntry = true;
                BookHistoryCollection.Current.Add(Book?.CreateMemento(), false);
            }

            lock (_lock)
            {
                if (BookUnit == null) return;
                BookUnit?.Book.UpdateViewPages(sender, e);
            }

            ViewContentsChanged?.Invoke(sender, e);
        }

        private void OnNextContentsChanged(object sender, ViewPageCollectionChangedEventArgs e)
        {
            if (BookUnit == null) return;
            NextContentsChanged?.Invoke(sender, e);
        }

        #endregion Callback Methods

        #region Engine Control

        public void Start()
        {
            _commandEngine.StartEngine();
        }

        public void Stop()
        {
            if (_disposedValue) return;

            // いろんなイベントをクリア
            this.AddressChanged = null;
            this.BookChanging = null;
            this.BookChanged = null;
            this.BookmarkChanged = null;
            this.EmptyMessage = null;
            this.FolderListSync = null;
            this.HistoryChanged = null;
            this.HistoryListSync = null;
            this.Loading = null;

            ResetPropertyChanged();

            Debug.WriteLine("BookHub Disposing...");

            // 開いているブックを閉じる(5秒待つ。それ以上は待たない)
            Task.Run(async () => await RequestUnload(false).WaitAsync()).Wait(5000);

            // コマンドエンジン停止
            _commandEngine.StopEngine();

            Debug.WriteLine("BookHub Disposed.");
        }

        #endregion Engine Control

        #region Requests

        /// <summary>
        /// リクエスト：フォルダーを開く
        /// </summary>
        /// <param name="path">開く場所</param>
        /// <param name="start">ページの指定</param>
        /// <param name="option"></param>
        /// <param name="isRefreshFolderList">フォルダーリストを同期する？</param>
        /// <returns></returns>
        public BookHubCommandLoad RequestLoad(string path, string start, BookLoadOption option, bool isRefreshFolderList)
        {
            if (!this.IsEnabled) return null;

            if (path == null) return null;

            path = LoosePath.NormalizeSeparator(path);

            if (FileShortcut.IsShortcut(path) && (System.IO.File.Exists(path) || System.IO.Directory.Exists(path)))
            {
                var shortcut = new FileShortcut(path);
                path = shortcut.TargetPath;
            }

            path = GetNormalizePathName(path);

            ////DebugTimer.Start($"\nStart: {path}");

            LoadRequested?.Invoke(this, new BookHubPathEventArgs(path));

            if (Book?.Address == path && option.HasFlag(BookLoadOption.SkipSamePlace)) return null;

            Address = path;

            _requestLoadCount++;

            if ((option & (BookLoadOption.IsBook | BookLoadOption.IsPage)) == 0)
            {
                option &= ~BookLoadOption.IsPage;
                option |= BookLoadOption.IsBook;
            }

            var command = new BookHubCommandLoad(this, new BookHubCommandLoadArgs()
            {
                Path = path,
                StartEntry = start,
                Option = option,
                IsRefreshFolderList = isRefreshFolderList
            });

            _commandEngine.Enqueue(command);

            return command;
        }


        // アンロード可能?
        public bool CanUnload()
        {
            return BookUnit != null || _commandEngine.Count > 0;
        }

        /// <summary>
        /// リクエスト：フォルダーを閉じる
        /// </summary>
        /// <param name="isClearViewContent"></param>
        /// <returns></returns>
        public BookHubCommandUnload RequestUnload(bool isClearViewContent, string message = null)
        {
            var command = new BookHubCommandUnload(this, new BookHubCommandUnloadArgs()
            {
                IsClearViewContent = isClearViewContent,
                Message = message
            });

            _commandEngine.Enqueue(command);

            // ルーペモードは解除
            if (MouseInput.Current.IsLoupeMode)
            {
                MouseInput.Current.IsLoupeMode = false;
            }

            return command;
        }


        // 再読込可能？
        public bool CanReload()
        {
            return (!string.IsNullOrWhiteSpace(Address));
        }

        /// <summary>
        /// リクエスト：再読込
        /// </summary>
        public void RequestReLoad()
        {
            if (_isLoading || Address == null) return;

            BookLoadOption options;
            lock (_lock)
            {
                options = BookUnit != null ? (BookUnit.LoadOptions & BookLoadOption.KeepHistoryOrder) | BookLoadOption.Resume : BookLoadOption.None;
            }
            RequestLoad(Address, null, options | BookLoadOption.IsBook, true);
        }

        // 上の階層に移動可能？
        public bool CanLoadParent()
        {
            var parent = BookUnit?.BookAddress?.Place;
            return parent?.Path != null && parent.Scheme == QueryScheme.File;
        }

        /// <summary>
        /// リクエスト：上の階層に移動
        /// </summary>
        public void RequestLoadParent()
        {
            var parent = BookUnit?.BookAddress?.Place;
            if (parent?.Path != null && parent.Scheme == QueryScheme.File)
            {
                var option = BookLoadOption.IsBook | BookLoadOption.SkipSamePlace;
                RequestLoad(parent.SimplePath, null, option, true);
            }
        }

        #endregion Requests

        #region BookHubCommand.Load

        // ロード中状態更新
        private void NotifyLoading(string path)
        {
            this.IsLoading = (path != null);
            AppDispatcher.Invoke(() => Loading?.Invoke(this, new BookHubPathEventArgs(path)));
        }

        /// <summary>
        /// 本を読み込む
        /// </summary>
        /// <param name="path"></param>
        /// <param name="option"></param>
        /// <param name="isRefleshFolderList"></param>
        /// <returns></returns>
        public async Task LoadAsync(BookHubCommandLoadArgs args, CancellationToken token)
        {
            ////DebugTimer.Check("LoadAsync...");

            AppDispatcher.Invoke(() =>
            {
                // 再生中のメディアをPAUSE
                MediaPlayerOperator.Current?.Pause();

                // 本の変更開始通知
                BookChanging?.Invoke(this, null);
            });

            // 現在の設定を記憶
            var lastBookMemento = this.Book?.Address != null ? this.Book.CreateMemento() : null;

            // 現在の本を開放
            await UnloadAsync(new BookHubCommandUnloadArgs() { IsClearViewContent = false });

            string place = args.Path;

            if (_bookHubToast != null)
            {
                _bookHubToast.Cancel();
                _bookHubToast = null;
            }

            try
            {
                // address
                var address = await BookAddress.CreateAsync(new QueryPath(args.Path), args.StartEntry, this.ArchiveRecursiveMode, args.Option, token);

                // Now Loading ON
                NotifyLoading(args.Path);

                // 履歴リスト更新
                if ((args.Option & BookLoadOption.SelectHistoryMaybe) != 0)
                {
                    AppDispatcher.Invoke(() => HistoryListSync?.Invoke(this, new BookHubPathEventArgs(address.Address.SimplePath)));
                }

                // 本の設定
                var memory = BookSetting.Current.CreateLastestBookMemento(address.Address.SimplePath, lastBookMemento);
                var setting = BookSetting.Current.GetSetting(address.Address.SimplePath, memory, args.Option);

                address.EntryName = address.EntryName ?? LoosePath.NormalizeSeparator(setting.Page);
                place = address.SystemPath;

                // 移動履歴登録
                BookHubHistory.Current.Add(address.Address);

                // フォルダーリスト更新
                if (args.IsRefreshFolderList)
                {
                    AppDispatcher.Invoke(() => FolderListSync?.Invoke(this, new FolderListSyncEventArgs() { Path = address.Address.SimplePath, Parent = address.Place.SimplePath, isKeepPlace = false }));
                }
                else if ((args.Option & BookLoadOption.SelectFoderListMaybe) != 0)
                {
                    AppDispatcher.Invoke(() => FolderListSync?.Invoke(this, new FolderListSyncEventArgs() { Path = address.Address.SimplePath, Parent = address.Place.SimplePath, isKeepPlace = true }));
                }

                // Load本体
                await LoadAsyncCore(address, args.Option, setting, token);

                ////DebugTimer.Check("LoadCore");

                AppDispatcher.Invoke(() =>
                {
                    // ビュー初期化
                    CommandTable.Current[CommandType.ViewReset].Execute(this, null);

                    // 本の設定を退避
                    BookSetting.Current.RaiseSettingChanged();

                    // 本の変更通知
                    BookChanged?.Invoke(this, new BookChangedEventArgs(BookUnit.GetBookMementoType()));
                });

                // ページがなかった時の処理
                if (Book.Pages.Count <= 0)
                {
                    ResetBookMementoPage(Book.Address);

                    AppDispatcher.Invoke(() =>
                    {
                        EmptyMessage?.Invoke(this, new BookHubMessageEventArgs(string.Format(Properties.Resources.NotifyNoPages, Book.Address)));

                        if (IsConfirmRecursive && (args.Option & BookLoadOption.ReLoad) == 0 && !Book.IsRecursiveFolder && Book.SubFolderCount > 0)
                        {
                            ConfirmRecursive();
                        }
                    });
                }
            }
            catch (OperationCanceledException)
            {
                // nop.
            }
            catch (Exception ex)
            {
                if (ex is BookAddressException)
                {
                    EmptyMessage?.Invoke(this, new BookHubMessageEventArgs(ex.Message));
                }
                else
                {
                    // ファイル読み込み失敗通知
                    var message = string.Format(Properties.Resources.ExceptionLoadFailed, place, ex.Message);
                    EmptyMessage?.Invoke(this, new BookHubMessageEventArgs(message));
                }

                // for .heic / .heif
                if (ex is NotSupportedFileTypeException exc && (exc.Extension == ".heic" || exc.Extension == ".heif") && Config.Current.IsWindows10())
                {
                    _bookHubToast = new Toast(Properties.Resources.NotifyHeifHelp, null, ToastIcon.Information, App.Current.IsNetworkEnabled ? Properties.Resources.WordOpenStore : null, () =>
                     {
                         System.Diagnostics.Process.Start(PictureProfile.HEIFImageExtensions.OriginalString);
                     });
                    ToastService.Current.Show(_bookHubToast);
                }

                AppDispatcher.Invoke(() =>
                {
                    // 現在表示されているコンテンツを無効
                    ViewContentsChanged?.Invoke(this, new ViewPageCollectionChangedEventArgs(new ViewPageCollection()));

                    // 本の変更通知
                    BookChanged?.Invoke(this, new BookChangedEventArgs(BookMementoType.None));

                    // 履歴リスト更新
                    if ((args.Option & BookLoadOption.SelectHistoryMaybe) != 0)
                    {
                        HistoryListSync?.Invoke(this, new BookHubPathEventArgs(Address));
                    }
                });
            }
            finally
            {
                // Now Loading OFF
                NotifyLoading(null);

                ////DebugTimer.Check("Done.");
            }
        }


        // 再帰読み込み確認
        private void ConfirmRecursive()
        {
            var dialog = new MessageDialog(string.Format(Properties.Resources.DialogConfirmRecursive, Book.Address), Properties.Resources.DialogConfirmRecursiveTitle);
            dialog.Commands.Add(UICommands.Yes);
            dialog.Commands.Add(UICommands.No);
            var result = dialog.ShowDialog();

            if (result == UICommands.Yes)
            {
                RequestLoad(Book.Address, Book.StartEntry, BookLoadOption.Recursive | BookLoadOption.ReLoad, true);
            }
        }


        /// <summary>
        /// 本を読み込む(本体)
        /// </summary>
        /// <param name="path">本のパス</param>
        /// <param name="startEntry">開始エントリ</param>
        /// <param name="option">読み込みオプション</param>
        private async Task LoadAsyncCore(BookAddress address, BookLoadOption option, Book.Memento setting, CancellationToken token)
        {
            // 新しい本を作成
            var book = new Book();

            // 設定の復元
            if ((option & BookLoadOption.ReLoad) == BookLoadOption.ReLoad)
            {
                // リロード時は設定そのまま
                book.Restore(BookSetting.Current.BookMemento);
            }
            else
            {
                book.Restore(setting);
            }

            // 最初の自動再帰設定
            if (IsAutoRecursive)
            {
                option |= BookLoadOption.AutoRecursive;
            }

            try
            {
                var bookSetting = new BookLoadSetting();
                bookSetting.Options = option;
                bookSetting.BookPageCollectMode = BookProfile.Current.BookPageCollectMode;

                // ロード。非同期で行う
                await book.LoadAsync(address, ArchiveRecursiveMode, bookSetting, token);

                _historyEntry = false;
                _historyRemoved = false;

                // カレントを設定し、開始する
                lock (_lock)
                {
                    BookUnit = new BookUnit(book, address, option);
                }

                // イベント設定
                book.ViewContentsChanged += OnViewContentsChanged;
                book.NextContentsChanged += OnNextContentsChanged;
                book.DartyBook += (s, e) => RequestLoad(Address, null, BookLoadOption.ReLoad | BookLoadOption.IsBook, false);

                // 開始
                BookUnit.Book.Start();

                // 最初のコンテンツ表示待ち
                if (book.Pages.Count > 0)
                {
                    //await Task.Run(() => book.ContentLoaded.Wait(token));
                    await TaskUtils.ActionAsync(() => book.ContentLoaded.Wait(token), token);
                }
            }
            catch (OperationCanceledException)
            {
                // 後始末
                lock (_lock)
                {
                    BookUnit?.Book?.Dispose();
                    BookUnit = null;
                }
                BookHistoryCollection.Current.LastAddress = null;

                throw;
            }
            catch (Exception)
            {
                // 後始末
                lock (_lock)
                {
                    BookUnit?.Book?.Dispose();
                    BookUnit = null;
                }
                BookHistoryCollection.Current.LastAddress = null;

                // 履歴から消去
                ////BookHistory.Current.Remove(address.Place);
                ////MenuBar.Current.UpdateLastFiles();

                throw;
            }

            // 本の設定を退避
            BookSetting.Current.BookMemento = this.Book.CreateMemento();

            Address = BookUnit.Book.Address;
            BookHistoryCollection.Current.LastAddress = Address;
        }

        #endregion BookHubCommand.Load

        #region BookHubCommand.Unload

        /// <summary>
        /// 本の開放
        /// </summary>
        public async Task UnloadAsync(BookHubCommandUnloadArgs param)
        {
            // 履歴の保存
            SaveBookMemento();

            // 現在の本を開放
            await ReleaseCurrentAsync();

            if (param.IsClearViewContent)
            {
                Address = null;
                BookHistoryCollection.Current.LastAddress = null;

                AppDispatcher.Invoke(() =>
                {
                    // 現在表示されているコンテンツを無効
                    ViewContentsChanged?.Invoke(this, null);

                    // 本の変更通知
                    BookChanged?.Invoke(this, new BookChangedEventArgs(BookMementoType.None));
                });
            }

            if (param.Message != null)
            {
                // TODO: 参照方向がおかしい
                ContentCanvas.Current.EmptyPageMessage = param.Message;
                ContentCanvas.Current.IsVisibleEmptyPageMessage = true;
            }
        }

        // 現在の本を解除
        private async Task ReleaseCurrentAsync()
        {
            var book = BookUnit?.Book;

            lock (_lock)
            {
                BookUnit = null;
            }

            if (book != null)
            {
                await book.DisposeAsync();
            }
        }

        #endregion BookHubCommand.Unload

        #region BookMemento Control

        //現在開いているブックの設定作成
        public Book.Memento CreateBookMemento()
        {
            lock (_lock)
            {
                return (BookUnit != null && BookUnit.Book.Pages.Count > 0) ? BookUnit.Book.CreateMemento() : null;
            }
        }

        // 設定の読込
        private Book.Memento LoadBookMemento(string place)
        {
            var unit = BookMementoCollection.Current.GetValid(place);
            return unit?.Memento;
        }

        //設定の保存
        public void SaveBookMemento()
        {
            var memento = CreateBookMemento();
            if (memento == null) return;

            bool isKeepHistoryOrder;
            lock (_lock)
            {
                if (BookUnit == null) return;
                isKeepHistoryOrder = BookUnit.IsKeepHistoryOrder || IsForceUpdateHistory;
            }
            SaveBookMemento(memento, isKeepHistoryOrder);
        }

        private void SaveBookMemento(Book.Memento memento, bool isKeepHistoryOrder)
        {
            if (memento == null) return;

            // 情報更新
            var unit = BookMementoCollection.Current.Get(memento.Place);
            if (unit != null)
            {
                unit.Memento = memento;
            }

            // 履歴の保存
            if (CanHistory())
            {
                BookHistoryCollection.Current.Add(memento, isKeepHistoryOrder);
            }
        }

        // 記録のページのみクリア
        private void ResetBookMementoPage(string place)
        {
            var unit = BookMementoCollection.Current.GetValid(place);
            if (unit?.Memento != null)
            {
                unit.Memento.Page = null;
            }
        }

        /// <summary>
        /// 最新の本の設定を取得
        /// </summary>
        /// <param name="address">場所</param>
        /// <param name="option"></param>
        /// <returns></returns>
        public Book.Memento GetLastestBookMemento(string address, BookLoadOption option)
        {
            if (address != null)
            {
                var bookMemento = this.Book?.Address == address ? this.Book.CreateMemento() : null;
                var memory = BookSetting.Current.CreateLastestBookMemento(address, bookMemento);
                return BookSetting.Current.GetSetting(address, memory, option);
            }
            else
            {
                return BookSetting.Current.GetSetting(null, null, option);
            }
        }

        // 履歴登録可
        private bool CanHistory()
        {
            // 履歴閲覧時の履歴更新は最低１操作を必要とする
            var historyEntryPageCount = this.HistoryEntryPageCount;
            if (BookUnit.IsKeepHistoryOrder && IsForceUpdateHistory && historyEntryPageCount <= 0)
            {
                historyEntryPageCount = 1;
            }

            return Book != null
                && !_historyRemoved
                && Book.Pages.Count > 0
                && (_historyEntry || Book.PageChangeCount > historyEntryPageCount || Book.IsPageTerminated)
                && (IsInnerArchiveHistoryEnabled || Book.ArchiveEntryCollection.Archiver?.Parent == null)
                && (IsUncHistoryEnabled || !LoosePath.IsUnc(Book.Address));
        }

        #endregion BookMemento Control

        #region IDisposable Support
        private bool _disposedValue = false;

        void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Stop();
                    _commandEngine.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        #region Memento

        /// <summary>
        /// BookHub Memento
        /// </summary>
        [DataContract]
        public class Memento : BindableBase
        {
            [DataMember]
            public int _Version { get; set; } = Config.Current.ProductVersionNumber;

            [DataMember(Order = 6)]
            public bool IsConfirmRecursive { get; set; }

            [DataMember(Order = 10)]
            public bool IsAutoRecursive { get; set; }

            [DataMember, DefaultValue(0)]
            public int HistoryEntryPageCount { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsInnerArchiveHistoryEnabled { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsUncHistoryEnabled { get; set; }

            [DataMember]
            public bool IsForceUpdateHistory { get; set; }

            [DataMember, DefaultValue(ArchiveEntryCollectionMode.IncludeSubArchives)]
            public ArchiveEntryCollectionMode ArchiveRecursveMode { get; set; }

            #region Obslete

            [Obsolete, DataMember(Order = 22)]
            public bool IsAutoRecursiveWithAllFiles { get; set; } // no used (ver.34)

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsArchiveRecursive { get; set; } // no used (ver.34)

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsEnableAnimatedGif { get; set; }

            [Obsolete, DataMember(Order = 1, EmitDefaultValue = false)]
            public bool IsEnableExif { get; set; }

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsEnableNoSupportFile { get; set; }

            [Obsolete, DataMember(Order = 19, EmitDefaultValue = false)]
            public PreLoadMode PreLoadMode { get; set; }

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsEnabledAutoNextFolder { get; set; } // no used

            [Obsolete, DataMember(Order = 19, EmitDefaultValue = false)]
            public PageEndAction PageEndAction { get; set; } // no used (ver.23)

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsSlideShowByLoop { get; set; } // no used (ver.22)

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public double SlideShowInterval { get; set; } // no used (ver.22)

            [Obsolete, DataMember(Order = 7, EmitDefaultValue = false)]
            public bool IsCancelSlideByMouseMove { get; set; } // no used (ver.22)

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public Book.Memento BookMemento { get; set; } // no used (v.23)

            [Obsolete, DataMember(Order = 2, EmitDefaultValue = false)]
            public bool IsEnarbleCurrentDirectory { get; set; } // no used

            [Obsolete, DataMember(Order = 4, EmitDefaultValue = false)]
            public bool IsSupportArchiveFile { get; set; } // no used (v.23)

            [Obsolete, DataMember(Order = 4, EmitDefaultValue = false)]
            public ExternalApplication ExternalApplication { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 5, EmitDefaultValue = false)]
            public bool AllowPagePreLoad { get; set; } // no used

            [Obsolete, DataMember(Order = 6, EmitDefaultValue = false)]
            public Book.Memento BookMementoDefault { get; set; } // no used (v.23)

            [Obsolete, DataMember(Order = 6, EmitDefaultValue = false)]
            public bool IsUseBookMementoDefault { get; set; } // no used (v.23)

            [Obsolete, DataMember(Order = 10, EmitDefaultValue = false)]
            public ClipboardUtility ClipboardUtility { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 19, EmitDefaultValue = false)]
            public BookMementoFilter HistoryMementoFilter { get; set; } // no used (v.23)

            [Obsolete, DataMember(Order = 20, EmitDefaultValue = false)]
            public string Home { get; set; } // no used (ver.23)

            #endregion

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

#pragma warning disable CS0612

            [OnDeserialized]
            private void Deserialized(StreamingContext c)
            {
                // before 1.19
                if (_Version < Config.GenerateProductVersionNumber(1, 19, 0))
                {
                    PageEndAction = IsEnabledAutoNextFolder ? PageEndAction.NextFolder : PageEndAction.None;
                    PreLoadMode = AllowPagePreLoad ? PreLoadMode.AutoPreLoad : PreLoadMode.None;
                }
                IsEnabledAutoNextFolder = false;
                AllowPagePreLoad = false;

                // before 34.0
                if (_Version < Config.GenerateProductVersionNumber(34, 0, 0))
                {
                    ArchiveRecursveMode = IsArchiveRecursive ? ArchiveEntryCollectionMode.IncludeSubArchives : ArchiveEntryCollectionMode.IncludeSubDirectories;
                }
            }

#pragma warning restore CS0612

        }

        // memento作成
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento._Version = Config.Current.ProductVersionNumber;

            memento.IsConfirmRecursive = IsConfirmRecursive;
            memento.IsAutoRecursive = IsAutoRecursive;
            memento.HistoryEntryPageCount = HistoryEntryPageCount;
            memento.IsInnerArchiveHistoryEnabled = IsInnerArchiveHistoryEnabled;
            memento.IsUncHistoryEnabled = IsUncHistoryEnabled;
            memento.IsForceUpdateHistory = IsForceUpdateHistory;
            memento.ArchiveRecursveMode = ArchiveRecursiveMode;

            return memento;
        }

        // memento反映
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            IsConfirmRecursive = memento.IsConfirmRecursive;
            IsAutoRecursive = memento.IsAutoRecursive;
            HistoryEntryPageCount = memento.HistoryEntryPageCount;
            IsInnerArchiveHistoryEnabled = memento.IsInnerArchiveHistoryEnabled;
            IsUncHistoryEnabled = memento.IsUncHistoryEnabled;
            IsForceUpdateHistory = memento.IsForceUpdateHistory;
            ArchiveRecursiveMode = memento.ArchiveRecursveMode;

        }


#pragma warning disable CS0612

        public void RestoreCompatible(Memento memento)
        {
            if (memento == null) return;

            // compatible before ver.22
            if (memento._Version < Config.GenerateProductVersionNumber(1, 22, 0))
            {
                SlideShow.Current.IsSlideShowByLoop = memento.IsSlideShowByLoop;
                SlideShow.Current.SlideShowInterval = memento.SlideShowInterval;
                SlideShow.Current.IsCancelSlideByMouseMove = memento.IsCancelSlideByMouseMove;
            }

            // compatible before ver.23
            if (memento._Version < Config.GenerateProductVersionNumber(1, 23, 0))
            {
                ////対応が難しいため設定を引き継がない
                ////ArchiverManager.Current.IsEnabled = memento.IsSupportArchiveFile;

                BookProfile.Current.IsEnableAnimatedGif = memento.IsEnableAnimatedGif;
                BookProfile.Current.BookPageCollectMode = memento.IsEnableNoSupportFile ? BookPageCollectMode.All : BookPageCollectMode.ImageAndBook;
                BookSetting.Current.IsUseBookMementoDefault = memento.IsUseBookMementoDefault;

                BookOperation.Current.PageEndAction = memento.PageEndAction;

                if (memento.ExternalApplication != null)
                {
                    BookOperation.Current.ExternalApplication = memento.ExternalApplication.Clone();
                }
                if (memento.ClipboardUtility != null)
                {
                    BookOperation.Current.ClipboardUtility = memento.ClipboardUtility.Clone();
                }
                if (memento.BookMemento != null)
                {
                    BookSetting.Current.BookMemento = memento.BookMemento.Clone();
                }
                if (memento.BookMementoDefault != null)
                {
                    BookSetting.Current.BookMementoDefault = memento.BookMementoDefault.Clone();
                }
                if (memento.HistoryMementoFilter != null)
                {
                    BookSetting.Current.HistoryMementoFilter = memento.HistoryMementoFilter;
                }

                BookshelfFolderList.Current.Home = memento.Home;
            }
        }


#pragma warning restore CS0612


        #endregion
    }
}

