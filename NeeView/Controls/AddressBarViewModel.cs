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
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="model"></param>
        public AddressBarViewModel(AddressBar model)
        {
            _model = model;

            NeeView.BookSetting.Current.SettingChanged +=
               (s, e) => RaisePropertyChanged(nameof(BookSetting));
        }


        /// <summary>
        /// Model property.
        /// </summary>
        public AddressBar Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        private AddressBar _model;


        //
        public Dictionary<CommandType, RoutedUICommand> BookCommands
        {
            get { return RoutedCommandTable.Current.Commands; }
        }


        // 本設定 公開
        public Book.Memento BookSetting
        {
            get { return NeeView.BookSetting.Current.BookMemento; }
        }


        //
        internal List<KeyValuePair<int, QueryPath>> GetHistory(int direction, int size)
        {
            return BookHubHistory.Current.GetHistory(direction, size);
        }

        /// <summary>
        /// MoveToHistory command.
        /// </summary>
        private RelayCommand<KeyValuePair<int, QueryPath>> _MoveToHistory;
        public RelayCommand<KeyValuePair<int, QueryPath>> MoveToHistory
        {
            get { return _MoveToHistory = _MoveToHistory ?? new RelayCommand<KeyValuePair<int, QueryPath>>(MoveToHistory_Executed); }
        }

        private void MoveToHistory_Executed(KeyValuePair<int, QueryPath> item)
        {
            BookHubHistory.Current.MoveToHistory(item);
        }
    }
}
