// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class PagemarkList : BindableBase
    {
        /// <summary>
        /// PanelListItemStyle property.
        /// </summary>
        public PanelListItemStyle PanelListItemStyle
        {
            get { return _panelListItemStyle; }
            set { if (_panelListItemStyle != value) { _panelListItemStyle = value; RaisePropertyChanged(); } }
        }

        //
        private PanelListItemStyle _panelListItemStyle;


        // メッセージ通知
        public event EventHandler<string> InfoMessage;


        //
        private BookHub _bookHub;

        //
        private BookOperation _bookOperation;

        //
        public PagemarkList(BookHub bookHub, BookOperation bookOperation)
        {
            _bookHub = bookHub;
            _bookOperation = bookOperation;
        }


        //
        public void RequestLoad(Pagemark mark)
        {
            if (mark == null) return;

            bool isJumped = _bookOperation.JumpPagemarkInPlace(mark);
            if (!isJumped)
            {
                _bookHub.RequestLoad(mark.Place, mark.EntryName, BookLoadOption.None, false);
            }
        }

        //
        internal void UpdatePagemark(Pagemark mark)
        {
            _bookOperation.UpdatePagemark(mark);
        }

        //
        public void PrevPagemark()
        {
            if (_bookHub.IsLoading) return;

            if (!ModelContext.Pagemarks.CanMoveSelected(-1))
            {
                InfoMessage?.Invoke(this, "前のページマークはありません");
                return;
            }

            Pagemark mark = ModelContext.Pagemarks.MoveSelected(-1);
            RequestLoad(mark);
        }

        //
        public void NextPagemark()
        {
            if (_bookHub.IsLoading) return;

            if (!ModelContext.Pagemarks.CanMoveSelected(+1))
            {
                InfoMessage?.Invoke(this, "次のページマークはありません");
                return;
            }

            Pagemark mark = ModelContext.Pagemarks.MoveSelected(+1);
            RequestLoad(mark);
        }


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public PanelListItemStyle PanelListItemStyle { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.PanelListItemStyle = this.PanelListItemStyle;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.PanelListItemStyle = memento.PanelListItemStyle;
        }

        #endregion
    }
}
