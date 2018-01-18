// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView
{
    public class FileIOProfile : BindableBase
    {
        public static FileIOProfile Current { get; private set; }

        public FileIOProfile()
        {
            Current = this;
        }

        //
        public bool IsRemoveConfirmed { get; set; } = true;

        /// <summary>
        /// IsEnabled property.
        /// </summary>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { if (_isEnabled != value) { _isEnabled = value; RaisePropertyChanged(); } }
        }

        private bool _isEnabled = true;



        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember, DefaultValue(true)]
            [PropertyMember("ファイル削除確認", Tips = "ファイル削除時に確認ダイアログを表示します")]
            public bool IsRemoveConfirmed { get; set; }

            [DataMember, DefaultValue(true)]
            //[PropertyMember("ファイル操作許可", Tips = "削除や名前変更等のファイル操作コマンドを使用可能にします"
            //    , Flags = PropertyMemberFlag.None)]
            public bool IsEnabled { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsRemoveConfirmed = this.IsRemoveConfirmed;
            memento.IsEnabled = this.IsEnabled;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.IsRemoveConfirmed = memento.IsRemoveConfirmed;
            this.IsEnabled = memento.IsEnabled;
        }
        #endregion

    }
}
