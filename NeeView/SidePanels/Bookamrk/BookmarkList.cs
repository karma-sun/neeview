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
    public class BookmarkList : BindableBase
    {
        private PanelListItemStyle _panelListItemStyle;

        //
        public BookmarkList(BookHub bookHub)
        {
            this.BookHub = bookHub;
        }

        //
        public PanelListItemStyle PanelListItemStyle
        {
            get { return _panelListItemStyle; }
            set { if (_panelListItemStyle != value) { _panelListItemStyle = value; RaisePropertyChanged(); } }
        }

        // サムネイル画像が表示される？？
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
        public BookHub BookHub { get; private set; }

        #region Command

        // ブックマークを戻る
        public void PrevBookmark()
        {
            if (BookHub.IsLoading) return;

            if (!BookmarkCollection.Current.CanMoveSelected(-1))
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.NotifyBookmarkPrevFailed);
                return;
            }

            var unit = BookmarkCollection.Current.MoveSelected(-1);
            if (unit != null)
            {
                BookHub.RequestLoad(unit.Value.Memento.Place, null, BookLoadOption.SkipSamePlace | BookLoadOption.IsBook, true);
            }
        }


        // ブックマークを進む
        public void NextBookmark()
        {
            if (BookHub.IsLoading) return;

            if (!BookmarkCollection.Current.CanMoveSelected(+1))
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.NotifyBookmarkNextFailed);
                return;
            }

            var unit = BookmarkCollection.Current.MoveSelected(+1);
            if (unit != null)
            {
                BookHub.RequestLoad(unit.Value.Memento.Place, null, BookLoadOption.SkipSamePlace | BookLoadOption.IsBook, true);
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
