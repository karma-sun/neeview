using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class PageListPlacementService : BindableBase
    {
        static PageListPlacementService() => Current = new PageListPlacementService();
        public static PageListPlacementService Current { get; }


        private PageListPanel _panel;

        private PageListPlacementService()
        {
            Config.Current.Bookshelf.AddPropertyChanged(nameof(BookshelfConfig.IsPageListDocked), (s, e) =>
            {
                Update();
            });
        }


        public PageListPanel Panel
        {
            get { return _panel = _panel ?? new PageListPanel(PageList.Current); }
        }


        public void Update()
        {
            if (Config.Current.Bookshelf.IsPageListDocked)
            {
                SidePanel.Current?.DetachPageListPanel();
                FolderPanelModel.Current?.SetVisual(Panel.View);
            }
            else
            {
                FolderPanelModel.Current?.SetVisual(null);
                SidePanel.Current?.AttachPageListPanel(Panel);
            }
        }

        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember, DefaultValue(true)]
            public bool IsPlacedInBookshelf { get; set; }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            public void RestoreConfig(Config config)
            {
                config.Bookshelf.IsPageListDocked = IsPlacedInBookshelf;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsPlacedInBookshelf = Config.Current.Bookshelf.IsPageListDocked;
            return memento;
        }

        #endregion

    }
}
