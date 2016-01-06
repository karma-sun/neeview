using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{




    public class BookHub
    {
        // いろんなイベントをするー
        public event EventHandler<bool> BookChanged;
        public event EventHandler<int> PageChanged;
        public event EventHandler<string> SettingChanged;
        public event EventHandler<string> Loaded;
        public event EventHandler<string> InfoMessage;
        public event EventHandler ViewContentsChanged;

        // アニメGIF
        #region Property: IsEnableAnimatedGif
        private bool _IsEnableAnimatedGif;
        public bool IsEnableAnimatedGif
        {
            get { return _IsEnableAnimatedGif; }
            set { _IsEnableAnimatedGif = value; Page.IsEnableAnimatedGif = value; }
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
                    //DartyBook?.Invoke(this, null);
                    Book_DartyBook(this, null); // ##
                }
            }
        }

        // 履歴から設定を復元する
        public bool IsEnabledAutoNextFolder { get; set; } = false;



        // いろんなパラメータをするー？
        // カレントでいいんじゃ？

        public static Book Current { get; private set; }

        // デフォルト本設定
        //public BookCommonSetting BookCommonSetting { get; set; }
        public Book.Memento BookMemento { get; set; }

        public BookHub()
        {
            //BookCommonSetting = new BookCommonSetting();
            BookMemento = new Book.Memento();
        }

        public Book.Memento StoreBookSetting()
        {
            if (Current != null)
            {
                return Current.CreateMemento();
            }
            return BookMemento.Clone();
        }

        //private bool _IsLoading = false;

        // いろんなメソッドは置き換え
        public async void Load(string path, Book.LoadFolderOption option = Book.LoadFolderOption.None)
        {
            /*
            if (_IsLoading)
            {
                Debug.WriteLine("Already Loading.");
                return;
            }
            */

            //try
            //{
            // _IsLoading = true;

            var current = Current;
            Current = null;

            // 履歴の保存
            ModelContext.BookHistory.Add(current);

            // 後始末
            current?.Dispose();

            // 新しい本
            var book = new Book();

            string start = null;

            bool isBookamrk = false;

            // 設定の復元
            //BookCommonSetting.Restore(book);

            //
            if (IsEnableNoSupportFile)
            {
                option |= Book.LoadFolderOption.SupportAllFile;
            }

            // 設定の復元
            if ((option & Book.LoadFolderOption.ReLoad) == Book.LoadFolderOption.ReLoad)
            {
                // リロード時は設定そのまま
                book.Restore(BookMemento); //.Restore(book);
            }
            else
            {
                if (IsEnableHistory)
                {
                    // 履歴が有るときはそれを使用する
                    var setting = ModelContext.BookHistory.Find(path);
                    if (setting != null && IsEnableHistory)
                    {
                        BookMemento = setting.Clone(); // setting.Restore(this);
                        book.Restore(BookMemento); // setting.Restore(book);
                        start = setting.BookMark;
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

            // リカーシブ設定
            if ((option & Book.LoadFolderOption.Recursive) == Book.LoadFolderOption.Recursive)
            {
                book.IsRecursiveFolder = true;
            }


            try
            {
                // 読み込み。非同期で行う。
                Loaded?.Invoke(this, path);

                await book.Load(path, start, option);
            }
            catch (Exception e)
            {
                // ファイル読み込み失敗通知
                Messenger.MessageBox(this, $"{path} の読み込みに失敗しました。\n\n理由：{e.Message}", "通知", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);

                // 現在表示されているコンテンツを無効
                ViewContentsChanged?.Invoke(this, null);

                // 履歴から消去
                ModelContext.BookHistory.Remove(path);
                Messenger.Send(this, "UpdateLastFiles");
                return;
            }
            finally
            {
                Loaded?.Invoke(this, null);
            }
            book.PageChanged += (s, e) => PageChanged?.Invoke(s, e);
            book.ViewContentsChanged += (s, e) => ViewContentsChanged?.Invoke(s, e);
            book.PageTerminated += Book_PageTerminated;
            book.DartyBook += Book_DartyBook;

            // カレント切り替え
            Current = book;

            // 開始
            Current.Start();

            BookMemento = book.CreateMemento(); // Store(book);
            SettingChanged?.Invoke(this, null);

            BookChanged?.Invoke(this, isBookamrk);


            // サブフォルダ確認
            if ((option & Book.LoadFolderOption.ReLoad) == 0 && Current.Pages.Count <= 0 && !Current.IsRecursiveFolder && Current.SubFolderCount > 0)
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
                    //_IsLoading = false;
                    Load(Current.Place, Book.LoadFolderOption.Recursive | Book.LoadFolderOption.ReLoad);
                }
            }
            //}
            //finally
            //{
            //    //_IsLoading = false;
            //}
        }

        private void Book_DartyBook(object sender, EventArgs e)
        {
            if (Current != null)
            {
                Load(Current.Place, Book.LoadFolderOption.ReLoad);
            }
        }

        //
        private void Book_PageTerminated(object sender, int e)
        {
            if (IsEnabledAutoNextFolder)
            {
                if (e < 0)
                {
                    PrevFolder(Book.LoadFolderOption.LastPage);
                }
                else
                {
                    NextFolder(Book.LoadFolderOption.FirstPage);
                }
            }
            else
            {
                if (e < 0)
                {
                    InfoMessage?.Invoke(this, "最初のページです");
                }
                else
                {
                    InfoMessage?.Invoke(this, "最後のページです");
                }
            }
        }

        public int GetPageIndex()
        {
            return Current == null ? 0 : Current.Index;
        }

        public void SetPageIndex(int index)
        {
            if (Current != null) Current.Index = index;
        }

        public int GetPageCount()
        {
            return Current == null ? 0 : Current.Pages.Count - 1;
        }

        // 次のフォルダに移動
        public bool MoveFolder(int direction, Book.LoadFolderOption option)
        {
            if (Current == null) return false;

            string place = File.Exists(Current.Place) ? Path.GetDirectoryName(Current.Place) : Current.Place;

            if (Directory.Exists(place))
            {
                var entries = Directory.GetFileSystemEntries(Path.GetDirectoryName(Current.Place)); //.ToList();

                // ディレクトリ、アーカイブ以外は除外
                var directories = entries.Where(e => Directory.Exists(e)).ToList();
                directories.Sort((a, b) => Win32Api.StrCmpLogicalW(a, b));
                var archives = entries.Where(e => ModelContext.ArchiverManager.IsSupported(e)).ToList();
                archives.Sort((a, b) => Win32Api.StrCmpLogicalW(a, b));

                directories.AddRange(archives);

                int index = directories.IndexOf(Current.Place);
                if (index < 0) return false;

                int next = 0;

                if ((option & Book.LoadFolderOption.RandomFolder) == Book.LoadFolderOption.RandomFolder)
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

        public void PrevPage()
        {
            Current?.PrevPage();
        }

        public void NextPage()
        {
            Current?.NextPage();
        }

        public void PrevOnePage()
        {
            Current?.PrevPage(1);
        }

        public void NextOnePage()
        {
            Current?.NextPage(1);
        }

        public void FirstPage()
        {
            Current?.FirstPage();
        }

        public void LastPage()
        {
            Current?.LastPage();
        }

        public void NextFolder(Book.LoadFolderOption option = Book.LoadFolderOption.None)
        {
            bool result = MoveFolder(+1, option);
            if (!result)
            {
                InfoMessage?.Invoke(this, "次のフォルダはありません");
            }
        }

        public void PrevFolder(Book.LoadFolderOption option = Book.LoadFolderOption.None)
        {
            bool result = MoveFolder(-1, option);
            if (!result)
            {
                InfoMessage?.Invoke(this, "前のフォルダはありません");
            }
        }

        public void RandomFolder(Book.LoadFolderOption option = Book.LoadFolderOption.None)
        {
            bool result = MoveFolder(0, option | Book.LoadFolderOption.RandomFolder);
            if (!result)
            {
                InfoMessage?.Invoke(this, "次のフォルダはありません");
            }
        }


        private void RefleshBookSetting()
        {
            Current?.Restore(BookMemento); //.Restore(Current);
            SettingChanged?.Invoke(this, null);
        }


        public void ToggleIsSupportedTitlePage()
        {
            BookMemento.IsSupportedTitlePage = !BookMemento.IsSupportedTitlePage;
            RefleshBookSetting(); // BookSetting.Restore(Current);
        }

        public void ToggleIsSupportedWidePage()
        {
            BookMemento.IsSupportedWidePage = !BookMemento.IsSupportedWidePage;
            RefleshBookSetting(); //BookSetting.Restore(Current);
        }

        public void ToggleIsRecursiveFolder()
        {
            BookMemento.IsRecursiveFolder = !BookMemento.IsRecursiveFolder;
            RefleshBookSetting(); //BookSetting.Restore(Current);
        }


        public void SetBookReadOrder(BookReadOrder order)
        {
            BookMemento.BookReadOrder = order;
            RefleshBookSetting(); //BookSetting.Restore(Current);
        }

        public void ToggleBookReadOrder()
        {
            BookMemento.BookReadOrder = BookMemento.BookReadOrder.GetToggle();
            RefleshBookSetting(); //BookSetting.Restore(Current);
        }


        public void SetPageMode(int mode)
        {
            BookMemento.PageMode = mode;
            RefleshBookSetting(); //BookSetting.Restore(Current);
        }

        public void TogglePageMode()
        {
            BookMemento.PageMode = 3 - BookMemento.PageMode;
            RefleshBookSetting(); //BookSetting.Restore(Current);
        }

        public void ToggleSortMode()
        {
            var mode = BookMemento.SortMode.GetToggle();
            Current?.SetSortMode(mode);
            BookMemento.SortMode = mode;
            RefleshBookSetting(); //BookSetting.Restore(Current);
        }

        public void SetSortMode(BookSortMode mode)
        {
            Current?.SetSortMode(mode);
            BookMemento.SortMode = mode;
            RefleshBookSetting(); //BookSetting.Restore(Current);
        }

        public void ToggleIsReverseSort()
        {
            BookMemento.IsReverseSort = !BookMemento.IsReverseSort;
            RefleshBookSetting(); //BookSetting.Restore(Current);
        }


        // すべての本に共通の設定
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
            public Book.Memento BookMemento { get; set; }

            //
            private void Constructor()
            {
                IsEnableHistory = true;
                IsEnableNoSupportFile = false;
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


        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsEnableAnimatedGif = IsEnableAnimatedGif;
            memento.IsEnableHistory = IsEnableHistory;
            memento.IsEnableNoSupportFile = IsEnableNoSupportFile;
            memento.IsEnabledAutoNextFolder = IsEnabledAutoNextFolder;
            memento.BookMemento = BookMemento.Clone();

            return memento;
        }


        public void Restore(Memento memento)
        {
            IsEnableAnimatedGif = memento.IsEnableAnimatedGif;
            IsEnableHistory = memento.IsEnableHistory;
            IsEnableNoSupportFile = memento.IsEnableNoSupportFile;
            IsEnabledAutoNextFolder = memento.IsEnabledAutoNextFolder;
            BookMemento = memento.BookMemento.Clone();

            // これは・・・要検証
            if (BookHub.Current != null)
            {
                BookHub.Current.Reflesh();
            }
        }
    }
}

