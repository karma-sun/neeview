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

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        //
        #region Property: ContextMenu
        private ContextMenu _contextMenu;
        public ContextMenu ContextMenu
        {
            get
            {
                _contextMenu = _contextMenu ?? SourceTree.CreateContextMenu();
                return _contextMenu;
            }
        }
        #endregion

        #region Property: SourceTree
        [DataMember]
        private MenuTree _sourceTree;
        public MenuTree SourceTree
        {
            get { return _sourceTree ?? MenuTree.CreateDefault(); }
            set
            {
                _sourceTree = value;
                _contextMenu = null;
            }
        }
        #endregion

        //
        public ContextMenuSetting Clone()
        {
            var clone = (ContextMenuSetting)this.MemberwiseClone();
            clone._sourceTree = _sourceTree?.Clone();
            clone._contextMenu = null;
            return clone;
        }

        //
        public void Validate()
        {
            _sourceTree?.Validate();
        }
    }
}
