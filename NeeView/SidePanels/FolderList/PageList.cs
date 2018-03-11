using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class PageList : BindableBase
    {
        #region Fields

        private PanelListItemStyle _panelListItemStyle;
        private PageNameFormat _format = PageNameFormat.Smart;

        #endregion

        #region Constructors

        public PageList(BookHub bookHub, BookOperation bookOperation)
        {
            this.BookHub = bookHub;
            this.BookOperation = bookOperation;

            this.BookOperation.AddPropertyChanged(nameof(BookOperation.PageList),
                (s, e) => RaisePropertyChanged(nameof(PageCollection)));
        }

        #endregion

        #region Properties

        /// <summary>
        /// ページリストのリスト項目表示形式
        /// </summary>
        public PanelListItemStyle PanelListItemStyle
        {
            get { return _panelListItemStyle; }
            set { if (_panelListItemStyle != value) { _panelListItemStyle = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// ページ名表示形式
        /// </summary>
        public PageNameFormat Format
        {
            get { return _format; }
            set { _format = value; RaisePropertyChanged(); }
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

        // ページリスト(表示部用)
        public ObservableCollection<Page> PageCollection => BookOperation.PageList;

        /// <summary>
        /// 一度だけフォーカスするフラグ
        /// </summary>
        public bool FocusAtOnce { get; set; }

        //
        public BookHub BookHub { get; private set; }

        //
        public BookOperation BookOperation { get; private set; }

        #endregion

        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public PanelListItemStyle PanelListItemStyle { get; set; }

            [DataMember]
            public PageNameFormat Format { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.PanelListItemStyle = this.PanelListItemStyle;
            memento.Format = this.Format;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.PanelListItemStyle = memento.PanelListItemStyle;
            this.Format = memento.Format;
        }
        #endregion
    }
}
