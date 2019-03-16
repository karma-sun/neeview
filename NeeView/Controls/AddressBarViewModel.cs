using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
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


        public AddressBarViewModel(AddressBar model)
        {
            _model = model;

            NeeView.BookSetting.Current.SettingChanged +=
               (s, e) => RaisePropertyChanged(nameof(BookSetting));
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

        public Dictionary<CommandType, RoutedUICommand> BookCommands
        {
            get { return RoutedCommandTable.Current.Commands; }
        }

        public Book.Memento BookSetting
        {
            get { return NeeView.BookSetting.Current.BookMemento; }
        }

        internal List<KeyValuePair<int, QueryPath>> GetHistory(int direction, int size)
        {
            return BookHubHistory.Current.GetHistory(direction, size);
        }


        /// <summary>
        /// MoveToHistory command.
        /// </summary>
        private RelayCommand<KeyValuePair<int, QueryPath>> _moveToHistory;
        public RelayCommand<KeyValuePair<int, QueryPath>> MoveToHistory
        {
            get { return _moveToHistory = _moveToHistory ?? new RelayCommand<KeyValuePair<int, QueryPath>>(MoveToHistory_Executed); }
        }

        private void MoveToHistory_Executed(KeyValuePair<int, QueryPath> item)
        {
            BookHubHistory.Current.MoveToHistory(item);
        }
    }
}
