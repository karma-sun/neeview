// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// 本の管理
    /// ロード、本の操作はここを通す
    /// </summary>
    public class BookHub
    {
        #region Events

        // 本の変更通知
        public event EventHandler<bool> BookChanged;

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

        #endregion


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
                    Current?.RequestReflesh(true); // 表示更新
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
                    Current?.RequestReflesh(true); // 表示更新
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
                    ReLoad(true);
                }
            }
        }

        // 履歴から設定を復元する
        public bool IsEnabledAutoNextFolder { get; set; } = false;

        // フォルダの並び順
        public FolderOrder _FolderOrder;
        public FolderOrder FolderOrder
        {
            get { return _FolderOrder; }
            set { _FolderOrder = value; _FolderOrderSeed = new Random().Next(); }
        }

        // フォルダのランダムな並び用シード
        public int _FolderOrderSeed;

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

        //「ファイルを開く」の初期フォルダを現在開いているフォルダ基準にする
        public bool IsEnarbleCurrentDirectory { get; set; }

        // 圧縮ファイルの有効/無効
        #region Property: IsSupportArchiveFile
        public bool IsSupportArchiveFile
        {
            get { return ModelContext.ArchiverManager.IsEnabled; }
            set { ModelContext.ArchiverManager.IsEnabled = value;}
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
                if (Current != null) Current.AllowPreLoad = _AllowPagePreLoad;
            }
        }
        #endregion


        // 現在の本
        public Book Current { get; private set; }

        // 本の設定、引き継ぎ用
        public Book.Memento BookMemento { get; set; } = new Book.Memento();

        // 外部アプリ設定
        public ExternalApplication ExternalApllication { get; set; } = new ExternalApplication();


        // ページ表示開始スレッドイベント
        private ManualResetEvent _ViewContentEvent = new ManualResetEvent(false);

        /// <summary>
        /// 本の開放
        /// </summary>
        public void Unload(bool isClearViewContent)
        {
            // 履歴の保存
            ModelContext.BookHistory.Add(Current);

            // 現在の本を開放
            Current?.Dispose();
            Current = null;

            // 現在表示されているコンテンツを無効
            if (isClearViewContent)
            {
                ViewContentsChanged?.Invoke(this, null);
            }
        }


        /// <summary>
        /// 本を読み込む
        /// </summary>
        /// <param name="path">本のパス</param>
        /// <param name="option">読み込みオプション</param>
        public async void Load(string path, BookLoadOption option)
        {
            // 現在の本を開放
            Unload(false);

            // 新しい本を作成
            var book = new Book();

            // 開始エントリ
            string startEntry = null;

            // 履歴を使用したか
            bool isBookamrk = false;

            // 設定の復元
            if ((option & BookLoadOption.ReLoad) == BookLoadOption.ReLoad)
            {
                // リロード時は設定そのまま
                book.Restore(BookMemento);
            }
            else
            {
                if (IsEnableHistory)
                {
                    // 履歴が有るときはそれを使用する
                    var setting = ModelContext.BookHistory.Find(GetArchivePath(path));
                    if (setting != null && IsEnableHistory)
                    {
                        BookMemento = setting.Clone();
                        book.Restore(BookMemento);
                        startEntry = setting.BookMark;
                        isBookamrk = true;
                    }
                    // 履歴がないときは設定はそのまま。再帰設定のみOFFにする。
                    else
                    {
                        book.Restore(BookMemento); //.Restore(book);
                        book.IsRecursiveFolder = false;
                    }
                }
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
                // Now Loading ON
                Loading?.Invoke(this, path);

                // ロード。非同期で行う
                await book.Load(path, startEntry, option);

                // ロード後にイベント設定
                book.PageChanged += (s, e) => PageChanged?.Invoke(s, e);
                book.ViewContentsChanged += (s, e) => ViewContentsChanged?.Invoke(s, e);
                book.PageTerminated += OnPageTerminated;
                book.DartyBook += (s, e) => ReLoad(true);

                // 最初のコンテンツ表示待ち設定
                _ViewContentEvent.Reset();
                book.ViewContentsChanged += (s, e) => _ViewContentEvent.Set();

                // カレントを設定し、開始する
                Current = book;
                Current.Start();

                // 最初のコンテンツ表示待ち
                await Task.Run(() => _ViewContentEvent.WaitOne());
            }
            catch (Exception e)
            {
                // 後始末
                Current?.Dispose();
                Current = null;

                // 現在表示されているコンテンツを無効
                ViewContentsChanged?.Invoke(this, null);

                // ファイル読み込み失敗通知
                EmptyMessage?.Invoke(this, $"{path} の読み込みに失敗しました。\n{e.Message}");

                // 履歴から消去
                ModelContext.BookHistory.Remove(path);
                Messenger.Send(this, "UpdateLastFiles");

                return;
            }
            finally
            {
                // Now Loading OFF
                Loading?.Invoke(this, null);
            }

            // ビュー初期化
            ModelContext.CommandTable[CommandType.ViewReset].Execute(null);

            // 本の設定を退避
            BookMemento = book.CreateMemento();
            SettingChanged?.Invoke(this, null);

            // 本の変更通知
            BookChanged?.Invoke(this, isBookamrk);

            if (Current.Pages.Count <= 0)
            {
                EmptyMessage?.Invoke(this, $"\"{Current.Place}\" には読み込めるファイルがありません");
            }

            // サブフォルダ確認
            if ((option & BookLoadOption.ReLoad) == 0 && Current.Pages.Count <= 0 && !Current.IsRecursiveFolder && Current.SubFolderCount > 0)
            {
                var message = new MessageEventArgs("MessageBox");
                message.Parameter = new MessageBoxParams()
                {
                    MessageBoxText = $"\"{Current.Place}\" には読み込めるファイルがありません。\n\nサブフォルダ(書庫)も読み込みますか？",
                    Caption = "確認",
                    Button = System.Windows.MessageBoxButton.YesNo,
                    Icon = MessageBoxExImage.Question
                };
                Messenger.Send(this, message);

                if (message.Result == true)
                {
                    Load(Current.Place, BookLoadOption.Recursive | BookLoadOption.ReLoad);
                }
            }
        }

        //
        public string GetArchivePath(string path)
        {
            if (File.Exists(path) && !ModelContext.ArchiverManager.IsSupported(path))
            {
                return Path.GetDirectoryName(path);
            }
            else
            {
                return path;
            }
        }

        // 再読み込み
        public void ReLoad(bool isKeepSetting)
        {
            if (Current != null)
            {
                Load(Current.Place, isKeepSetting ? BookLoadOption.ReLoad : BookLoadOption.None);
            }
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
            return Current == null ? 0 : Current.DisplayIndex; // GetPosition().Index;
        }

        // 現在ページ番号設定 (先読み無し)
        public void SetPageIndex(int index)
        {
            Current?.RequestSetPosition(new PagePosition(index, 0), 1, false);
        }

        // 総ページ数取得
        public int GetPageCount()
        {
            return Current == null ? 0 : Current.Pages.Count - 1;
        }


        /// <summary>
        /// フォルダ移動
        /// </summary>
        /// <param name="direction">移動方向</param>
        /// <param name="folderOrder">フォルダの並び</param>
        /// <param name="option">ロードオプション</param>
        /// <returns></returns>
        private bool MoveFolder(int direction, FolderOrder folderOrder, BookLoadOption option)
        {
            if (Current == null) return false;

            string place = File.Exists(Current.Place) ? Path.GetDirectoryName(Current.Place) : Current.Place;

            if (Directory.Exists(place))
            {
                var entries = Directory.GetFileSystemEntries(Path.GetDirectoryName(Current.Place)); //.ToList();

                // ディレクトリ、アーカイブ以外は除外
                var directories = entries.Where(e => Directory.Exists(e)).ToList();
                if (folderOrder == FolderOrder.TimeStamp)
                {
                    directories = directories.OrderBy((e) => Directory.GetLastWriteTime(e)).ToList();
                }
                else
                {
                    directories.Sort((a, b) => Win32Api.StrCmpLogicalW(a, b));
                }
                var archives = entries.Where(e => ModelContext.ArchiverManager.IsSupported(e)).ToList();
                if (folderOrder == FolderOrder.TimeStamp)
                {
                    archives = archives.OrderBy((e) => File.GetLastWriteTime(e)).ToList();
                }
                else
                {
                    archives.Sort((a, b) => Win32Api.StrCmpLogicalW(a, b));
                }

                directories.AddRange(archives);

                // １フォルダしかない場合は移動不可
                if (directories.Count <= 1) return false;

                // 日付順は逆順にする (エクスプローラー標準にあわせる)
                if (folderOrder == FolderOrder.TimeStamp)
                {
                    directories.Reverse();
                }
                // ランダムに並べる
                else if (folderOrder == FolderOrder.Random)
                {
                    var random = new Random(_FolderOrderSeed);
                    directories = directories.OrderBy(e => random.Next()).ToList();
                }

                int index = directories.IndexOf(Current.Place);
                if (index < 0) return false;

                int next = (folderOrder == FolderOrder.Random)
                    ? (index + directories.Count + direction) % directories.Count
                    : index + direction;

                if (next < 0 || next >= directories.Count) return false;

                Load(directories[next], option);

                return true;
            }

            return false;
        }

        // 前のページに移動
        public void PrevPage()
        {
            Current?.PrevPage();
        }

        // 次のページに移動
        public void NextPage()
        {
            Current?.NextPage();
        }

        // 1ページ前に移動
        public void PrevOnePage()
        {
            Current?.PrevPage(1);
        }

        // 1ページ後に移動
        public void NextOnePage()
        {
            Current?.NextPage(1);
        }

        // 最初のページに移動
        public void FirstPage()
        {
            Current?.FirstPage();
        }

        // 最後のページに移動
        public void LastPage()
        {
            Current?.LastPage();
        }

        // スライドショー用：次のページへ移動
        public void NextSlide()
        {
            if (IsEnableSlideShow) NextPage();
        }

        // 次のフォルダに移動
        public void NextFolder(BookLoadOption option = BookLoadOption.None)
        {
            bool result = MoveFolder(+1, FolderOrder, option);
            if (!result)
            {
                InfoMessage?.Invoke(this, "次のフォルダはありません");
            }
        }

        // 前のフォルダに移動
        public void PrevFolder(BookLoadOption option = BookLoadOption.None)
        {
            bool result = MoveFolder(-1, FolderOrder, option);
            if (!result)
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

        // フォルダの並びの変更
        public void ToggleFolderOrder()
        {
            FolderOrder = FolderOrder.GetToggle();
            SettingChanged?.Invoke(this, null);
        }

        // フォルダの並びの設定
        public void SetFolderOrder(FolderOrder order)
        {
            FolderOrder = order;
            SettingChanged?.Invoke(this, null);
        }


        // 本の設定を更新
        private void RefleshBookSetting()
        {
            Current?.Restore(BookMemento);
            SettingChanged?.Invoke(this, null);
        }

        // 先頭ページの単ページ表示ON/OFF 
        public void ToggleIsSupportedSingleFirstPage()
        {
            BookMemento.IsSupportedSingleFirstPage = !BookMemento.IsSupportedSingleFirstPage;
            RefleshBookSetting();
        }

        // 最終ページの単ページ表示ON/OFF 
        public void ToggleIsSupportedSingleLastPage()
        {
            BookMemento.IsSupportedSingleLastPage = !BookMemento.IsSupportedSingleLastPage;
            RefleshBookSetting();
        }

        // 横長ページの分割ON/OFF
        public void ToggleIsSupportedDividePage()
        {
            BookMemento.IsSupportedDividePage = !BookMemento.IsSupportedDividePage;
            RefleshBookSetting();
        }

        // 横長ページの見開き判定ON/OFF
        public void ToggleIsSupportedWidePage()
        {
            BookMemento.IsSupportedWidePage = !BookMemento.IsSupportedWidePage;
            RefleshBookSetting();
        }

        // フォルダ再帰読み込みON/OFF
        public void ToggleIsRecursiveFolder()
        {
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
            BookMemento.PageMode = BookMemento.PageMode.GetToggle();
            RefleshBookSetting();
        }

        // ページ並び変更
        public void ToggleSortMode()
        {
            var mode = BookMemento.SortMode.GetToggle();
            Current?.SetSortMode(mode);
            BookMemento.SortMode = mode;
            RefleshBookSetting();
        }

        // ページ並び設定
        public void SetSortMode(PageSortMode mode)
        {
            Current?.SetSortMode(mode);
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
                    ExternalApllication.Call(Current?.GetViewPages());
                }
                catch (Exception e)
                {
                    Messenger.MessageBox(this, $"外部アプリ実行に失敗しました\n\n原因: {e.Message}", "エラー", System.Windows.MessageBoxButton.OK, MessageBoxExImage.Error);
                }
            }
        }


        // ファイルの場所を開くことが可能？
        public bool CanOpenFilePlace()
        {
            return Current?.GetViewPage() != null;
        }

        // ファイルの場所を開く
        public void OpenFilePlace()
        {
            if (CanOpenFilePlace())
            {
                string place = Current.GetViewPage()?.GetFilePlace();
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
            if (IsEnarbleCurrentDirectory && Current != null)
            {
                return Path.GetDirectoryName(Current.Place);
            }
            else
            {
                return "";
            }
        }



        // ファイルに保存する
        public void Export()
        {
            if (Current != null && CanOpenFilePlace())
            {
                try
                {
                    var pages = Current.GetViewPages();
                    int index = Current.GetViewPageindex() + 1;
                    string name = $"{Path.GetFileNameWithoutExtension(Current.Place)}_{index:000}-{index + pages.Count - 1:000}.png";
                    var exporter = new Exporter();
                    exporter.Initialize(pages, Current.BookReadOrder, name);
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
            var page = Current?.GetViewPage();
            if (page == null) return false;
            if (!page.IsFile()) return false;
            return (File.Exists(page.GetFilePlace()));
        }

        // ファイルを削除する
        public void DeleteFile()
        {
            if (CanDeleteFile())
            {
                var page = Current?.GetViewPage();
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

                // 削除確認
                var param = new MessageBoxParams()
                {
                    Caption = "削除の確認",
                    MessageBoxText = "このファイルをごみ箱に移動しますか？",
                    Button = System.Windows.MessageBoxButton.OKCancel,
                    Icon = MessageBoxExImage.RecycleBin,
                    VisualContent = stackPanel,
                };
                var result = Messenger.Send(this, new MessageEventArgs("MessageBox") { Parameter = param });

                // 削除する
                if (result == true)
                {
                    try
                    {
                        // ゴミ箱に捨てる
                        Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(path, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                    }
                    catch (Exception e)
                    {
                        Messenger.MessageBox(this, $"ファイル削除に失敗しました\n\n原因: {e.Message}", "エラー", System.Windows.MessageBoxButton.OK, MessageBoxExImage.Error);
                    }

                    // ページを本から削除
                    Current?.RequestRemove(page);
                }
            }
        }


        #region Memento

        /// <summary>
        /// BookHub Memento
        /// </summary>
        [DataContract]
        public class Memento
        {
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
            public FolderOrder FolderOrder { get; set; }

            [DataMember]
            public bool IsSlideShowByLoop { get; set; }

            [DataMember]
            public double SlideShowInterval { get; set; }

            [DataMember]
            public Book.Memento BookMemento { get; set; }

            [DataMember(Order = 2)]
            public bool IsEnarbleCurrentDirectory { get; set; }

            [DataMember(Order =4)]
            public bool IsSupportArchiveFile { get; set; }

            [DataMember(Order =4)]
            public ExternalApplication ExternalApplication { get; set; }

            [DataMember(Order = 5)]
            public bool AllowPagePreLoad { get; set; }

            //
            private void Constructor()
            {
                IsEnableHistory = true;
                IsEnableNoSupportFile = false;
                FolderOrder = FolderOrder.FileName;
                IsSlideShowByLoop = true;
                SlideShowInterval = 5.0;
                IsSupportArchiveFile = true;
                BookMemento = new Book.Memento();
                ExternalApplication = new ExternalApplication();
                AllowPagePreLoad = true;
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
            memento.FolderOrder = FolderOrder;
            memento.IsSlideShowByLoop = IsSlideShowByLoop;
            memento.SlideShowInterval = SlideShowInterval;
            memento.BookMemento = BookMemento.Clone();
            memento.IsEnarbleCurrentDirectory = IsEnarbleCurrentDirectory;
            memento.IsSupportArchiveFile = IsSupportArchiveFile;
            memento.ExternalApplication = ExternalApllication.Clone();
            memento.AllowPagePreLoad = AllowPagePreLoad;

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
            FolderOrder = memento.FolderOrder;
            IsSlideShowByLoop = memento.IsSlideShowByLoop;
            SlideShowInterval = memento.SlideShowInterval;
            BookMemento = memento.BookMemento.Clone();
            IsEnarbleCurrentDirectory = memento.IsEnarbleCurrentDirectory;
            IsSupportArchiveFile = memento.IsSupportArchiveFile;
            ExternalApllication = memento.ExternalApplication.Clone();
            AllowPagePreLoad = memento.AllowPagePreLoad;
        }

        #endregion
    }


}

