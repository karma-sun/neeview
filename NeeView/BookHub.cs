// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
    public class FolderListSyncArguments
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
        [AliasName("そのまま")]
        None,

        [AliasName("次のブックに移動")]
        NextFolder,

        [AliasName("ループする")]
        Loop,
    }


    /// <summary>
    /// 先読みモード
    /// </summary>
    public enum PreLoadMode
    {
        [AliasName("先読み無し")]
        None,

        [AliasName("自動先読み")]
        AutoPreLoad,

        [AliasName("先読みする")]
        PreLoad,

        [AliasName("先読みする(開放なし)")]
        PreLoadNoUnload, 
    }

    //
    public class BookUnit
    {
        public Book Book { get; set; }

        public BookLoadOption LoadOptions { get; set; }
        public BookMementoUnit BookMementoUnit { get; set; }

        public BookUnit(Book book)
        {
            Book = book;
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
    public class BookHub : BindableBase, IEngine
    {
        public static BookHub Current { get; private set; }

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


        #region Events

        // 本の変更通知
        public event EventHandler BookChanging;
        public event EventHandler<BookMementoType> BookChanged;

        // ロード中通知
        public event EventHandler<string> Loading;

        // ViewContentsの変更通知
        public event EventHandler<ViewPageCollectionChangedEventArgs> ViewContentsChanged;

        // NextContentsの変更通知
        public event EventHandler<ViewPageCollectionChangedEventArgs> NextContentsChanged;

        // 空ページメッセージ
        public event EventHandler<string> EmptyMessage;

        // フォルダー列更新要求
        public event EventHandler<FolderListSyncArguments> FolderListSync;

        // 履歴リスト更新要求
        public event EventHandler<string> HistoryListSync;

        // 履歴に追加、削除された
        public event EventHandler<BookMementoCollectionChangedArgs> HistoryChanged;

        // ブックマークにに追加、削除された
        public event EventHandler<BookMementoCollectionChangedArgs> BookmarkChanged;

        // アドレスが変更された
        public event EventHandler AddressChanged;

        #endregion

        public void SetEmptyMessage(string message)
        {
            EmptyMessage?.Invoke(this, message);
        }

        public void SetInfoMessage(string message)
        {
            InfoMessage.Current.SetMessage(InfoMessageType.Notify, message);
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
                if (_isLoading != value)
                {
                    _isLoading = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _isLoading;


        // 再帰を確認する
        [PropertyMember("ページがない場合、サブフォルダーも読み込むか問い合わせる", Tips = "フォルダー形式のブックを開いた時に表示可能なページがなく、かつサブフォルダーが存在する場合に\nサブフォルダーも読み込むかを問い合わせるダイアログを表示します。")]
        public bool IsConfirmRecursive { get; set; }


        // 自動再帰
        private bool _isAutoRecursive = true;
        [PropertyMember("ページがなくサブフォルダーが１つだけ存在する場合にサブフォルダーを読み込む", Tips = "フォルダー形式のブックで機能します")]
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
        [PropertyMember("ページ外ファイルも含めて判定する", Tips = "画像ファイル以外が存在する場合はサブフォルダーを読み込みません")]
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
        [PropertyMember("圧縮ファイルに含まれる圧縮ファイルをすべて展開する", Tips = "FFにした場合、含まれる圧縮ファイルはサブフォルダ扱いになります")]
        public bool IsArchiveRecursive
        {
            get { return _isArchiveRecursive; }
            set { if (_isArchiveRecursive != value) { _isArchiveRecursive = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// アーカイブ内アーカイブの履歴保存
        /// </summary>
        [PropertyMember("圧縮ファイルに含まれる圧縮ファイルを履歴に保存する")]
        public bool IsInnerArchiveHistoryEnabled { get; set; }

        /// <summary>
        /// UNCパスの履歴保存
        /// </summary>
        [PropertyMember("UNCパスを履歴に保存する", Tips = "\\\\コンピュータ名\\~ のようなネットワーク上のパスです")]
        public bool IsUncHistoryEnabled { get; set; }

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
                BookHistory.Current.LastAddress = _address;
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
                            InfoMessage.Current.SetMessage(InfoMessageType.Notify, $"{LoosePath.GetFileName(this.Book.NotFoundStartPage)} が見つかりません", null, 2.0);
                        }
                        else
                        {
                            InfoMessage.Current.SetMessage(InfoMessageType.Notify, LoosePath.GetFileName(Address), null, 2.0, e);
                        }
                    });
                };

            BookHistory.Current.HistoryChanged += (s, e) => HistoryChanged?.Invoke(s, e);
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

            Debug.WriteLine("BokHub Disposing...");

            // 開いているブックを閉じる(5秒待つ。それ以上は待たない)
            Task.Run(async () => await RequestUnload(false).WaitAsync()).Wait(5000);

            // コマンドエンジン停止
            _commandEngine.StopEngine();

            Debug.WriteLine("BookHub Disposed.");
        }


        //現在開いているブックの設定作成
        private Book.Memento CreateBookMemento()
        {
            return (BookUnit != null && BookUnit.Book.Pages.Count > 0) ? BookUnit.Book.CreateMemento() : null;
        }

        // 設定の読込
        public Book.Memento LoadBookMemento(string place)
        {
            var unit = BookMementoCollection.Current.Find(place);
            return unit?.Memento;
        }

        //設定の保存
        public void SaveBookMemento()
        {
            var memento = CreateBookMemento();
            if (memento == null) return;

            SaveBookMemento(BookUnit.BookMementoUnit, memento, BookUnit.IsKeepHistoryOrder);
        }

        private void SaveBookMemento(BookMementoUnit unit, Book.Memento memento, bool isKeepHistoryOrder)
        {
            if (memento == null) return;
            unit = unit ?? BookMementoCollection.Current.Find(memento.Place);

            // 設定保存時間を保障
            memento.LastAccessTime = DateTime.Now;

            // 履歴の保存
            if (CanHistory())
            {
                BookHistory.Current.Add(unit, memento, isKeepHistoryOrder);
            }

            // ブックマーク更新
            BookmarkCollection.Current.Update(unit, memento);

            // ページマーク更新
            PagemarkCollection.Current.Update(unit, memento);
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

                // 現在表示されているコンテンツを無効
                App.Current?.Dispatcher.Invoke(() => ViewContentsChanged?.Invoke(this, null));

                // 本の変更通知
                App.Current?.Dispatcher.Invoke(() => BookChanged?.Invoke(this, BookMementoType.None));
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
            App.Current?.Dispatcher.Invoke(() => Loading?.Invoke(this, path));
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
            // 本の変更開始通知
            App.Current?.Dispatcher.Invoke(() => BookChanging?.Invoke(this, null));

            // 現在の設定を記憶
            var lastBookMemento = this.Book?.Place != null ? this.Book.CreateMemento() : null;

            // 現在の本を開放
            await UnloadAsync(new BookHubCommandUnloadArgs() { IsClearViewContent = false });

            try
            {
                // address
                using (var address = new BookAddress())
                {
                    await address.InitializeAsync(args.Path, args.StartEntry, this.IsArchiveRecursive, token);

                    // Now Loading ON
                    NotifyLoading(args.Path);

                    // フォルダーリスト更新
                    if (args.IsRefleshFolderList)
                    {
                        var parent = address.Archiver.GetParentPlace();
                        App.Current?.Dispatcher.Invoke(() => FolderListSync?.Invoke(this, new FolderListSyncArguments() { Path = address.Place, Parent = address.Archiver.GetParentPlace(), isKeepPlace = false }));
                    }
                    else if ((args.Option & BookLoadOption.SelectFoderListMaybe) != 0)
                    {
                        App.Current?.Dispatcher.Invoke(() => FolderListSync?.Invoke(this, new FolderListSyncArguments() { Path = address.Place, Parent = address.Archiver.GetParentPlace(), isKeepPlace = true }));
                    }

                    // 履歴リスト更新
                    if ((args.Option & BookLoadOption.SelectHistoryMaybe) != 0)
                    {
                        App.Current?.Dispatcher.Invoke(() => HistoryListSync?.Invoke(this, address.Place));
                    }

                    // 本の設定
                    var unit = BookMementoCollection.Current.Find(address.Place);
                    var memory = _bookSetting.CreateLastestBookMemento(address.Place, lastBookMemento, unit);
                    var setting = _bookSetting.GetSetting(address.Place, memory, args.Option);

                    address.EntryName = address.EntryName ?? LoosePath.NormalizeSeparator(setting.Page);

                    // Load本体
                    await LoadAsyncCore(address, args.Option, setting, unit, token);
                }

                // Now Loading OFF
                ////NotifyLoading(null);

                // ビュー初期化
                App.Current?.Dispatcher.Invoke(() => CommandTable.Current[CommandType.ViewReset].Execute(this, null));

                // 本の設定を退避
                App.Current?.Dispatcher.Invoke(() => _bookSetting.RaiseSettingChanged());

                // 本の変更通知
                App.Current?.Dispatcher.Invoke(() => BookChanged?.Invoke(this, BookUnit.BookMementoType));

                // ページがなかった時の処理
                if (Book.Pages.Count <= 0)
                {
                    App.Current?.Dispatcher.Invoke(() => EmptyMessage?.Invoke(this, $"\"{Book.Place}\" には読み込めるファイルがありません"));

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
            catch (Exception e)
            {
                // ファイル読み込み失敗通知
                EmptyMessage?.Invoke(this, e.Message);

                // 現在表示されているコンテンツを無効
                App.Current?.Dispatcher.Invoke(() => ViewContentsChanged?.Invoke(this, new ViewPageCollectionChangedEventArgs(new ViewPageCollection())));

                // 本の変更通知
                App.Current?.Dispatcher.Invoke(() => BookChanged?.Invoke(this, BookMementoType.None));

                // 履歴リスト更新
                if ((args.Option & BookLoadOption.SelectHistoryMaybe) != 0)
                {
                    App.Current?.Dispatcher.Invoke(() => HistoryListSync?.Invoke(this, Address));
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
            var dialog = new MessageDialog($"\"{Book.Place}\" には読み込めるファイルがありません。サブフォルダーまたは圧縮ファイルも読み込みますか？", "サブフォルダーも読み込みますか？");
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
        private async Task LoadAsyncCore(BookAddress address, BookLoadOption option, Book.Memento setting, BookMementoUnit unit, CancellationToken token)
        {
            // 履歴に登録済の場合は履歴先頭に移動させる
            if (unit?.HistoryNode != null && (option & BookLoadOption.KeepHistoryOrder) == 0)
            {
                BookHistory.Current.Add(unit, unit.Memento, false);
            }

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
                BookUnit.BookMementoUnit = unit;

                // イベント設定
                book.ViewContentsChanged += OnViewContentsChanged;
                book.NextContentsChanged += OnNextContentsChanged;
                book.DartyBook += (s, e) => RequestLoad(Address, null, BookLoadOption.ReLoad, false);

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

                throw;
            }
            catch (Exception e)
            {
                // 後始末
                BookUnit?.Book?.Dispose();
                BookUnit = null;

                // 履歴から消去
                BookHistory.Current.Remove(address.Place);
                MenuBar.Current.UpdateLastFiles();

                throw new ApplicationException($"{address.Place} の読み込みに失敗しました。\n{e.Message}", e);
            }

            // 本の設定を退避
            _bookSetting.BookMemento = this.Book.CreateMemento();

            Address = BookUnit.Book.Place;
        }


        //
        private bool _historyEntry;
        private int HistoryEntryPageCount { get; set; } = 1;

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
                    BookUnit.BookMementoUnit = BookHistory.Current.Add(BookUnit.BookMementoUnit, Book?.CreateMemento(), BookUnit.IsKeepHistoryOrder);
                }
            }

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
        /// <param name="isRefleshFolderList"></param>
        /// <returns></returns>
        public BookHubCommandLoad RequestLoad(string path, string start, BookLoadOption option, bool isRefleshFolderList)
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

            if (Book?.Place == path && option.HasFlag(BookLoadOption.SkipSamePlace)) return null;

            Address = path;

            _requestLoadCount++;

            var command = new BookHubCommandLoad(this, new BookHubCommandLoadArgs()
            {
                Path = path,
                StartEntry = start,
                Option = option,
                IsRefleshFolderList = isRefleshFolderList
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
            RequestLoad(Address, null, options, true);
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
                var unit = BookMementoCollection.Current.Find(place);
                var memory = _bookSetting.CreateLastestBookMemento(place, bookMemento, unit);
                return _bookSetting.GetSetting(place, memory, option);
            }
            else
            {
                return _bookSetting.GetSetting(null, null, option);
            }
        }

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

            [DataMember, DefaultValue(1)]
            [PropertyMember("履歴登録開始ページ操作回数", Tips = "この回数のページ移動操作をしたら履歴に登録するようにする。")]
            public int HistoryEntryPageCount { get; set; }

            [DataMember, DefaultValue(true)]
            [PropertyMember("圧縮ファイルに含まれる圧縮ファイルをすべて展開する", Tips = "FFにした場合、含まれる圧縮ファイルはサブフォルダ扱いになります", IsVisible = false)]
            public bool IsArchiveRecursive { get; set; }

            [DataMember]
            public bool IsInnerArchiveHistoryEnabled { get; set; }

            [DataMember]
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
                HistoryEntryPageCount = 1;
                IsArchiveRecursive = true;
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
                ArchiverManager.Current.IsEnabled = memento.IsSupportArchiveFile;

                BookProfile.Current.PreLoadMode = memento.PreLoadMode;
                BookProfile.Current.IsEnableAnimatedGif = memento.IsEnableAnimatedGif;
                BookProfile.Current.IsEnableNoSupportFile = memento.IsEnableNoSupportFile;

                BookOperation.Current.PageEndAction = memento.PageEndAction;
                BookOperation.Current.ExternalApplication = memento.ExternalApplication.Clone();
                BookOperation.Current.ClipboardUtility = memento.ClipboardUtility.Clone();

                BookSetting.Current.BookMemento = memento.BookMemento.Clone();
                BookSetting.Current.BookMementoDefault = memento.BookMementoDefault.Clone();
                BookSetting.Current.IsUseBookMementoDefault = memento.IsUseBookMementoDefault;
                BookSetting.Current.HistoryMementoFilter = memento.HistoryMementoFilter;

                FolderList.Current.Home = memento.Home;
            }
        }

#pragma warning restore CS0612


        #endregion
    }
}

