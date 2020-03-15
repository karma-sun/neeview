namespace NeeView
{
    public class BookConfigAccessor
    {
        // PageMode
        public int ViewPageSize
        {
            get { return BookSettingPresenter.Current.LatestSetting.PageMode == PageMode.WidePage ? 2 : 1; }
            set { BookSettingPresenter.Current.SetPageMode(value == 2 ? PageMode.WidePage : PageMode.SinglePage); }
        }


        // [Parameter(typeof(BookReadOrder))]
        public string BookReadOrder
        {
            get { return BookSettingPresenter.Current.LatestSetting.BookReadOrder.ToString(); }
            set { BookSettingPresenter.Current.SetBookReadOrder(value.ToEnum<PageReadOrder>()); }
        }

        public bool IsSupportedDividePage
        {
            get { return BookSettingPresenter.Current.LatestSetting.IsSupportedDividePage; }
            set { BookSettingPresenter.Current.SetIsSupportedDividePage(value); }
        }

        public bool IsSupportedSingleFirstPage
        {
            get { return BookSettingPresenter.Current.LatestSetting.IsSupportedSingleFirstPage; }
            set { BookSettingPresenter.Current.SetIsSupportedSingleFirstPage(value); }
        }

        public bool IsSupportedSingleLastPage
        {
            get { return BookSettingPresenter.Current.LatestSetting.IsSupportedSingleLastPage; }
            set { BookSettingPresenter.Current.SetIsSupportedSingleLastPage(value); }
        }

        public bool IsSupportedWidePage
        {
            get { return BookSettingPresenter.Current.LatestSetting.IsSupportedWidePage; }
            set { BookSettingPresenter.Current.SetIsSupportedWidePage(value); }
        }

        public bool IsRecursiveFolder
        {
            get { return BookSettingPresenter.Current.LatestSetting.IsRecursiveFolder; }
            set { BookSettingPresenter.Current.SetIsRecursiveFolder(value); }
        }

        // [Parameter(typeof(PageSortMode))]
        public string SortMode
        {
            get { return BookSettingPresenter.Current.LatestSetting.SortMode.ToString(); }
            set { BookSettingPresenter.Current.SetSortMode(value.ToEnum<PageSortMode>()); }
        }
    }



}
