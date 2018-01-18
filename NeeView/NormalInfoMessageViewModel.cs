// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using System;
using System.ComponentModel;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// NormalInfoMessage : ViewModel
    /// </summary>
    public class NormalInfoMessageViewModel : BindableBase
    {
        /// <summary>
        /// ChangeCount property.
        /// 表示の更新通知に利用される。
        /// </summary>
        public int ChangeCount
        {
            get { return _changeCount; }
            set { if (_changeCount != value) { _changeCount = value; RaisePropertyChanged(); } }
        }

        private int _changeCount;



        /// <summary>
        /// Model property.
        /// </summary>
        public NormalInfoMessage Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        private NormalInfoMessage _model;


        //
        public TimeSpan DispTime => TimeSpan.FromSeconds(_model.DispTime);

        //
        public Visibility Visibility => string.IsNullOrEmpty(_model.Message) ? Visibility.Collapsed : Visibility.Visible;

        //
        public Visibility BookmarkIconVisibility => _model.BookMementoIcon == BookMementoType.Bookmark ? Visibility.Visible : Visibility.Collapsed;

        //
        public Visibility HistoryIconVisibility => _model.BookMementoIcon == BookMementoType.History ? Visibility.Visible : Visibility.Collapsed;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="model"></param>
        public NormalInfoMessageViewModel(NormalInfoMessage model)
        {
            _model = model;

            _model.AddPropertyChanged(nameof(_model.Message),
                (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(_model.Message)) ChangeCount++;
                    RaisePropertyChanged(nameof(Visibility));
                });

            _model.AddPropertyChanged(nameof(_model.BookMementoIcon),
                (s, e) =>
                {
                    RaisePropertyChanged(nameof(BookmarkIconVisibility));
                    RaisePropertyChanged(nameof(HistoryIconVisibility));
                });

            _model.AddPropertyChanged(nameof(_model.DispTime),
                (s, e) =>
                {
                    RaisePropertyChanged(nameof(DispTime));
                });
        }
    }

}
