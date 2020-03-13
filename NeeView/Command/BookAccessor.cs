
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// 本の状態
    /// </summary>
    public class BookAccessor
    {
        public string Path => Book()?.Address;

        public bool IsMedia => Book()?.IsMedia == true;

        public bool IsNew => Book()?.IsNew == true;

        public int PageSize
        {
            get
            {
                var book = Book();
                return (book != null) ? book.Pages.Count : 0;
            }
        }

        public int ViewPageSize
        {
            get
            {
                var book = Book();
                return (book != null) ? book.Viewer.ViewPageCollection.Collection.Count : 0;
            }
        }

        // NOTE: index is 1 start
        public PageAccessor Page(int index)
        {
            var book = Book();
            if (book != null)
            {
                var id = book.Pages.ClampPageNumber(index - 1);
                if (id == index - 1)
                {
                    return new PageAccessor(book.Pages[id]);
                }
            }
            return null;
        }

        public PageAccessor ViewPage(int index)
        {
            var book = Book();
            if (book != null)
            {
                if (index >= 0 && index < book.Viewer.ViewPageCollection.Collection.Count)
                {
                    return new PageAccessor(book.Viewer.ViewPageCollection.Collection[index].Page);
                }
            }
            return null;
        }

        private Book Book() => BookOperation.Current.Book;
    }

    public class PageAccessor
    {
        private Page _page;

        public PageAccessor(Page page)
        {
            _page = page;
        }

        public string Path => _page.SystemPath;

        public double Width => _page.Size.Width;
        public double Height => _page.Size.Height;
    }
}
