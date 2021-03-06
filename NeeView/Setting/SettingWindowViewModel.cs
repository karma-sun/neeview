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
        private string _searchKeyword;
        private SettingPage _currentPage;
        private SettingPage _lastPage;


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
            get { return _currentPage == _model.SearchPage; }
        }

        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set
            {
                if (SetProperty(ref _searchKeyword, value))
                {
                    if (!string.IsNullOrWhiteSpace(_searchKeyword))
                    {
                        _model.ClearPageContentCache();
                        _model.UpdateSearchPage(_searchKeyword);
                        CurrentPage = _model.SearchPage;
                    }
                    else
                    {
                        if (IsSearchPageSelected && _lastPage != null)
                        {
                            _lastPage.IsSelected = true;
                        }
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
                _lastPage = settingPage;
                SearchKeyword = "";
            }
        }
    }
}
