﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// 画面に表示する通知：小さく通知
    /// </summary>
    public class TinyInfoMessage : INotifyPropertyChanged
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
        /// Message property.
        /// </summary>
        public string Message
        {
            get { return _Message; }
            set { if (_Message != value) { _Message = value; RaisePropertyChanged(); } }
        }

        private string _Message;


        /// <summary>
        /// DispTime property. (sec)
        /// </summary>
        public double DispTime
        {
            get { return _dispTime; }
            set { if (_dispTime != value) { _dispTime = value; RaisePropertyChanged(); } }
        }

        private double _dispTime = 1.0;


        /// <summary>
        /// 通知
        /// </summary>
        /// <param name="message"></param>
        public void SetMessage(string message)
        {
            this.Message = message;
        }
    }
}