// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeLaboratory.ComponentModel;
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
        private PanelListItemStyle _panelListItemStyle;
        private BookHub _bookHub;
        private BookOperation _bookOperation;

        //
        public PagemarkList(BookHub bookHub, BookOperation bookOperation)
        {
            _bookHub = bookHub;
            _bookOperation = bookOperation;
        }


        public PanelListItemStyle PanelListItemStyle
        {
            get { return _panelListItemStyle; }
            set { if (_panelListItemStyle != value) { _panelListItemStyle = value; RaisePropertyChanged(); } }
        }

        public bool IsThumbnailVisibled
        {
            get
            {
                switch (_panelListItemStyle)
                {
                    default:
                        return false;
                    case PanelListItemStyle.Content:
                        return ThumbnailProfile.Current.ThumbnailWidth > 0.0;
                    case PanelListItemStyle.Banner:
                        return ThumbnailProfile.Current.BannerWidth > 0.0;
                }
            }
        }


        //
        public void RequestLoad(Pagemark mark)
        {
            if (mark == null) return;

            bool isJumped = _bookOperation.JumpPagemarkInPlace(mark);
            if (!isJumped)
            {
                _bookHub.RequestLoad(mark.Place, mark.EntryName, BookLoadOption.IsPage, true);
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

            if (!PagemarkCollection.Current.CanMoveSelected(-1))
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, "前のページマークはありません");
                return;
            }

            Pagemark mark = PagemarkCollection.Current.MoveSelected(-1);
            RequestLoad(mark);
        }

        //
        public void NextPagemark()
        {
            if (_bookHub.IsLoading) return;

            if (!PagemarkCollection.Current.CanMoveSelected(+1))
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, "次のページマークはありません");
                return;
            }

            Pagemark mark = PagemarkCollection.Current.MoveSelected(+1);
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
