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
        static PagemarkList() => Current = new PagemarkList();
        public static PagemarkList Current { get; }


        private PanelListItemStyle _panelListItemStyle;
        private PagemarkListBoxModel _listBox;


        private PagemarkList()
        {
            _listBox = new PagemarkListBoxModel(this);
            _listBox.AddPropertyChanged(nameof(_listBox.PlaceDispString), (s, e) => RaisePropertyChanged(nameof(PlaceDispString)));

            PagemarkCollection.Current.AddPropertyChanged(nameof(PagemarkCollection.PagemarkOrder), (s, e) => RaisePropertyChanged(nameof(IsSortPath)));
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
                        return SidePanelProfile.Current.ContentItemImageWidth > 0.0;
                    case PanelListItemStyle.Banner:
                        return SidePanelProfile.Current.BannerItemImageWidth > 0.0;
                }
            }
        }

        public PagemarkListBoxModel ListBox
        {
            get { return _listBox; }
            set { SetProperty(ref _listBox, value); }
        }


        private bool _isCurrentBook;
        public bool IsCurrentBook
        {
            get { return _isCurrentBook; }
            set
            {
                if (SetProperty(ref _isCurrentBook, value))
                {
                    _listBox.UpdateItems();
                }
            }
        }

        public bool IsSortPath
        {
            get { return PagemarkCollection.Current.PagemarkOrder == PagemarkOrder.Path; }
            set { PagemarkCollection.Current.PagemarkOrder = value ? PagemarkOrder.Path : PagemarkOrder.FileName; }
        }

        public string PlaceDispString
        {
            get { return _listBox.PlaceDispString; }
        }


        public void Jump(string place, string entryName)
        {
            ListBox.Jump(place, entryName);
        }

        public void PrevPagemark()
        {
            ListBox?.PrevPagemark();
        }

        public void NextPagemark()
        {
            ListBox?.NextPagemark();
        }

        public void AddPagemark()
        {
            var pagemark = BookOperation.Current.SetPagemark(true);
            if (pagemark != null)
            {
                ListBox.SetSelectedItem(pagemark.Place, pagemark.EntryName);
            }
        }

        public void OpenAsBook()
        {
            BookHub.Current.RequestLoad(QueryScheme.Pagemark.ToSchemeString(), null, BookLoadOption.IsBook, true);
        }


        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public PanelListItemStyle PanelListItemStyle { get; set; }

            public void RestoreConfig(Config config)
            {
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.PanelListItemStyle = this.PanelListItemStyle;
            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.PanelListItemStyle = memento.PanelListItemStyle;
        }

        #endregion
    }
}
