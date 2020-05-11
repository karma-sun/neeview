using System.ComponentModel;

namespace NeeView
{
    public class PageListPresenter
    {
        private PageListView _pageListView;
        private PageList _pageList;
        private PageListBox _pageListBox;
        private PageListBoxViewModel _listBoxViewModel;


        public PageListPresenter(PageListView pageListView, PageList pageList)
        {
            _pageListView = pageListView;
            _pageList = pageList;

            _listBoxViewModel = new PageListBoxViewModel(_pageList);

            Config.Current.PageList.AddPropertyChanged(nameof(PageListConfig.PanelListItemStyle), (s, e) => UpdateListBoxContent());

            UpdateListBoxContent();
        }


        private void UpdateListBoxContent()
        {
            _pageListBox = new PageListBox(_listBoxViewModel);
            _pageListView.ListBoxContent.Content = _pageListBox;
        }

        public void FocusAtOnce()
        {
            _listBoxViewModel.FocusAtOnce = true;
        }
    }
}
