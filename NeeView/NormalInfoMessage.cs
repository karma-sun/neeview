// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// 画面に表示する通知：通常
    /// </summary>
    public class NormalInfoMessage : INotifyPropertyChanged
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
        /// BookMementoIcon property.
        /// </summary>
        public BookMementoType BookMementoIcon
        {
            get { return _BookMementoIcon; }
            set { if (_BookMementoIcon != value) { _BookMementoIcon = value; RaisePropertyChanged(); } }
        }

        private BookMementoType _BookMementoIcon;

        /// <summary>
        /// DispTime property. (sec)
        /// </summary>
        public double DispTime
        {
            get { return _dispTime; }
            set { if (_dispTime != value) { _dispTime = value; RaisePropertyChanged(); } }
        }

        private double _dispTime = 1.0;


        // 通知テキスト
        private string _message;
        public string Message
        {
            get { return _message; }
            set { _message = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 通知
        /// </summary>
        /// <param name="message"></param>
        /// <param name="dispTime"></param>
        /// <param name="bookmarkType"></param>
        public void SetMessage(string message, double dispTime = 1.0, BookMementoType bookmarkType = BookMementoType.None)
        {
            this.BookMementoIcon = bookmarkType;
            this.DispTime = dispTime;
            this.Message = message;
        }
    }
}
