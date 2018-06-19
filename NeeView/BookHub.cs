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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

// TODO: 高速切替でテンポラリが残るバグ
// ----------------------------
// TODO: フォルダーサムネイル(非同期) 
// TODO: コマンド類の何時でも受付。ロード中だから弾く、ではない別の方法を。

namespace NeeView
{
    //
    public class RemoveFileParams
    {
        public string Path { get; set; }
        public System.Windows.FrameworkElement Visual { get; set; }
    }

    //
    public class RenameFileParams
    {
        public string Path { get; set; }
        public string OldPath { get; set; }
        public System.Windows.FrameworkElement Visual { get; set; }
    }

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

    //
    public class BookUnit
    {
        public Book Book { get; set; }

        public BookLoadOption LoadOptions { get; set; }

        public BookUnit(Book book)
        {
            Book = book;
        }

        public BookMementoType GetBookMementoType()
        {
            if (BookmarkCollection.Current.Contains(Book.Place))
            {
                return BookMementoType.Bookmark;
            }
            else if (BookHistoryCollection.Current.Contains(Book.Place))
            {
                return BookMementoType.History;
            }
            else
            {
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

    /// <summary>
    /// 本の管理
    /// ロード、本の操作はここを通す
    /// </summary>
    public sealed class BookHub : BindableBase, IEngine, IDisposable
    {
        public static BookHub Current { get; private set; }

        #region NormalizePathname

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

        public void SetEmptyMessage(string message)
        {
            EmptyMessage?.Invoke(this, new BookHubMessageEventArgs(message));
        }

        public void SetInfoMessage(string message)
        {
            InfoMessage.Current.SetMessage(InfoMessageType.Notify, message);
        }


        private Toast _bookHubToast;

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
                if (_isLoading != value)
                {
                    _isLoading = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _isLoading;


        // 再帰を確認する
        [PropertyMember("@ParamIsConfirmRecursive", Tips = "@ParamIsConfirmRecursiveTips")]
        public bool IsConfirmRecursive { get; set; }


        // 自動再帰
        private bool _isAutoRecursive = false;
        [PropertyMember("@ParamIsAutoRecursive")]
        public bool IsAutoRecursive
        {
            get { return _isAutoRecursive; }
            set
            {
                _isAutoRecursive = value;
                EntryCollection.IsAutoRecursive = _isAutoRecursive;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// IsAutoRecursiveWithAllFiles property.
        /// </summary>
        private bool _isAutoRecursiveWithAllFiles = true;
        [PropertyMember("@ParamIsAutoRecursiveWithAllFiles", Tips = "@ParamIsAutoRecursiveWithAllFilesTips")]
        public bool IsAutoRecursiveWithAllFiles
        {
            get { return _isAutoRecursiveWithAllFiles; }
            set
            {
                _isAutoRecursiveWithAllFiles = value;
                EntryCollection.IsAutoRecursiveWithAllFiles = _isAutoRecursiveWithAllFiles;
            }
        }

        /// <summary>
        /// アーカイブの自動再帰展開
        /// </summary>
        private bool _isArchiveRecursive = true;
        [PropertyMember("@ParamIsArchiveRecursive", Tips = "@ParamIsArchiveRecursiveTips")]
        public bool IsArchiveRecursive
        {
            get { return _isArchiveRecursive; }
            set { if (_isArchiveRecursive != value) { _isArchiveRecursive = value; RaisePropertyChanged(); } }
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
        /// 現在の本
        /// </summary>
        public BookUnit BookUnit
        {
            get { return _bookUnit; }
            private set
            {
                if (_bookUnit != value)
                {
                    _bookUnit = value;
                    RaisePropertyChanged();
                }
            }
        }

        private BookUnit _bookUnit;



        // 現在の本
        public Book Book => BookUnit?.Book;


        // 現在の本を解除
        private async Task ReleaseCurrentAsync()
        {
            if (BookUnit != null)
            {
                var current = BookUnit;
                BookUnit = null;
                await current.Book?.DisposeAsync();
            }
        }

        // アドレス
        private string _address;
        public string Address
        {
            get { return _address; }
            set
            {
                _address = value;
                AddressChanged?.Invoke(this, null);
            }
        }




        // command engine
        private BookHubCommandEngine _commandEngine;

        //
        public bool IsBusy() => _commandEngine.Count > 0;

        //
        private BookSetting _bookSetting;


        // コンストラクタ
        public BookHub(BookSetting bookSetting)
        {
            Current = this;

            _bookSetting = bookSetting;

            this.BookChanged +=
                (s, e) =>
                {
                    App.Current?.Dispatcher.Invoke(() =>
                    {
                        if (this.Book?.NotFoundStartPage != null)
                        {
                            InfoMessage.Current.SetMessage(InfoMessageType.BookName, string.Format(Properties.Resources.NotifyCannotOpen, LoosePath.GetFileName(this.Book.NotFoundStartPage)), null, 2.0);
                        }
                        else
                        {
                            InfoMessage.Current.SetMessage(InfoMessageType.BookName, LoosePath.GetFileName(Address), null, 2.0, e.BookMementoType);
                        }
                    });
                };

            BookHistoryCollection.Current.HistoryChanged += (s, e) => HistoryChanged?.Invoke(s, e);
            BookmarkCollection.Current.BookmarkChanged += (s, e) => BookmarkChanged?.Invoke(s, e);

            // command engine
            _commandEngine = new BookHubCommandEngine();
            _commandEngine.Log = new Log(nameof(BookHubCommandEngine), 0);
        }

        //
        public void StartEngine()
        {
            _commandEngine.StartEngine();
        }

        //
        public void StopEngine()
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


        //現在開いているブックの設定作成
        public Book.Memento CreateBookMemento()
        {
            return (BookUnit != null && BookUnit.Book.Pages.Count > 0) ? BookUnit.Book.CreateMemento() : null;
        }

        // 設定の読込
        public Book.Memento LoadBookMemento(string place)
        {
            var unit = BookMementoCollection.Current.GetValid(place);
            return unit?.Memento;
        }

        //設定の保存
        public void SaveBookMemento()
        {
            var memento = CreateBookMemento();
            if (memento == null) return;

            SaveBookMemento(memento, BookUnit.IsKeepHistoryOrder);
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

                // 現在表示されているコンテンツを無効
                App.Current?.Dispatcher.Invoke(() => ViewContentsChanged?.Invoke(this, null));

                // 本の変更通知
                App.Current?.Dispatcher.Invoke(() => BookChanged?.Invoke(this, new BookChangedEventArgs(BookMementoType.None)));
            }

            if (param.Message != null)
            {
                // TODO: 参照方向がおかしい
                ContentCanvas.Current.EmptyPageMessage = param.Message;
                ContentCanvas.Current.IsVisibleEmptyPageMessage = true;
            }
        }


        // ロード中状態更新
        private void NotifyLoading(string path)
        {
            this.IsLoading = (path != null);
            App.Current?.Dispatcher.Invoke(() => Loading?.Invoke(this, new BookHubPathEventArgs(path)));
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
            // 再生中のメディアをPAUSE
            App.Current?.Dispatcher.Invoke(() => MediaPlayerOperator.Current?.Pause());

            // 本の変更開始通知
            App.Current?.Dispatcher.Invoke(() => BookChanging?.Invoke(this, null));

            // 現在の設定を記憶
            var lastBookMemento = this.Book?.Place != null ? this.Book.CreateMemento() : null;

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
                using (var address = new BookAddress())
                {
                    await address.InitializeAsync(args.Path, args.StartEntry, args.Option, this.IsArchiveRecursive, token);

                    // Now Loading ON
                    NotifyLoading(args.Path);

                    // フォルダーリスト更新
                    if (args.IsRefreshFolderList)
                    {
                        var parent = address.Archiver.GetParentPlace();
                        App.Current?.Dispatcher.Invoke(() => FolderListSync?.Invoke(this, new FolderListSyncEventArgs() { Path = address.Place, Parent = address.Archiver.GetParentPlace(), isKeepPlace = false }));
                    }
                    else if ((args.Option & BookLoadOption.SelectFoderListMaybe) != 0)
                    {
                        App.Current?.Dispatcher.Invoke(() => FolderListSync?.Invoke(this, new FolderListSyncEventArgs() { Path = address.Place, Parent = address.Archiver.GetParentPlace(), isKeepPlace = true }));
                    }

                    // 履歴リスト更新
                    if ((args.Option & BookLoadOption.SelectHistoryMaybe) != 0)
                    {
                        App.Current?.Dispatcher.Invoke(() => HistoryListSync?.Invoke(this, new BookHubPathEventArgs(address.Place)));
                    }

                    // 本の設定
                    var memory = _bookSetting.CreateLastestBookMemento(address.Place, lastBookMemento);
                    var setting = _bookSetting.GetSetting(address.Place, memory, args.Option);

                    address.EntryName = address.EntryName ?? LoosePath.NormalizeSeparator(setting.Page);
                    place = address.FullPath;
                    
                    // Load本体
                    await LoadAsyncCore(address, args.Option, setting, token);
                }

                // Now Loading OFF
                ////NotifyLoading(null);

                // ビュー初期化
                App.Current?.Dispatcher.Invoke(() => CommandTable.Current[CommandType.ViewReset].Execute(this, null));

                // 本の設定を退避
                App.Current?.Dispatcher.Invoke(() => _bookSetting.RaiseSettingChanged());

                // 本の変更通知
                App.Current?.Dispatcher.Invoke(() => BookChanged?.Invoke(this, new BookChangedEventArgs(BookUnit.GetBookMementoType())));

                // ページがなかった時の処理
                if (Book.Pages.Count <= 0)
                {
                    App.Current?.Dispatcher.Invoke(() => EmptyMessage?.Invoke(this, new BookHubMessageEventArgs(string.Format(Properties.Resources.NotifyNoPages, Book.Place))));

                    if (IsConfirmRecursive && (args.Option & BookLoadOption.ReLoad) == 0 && !Book.IsRecursiveFolder && Book.SubFolderCount > 0)
                    {
                        App.Current?.Dispatcher.Invoke(() => ConfirmRecursive());
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // nop.
            }
            catch (Exception ex)
            {
                // ファイル読み込み失敗通知
                var message = string.Format(Properties.Resources.ExceptionLoadFailed, place, ex.Message);
                EmptyMessage?.Invoke(this, new BookHubMessageEventArgs(message));

                // for .heic
                if (ex is NotSupportedFileTypeException exc && exc.Extension == ".heic" && Config.Current.IsWindows10())
                {
                    _bookHubToast = new Toast(Properties.Resources.NotifyHeifHelp, App.Current.IsNetworkEnabled ? Properties.Resources.WordOpenStore : null, () =>
                    {
                        System.Diagnostics.Process.Start(PictureProfile.HEIFImageExtensions.OriginalString);
                    });
                    ToastService.Current.Show(_bookHubToast);
                }

                // 現在表示されているコンテンツを無効
                App.Current?.Dispatcher.Invoke(() => ViewContentsChanged?.Invoke(this, new ViewPageCollectionChangedEventArgs(new ViewPageCollection())));

                // 本の変更通知
                App.Current?.Dispatcher.Invoke(() => BookChanged?.Invoke(this, new BookChangedEventArgs(BookMementoType.None)));

                // 履歴リスト更新
                if ((args.Option & BookLoadOption.SelectHistoryMaybe) != 0)
                {
                    App.Current?.Dispatcher.Invoke(() => HistoryListSync?.Invoke(this, new BookHubPathEventArgs(Address)));
                }
            }
            finally
            {
                // Now Loading OFF
                NotifyLoading(null);
            }
        }


        // 再帰読み込み確認
        private void ConfirmRecursive()
        {
            var dialog = new MessageDialog(string.Format(Properties.Resources.DialogConfirmRecursive, Book.Place), Properties.Resources.DialogConfirmRecursiveTitle);
            dialog.Commands.Add(UICommands.Yes);
            dialog.Commands.Add(UICommands.No);
            var result = dialog.ShowDialog();

            if (result == UICommands.Yes)
            {
                RequestLoad(Book.Place, Book.StartEntry, BookLoadOption.Recursive | BookLoadOption.ReLoad, true);
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
                book.Restore(_bookSetting.BookMemento);
            }
            else
            {
                book.Restore(setting);
            }

            // 全種類ファイルサポート設定
            if (BookProfile.Current.IsEnableNoSupportFile)
            {
                option |= BookLoadOption.SupportAllFile;
            }

            // リカーシブ設定
            ////if (option.HasFlag(BookLoadOption.Recursive) && !option.HasFlag(BookLoadOption.NotRecursive))
            ////{
            ////    book.IsRecursiveFolder = true;
            ////}

            // 最初の自動再帰設定
            if (IsAutoRecursive)
            {
                option |= BookLoadOption.AutoRecursive;
            }

            // 圧縮ファイル内圧縮ファイルを再帰
            if (IsArchiveRecursive)
            {
                option |= BookLoadOption.ArchiveRecursive;
            }

            //
            try
            {
                // ロード。非同期で行う
                await book.LoadAsync(address, option, token);

                _historyEntry = false;

                // カレントを設定し、開始する
                BookUnit = new BookUnit(book);
                BookUnit.LoadOptions = option;

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
                BookUnit?.Book?.Dispose();
                BookUnit = null;
                BookHistoryCollection.Current.LastAddress = null;

                throw;
            }
            catch (Exception)
            {
                // 後始末
                BookUnit?.Book?.Dispose();
                BookUnit = null;
                BookHistoryCollection.Current.LastAddress = null;

                // 履歴から消去
                ////BookHistory.Current.Remove(address.Place);
                ////MenuBar.Current.UpdateLastFiles();

                throw;
            }

            // 本の設定を退避
            _bookSetting.BookMemento = this.Book.CreateMemento();

            Address = BookUnit.Book.Place;
            BookHistoryCollection.Current.LastAddress = Address;
        }


        //
        private bool _historyEntry;
        [PropertyMember("@ParamHistoryEntryPageCount", Tips = "@ParamHistoryEntryPageCountTips")]
        public int HistoryEntryPageCount { get; set; } = 0;

        // 履歴登録可
        private bool CanHistory()
        {
            return Book != null
                && Book.Pages.Count > 0
                && (_historyEntry || Book.PageChangeCount > this.HistoryEntryPageCount || Book.IsPageTerminated)
                && (IsInnerArchiveHistoryEnabled || Book.Archiver?.Parent == null)
                && (IsUncHistoryEnabled || !LoosePath.IsUnc(Book.Place));
        }

        // 
        private void OnViewContentsChanged(object sender, ViewPageCollectionChangedEventArgs e)
        {
            if (BookUnit == null) return;

            // 新規履歴
            if (!BookUnit.IsKeepHistoryOrder)
            {
                if (!_historyEntry && CanHistory())
                {
                    _historyEntry = true;
                    BookHistoryCollection.Current.Add(Book?.CreateMemento(), BookUnit.IsKeepHistoryOrder);
                }
            }

            BookUnit.Book.UpdateViewPages(sender, e);

            ViewContentsChanged?.Invoke(sender, e);
        }

        // 
        private void OnNextContentsChanged(object sender, ViewPageCollectionChangedEventArgs e)
        {
            if (BookUnit == null) return;
            NextContentsChanged?.Invoke(sender, e);
        }


        /// <summary>
        /// ロードリクエストカウント.
        /// 名前変更時の再読込判定に使用される
        /// </summary>
        private volatile int _requestLoadCount;
        public int RequestLoadCount => _requestLoadCount;

        /// <summary>
        /// リクエスト：フォルダーを開く
        /// </summary>
        /// <param name="path"></param>
        /// <param name="start"></param>
        /// <param name="option"></param>
        /// <param name="isRefreshFolderList"></param>
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

            LoadRequested?.Invoke(this, new BookHubPathEventArgs(path));

            if (Book?.Place == path && option.HasFlag(BookLoadOption.SkipSamePlace)) return null;

            Address = path;

            _requestLoadCount++;

            if ((option & (BookLoadOption.IsBook | BookLoadOption.IsPage)) == 0 && IsArchiveMaybe(path))
            {
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

        // 再読み込み
        public void ReLoad()
        {
            if (_isLoading || Address == null) return;

            var options = BookUnit != null ? (BookUnit.LoadOptions & BookLoadOption.KeepHistoryOrder) | BookLoadOption.Resume : BookLoadOption.None;
            RequestLoad(Address, null, options | BookLoadOption.IsBook, true);
        }


        /// <summary>
        /// 最新の本の設定を取得
        /// </summary>
        /// <param name="place">場所</param>
        /// <param name="option"></param>
        /// <returns></returns>
        public Book.Memento GetLastestBookMemento(string place, BookLoadOption option)
        {
            if (place != null)
            {
                var bookMemento = this.Book?.Place == place ? this.Book.CreateMemento() : null;
                var memory = _bookSetting.CreateLastestBookMemento(place, bookMemento);
                return _bookSetting.GetSetting(place, memory, option);
            }
            else
            {
                return _bookSetting.GetSetting(null, null, option);
            }
        }

        /// <summary>
        /// パスのみでアーカイブであるかをおおよそ判定
        /// </summary>
        private bool IsArchiveMaybe(string path)
        {
            return (System.IO.Directory.Exists(path) || (!BookProfile.Current.IsEnableNoSupportFile && !PictureProfile.Current.IsSupported(path) && System.IO.Path.GetExtension(path).ToLower() != ".lnk"));
        }

        #region IDisposable Support
        private bool _disposedValue;

        public void Dispose()
        {
            if (_disposedValue) return;

            StopEngine();
            _commandEngine.Dispose();

            _disposedValue = true;
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

            [DataMember(Order = 22)]
            public bool IsAutoRecursiveWithAllFiles { get; set; }

            [DataMember, DefaultValue(0)]
            public int HistoryEntryPageCount { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsArchiveRecursive { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsInnerArchiveHistoryEnabled { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsUncHistoryEnabled { get; set; }

            #region Obslete

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
            memento.IsAutoRecursiveWithAllFiles = IsAutoRecursiveWithAllFiles;
            memento.HistoryEntryPageCount = HistoryEntryPageCount;
            memento.IsArchiveRecursive = IsArchiveRecursive;
            memento.IsInnerArchiveHistoryEnabled = IsInnerArchiveHistoryEnabled;
            memento.IsUncHistoryEnabled = IsUncHistoryEnabled;

            return memento;
        }

        // memento反映
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            IsConfirmRecursive = memento.IsConfirmRecursive;
            IsAutoRecursive = memento.IsAutoRecursive;
            IsAutoRecursiveWithAllFiles = memento.IsAutoRecursiveWithAllFiles;
            HistoryEntryPageCount = memento.HistoryEntryPageCount;
            IsArchiveRecursive = memento.IsArchiveRecursive;
            IsInnerArchiveHistoryEnabled = memento.IsInnerArchiveHistoryEnabled;
            IsUncHistoryEnabled = memento.IsUncHistoryEnabled;
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

                BookProfile.Current.PreLoadMode = memento.PreLoadMode;
                BookProfile.Current.IsEnableAnimatedGif = memento.IsEnableAnimatedGif;
                BookProfile.Current.IsEnableNoSupportFile = memento.IsEnableNoSupportFile;
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

                FolderList.Current.Home = memento.Home;
            }
        }

#pragma warning restore CS0612


        #endregion
    }
}

