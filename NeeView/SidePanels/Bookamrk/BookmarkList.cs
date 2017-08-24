// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
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
    public class BookmarkList : BindableBase
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
        

        //
        public BookHub BookHub { get; private set; }

        //
        public BookmarkList(BookHub bookHub)
        {
            this.BookHub = bookHub;
        }



        #region Command

        // ブックマークを戻る
        public void PrevBookmark()
        {
            if (BookHub.IsLoading) return;

            if (!BookmarkCollection.Current.CanMoveSelected(-1))
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, "前のブックマークはありません");
                return;
            }

            var unit = BookmarkCollection.Current.MoveSelected(-1);
            if (unit != null)
            {
                BookHub.RequestLoad(unit.Value.Memento.Place, null, BookLoadOption.SkipSamePlace, false);
            }
        }


        // ブックマークを進む
        public void NextBookmark()
        {
            if (BookHub.IsLoading) return;

            if (!BookmarkCollection.Current.CanMoveSelected(+1))
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, "次のブックマークはありません");
                return;
            }

            var unit = BookmarkCollection.Current.MoveSelected(+1);
            if (unit != null)
            {
                BookHub.RequestLoad(unit.Value.Memento.Place, null, BookLoadOption.SkipSamePlace, false);
            }
        }

        #endregion



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
