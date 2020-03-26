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
    public class HistoryList : BindableBase
    {
        static HistoryList() => Current = new HistoryList();
        public static HistoryList Current { get; }



        private HistoryList()
        {
        }

#if false
        private PanelListItemStyle _panelListItemStyle;
        public PanelListItemStyle PanelListItemStyle
        {
            get { return _panelListItemStyle; }
            set { if (_panelListItemStyle != value) { _panelListItemStyle = value; RaisePropertyChanged(); } }
        }
#endif

        public bool IsThumbnailVisibled
        {
            get
            {
                switch (Config.Current.History.PanelListItemStyle)
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


        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public PanelListItemStyle PanelListItemStyle { get; set; }

            public void RestoreConfig(Config config)
            {
                config.History.PanelListItemStyle = PanelListItemStyle;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.PanelListItemStyle = Config.Current.History.PanelListItemStyle;
            return memento;
        }

        [Obsolete]
        public void Restore(Memento memento)
        {
            //if (memento == null) return;
            //this.PanelListItemStyle = memento.PanelListItemStyle;
        }
        #endregion
    }
}
