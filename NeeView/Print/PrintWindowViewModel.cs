// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using NeeView.Windows.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

namespace NeeView
{
    /// <summary>
    /// 
    /// </summary>
    public class PrintWindowCloseEventArgs : EventArgs
    {
        public bool? Result { get; set; }
    }

    /// <summary>
    /// PrintWindow ViewModel
    /// </summary>
    public class PrintWindowViewModel : BindableBase
    {
        /// <summary>
        /// Model property.
        /// </summary>
        private PrintModel _model;
        public PrintModel Model => _model;

        /// <summary>
        /// MainContent property.
        /// </summary>
        private FrameworkElement _MainContent;
        public FrameworkElement MainContent
        {
            get { return _MainContent; }
            set { if (_MainContent != value) { _MainContent = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// PageCollection property.
        /// </summary>
        private List<FixedPage> _PageCollection;
        public List<FixedPage> PageCollection
        {
            get { return _PageCollection; }
            set { if (_PageCollection != value) { _PageCollection = value; RaisePropertyChanged(); } }
        }


        /// <summary>
        /// 設定保存
        /// </summary>
        private static PrintModel.Memento _memento;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="context"></param>
        public PrintWindowViewModel(PrintContext context)
        {
            _model = new PrintModel(context);
            _model.Restore(_memento);

            _model.PropertyChanged += PrintService_PropertyChanged;
            _model.Margin.PropertyChanged += PrintService_PropertyChanged;

            UpdatePreview();
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        public void Closed()
        {
            _memento = _model.CreateMemento();
        }

        /// <summary>
        /// パラメータ変更イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrintService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdatePreview();
        }

        /// <summary>
        /// プレビュー更新
        /// </summary>
        private void UpdatePreview()
        {
            PageCollection = _model.CreatePageCollection();
        }

        /// <summary>
        /// PrintCommand command.
        /// </summary>
        private RelayCommand _PrintCommand;
        public RelayCommand PrintCommand
        {
            get { return _PrintCommand = _PrintCommand ?? new RelayCommand(PrintCommand_Executed); }
        }

        private void PrintCommand_Executed()
        {
            _model.Print();
            Close?.Invoke(this, new PrintWindowCloseEventArgs() { Result = true });
        }


        /// <summary>
        /// CancelCommand command.
        /// </summary>
        private RelayCommand _CancelCommand;
        public RelayCommand CancelCommand
        {
            get { return _CancelCommand = _CancelCommand ?? new RelayCommand(CancelCommand_Executed); }
        }

        public event EventHandler<PrintWindowCloseEventArgs> Close;

        private void CancelCommand_Executed()
        {
            Close?.Invoke(this, new PrintWindowCloseEventArgs() { Result = false });
        }


        /// <summary>
        /// PrintDialogCommand command.
        /// </summary>
        private RelayCommand _PrintDialogCommand;
        public RelayCommand PrintDialogCommand
        {
            get { return _PrintDialogCommand = _PrintDialogCommand ?? new RelayCommand(PrintDialogCommand_Executed); }
        }

        private void PrintDialogCommand_Executed()
        {
            _model.ShowPrintDialog();
            UpdatePreview();
        }
    }
}
