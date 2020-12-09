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


        private PagemarkListBoxModel _listBox;


        private PagemarkList()
        {
            _listBox = new PagemarkListBoxModel(this);
            _listBox.AddPropertyChanged(nameof(_listBox.PlaceDispString), (s, e) => RaisePropertyChanged(nameof(PlaceDispString)));

            Config.Current.Pagemark.AddPropertyChanged(nameof(PagemarkConfig.PagemarkOrder), (s, e) => RaisePropertyChanged(nameof(IsSortPath)));
        }


        public bool IsThumbnailVisibled
        {
            get
            {
                switch (Config.Current.Pagemark.PanelListItemStyle)
                {
                    default:
                        return false;
                    case PanelListItemStyle.Content:
                        return Config.Current.Panels.ContentItemProfile.ImageWidth > 0.0;
                    case PanelListItemStyle.Banner:
                        return Config.Current.Panels.BannerItemProfile.ImageWidth > 0.0;
                }
            }
        }

        public PanelListItemStyle PanelListItemStyle
        {
            get => Config.Current.Pagemark.PanelListItemStyle;
            set => Config.Current.Pagemark.PanelListItemStyle = value;
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
            get { return Config.Current.Pagemark.PagemarkOrder == PagemarkOrder.Path; }
            set { Config.Current.Pagemark.PagemarkOrder = value ? PagemarkOrder.Path : PagemarkOrder.FileName; }
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
                ListBox.SetSelectedItem(pagemark.Path, pagemark.EntryName);
            }
        }

        public void OpenAsBook()
        {
            BookHub.Current.RequestLoad(this, QueryScheme.Pagemark.ToSchemeString(), null, BookLoadOption.IsBook, true);
        }


        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public PanelListItemStyle PanelListItemStyle { get; set; }

            public void RestoreConfig(Config config)
            {
                config.Pagemark.PanelListItemStyle = PanelListItemStyle;
            }
        }

        #endregion
    }
}
