using NeeLaboratory.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class PageSortModePaletteModel : BindableBase
    {
        public PageSortModePaletteModel()
        {
            BookOperation.Current.BookChanged += BookOperation_BookChanged;

            UpdatePageSortModeList();
        }

        private List<PageSortMode> _pageSortModeList;
        public List<PageSortMode> PageSortModeList
        {
            get { return _pageSortModeList; }
            set { SetProperty(ref _pageSortModeList, value); }
        }

        private PageSortModeClass _pageSortModeClass = PageSortModeClass.Full;
        public PageSortModeClass PageSortModeClass
        {
            get { return _pageSortModeClass; }
            set
            {
                if (SetProperty(ref _pageSortModeClass, value))
                {
                    UpdatePageSortModeList();
                }
            }
        }

        private void UpdatePageSortModeList()
        {
            PageSortModeList = _pageSortModeClass.GetPageSortModeMap().Select(e => e.Key).ToList();
    }

        private void BookOperation_BookChanged(object sender, BookChangedEventArgs e)
        {
            PageSortModeClass = e.Book != null ? e.Book.PageSortModeClass : PageSortModeClass.Full;
        }
    }

}
