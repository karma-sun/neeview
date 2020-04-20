using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class PageList : BindableBase
    {
        static PageList() => Current = new PageList();
        public static PageList Current { get; }


        private PageList()
        {
            ListBoxModel = new PageListBoxModel();

            BookOperation.Current.AddPropertyChanged(nameof(BookOperation.PageList), BookOperation_PageListChanged);
        }


        public event EventHandler CollectionChanging;
        public event EventHandler CollectionChanged;


        // サムネイル画像が表示される？？
        public bool IsThumbnailVisibled
        {
            get
            {
                switch (Config.Current.PageList.PanelListItemStyle)
                {
                    default:
                        return false;
                    case PanelListItemStyle.Thumbnail:
                        return true;
                    case PanelListItemStyle.Content:
                        return Config.Current.Panels.ContentItemProfile.ImageWidth > 0.0;
                    case PanelListItemStyle.Banner:
                        return Config.Current.Panels.BannerItemProfile.ImageWidth > 0.0;
                }
            }
        }

        /// <summary>
        /// ListBox の Model
        /// </summary>
        public PageListBoxModel ListBoxModel { get; set; }


        /// <summary>
        /// 配置
        /// </summary>
        public PageListPlacementService PageListPlacementService => PageListPlacementService.Current;

        /// <summary>
        /// サイドパネルでの場所表示用
        /// </summary>
        public string PlaceDispString
        {
            get { return LoosePath.GetFileName(BookOperation.Current.Address); }
        }



        private void BookOperation_PageListChanged(object sender, PropertyChangedEventArgs e)
        {
            CollectionChanging?.Invoke(this, null);

            ListBoxModel?.Unloaded();
            ListBoxModel = new PageListBoxModel();

            CollectionChanged?.Invoke(this, null);
            RaisePropertyChanged(nameof(PlaceDispString));
        }

        public void FocusAtOnce()
        {
            ListBoxModel.FocusAtOnce = true;
        }


        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public PanelListItemStyle PanelListItemStyle { get; set; }

            [DataMember]
            public PageNameFormat Format { get; set; }

            public void RestoreConfig(Config config)
            {
                config.PageList.PanelListItemStyle = PanelListItemStyle;
                config.PageList.Format = Format;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.PanelListItemStyle = Config.Current.PageList.PanelListItemStyle;
            memento.Format = Config.Current.PageList.Format;
            return memento;
        }

        #endregion Memento
    }

}
