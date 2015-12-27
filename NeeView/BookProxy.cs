using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class BookProxy
    {
        // いろんなイベントをするー
        public event EventHandler<Book> BookChanged;
        public event EventHandler<int> PageChanged;
        public event EventHandler<string> SettingChanged;
        public event EventHandler<bool> Loaded;
        public event EventHandler<string> InfoMessage;
        public event EventHandler ViewContentsChanged;


        // いろんなパラメータをするー？
        // カレントでいいんじゃ？

        public static Book Current { get; private set; }

        // デフォルト本設定
        public BookCommonSetting BookCommonSetting { get; set; }
        public BookSetting BookSetting { get; set; }

        public BookProxy()
        {
            BookCommonSetting = new BookCommonSetting();
            BookSetting = new BookSetting();
        }

        public BookSetting StoreBookSetting()
        {
            if (Current != null)
            {
                BookSetting.Store(Current);
            }
            return BookSetting.Clone();
        }


        // いろんなメソッドは置き換え
        public async void Load(string path, Book.LoadFolderOption option = Book.LoadFolderOption.None)
        {
            // 履歴の保存
            ModelContext.BookHistory.Add(Current);

            // 後始末
            Current?.Dispose();
            Current = null;

            // 新しい本
            var book = new Book();

            string start = null;

            // 設定の復元
            BookCommonSetting.Restore(book);

            // 設定の復元
            if (BookCommonSetting.IsEnableHistory)
            {
                var setting = ModelContext.BookHistory.Find(path);
                if (setting != null)
                {
                    setting.Restore(this);
                    setting.Restore(book);
                    start = setting.BookMark;
                }
            }

            // 読み込み。非同期で行う。
            Loaded?.Invoke(this, true);

            await book.Load(path, start, option);

            Loaded?.Invoke(this, false);

            book.PageChanged += (s, e) => PageChanged?.Invoke(s, e);
            //book.PropertyChanged += (s, e) => PropertyChanged?.Invoke(s, e);
            book.ViewContentsChanged += (s, e) => ViewContentsChanged?.Invoke(s, e);

            book.PageTerminated += Book_PageTerminated;
            book.DartyBook += Book_DartyBook;

            // カレント切り替え
            Current = book;

            // 開始
            Current.Start();

            BookChanged?.Invoke(this, Current);
            SettingChanged?.Invoke(this, null);
        }

        private void Book_DartyBook(object sender, EventArgs e)
        {
            Load(Current.Place);
        }

        //
        private void Book_PageTerminated(object sender, int e)
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
                var entries = Directory.GetFileSystemEntries(Path.GetDirectoryName(Current.Place)).ToList();

                // ディレクトリ、アーカイブ以外は除外
                var directories = entries.Where(e => Directory.Exists(e) || ModelContext.ArchiverManager.IsSupported(e)).ToList();

                // TODO: ディレクトリの並び順ソート

                int index = directories.IndexOf(Current.Place);
                if (index < 0) return false;

                int next = index + direction;
                if (next < 0 || next >= directories.Count) return false;

                Load(directories[next], option);
            }

            return true;
        }

        public void PrevPage()
        {
            Current?.PrevPage();
        }

        public void NextPage()
        {
            Current?.NextPage();
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


        private void RefleshBookSetting()
        {
            BookSetting.Restore(Current);
            SettingChanged?.Invoke(this, null);
        }


        public void ToggleIsSupportedTitlePage()
        {
            BookSetting.IsSupportedTitlePage = !BookSetting.IsSupportedTitlePage;
            RefleshBookSetting(); // BookSetting.Restore(Current);
        }

        public void ToggleIsSupportedWidePage()
        {
            BookSetting.IsSupportedWidePage = !BookSetting.IsSupportedWidePage;
            RefleshBookSetting(); //BookSetting.Restore(Current);
        }

        public void ToggleIsRecursiveFolder()
        {
            BookSetting.IsRecursiveFolder = !BookSetting.IsRecursiveFolder;
            RefleshBookSetting(); //BookSetting.Restore(Current);
        }


        public void SetBookReadOrder(BookReadOrder order)
        {
            BookSetting.BookReadOrder = order;
            RefleshBookSetting(); //BookSetting.Restore(Current);
        }

        public void ToggleBookReadOrder()
        {
            BookSetting.BookReadOrder = BookSetting.BookReadOrder.GetToggle();
            RefleshBookSetting(); //BookSetting.Restore(Current);
        }


        public void SetPageMode(int mode)
        {
            BookSetting.PageMode = mode;
            RefleshBookSetting(); //BookSetting.Restore(Current);
        }

        public void TogglePageMode()
        {
            BookSetting.PageMode = 3 - BookSetting.PageMode;
            RefleshBookSetting(); //BookSetting.Restore(Current);
        }

        public void ToggleSortMode()
        {
            var mode = BookSetting.SortMode.GetToggle();
            Current?.SetSortMode(mode);
            BookSetting.SortMode = mode;
            RefleshBookSetting(); //BookSetting.Restore(Current);
        }

        public void SetSortMode(BookSortMode mode)
        {
            Current?.SetSortMode(mode);
            BookSetting.SortMode = mode;
            RefleshBookSetting(); //BookSetting.Restore(Current);
        }

        public void ToggleIsReverseSort()
        {
            BookSetting.IsReverseSort = !BookSetting.IsReverseSort;
            RefleshBookSetting(); //BookSetting.Restore(Current);
        }
    }
}
