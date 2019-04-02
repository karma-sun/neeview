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



        // ソートされた
        public event EventHandler PagesSorted;

        // ファイル削除された
        public event EventHandler<PageChangedEventArgs> PageRemoved;



        // この本のアーカイバ
        public ArchiveEntryCollection ArchiveEntryCollection { get; private set; }

        // メディアアーカイバ？
        public bool IsMedia => ArchiveEntryCollection?.Archiver is MediaArchiver;

        // ページマークアーカイバ？
        public bool IsPagemarkFolder => ArchiveEntryCollection?.Archiver is PagemarkArchiver;


        public List<Page> Pages { get; private set; }

        public int Count => Pages.Count;

        public int IndexOf(Page page) => Pages.IndexOf(page);

        public Page First() => Pages.First();

        public Page Last() => Pages.Last();


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

            Sort();
        }


        public PageSortMode SortMode
        {
            get => _sortMode;
            set => SetProperty(ref _sortMode, value);
            ////RequestSort(this); TODO: PropertyChangedイベントで処理する
        }

        public Page this[int index]
        {
            get { return Pages[index]; }
            set { Pages[index] = value; }
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

        //
        public Page GetPage(string name) => Pages.FirstOrDefault(e => e.EntryFullName == name);

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

            switch (SortMode)
            {
                case PageSortMode.FileName:
                    Pages = Pages.OrderBy(e => e.PageType).ThenBy(e => e, new ComparerFileName()).ToList();
                    break;
                case PageSortMode.FileNameDescending:
                    Pages = Pages.OrderBy(e => e.PageType).ThenByDescending(e => e, new ComparerFileName()).ToList();
                    break;
                case PageSortMode.TimeStamp:
                    Pages = Pages.OrderBy(e => e.PageType).ThenBy(e => e.Entry.LastWriteTime).ThenBy(e => e, new ComparerFileName()).ToList();
                    break;
                case PageSortMode.TimeStampDescending:
                    Pages = Pages.OrderBy(e => e.PageType).ThenByDescending(e => e.Entry.LastWriteTime).ThenBy(e => e, new ComparerFileName()).ToList();
                    break;
                case PageSortMode.Size:
                    Pages = Pages.OrderBy(e => e.PageType).ThenBy(e => e.Entry.Length).ThenBy(e => e, new ComparerFileName()).ToList();
                    break;
                case PageSortMode.SizeDescending:
                    Pages = Pages.OrderBy(e => e.PageType).ThenByDescending(e => e.Entry.Length).ThenBy(e => e, new ComparerFileName()).ToList();
                    break;
                case PageSortMode.Random:
                    var random = new Random();
                    Pages = Pages.OrderBy(e => e.PageType).ThenBy(e => random.Next()).ToList();
                    break;
                default:
                    throw new NotImplementedException();
            }

            // ページ ナンバリング
            PagesNumbering();

            PagesSorted?.Invoke(this, null);
        }

        /// <summary>
        /// ページ番号設定
        /// </summary>
        private void PagesNumbering()
        {
            for (int i = 0; i < Pages.Count; ++i) Pages[i].Index = i;
        }

        /// <summary>
        /// ファイル名ソート用比較クラス
        /// </summary>
        private class ComparerFileName : IComparer<Page>
        {
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
                            return xIsDirectory ? -1 : 1;
                        }
                        return NativeMethods.StrCmpLogicalW(xName[i], yName[i]);
                    }
                }

                return xName.Length - yName.Length;
            }
        }

        #endregion

        #region ページの削除

        // ページの削除
        public void Remove(Page page)
        {
            if (Pages.Count <= 0) return;

            int index = Pages.IndexOf(page);
            if (index < 0) return;

            Pages.RemoveAt(index);

            PagesNumbering();

            PageRemoved?.Invoke(this, new PageChangedEventArgs(page));

#if false
            index = ClampPageNumber(index);

            // TODO: ## BookCommandEngine へ。
            RequestSetPosition(this, new PagePosition(index, 0), 1);

            // TODO: ## BookPageMarkerへ
            if (_pageMap.TryGetValue(page.EntryFullName, out Page target) && page == target)
            {
                _pageMap.Remove(page.EntryFullName);
            }
#endif
        }

        #endregion


    }
}
