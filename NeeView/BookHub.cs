// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
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
        PreLoadNoUnload, // 固定先読み開放なし
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
    public class BookHub : BindableBase
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
        public event EventHandler<ViewSource> ViewContentsChanged;

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
                    Book?.RequestReflesh(this, true); // 表示更新
                }
            }
        }
        #endregion


        #region Property: IsEnableExif
        public bool IsEnableExif
        {
            get { return BitmapContent.IsEnableExif; }
            set
            {
                if (BitmapContent.IsEnableExif != value)
                {
                    BitmapContent.IsEnableExif = value;
                    Book?.RequestReflesh(this, true); // 表示更新
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
                EntryCollection.IsAutoRecursive = _isAutoRecursive;
            }
        }
        #endregion

        /// <summary>
        /// IsAutoRecursiveWithAllFiles property.
        /// </summary>
        private bool _isAutoRecursiveWithAllFiles = true;
        public bool IsAutoRecursiveWithAllFiles
        {
            get { return _isAutoRecursiveWithAllFiles; }
            set
            {
                _isAutoRecursiveWithAllFiles = value;
                EntryCollection.IsAutoRecursiveWithAllFiles = _isAutoRecursiveWithAllFiles;
            }
        }


        // 圧縮ファイルの有効/無効
        #region Property: IsSupportArchiveFile
        public bool IsSupportArchiveFile
        {
            get { return ArchiverManager.Current.IsEnabled; }
            set { ArchiverManager.Current.IsEnabled = value; }
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
        public Book Book => BookUnit?.Book;

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




        //
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
        #region Property: Address
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
        #endregion

        // 外部アプリ設定
        public ExternalApplication ExternalApllication { get; set; } = new ExternalApplication();

        // クリップボード設定
        public ClipboardUtility ClipboardUtility { get; set; } = new ClipboardUtility();

        /// <summary>
        /// Home property.
        /// フォルダーリストのHOME
        /// TODO: FolderListで保持すべき
        /// </summary>
        private string _home;
        public string Home
        {
            get { return _home; }
            set { if (_home != value) { _home = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// 補正されたHOME取得
        /// </summary>
        /// <returns></returns>
        public string GetFixedHome()
        {
            if (Directory.Exists(_home)) return _home;

            var myPicture = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);
            if (Directory.Exists(myPicture)) return myPicture;

            return Environment.CurrentDirectory;
        }


        // command engine
        private BookHubCommandEngine _commandEngine;

        //
        public bool IsBusy() => _commandEngine.Count > 0;


        //
        private BookOperation _bookOperation;

        //
        private BookSetting _bookSetting;

        // コンストラクタ
        public BookHub(BookSetting bookSetting, BookOperation bookOperation)
        {
            Current = this;

            _bookSetting = bookSetting;
            _bookOperation = bookOperation;

            this.BookChanged +=
                (s, e) =>
                {
                    RaisePropertyChanged(nameof(IsBookmark));
                    App.Current?.Dispatcher.Invoke(() => InfoMessage.Current.SetMessage(InfoMessageType.Notify, LoosePath.GetFileName(Address), null, 2.0, e));
                    _bookOperation.SetBook(this.BookUnit);
                };

            BookHistory.Current.HistoryChanged += (s, e) => HistoryChanged?.Invoke(s, e);
            BookmarkCollection.Current.BookmarkChanged += (s, e) => BookmarkChanged?.Invoke(s, e);

            // command engine
            _commandEngine = new BookHubCommandEngine();
            _commandEngine.Initialize();
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        public void Dispose()
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
            _commandEngine.Dispose();

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

            // 履歴の保存
            BookHistory.Current.Add(unit, memento, isKeepHistoryOrder);

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


        // 入力パスから場所を取得
        private string GetPlaceEx(string path, BookLoadOption option)
        {
            if (Directory.Exists(path))
            {
                return path;
            }

            if (File.Exists(path))
            {
                if (ArchiverManager.Current.IsSupported(path))
                {
                    Archiver archiver = ArchiverManager.Current.CreateArchiver(path, null);
                    if (archiver.IsSupported())
                    {
                        return path;
                    }
                }

                if (BitmapLoaderManager.Current.IsSupported(path) || (option & BookLoadOption.SupportAllFile) == BookLoadOption.SupportAllFile)
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

        // パスから対応するアーカイバーを取得する
        private string GetPlace(string path, BookLoadOption option)
        {
            if (Directory.Exists(path))
            {
                return path;
            }

            if (File.Exists(path))
            {
                if (ArchiverManager.Current.IsSupported(path))
                {
                    Archiver archiver = ArchiverManager.Current.CreateArchiver(path, null);
                    if (archiver.IsSupported())
                    {
                        return path;
                    }
                }

                if (BitmapLoaderManager.Current.IsSupported(path) || (option & BookLoadOption.SupportAllFile) == BookLoadOption.SupportAllFile)
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

            // 現在の本を開放
            await UnloadAsync(new BookHubCommandUnloadArgs() { IsClearViewContent = false });

            try
            {
                // place
                string place = GetPlace(args.Path, args.Option);

                // start
                string startEntry = args.Path == place ? args.StartEntry : Path.GetFileName(args.Path);

                // Now Loading ON
                NotifyLoading(args.Path);

                // 本操作無効
                _bookOperation.IsEnabled = false;

                // フォルダーリスト更新
                if (args.IsRefleshFolderList)
                {
                    App.Current?.Dispatcher.Invoke(() => FolderListSync?.Invoke(this, new FolderListSyncArguments() { Path = place, isKeepPlace = false }));
                }
                else if ((args.Option & BookLoadOption.SelectFoderListMaybe) != 0)
                {
                    App.Current?.Dispatcher.Invoke(() => FolderListSync?.Invoke(this, new FolderListSyncArguments() { Path = place, isKeepPlace = true }));
                }

                // 履歴リスト更新
                if ((args.Option & BookLoadOption.SelectHistoryMaybe) != 0)
                {
                    App.Current?.Dispatcher.Invoke(() => HistoryListSync?.Invoke(this, place));
                }

                // 本の設定
                var unit = BookMementoCollection.Current.Find(place);
                var setting = _bookSetting.GetSetting(unit, place, args.Option);

                // Load本体
                await LoadAsyncCore(place, startEntry ?? setting.Page, args.Option, setting, unit, token);

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
                // 現在表示されているコンテンツを無効
                App.Current?.Dispatcher.Invoke(() => ViewContentsChanged?.Invoke(this, new ViewSource()));

                // 本の変更通知
                App.Current?.Dispatcher.Invoke(() => BookChanged?.Invoke(this, BookMementoType.None)); //  Current.BookMementoType));

                // ファイル読み込み失敗通知
                EmptyMessage?.Invoke(this, e.Message);

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



        /// <summary>
        /// ブック読み込み
        /// </summary>
        /// <param name="path"></param>
        /// <param name="option"></param>
        public void Load(string path, BookLoadOption option = BookLoadOption.None)
        {
            if (Utility.FileShortcut.IsShortcut(path) && (System.IO.File.Exists(path) || System.IO.Directory.Exists(path)))
            {
                var shortcut = new Utility.FileShortcut(path);
                path = shortcut.TargetPath;
            }

            RequestLoad(path, null, option, true);
        }

        /// <summary>
        /// ロードリクエストカウント.
        /// 名前変更時の再読込判定に使用される
        /// </summary>
        private volatile int _requestLoadCount;

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
            if (path == null) return null;
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

        // アンロード可能?
        public bool CanUnload()
        {
            return BookUnit != null || _commandEngine.Count > 0;
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
        private async Task LoadAsyncCore(string path, string startEntry, BookLoadOption option, Book.Memento setting, BookMementoUnit unit, CancellationToken token)
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
                await book.LoadAsync(path, startEntry, option, token);

                // カレントを設定し、開始する
                BookUnit = new BookUnit(book);
                BookUnit.LoadOptions = option;
                BookUnit.BookMementoUnit = unit;

                // イベント設定
                book.ViewContentsChanged += (s, e) => { if (BookUnit != null) ViewContentsChanged?.Invoke(s, e); };
                book.DartyBook += (s, e) => RequestLoad(Address, null, BookLoadOption.ReLoad, false);

                // 開始
                BookUnit.Book.Start();

                // 最初のコンテンツ表示待ち
                if (book.Pages.Count > 0)
                {
                    //await Task.Run(() => book.ContentLoaded.Wait(token));
                    await Utility.Process.ActionAsync(() => book.ContentLoaded.Wait(token), token);
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
                BookHistory.Current.Remove(path);
                MenuBar.Current.UpdateLastFiles();

                throw new ApplicationException($"{path} の読み込みに失敗しました。\n{e.Message}", e);
            }

            // 本の設定を退避
            _bookSetting.BookMemento = this.Book.CreateMemento();

            // 新規履歴
            if (BookUnit.BookMementoUnit?.HistoryNode == null && Book.Pages.Count > 0 && !BookUnit.IsKeepHistoryOrder)
            {
                BookUnit.BookMementoUnit = BookHistory.Current.Add(BookUnit.BookMementoUnit, Book?.CreateMemento(), BookUnit.IsKeepHistoryOrder);
            }

            Address = BookUnit.Book.Place;
        }


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



        // 外部アプリで開く
        public void OpenApplication()
        {
            if (CanOpenFilePlace())
            {
                try
                {
                    ExternalApllication.Call(Book?.GetViewPages());
                }
                catch (Exception e)
                {
                    new MessageDialog($"原因: {e.Message}", "外部アプリ実行に失敗しました").ShowDialog();
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
                    ClipboardUtility.Copy(Book?.GetViewPages());
                }
                catch (Exception e)
                {
                    new MessageDialog($"原因: {e.Message}", "コピーに失敗しました").ShowDialog();
                }
            }
        }


        // ブックマーク登録可能？
        public bool CanBookmark()
        {
            return (Book != null);
        }


        // ブックマーク切り替え
        public void ToggleBookmark()
        {
            if (CanBookmark())
            {
                if (BookUnit.Book.Place.StartsWith(Temporary.TempDirectory))
                {
                    new MessageDialog($"原因: 一時フォルダーはブックマークできません", "ブックマークできません").ShowDialog();
                }
                else
                {
                    BookUnit.BookMementoUnit = BookmarkCollection.Current.Toggle(BookUnit.BookMementoUnit, Book.CreateMemento());
                    RaisePropertyChanged(nameof(IsBookmark));
                }
            }
        }

        // ブックマーク判定
        public bool IsBookmark
        {
            get
            {
                if (BookUnit?.BookMementoUnit != null && BookUnit.BookMementoUnit.Memento.Place == Book.Place)
                {
                    return BookUnit.BookMementoUnit.BookmarkNode != null;
                }
                else
                {
                    return false;
                }
            }
        }



        // ブックマークを戻る
        public void PrevBookmark()
        {
            if (_isLoading) return;

            if (!BookmarkCollection.Current.CanMoveSelected(-1))
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, "前のブックマークはありません");
                return;
            }

            var unit = BookmarkCollection.Current.MoveSelected(-1);
            if (unit != null)
            {
                RequestLoad(unit.Value.Memento.Place, null, BookLoadOption.SkipSamePlace, false);
            }
        }


        // ブックマークを進む
        public void NextBookmark()
        {
            if (_isLoading) return;

            if (!BookmarkCollection.Current.CanMoveSelected(+1))
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, "次のブックマークはありません");
                return;
            }

            var unit = BookmarkCollection.Current.MoveSelected(+1);
            if (unit != null)
            {
                RequestLoad(unit.Value.Memento.Place, null, BookLoadOption.SkipSamePlace, false);
            }
        }

        // ファイルの場所を開くことが可能？
        public bool CanOpenFilePlace()
        {
            return Book?.GetViewPage() != null;
        }

        // ファイルの場所を開く
        public void OpenFilePlace()
        {
            if (CanOpenFilePlace())
            {
                string place = Book.GetViewPage()?.GetFilePlace();
                if (place != null)
                {
                    System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + place + "\"");
                }
            }
        }

        // ファイルを開く基準となるフォルダーを取得
        public string GetDefaultFolder()
        {
            // 既に開いている場合、その場所を起点とする
            if (Preference.Current.openbook_begin_current && Book != null)
            {
                return Path.GetDirectoryName(Book.Place);
            }
            else
            {
                return "";
            }
        }


        /// <summary>
        /// ファイル保存可否
        /// </summary>
        /// <returns></returns>
        public bool CanExport()
        {
            var pages = Book?.GetViewPages();
            if (pages == null || pages.Count == 0) return false;

            var bitmapSource = (pages[0].Content as BitmapContent)?.BitmapSource;
            if (bitmapSource == null) return false;

            return true;
        }

        // ファイルに保存する
        // TODO: ここじゃない
        // TODO: OutOfMemory対策
        public void Export()
        {
            if (CanExport())
            {
                try
                {
                    var pages = Book.GetViewPages();
                    int index = Book.GetViewPageindex() + 1;
                    string name = $"{Path.GetFileNameWithoutExtension(Book.Place)}_{index:000}-{index + pages.Count - 1:000}.png";
                    var exporter = new Exporter();
                    exporter.Initialize(pages, Book.BookReadOrder, name);
                    exporter.Background = ContentCanvasBrush.Current.CreateBackgroundBrush();
                    exporter.BackgroundFront = ContentCanvasBrush.Current.CreateBackgroundFrontBrush(new DpiScale(1, 1));
                    if (exporter.ShowDialog() == true)
                    {
                        try
                        {
                            exporter.Export();
                        }
                        catch (Exception e)
                        {
                            new MessageDialog($"原因: {e.Message}", "ファイル保存に失敗しました").ShowDialog();
                        }
                    }
                }
                catch (Exception e)
                {
                    new MessageDialog($"この画像は出力できません。\n原因: {e.Message}", "ファイル保存に失敗しました").ShowDialog();
                    return;
                }
            }
        }


        // ファイル削除可能？
        public bool CanDeleteFile()
        {
            return Preference.Current.file_permit_command && CanRemoveFile(Book?.GetViewPage());
        }

        // ファイルを削除する
        public async void DeleteFile()
        {
            if (CanDeleteFile())
            {
                await RemoveFile(Book?.GetViewPage());
            }
        }

        // ファイル削除可能？
        public bool CanRemoveFile(Page page)
        {
            if (page == null) return false;
            if (!page.Entry.IsFileSystem) return false;
            return (File.Exists(page.GetFilePlace()));
        }

        // ファイルを削除する
        public async Task RemoveFile(Page page)
        {
            if (page == null) return;

            var path = page.GetFilePlace();

            if (Preference.Current.file_remove_confirm)
            {
                bool isDirectory = System.IO.Directory.Exists(path);
                string itemType = isDirectory ? "フォルダー" : "ファイル";

                // ビジュアル作成
                var dockPanel = new DockPanel();

                var message = new TextBlock();
                message.Text = $"この{itemType}をごみ箱に移動しますか？";
                message.Margin = new System.Windows.Thickness(0, 0, 0, 10);
                DockPanel.SetDock(message, Dock.Top);
                dockPanel.Children.Add(message);

                var thumbnail = await new PageVisual(page).CreateVisualContentAsync(new System.Windows.Size(64, 64), true);
                if (thumbnail != null)
                {
                    thumbnail.Margin = new System.Windows.Thickness(0, 0, 20, 0);
                    dockPanel.Children.Add(thumbnail);
                }

                var textblock = new TextBlock();
                textblock.Text = Path.GetFileName(path);
                textblock.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                textblock.Margin = new System.Windows.Thickness(0, 0, 0, 2);
                dockPanel.Children.Add(textblock);

                //
                var dialog = new MessageDialog(dockPanel, $"{itemType}を削除します");
                dialog.Commands.Add(UICommands.Remove);
                dialog.Commands.Add(UICommands.Cancel);
                var answer = dialog.ShowDialog();

                if (answer != UICommands.Remove) return;
            }

            // 削除実行
            bool isRemoved = await RemoveFileAsync(path);

            // ページを本から削除
            if (isRemoved == true && Book != null)
            {
                Book.RequestRemove(this, page);
            }
        }


        // ファイルを削除する
        public async Task<bool> RemoveFileAsync(string path)
        {
            int retryCount = 1;

            Retry:

            try
            {
                // 開いている本を閉じる
                if (this.Address == path)
                {
                    await RequestUnload(true).WaitAsync();
                }

                // ゴミ箱に捨てる
                bool isDirectory = System.IO.Directory.Exists(path);
                if (isDirectory)
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(path, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                }
                else
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(path, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                }

                //
                return true;
            }
            catch (Exception ex)
            {
                if (retryCount > 0)
                {
                    await Task.Delay(1000);
                    retryCount--;
                    goto Retry;
                }
                else
                {
                    var dialog = new MessageDialog($"削除できませんでした。もう一度実行しますか？\n\n原因: {ex.Message}", "削除できませんでした。リトライしますか？");
                    dialog.Commands.Add(UICommands.Retry);
                    dialog.Commands.Add(UICommands.Cancel);
                    var confirm = dialog.ShowDialog();
                    if (confirm == UICommands.Retry)
                    {
                        retryCount = 1;
                        goto Retry;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        // ファイルの名前を変える
        public async Task<bool> RenameFileAsync(string src, string dst)
        {
            int retryCount = 1;

            Retry:

            try
            {
                bool isContinue = false;
                int requestLoadCount = _requestLoadCount;

                // 開いている本を閉じる
                if (this.Address == src)
                {

                    isContinue = true;
                    await RequestUnload(false).WaitAsync();
                }

                // rename
                if (System.IO.Directory.Exists(src))
                {
                    System.IO.Directory.Move(src, dst);
                }
                else
                {
                    System.IO.File.Move(src, dst);
                }

                if (isContinue && requestLoadCount == _requestLoadCount)
                {
                    RenameHistory(src, dst);
                    RequestLoad(dst, null, BookLoadOption.Resume, false);
                }

                return true;
            }
            catch (Exception ex)
            {
                if (retryCount > 0)
                {
                    await Task.Delay(1000);
                    retryCount--;
                    goto Retry;
                }

                var confirm = new MessageDialog($"名前の変更に失敗しました。もう一度実行しますか？\n\n{ex.Message}", "名前を変更できませんでした");
                confirm.Commands.Add(UICommands.Retry);
                confirm.Commands.Add(UICommands.Cancel);
                var answer = confirm.ShowDialog();
                if (answer == UICommands.Retry)
                {
                    retryCount = 1;
                    goto Retry;
                }
                else
                {
                    return false;
                }
            }
        }

        // 履歴上のファイル名変更
        private void RenameHistory(string src, string dst)
        {
            BookMementoCollection.Current.Rename(src, dst);
            PagemarkCollection.Current.Rename(src, dst);
        }


        #region Memento

        /// <summary>
        /// BookHub Memento
        /// </summary>
        [DataContract]
        public class Memento : BindableBase
        {
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

            [DataMember(Order = 19, EmitDefaultValue = false)]
            public PageEndAction PageEndAction { get; set; } // no used (ver.23)

            [DataMember(EmitDefaultValue = false)]
            public bool IsSlideShowByLoop { get; set; } // no used (ver.22)

            [DataMember(EmitDefaultValue = false)]
            public double SlideShowInterval { get; set; } // no used (ver.22)

            [DataMember(Order = 7, EmitDefaultValue = false)]
            public bool IsCancelSlideByMouseMove { get; set; } // no used (ver.22)

            [DataMember(EmitDefaultValue = false)]
            public Book.Memento BookMemento { get; set; } // no used (v.23)

            [DataMember(Order = 2, EmitDefaultValue = false)]
            public bool IsEnarbleCurrentDirectory { get; set; } // no used

            [DataMember(Order = 4)]
            public bool IsSupportArchiveFile { get; set; }

            [DataMember(Order = 4)]
            public ExternalApplication ExternalApplication { get; set; }

            [DataMember(Order = 5, EmitDefaultValue = false)]
            public bool AllowPagePreLoad { get; set; } // no used

            [DataMember(Order = 6)]
            public bool IsConfirmRecursive { get; set; }

            [DataMember(Order = 6, EmitDefaultValue = false)]
            public Book.Memento BookMementoDefault { get; set; } // no used (v.23)

            [DataMember(Order = 6, EmitDefaultValue = false)]
            public bool IsUseBookMementoDefault { get; set; } // no used (v.23)

            [DataMember(Order = 10)]
            public ClipboardUtility ClipboardUtility { get; set; }

            [DataMember(Order = 10)]
            public bool IsAutoRecursive { get; set; }

            [DataMember(Order = 22)]
            public bool IsAutoRecursiveWithAllFiles { get; set; }

            [DataMember(Order = 19, EmitDefaultValue = false)]
            public BookMementoFilter HistoryMementoFilter { get; set; } // no used (v.23)

            [DataMember(Order = 19)]
            public PreLoadMode PreLoadMode { get; set; }

            [DataMember(Order = 20, EmitDefaultValue = false)]
            public string Home { get; set; }



            //
            private void Constructor()
            {
                _Version = App.Config.ProductVersionNumber;

                IsEnableNoSupportFile = false;
                IsSupportArchiveFile = true;
                ExternalApplication = new ExternalApplication();
                ClipboardUtility = new ClipboardUtility();
                IsAutoRecursive = true;
                IsAutoRecursiveWithAllFiles = true;
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
                // before 1.19
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
            memento.IsSupportArchiveFile = IsSupportArchiveFile;
            memento.ExternalApplication = ExternalApllication.Clone();
            memento.IsConfirmRecursive = IsConfirmRecursive;
            memento.ClipboardUtility = ClipboardUtility.Clone();
            memento.IsAutoRecursive = IsAutoRecursive;
            memento.IsAutoRecursiveWithAllFiles = IsAutoRecursiveWithAllFiles;
            memento.PreLoadMode = PreLoadMode;
            memento.Home = Home;

            return memento;
        }

        // memento反映
        public void Restore(Memento memento)
        {
            IsEnableAnimatedGif = memento.IsEnableAnimatedGif;
            IsEnableExif = memento.IsEnableExif;
            IsEnableNoSupportFile = memento.IsEnableNoSupportFile;
            IsSupportArchiveFile = memento.IsSupportArchiveFile;
            ExternalApllication = memento.ExternalApplication.Clone();
            IsConfirmRecursive = memento.IsConfirmRecursive;
            ClipboardUtility = memento.ClipboardUtility.Clone();
            IsAutoRecursive = memento.IsAutoRecursive;
            IsAutoRecursiveWithAllFiles = memento.IsAutoRecursiveWithAllFiles;
            PreLoadMode = memento.PreLoadMode;
            Home = memento.Home;

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
                BookOperation.Current.PageEndAction = memento.PageEndAction;

                BookSetting.Current.BookMemento = memento.BookMemento.Clone();
                BookSetting.Current.BookMementoDefault = memento.BookMementoDefault.Clone();
                BookSetting.Current.IsUseBookMementoDefault = memento.IsUseBookMementoDefault;
                BookSetting.Current.HistoryMementoFilter = memento.HistoryMementoFilter;
            }
        }

        #endregion
    }
}

