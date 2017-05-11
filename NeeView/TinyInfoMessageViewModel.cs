// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.ComponentModel;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// TinyInfomessage : ViewModel
    /// </summary>
    public class TinyInfoMessageViewModel : INotifyPropertyChanged
    {
        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion

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
        public TinyInfoMessage Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        private TinyInfoMessage _model;


        //
        public TimeSpan DispTime => TimeSpan.FromSeconds(_model.DispTime);

        //
        public Visibility Visibility => string.IsNullOrEmpty(_model.Message) ? Visibility.Collapsed : Visibility.Visible;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="model"></param>
        public TinyInfoMessageViewModel(TinyInfoMessage model)
        {
            _model = model;

            _model.AddPropertyChanged(nameof(_model.Message),
                (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(_model.Message)) ChangeCount++;
                    RaisePropertyChanged(nameof(Visibility));
                });

            _model.AddPropertyChanged(nameof(_model.DispTime),
                (s, e) =>
                {
                    RaisePropertyChanged(nameof(DispTime));
                });
        }
    }
}
