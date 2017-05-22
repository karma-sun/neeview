// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using NeeView.Windows.Input;
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

            BookHub.Current.SettingChanged +=
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
            get { return BookHub.Current.BookMemento; }
        }


        //
        internal List<string> GetHistory(int direction, int size)
        {
            return ModelContext.BookHistory.ListUp(BookHub.Current.BookUnit?.Address, direction, size);
        }

        /// <summary>
        /// MoveToHistory command.
        /// </summary>
        private RelayCommand<string> _MoveToHistory;
        public RelayCommand<string> MoveToHistory
        {
            get { return _MoveToHistory = _MoveToHistory ?? new RelayCommand<string>(MoveToHistory_Executed); }
        }

        private void MoveToHistory_Executed(string item)
        {
            if (item == null) return;
            BookHub.Current.RequestLoad(item, null, BookLoadOption.KeepHistoryOrder | BookLoadOption.SelectHistoryMaybe, true);
        }
    }
}
