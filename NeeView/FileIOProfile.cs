// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView
{
    public class FileIOProfile : BindableBase
    {
        public static FileIOProfile Current { get; private set; }

        private bool _isEnabled = true;


        public FileIOProfile()
        {
            Current = this;
        }


        [PropertyMember("ファイル削除時に確認ダイアログを表示する")]
        public bool IsRemoveConfirmed { get; set; } = true;

        [PropertyMember("ファイル操作有効")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { if (_isEnabled != value) { _isEnabled = value; RaisePropertyChanged(); } }
        }


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember, DefaultValue(true)]
            public bool IsRemoveConfirmed { get; set; }

            [DataMember, DefaultValue(true)]
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
