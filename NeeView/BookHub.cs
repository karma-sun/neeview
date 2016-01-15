// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        public event EventHandler ViewContentsChanged;

        // スライドショーモード変更通知
        public event EventHandler<bool> SlideShowModeChanged;

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
                    Current?.Reflesh(); // 表示更新
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

        // フォルダの並び順
        public FolderOrder FolderOrder { get; set; }

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



        // 現在の本
        public Book Current { get; private set; }

        // 本の設定、引き継ぎ用
        public Book.Memento BookMemento { get; set; } = new Book.Memento();


        // ページ表示開始スレッドイベント
        private ManualResetEvent _ViewContentEvent = new ManualResetEvent(false);


        /// <summary>
        /// 本を読み込む
        /// </summary>
        /// <param name="path">本のパス</param>
        /// <param name="option">読み込みオプション</param>
        public async void Load(string path, BookLoadOption option)
        {
            // 履歴の保存
            ModelContext.BookHistory.Add(Current);

            // 現在の本を開放
            Current?.Dispose();
            Current = null;

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
                    var setting = ModelContext.BookHistory.Find(path);
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
                book.DartyBook += (s, e) => ReLoad();

                // 最初のコンテンツ表示待ち設定
                _ViewContentEvent.Reset();
                book.ViewContentsChanged += (s, e) => _ViewContentEvent.Set();

                // カレントを設定し、開始する
                Current = book;
                Current.Start();

                // 最初のコンテンツ表示待ち
                await Task.Run(()=>_ViewContentEvent.WaitOne());
            }
            catch (Exception e)
            {
                // 後始末
                Current?.Dispose();
                Current = null;

                // 現在表示されているコンテンツを無効
                ViewContentsChanged?.Invoke(this, null);

                // ファイル読み込み失敗通知
                Messenger.MessageBox(this, $"{path} の読み込みに失敗しました。\n\n理由：{e.Message}", "通知", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);

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

            // 本の設定を退避
            BookMemento = book.CreateMemento();
            SettingChanged?.Invoke(this, null);

            // 本の変更通知
            BookChanged?.Invoke(this, isBookamrk);


            // サブフォルダ確認
            if ((option & BookLoadOption.ReLoad) == 0 && Current.Pages.Count <= 0 && !Current.IsRecursiveFolder && Current.SubFolderCount > 0)
            {
                var message = new MessageEventArgs("MessageBox");
                message.Parameter = new MessageBoxParams()
                {
                    MessageBoxText = $"\"{Current.Place}\" には読み込めるファイルがありません。\n\nサブフォルダ(書庫)も読み込みますか？",
                    Caption = "確認",
                    Button = System.Windows.MessageBoxButton.YesNo,
                    Icon = System.Windows.MessageBoxImage.Question
                };
                Messenger.Send(this, message);

                if (message.Result == true)
                {
                    Load(Current.Place, BookLoadOption.Recursive | BookLoadOption.ReLoad);
                }
            }
        }


        // 再読み込み
        private void ReLoad()
        {
            if (Current != null)
            {
                Load(Current.Place, BookLoadOption.ReLoad);
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
            return Current == null ? 0 : Current.Index.Index;
        }

        // 現在ページ番号設定 (先読み無し)
        public void SetPageIndex(int index)
        {
            if (Current != null)
            {
                Current.IsEnablePreLoad = false;
                Current.SetIndex(new PageValue(index, 0), 1);
                Current.IsEnablePreLoad = true;
            }
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

                // 日付順は逆順にする (エクスプローラー標準にあわせる)
                if (folderOrder == FolderOrder.TimeStamp)
                {
                    directories.Reverse();
                }

                int index = directories.IndexOf(Current.Place);
                if (index < 0) return false;

                int next = 0;

                if (folderOrder == FolderOrder.Random)
                {
                    directories.RemoveAt(index);
                    if (directories.Count <= 0) return false;
                    next = new Random().Next(directories.Count);
                }
                else
                {
                    next = index + direction;
                    if (next < 0 || next >= directories.Count) return false;
                }

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

        // 単ページ/見開き表示設定
        public void SetPageMode(int mode)
        {
            BookMemento.PageMode = mode;
            RefleshBookSetting();
        }

        // 単ページ/見開き表示トグル取得
        public int GetTogglePageMode()
        {
            return 3 - BookMemento.PageMode;
        }

        // 単ページ/見開き表示トグル
        public void TogglePageMode()
        {
            BookMemento.PageMode = GetTogglePageMode();
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

        // ページ並び逆順設定
        public void ToggleIsReverseSort()
        {
            BookMemento.IsReverseSort = !BookMemento.IsReverseSort;
            RefleshBookSetting();
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

            //
            private void Constructor()
            {
                IsEnableHistory = true;
                IsEnableNoSupportFile = false;
                FolderOrder = FolderOrder.FileName;
                IsSlideShowByLoop = true;
                SlideShowInterval = 5.0;
                BookMemento = new Book.Memento();
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
            memento.IsEnableHistory = IsEnableHistory;
            memento.IsEnableNoSupportFile = IsEnableNoSupportFile;
            memento.IsEnabledAutoNextFolder = IsEnabledAutoNextFolder;
            memento.FolderOrder = FolderOrder;
            memento.IsSlideShowByLoop = IsSlideShowByLoop;
            memento.SlideShowInterval = SlideShowInterval;
            memento.BookMemento = BookMemento.Clone();

            return memento;
        }

        // memento反映
        public void Restore(Memento memento)
        {
            IsEnableAnimatedGif = memento.IsEnableAnimatedGif;
            IsEnableHistory = memento.IsEnableHistory;
            IsEnableNoSupportFile = memento.IsEnableNoSupportFile;
            IsEnabledAutoNextFolder = memento.IsEnabledAutoNextFolder;
            FolderOrder = memento.FolderOrder;
            IsSlideShowByLoop = memento.IsSlideShowByLoop;
            SlideShowInterval = memento.SlideShowInterval;
            BookMemento = memento.BookMemento.Clone();
        }

        #endregion
    }
}

