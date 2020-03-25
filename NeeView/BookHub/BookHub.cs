﻿using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Diagnostics;
using NeeView.IO;
using NeeView.Threading.Tasks;
using NeeView.Windows.Property;
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
    public class BookUnit : IDisposable
    {
        public BookUnit(Book book, BookAddress bookAddress, BookLoadOption loadOptions)
        {
            Debug.Assert(book != null);
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

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Book.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        public BookMementoType GetBookMementoType()
        {
            if (Book is null) return BookMementoType.None;

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
        ////private ArchiveEntryCollectionMode _archiveRecursiveMode = ArchiveEntryCollectionMode.IncludeSubArchives;
        private BookUnit _bookUnit;
        private string _address;
        private BookHubCommandEngine _commandEngine;
        private bool _historyEntry;
        private bool _historyRemoved;
        private int _requestLoadCount;
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

            BookmarkCollection.Current.BookmarkChanged +=
                (s, e) => BookmarkChanged?.Invoke(s, e);

            BookSettingPresenter.Current.SettingChanged +=
                (s, e) => Book?.Restore(BookSettingPresenter.Current.LatestSetting.ToBookMemento());


            // command engine
            _commandEngine = new BookHubCommandEngine();
            _commandEngine.Name = "BookHubJobEngine";
            _commandEngine.Log = new Log(nameof(BookHubCommandEngine), 0);
            _commandEngine.StartEngine();

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
        public event EventHandler<ViewContentSourceCollectionChangedEventArgs> ViewContentsChanged;

        // NextContentsの変更通知
        public event EventHandler<ViewContentSourceCollectionChangedEventArgs> NextContentsChanged;

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

#if false
        // 再帰を確認する
        [PropertyMember("@ParamIsConfirmRecursive", Tips = "@ParamIsConfirmRecursiveTips")]
        public bool IsConfirmRecursive { get; set; }

        // 自動再帰
        private bool _isAutoRecursive = false;
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
#endif

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
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    BookSettingPresenter.Current.IsLocked = _isLoading;
                }
            }
        }

        public bool IsBusy => _commandEngine.Count > 0;

        /// <summary>
        /// ロードリクエストカウント.
        /// 名前変更時の再読込判定に使用される
        /// </summary>
        public int RequestLoadCount => _requestLoadCount;

        #endregion Properties

        #region IDisposable Support
        private bool _disposedValue = false;

        void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // reset event
                    this.BookChanging = null;
                    this.BookChanged = null;
                    this.LoadRequested = null;
                    this.Loading = null;
                    this.ViewContentsChanged = null;
                    this.NextContentsChanged = null;
                    this.EmptyMessage = null;
                    this.FolderListSync = null;
                    this.HistoryListSync = null;
                    this.HistoryChanged = null;
                    this.BookmarkChanged = null;
                    this.AddressChanged = null;
                    ResetPropertyChanged();

                    _commandEngine.Dispose();

                    this.BookUnit?.Dispose();

                    Debug.WriteLine("BookHub Disposed.");
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        #region Callback Methods

        private void BookHistoryCollection_HistoryChanged(object sender, BookMementoCollectionChangedArgs e)
        {
            HistoryChanged?.Invoke(sender, e);

            lock (_lock)
            {
                if (BookUnit == null) return;

                // 履歴削除されたものを履歴登録しないようにする
                if (e.HistoryChangedType == BookMementoCollectionChangedType.Remove && this.BookUnit.Book.Address == e.Key)
                {
                    _historyRemoved = true;
                }
            }
        }

        private void OnViewContentsChanged(object sender, ViewContentSourceCollectionChangedEventArgs e)
        {
            AppDispatcher.Invoke(() =>
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
                    var viewPages = e?.ViewPageCollection?.Collection.Where(x => x != null).Select(x => x.Page).ToList() ?? new List<Page>();
                    BookUnit.Book.Pages.SetViewPageFlag(viewPages);
                }

                ViewContentsChanged?.Invoke(sender, e);
            });
        }

        private void OnNextContentsChanged(object sender, ViewContentSourceCollectionChangedEventArgs e)
        {
            if (BookUnit == null) return;
            NextContentsChanged?.Invoke(sender, e);
        }

        #endregion Callback Methods

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
            var sourcePath = path;
            if (FileShortcut.IsShortcut(path) && (System.IO.File.Exists(path) || System.IO.Directory.Exists(path)))
            {
                var shortcut = new FileShortcut(path);
                if (shortcut.IsValid)
                {
                    path = shortcut.TargetPath;
                }
            }

            path = GetNormalizePathName(path);

            ////DebugTimer.Start($"\nStart: {path}");

            LoadRequested?.Invoke(this, new BookHubPathEventArgs(path));

            if (Book?.Address == path && option.HasFlag(BookLoadOption.SkipSamePlace)) return null;

            Address = path;

            Interlocked.Increment(ref _requestLoadCount);

            var command = new BookHubCommandLoad(this, new BookHubCommandLoadArgs()
            {
                Path = path,
                SourcePath = sourcePath,
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
            RequestLoad(Address, null, options | BookLoadOption.IsBook | BookLoadOption.IgnoreCache, true);
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
        /// <param name="isRefreshFolderList"></param>
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
            Unload(new BookHubCommandUnloadArgs() { IsClearViewContent = false });

            string place = args.Path;

            if (_bookHubToast != null)
            {
                _bookHubToast.Cancel();
                _bookHubToast = null;
            }

            bool isNew = true;

            try
            {
                // address
                var address = await BookAddress.CreateAsync(new QueryPath(args.Path), new QueryPath(args.SourcePath), args.StartEntry, Config.Current.System.ArchiveRecursiveMode, args.Option, token);

                // Now Loading ON
                NotifyLoading(args.Path);

                // 履歴リスト更新
                if ((args.Option & BookLoadOption.SelectHistoryMaybe) != 0)
                {
                    AppDispatcher.Invoke(() => HistoryListSync?.Invoke(this, new BookHubPathEventArgs(address.Address.SimplePath)));
                }

                // 本の設定
                var setting = CreateOpenBookMemento(address.Address.SimplePath, lastBookMemento, args.Option);

                address.EntryName = address.EntryName ?? LoosePath.NormalizeSeparator(setting.Page);
                place = address.SystemPath;

                // 移動履歴登録
                BookHubHistory.Current.Add(address.Address);

                // フォルダーリスト更新
                if (args.IsRefreshFolderList)
                {
                    AppDispatcher.Invoke(() => FolderListSync?.Invoke(this, new FolderListSyncEventArgs() { Path = address.Address.SimplePath, Parent = address.Place.SimplePath, isKeepPlace = false }));
                }

                isNew = BookMementoCollection.Current.GetValid(address.Address.SimplePath) == null;

                // Load本体
                await LoadAsyncCore(address, args.Option, setting, token);

                ////DebugTimer.Check("LoadCore");

                AppDispatcher.Invoke(() =>
                {
                    // ビュー初期化
                    CommandTable.Current.GetElement("ViewReset").Execute(CommandElement.EmptyArgs, CommandOption.None);

                    // 本の設定を更新
                    BookSettingPresenter.Current.SetLatestSetting(BookSettingConfigExtensions.FromBookMement(BookUnit?.Book.CreateMemento()));

                    // 本の変更通知
                    lock (_lock)
                    {
                        if (BookUnit != null)
                        {
                            BookChanged?.Invoke(this, new BookChangedEventArgs(BookUnit.GetBookMementoType()));
                        }
                    }
                });

                // ページがなかった時の処理
                if (Book.Pages.Count <= 0)
                {
                    ResetBookMementoPage(Book.Address);

                    AppDispatcher.Invoke(() =>
                    {
                        EmptyMessage?.Invoke(this, new BookHubMessageEventArgs(string.Format(Properties.Resources.NotifyNoPages, Book.Address)));

                        if (Config.Current.Book.IsConfirmRecursive && (args.Option & BookLoadOption.ReLoad) == 0 && !Book.Source.IsRecursiveFolder && Book.Source.SubFolderCount > 0)
                        {
                            ConfirmRecursive(token);
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
                if (ex is NotSupportedFileTypeException exc && (exc.Extension == ".heic" || exc.Extension == ".heif") && Environment.IsWindows10)
                {
                    _bookHubToast = new Toast(Properties.Resources.NotifyHeifHelp, null, ToastIcon.Information, Config.Current.System.IsNetworkEnabled ? Properties.Resources.WordOpenStore : null, () =>
                     {
                         System.Diagnostics.Process.Start(PictureProfile.HEIFImageExtensions.OriginalString);
                     });
                    ToastService.Current.Show(_bookHubToast);
                }

                AppDispatcher.Invoke(() =>
                {
                    // 現在表示されているコンテンツを無効
                    ViewContentsChanged?.Invoke(this, new ViewContentSourceCollectionChangedEventArgs(new ViewContentSourceCollection()));

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

            // Script fook
            if (BookUnit != null)
            {
                BookUnit.Book.IsNew = isNew;
                var commandName = ScriptCommand.Prefix + ScriptCommand.EventOnBookLoaded;
                if (CommandTable.Current.ContainsKey(commandName))
                {
                    AppDispatcher.Invoke(() => CommandTable.Current.TryExecute(commandName, null, CommandOption.None));
                }
            }
        }


        // 再帰読み込み確認
        private void ConfirmRecursive(CancellationToken token)
        {
            var dialog = new MessageDialog(string.Format(Properties.Resources.DialogConfirmRecursive, Book.Address), Properties.Resources.DialogConfirmRecursiveTitle);
            dialog.Commands.Add(UICommands.Yes);
            dialog.Commands.Add(UICommands.No);

            token.ThrowIfCancellationRequested();
            using (var register = token.Register(() => dialog.Close()))
            {
                var result = dialog.ShowDialog();

                if (result == UICommands.Yes)
                {
                    RequestReloadRecursive(Book);
                }
            }
        }

        private void RequestReloadRecursive(Book book)
        {
            // TODO: BookAddressそのまま渡せばよいと思う
            RequestLoad(book.Address, book.StartEntry, BookLoadOption.Recursive | BookLoadOption.ReLoad, true);
        }


        /// <summary>
        /// 本を読み込む(本体)
        /// </summary>
        /// <param name="path">本のパス</param>
        /// <param name="startEntry">開始エントリ</param>
        /// <param name="option">読み込みオプション</param>
        private async Task LoadAsyncCore(BookAddress address, BookLoadOption option, Book.Memento setting, CancellationToken token)
        {
            try
            {
                var memento = ((option & BookLoadOption.ReLoad) == BookLoadOption.ReLoad) ? BookSettingPresenter.Current.LatestSetting.ToBookMemento() : setting;

                var bookSetting = new BookCreateSetting()
                {
                    StartPage = BookLoadOptionHelper.CreateBookStartPage(address.EntryName, option),
                    IsRecursiveFolder = BookLoadOptionHelper.CreateIsRecursiveFolder(memento.IsRecursiveFolder, option),
                    ArchiveRecursiveMode = Config.Current.System.ArchiveRecursiveMode,
                    BookPageCollectMode = Config.Current.System.BookPageCollectMode,
                    SortMode = memento.SortMode,
                    IsIgnoreCache = option.HasFlag(BookLoadOption.IgnoreCache),
                    LoadOption = option,
                };

                var book = await BookFactory.CreateAsync(address.Address, address.SourceAddress, bookSetting, memento, token);

                // auto recursive
                if (Config.Current.Book.IsAutoRecursive && !book.Source.IsRecursiveFolder && book.Source.SubFolderCount == 1)
                {
                    bookSetting.IsRecursiveFolder = true;
                    book = await BookFactory.CreateAsync(address.Address, address.SourceAddress, bookSetting, memento, token);
                }

                _historyEntry = false;
                _historyRemoved = false;

                // カレントを設定し、開始する
                lock (_lock)
                {
                    token.ThrowIfCancellationRequested();
                    BookUnit?.Dispose();
                    BookUnit = new BookUnit(book, address, option);
                }

                // イベント設定
                book.Viewer.ViewContentsChanged += OnViewContentsChanged;
                book.Viewer.NextContentsChanged += OnNextContentsChanged;
                book.Source.DartyBook += (s, e) => RequestLoad(Address, null, BookLoadOption.ReLoad | BookLoadOption.IsBook, false);

                var tcs = new TaskCompletionSource<bool>();
                book.Viewer.ViewContentsChanged += OnViewContentsChangedInner;

                // 開始
                BookUnit?.Book.Start();

                // 最初のコンテンツ表示待ち
                if (book.Pages.Count > 0)
                {
                    using (var register = token.Register(() => tcs.TrySetCanceled()))
                    {
                        await tcs.Task;
                    }
                }

                book.Viewer.ViewContentsChanged -= OnViewContentsChangedInner;

                //// inner callback function define.
                void OnViewContentsChangedInner(object sender, ViewContentSourceCollectionChangedEventArgs e)
                {
                    if (e.ViewPageCollection.Collection.All(e_ => e_.Content.IsViewReady))
                    {
                        tcs.TrySetResult(true);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 後始末
                lock (_lock)
                {
                    BookUnit?.Dispose();
                    BookUnit = null;
                }
                Config.Current.StartUp.LastBookPath = null;

                throw;
            }
            catch (Exception)
            {
                // 後始末
                lock (_lock)
                {
                    BookUnit?.Dispose();
                    BookUnit = null;
                }
                Config.Current.StartUp.LastBookPath = null;

                // 履歴から消去
                ////BookHistory.Current.Remove(address.Place);
                ////MenuBar.Current.UpdateLastFiles();

                throw;
            }

            Address = BookUnit?.Book.Address;
            Config.Current.StartUp.LastBookPath = Address;
        }


        #endregion BookHubCommand.Load

        #region BookHubCommand.Unload

        /// <summary>
        /// 本の開放
        /// </summary>
        public void Unload(BookHubCommandUnloadArgs param)
        {
            // 履歴の保存
            SaveBookMemento();

            // 現在の本を開放
            ReleaseCurrent();

            if (param.IsClearViewContent)
            {
                Address = null;
                Config.Current.StartUp.LastBookPath = null;

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
        private void ReleaseCurrent()
        {
            lock (_lock)
            {
                BookUnit?.Dispose();
                BookUnit = null;
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

        // ブック設定の作成
        // 開いているブックならばその設定を取得する
        public Book.Memento CreateBookMemento(string place)
        {
            if (place == null) throw new ArgumentNullException();

            var memento = CreateBookMemento();
            if (memento == null || memento.Place != place)
            {
                memento = BookSettingPresenter.Current.DefaultSetting.ToBookMemento();
                memento.Place = place;
            }
            return memento;
        }

        /// <summary>
        /// 最新の設定を取得
        /// </summary>
        /// <param name="place">場所</param>
        /// <param name="lastest">現在の情報</param>
        private Book.Memento CreateLastestBookMemento(string place, Book.Memento lastest)
        {
            Book.Memento memento = null;

            if (lastest?.Place == place)
            {
                memento = lastest.Clone();
            }
            else
            {
                var unit = BookMementoCollection.Current.GetValid(place);
                if (unit != null)
                {
                    memento = unit.Memento.Clone();
                }
            }

            return memento;
        }

        private Book.Memento CreateOpenBookMemento(string place, Book.Memento lastest, BookLoadOption option)
        {
            var memory = CreateLastestBookMemento(place, lastest);
            Debug.Assert(memory == null || memory.Place == place);

            if (memory != null && option.HasFlag(BookLoadOption.Resume))
            {
                return memory.Clone();
            }
            else
            {
                return BookSettingPresenter.Current.GetSetting(BookSettingConfigExtensions.FromBookMement(memory), option.HasFlag(BookLoadOption.DefaultRecursive)).ToBookMemento();
            }
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

        /// <summary>
        /// 記録のページのみクリア
        /// </summary>
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
            var current = (address != null && this.Book?.Address == address) ? this.Book?.CreateMemento() : null;
            if (current != null)
            {
                return current;
            }
            return CreateOpenBookMemento(address, null, option);
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
                && (_historyEntry || Book.Viewer.PageChangeCount > historyEntryPageCount || Book.Viewer.IsPageTerminated)
                && (IsInnerArchiveHistoryEnabled || Book.Source.ArchiveEntryCollection.Archiver?.Parent == null)
                && (IsUncHistoryEnabled || !LoosePath.IsUnc(Book.Address));
        }

        #endregion BookMemento Control


        #region Memento

        /// <summary>
        /// BookHub Memento
        /// </summary>
        [DataContract]
        public class Memento : BindableBase, IMemento
        {
            [DataMember]
            public int _Version { get; set; } = Environment.ProductVersionNumber;

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

            [Obsolete, DataMember(Order = 22, EmitDefaultValue = false)]
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
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            [OnDeserialized]
            private void OnDeserialized(StreamingContext c)
            {
#pragma warning disable CS0612
                // before 34.0
                if (_Version < Environment.GenerateProductVersionNumber(34, 0, 0))
                {
                    ArchiveRecursveMode = IsArchiveRecursive ? ArchiveEntryCollectionMode.IncludeSubArchives : ArchiveEntryCollectionMode.IncludeSubDirectories;
                }
#pragma warning restore CS0612
            }

            public void RestoreConfig(Config config)
            {
                config.System.ArchiveRecursiveMode = ArchiveRecursveMode;
                config.Book.IsConfirmRecursive = IsConfirmRecursive;
                config.Book.IsAutoRecursive = IsAutoRecursive;
            }
        }

        // memento作成
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento._Version = Environment.ProductVersionNumber;

            memento.IsConfirmRecursive = Config.Current.Book.IsConfirmRecursive;
            memento.IsAutoRecursive = Config.Current.Book.IsAutoRecursive;
            memento.HistoryEntryPageCount = HistoryEntryPageCount;
            memento.IsInnerArchiveHistoryEnabled = IsInnerArchiveHistoryEnabled;
            memento.IsUncHistoryEnabled = IsUncHistoryEnabled;
            memento.IsForceUpdateHistory = IsForceUpdateHistory;
            memento.ArchiveRecursveMode = Config.Current.System.ArchiveRecursiveMode;

            return memento;
        }

        // memento反映
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            //IsConfirmRecursive = memento.IsConfirmRecursive;
            //IsAutoRecursive = memento.IsAutoRecursive;
            HistoryEntryPageCount = memento.HistoryEntryPageCount;
            IsInnerArchiveHistoryEnabled = memento.IsInnerArchiveHistoryEnabled;
            IsUncHistoryEnabled = memento.IsUncHistoryEnabled;
            IsForceUpdateHistory = memento.IsForceUpdateHistory;
            ////this.ArchiveRecursiveMode = memento.ArchiveRecursveMode;
        }

        #endregion
    }
}

