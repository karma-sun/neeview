using NeeLaboratory.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class BookPageCollection : BindableBase, IEnumerable<Page>, IDisposable
    {
        // サムネイル寿命管理
        private PageThumbnailPool _thumbnaulPool = new PageThumbnailPool();

        // ページ列
        private PageSortMode _sortMode = PageSortMode.FileName;


        public BookPageCollection(List<Page> pages, PageSortMode sortMode)
        {
            Pages = pages;
            _sortMode = sortMode;

            foreach (var page in Pages)
            {
                page.Thumbnail.Touched += Thumbnail_Touched;
            }

            for (int i = 0; i < Pages.Count; ++i)
            {
                Pages[i].EntryIndex = i;
            }

            Sort();
        }


        // ソートされた
        public event EventHandler PagesSorted;

        // ファイル削除された
        public event EventHandler<PageRemovedEventArgs> PageRemoved;



        // この本のアーカイバ
        public ArchiveEntryCollection ArchiveEntryCollection { get; private set; }

        // メディアアーカイバ？
        public bool IsMedia => ArchiveEntryCollection?.Archiver is MediaArchiver;


        public List<Page> Pages { get; private set; }

        public PageSortMode SortMode
        {
            get => _sortMode;
            set => SetProperty(ref _sortMode, value);
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    this.ResetPropertyChanged();
                    this.PageRemoved = null;
                    this.PagesSorted = null;

                    if (Pages != null)
                    {
                        Pages.ForEach(e => e?.Dispose());
                    }
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        #region IEnumerable<Page> Support

        public IEnumerator<Page> GetEnumerator()
        {
            return Pages.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region List like

        public Page this[int index]
        {
            get { return Pages[index]; }
            set { Pages[index] = value; }
        }

        public int Count => Pages.Count;

        public int IndexOf(Page page) => Pages.IndexOf(page);

        public Page First() => Pages.First();

        public Page Last() => Pages.Last();

        #endregion

        /// <summary>
        /// サムネイル参照イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Thumbnail_Touched(object sender, EventArgs e)
        {
            var thumb = (Thumbnail)sender;
            _thumbnaulPool.Add(thumb);
        }

        // ページ
        public Page GetPage(int index) => Pages.Count > 0 ? Pages[ClampPageNumber(index)] : null;

        public Page GetPage(string name) => Pages.FirstOrDefault(e => e.EntryFullName == name);

        public Page GetPageWithSystemPath(string name) => Pages.FirstOrDefault(e => e.SystemPath == name);

        // ページ番号
        public int GetIndex(Page page) => Pages.IndexOf(page);

        // 先頭ページの場所
        public PagePosition FirstPosition() => PagePosition.Zero;

        // 最終ページの場所
        public PagePosition LastPosition() => Pages.Count > 0 ? new PagePosition(Pages.Count - 1, 1) : FirstPosition();

        // ページ番号のクランプ
        public int ClampPageNumber(int index)
        {
            if (index > Pages.Count - 1) index = Pages.Count - 1;
            if (index < 0) index = 0;
            return index;
        }

        // ページ場所の有効判定
        public bool IsValidPosition(PagePosition position)
        {
            return (FirstPosition() <= position && position <= LastPosition());
        }


        #region ページの並び替え

        // ページの並び替え
        public void Sort()
        {
            if (Pages.Count <= 0) return;

            var isSortFileFirst = Config.Current.Book.IsSortFileFirst;

            IEnumerable<Page> pages = null;

            switch (SortMode)
            {
                case PageSortMode.FileName:
                    pages = Pages.OrderBy(e => e.PageType).ThenBy(e => e, new ComparerFileName(isSortFileFirst));
                    break;
                case PageSortMode.FileNameDescending:
                    pages = Pages.OrderBy(e => e.PageType).ThenByDescending(e => e, new ComparerFileName(isSortFileFirst));
                    break;
                case PageSortMode.TimeStamp:
                    pages = Pages.OrderBy(e => e.PageType).ThenBy(e => e.Entry.LastWriteTime).ThenBy(e => e, new ComparerFileName(isSortFileFirst));
                    break;
                case PageSortMode.TimeStampDescending:
                    pages = Pages.OrderBy(e => e.PageType).ThenByDescending(e => e.Entry.LastWriteTime).ThenBy(e => e, new ComparerFileName(isSortFileFirst));
                    break;
                case PageSortMode.Size:
                    pages = Pages.OrderBy(e => e.PageType).ThenBy(e => e.Entry.Length).ThenBy(e => e, new ComparerFileName(isSortFileFirst));
                    break;
                case PageSortMode.SizeDescending:
                    pages = Pages.OrderBy(e => e.PageType).ThenByDescending(e => e.Entry.Length).ThenBy(e => e, new ComparerFileName(isSortFileFirst));
                    break;
                case PageSortMode.Random:
                    var random = new Random();
                    pages = Pages.OrderBy(e => e.PageType).ThenBy(e => random.Next());
                    break;
                case PageSortMode.Entry:
                    pages = Pages.OrderBy(e => e.EntryIndex);
                    break;
                case PageSortMode.EntryDescending:
                    pages = Pages.OrderByDescending(e => e.EntryIndex);
                    break;
                default:
                    throw new NotImplementedException();
            }

            Pages = pages.ToList();

            // ページ ナンバリング
            PagesNumbering();

            PagesSorted?.Invoke(this, null);
        }

        /// <summary>
        /// ページ番号設定
        /// </summary>
        private void PagesNumbering()
        {
            for (int i = 0; i < Pages.Count; ++i)
            {
                Pages[i].Index = i;
            }
        }

        /// <summary>
        /// ファイル名ソート用比較クラス
        /// </summary>
        private class ComparerFileName : IComparer<Page>
        {
            private int _sortFileFirstSign;

            public ComparerFileName(bool isSortFileFirst)
            {
                _sortFileFirstSign = isSortFileFirst ? 1 : -1;
            }

            public int Compare(Page x, Page y)
            {
                var xName = x.GetEntryFullNameTokens();
                var yName = y.GetEntryFullNameTokens();

                var limit = Math.Min(xName.Length, yName.Length);
                for (int i = 0; i < limit; ++i)
                {
                    if (xName[i] != yName[i])
                    {
                        var xIsDirectory = i + 1 == xName.Length ? x.Entry.IsDirectory : true;
                        var yIsDirectory = i + 1 == yName.Length ? y.Entry.IsDirectory : true;
                        if (xIsDirectory != yIsDirectory)
                        {
                            return (xIsDirectory ? 1 : -1) * _sortFileFirstSign;
                        }
                        return NaturalSort.Compare(xName[i], yName[i]);
                    }
                }

                return xName.Length - yName.Length;
            }
        }

        #endregion

        #region ページの削除

        // ページの削除
        public void Remove(List<Page> pages)
        {
            if (Pages.Count <= 0) return;
            if (pages == null) return;

            var removes = pages.Where(e => Pages.Contains(e)).ToList();
            if (removes.Count <= 0) return;

            foreach (var page in removes)
            {
                Pages.Remove(page);
            }

            PagesNumbering();

            AppDispatcher.Invoke(() => PageRemoved?.Invoke(this, new PageRemovedEventArgs(removes)));
        }

        // 近くの有効なページを取得
        public Page GetValidPage(Page page)
        {
            var index = page != null ? page.Index : 0;
            var answer = Pages.Skip(index).Concat(Pages.Take(index).Reverse()).FirstOrDefault(e => !e.IsDeleted);
            return answer;
        }

        #endregion

        #region ページリスト用現在ページ表示フラグ

        // 表示中ページ
        private List<Page> _viewPages = new List<Page>();

        /// <summary>
        /// 表示中ページフラグ更新
        /// </summary>
        public void SetViewPageFlag(List<Page> viewPages)
        {
            var hidePages = _viewPages.Where(e => !viewPages.Contains(e));

            foreach (var page in viewPages)
            {
                page.IsVisibled = true;
            }

            foreach (var page in hidePages)
            {
                page.IsVisibled = false;
            }

            _viewPages = viewPages.ToList();
        }

        #endregion

        #region フォルダーの先頭ページを取得

        public int GetNextFolderIndex(int start)
        {
            if (Pages.Count == 0 || !SortMode.IsFileNameCategory() || start < 0)
            {
                return -1;
            }

            string currentFolder = LoosePath.GetDirectoryName(Pages[start].EntryFullName);

            for (int index = start + 1; index < Pages.Count; ++index)
            {
                var folder = LoosePath.GetDirectoryName(Pages[index].EntryFullName);
                if (currentFolder != folder)
                {
                    return index;
                }
            }

            return -1;
        }

        public int GetPrevFolderIndex(int start)
        {
            if (Pages.Count == 0 || !SortMode.IsFileNameCategory() || start < 0)
            {
                return -1;
            }

            if (start == 0)
            {
                return -1;
            }

            string currentFolder = LoosePath.GetDirectoryName(Pages[start - 1].EntryFullName);

            for (int index = start - 1; index > 0; --index)
            {
                var folder = LoosePath.GetDirectoryName(Pages[index - 1].EntryFullName);
                if (currentFolder != folder)
                {
                    return index;
                }
            }

            return 0;
        }

        #endregion
    }
}
