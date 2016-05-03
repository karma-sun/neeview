// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NeeView
{
    //
    [DataContract]
    public class ContextMenuSetting : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        //
        #region Property: ContextMenu
        private ContextMenu _ContextMenu;
        public ContextMenu ContextMenu
        {
            get
            {
                _ContextMenu = _ContextMenu ?? SourceTree.CreateContextMenu();
                return _ContextMenu;
            }
        }
        #endregion

        #region Property: SourceTree
        private MenuTree _SourceTree;
        [DataMember]
        public MenuTree SourceTree
        {
            get { return _SourceTree ?? MenuTree.CreateDefault(); }
            set
            {
                _SourceTree = value;
                _ContextMenu = null;
            }
        }
        #endregion

        [DataMember]
        public bool IsEnabled { get; set; }

        [DataMember]
        public bool IsOpenByCtrl { get; set; }

        [DataMember]
        public bool IsOpenByGesture { get; set; }


        #region Property: MouseGesture
        private string _MouseGesture = "D";
        [DataMember]
        public string MouseGesture
        {
            get { return _MouseGesture; }
            set { _MouseGesture = value; OnPropertyChanged(); }
        }
        #endregion

        //
        public ContextMenuSetting Clone()
        {
            var clone = (ContextMenuSetting)this.MemberwiseClone();
            clone._SourceTree = this._SourceTree?.Clone();
            clone._ContextMenu = null;
            return clone;
        }
    }


}
