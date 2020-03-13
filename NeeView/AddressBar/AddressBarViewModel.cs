using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Windows.Data;
using System.Collections.Generic;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// AddresBar : ViewModel
    /// </summary>
    public class AddressBarViewModel : BindableBase
    {
        private AddressBar _model;
        private RelayCommand<KeyValuePair<int, QueryPath>> _moveToHistory;
        private DelayValue<bool> _isLoading;


        public AddressBarViewModel(AddressBar model)
        {
            _model = model;

            BookSettingPresenter.Current.SettingChanged +=
               (s, e) => RaisePropertyChanged(nameof(BookSetting));

            _isLoading = new DelayValue<bool>();
            _isLoading.ValueChanged += (s, e) => RaisePropertyChanged(nameof(IsLoading));
            BookHub.Current.Loading += BookHub_Loading;
        }


        public AddressBar Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        public ThemeProfile ThemeProfile
        {
            get { return ThemeProfile.Current; }
        }

        public bool IsLoading => _isLoading.Value;

        public Dictionary<string, RoutedUICommand> BookCommands
        {
            get { return RoutedCommandTable.Current.Commands; }
        }

        public BookSetting BookSetting
        {
            get { return NeeView.BookSettingPresenter.Current.LatestSetting; }
        }


        public List<KeyValuePair<int, QueryPath>> GetHistory(int direction, int size)
        {
            return BookHubHistory.Current.GetHistory(direction, size);
        }


        public RelayCommand<KeyValuePair<int, QueryPath>> MoveToHistory
        {
            get { return _moveToHistory = _moveToHistory ?? new RelayCommand<KeyValuePair<int, QueryPath>>(MoveToHistory_Executed); }
        }


        private void BookHub_Loading(object sender, BookHubPathEventArgs e)
        {
            if (e.Path != null)
            {
                _isLoading.SetValue(true, 1000);
            }
            else
            {
                _isLoading.SetValue(false, 0);
            }
        }

        private void MoveToHistory_Executed(KeyValuePair<int, QueryPath> item)
        {
            BookHubHistory.Current.MoveToHistory(item);
        }
    }
}
