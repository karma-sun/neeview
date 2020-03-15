
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// 本の状態
    /// </summary>
    public class BookAccessor
    {
        public string Path => BookOperation.Current.Book?.Address;

        public bool IsMedia => BookOperation.Current.Book?.IsMedia == true;

        public bool IsNew => BookOperation.Current.Book?.IsNew == true;

        public BookConfigAccessor Config { get; } = new BookConfigAccessor();

        public int PageSize
        {
            get
            {
                var book = BookOperation.Current.Book;
                return (book != null) ? book.Pages.Count : 0;
            }
        }

        public int ViewPageSize
        {
            get
            {
                var book = BookOperation.Current.Book;
                return (book != null) ? book.Viewer.ViewPageCollection.Collection.Count : 0;
            }
        }

        // NOTE: index is 1 start
        public PageAccessor Page(int index)
        {
            var book = BookOperation.Current.Book;
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
            var book = BookOperation.Current.Book;
            if (book != null)
            {
                if (index >= 0 && index < book.Viewer.ViewPageCollection.Collection.Count)
                {
                    return new PageAccessor(book.Viewer.ViewPageCollection.Collection[index].Page);
                }
            }
            return null;
        }
    }
}
