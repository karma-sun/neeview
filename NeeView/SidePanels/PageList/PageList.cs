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


        private PanelListItemStyle _panelListItemStyle;
        private PageNameFormat _format = PageNameFormat.Smart;


        private PageList()
        {
            ListBoxModel = new PageListBoxModel();

            BookOperation.Current.AddPropertyChanged(nameof(BookOperation.PageList), BookOperation_PageListChanged);
        }


        public event EventHandler CollectionChanging;
        public event EventHandler CollectionChanged;


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
                    case PanelListItemStyle.Thumbnail:
                        return true;
                    case PanelListItemStyle.Content:
                        return SidePanelProfile.Current.ContentItemImageWidth > 0.0;
                    case PanelListItemStyle.Banner:
                        return SidePanelProfile.Current.BannerItemImageWidth > 0.0;
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
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.PanelListItemStyle = this.PanelListItemStyle;
            memento.Format = this.Format;
            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.PanelListItemStyle = memento.PanelListItemStyle;
            this.Format = memento.Format;
        }
    }

    #endregion Memento
}
