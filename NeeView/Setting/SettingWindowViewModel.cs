using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView.Setting
{
    /// <summary>
    /// 設定画面 ViewModel
    /// </summary>
    public class SettingWindowViewModel : BindableBase
    {
        private SettingWindowModel _model;
        private bool _isSearchPageSelected;
        private string _searchKeyword;
        private SettingPage _currentPage;


        public SettingWindowViewModel(SettingWindowModel model)
        {
            _model = model;
        }


        public SettingWindowModel Model
        {
            get { return _model; }
            set { SetProperty(ref _model, value); }
        }

        public bool IsSearchPageSelected
        {
            get { return _isSearchPageSelected; }
            set { SetProperty(ref _isSearchPageSelected, value); }
        }

        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set
            {
                if (SetProperty(ref _searchKeyword, value))
                {
                    if (!string.IsNullOrWhiteSpace(_searchKeyword) || IsSearchPageSelected)
                    {
                        _model.UpdateSearchPage(_searchKeyword);
                        CurrentPage = _model.SearchPage;
                        IsSearchPageSelected = true;
                    }
                }
            }
        }

        public SettingPage CurrentPage
        {
            get { return _currentPage; }
            set
            {
                if (SetProperty(ref _currentPage, value))
                {
                    _model.SetSelectedPage(_currentPage);
                }
            }
        }


        public void SelectedItemChanged(SettingPage settingPage)
        {
            if (settingPage != null)
            {
                CurrentPage = settingPage;
                IsSearchPageSelected = false;
                SearchKeyword = "";
            }
        }
    }
}
