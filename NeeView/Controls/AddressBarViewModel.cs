// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
        internal List<string> GetHistory(int direction, int size)
        {
            return BookHistory.Current.ListUp(BookHub.Current.BookUnit?.Address, direction, size);
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
