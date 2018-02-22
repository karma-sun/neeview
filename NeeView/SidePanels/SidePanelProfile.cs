// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.Windows.Property;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView
{
    public class SidePanelProfile
    {
        public static SidePanelProfile Current { get; private set; }

        public SidePanelProfile()
        {
            Current = this;
        }

        [PropertyMember("サイドパネルでの左右キー有効", Tips = "サイドパネルでの左右キー操作を有効にします。フォルダーリストではフォルダーの階層移動を行います。")]
        public bool IsLeftRightKeyEnabled { get; set; } = true;

        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember, DefaultValue(true)]
            public bool IsLeftRightKeyEnabled { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsLeftRightKeyEnabled = this.IsLeftRightKeyEnabled;

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.IsLeftRightKeyEnabled = memento.IsLeftRightKeyEnabled;
        }

        #endregion
    }
}
