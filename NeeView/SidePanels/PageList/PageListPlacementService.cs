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
            Config.Current.Layout.Bookshelf.AddPropertyChanged(nameof(BookshelfPanelConfig.IsPageListDocked), (s, e) =>
            {
                Update();
            });
        }


        public PageListPanel Panel
        {
            get { return _panel = _panel ?? new PageListPanel(PageList.Current); }
        }


#if false
        private bool _isPlacedInBookshelf = true;
        [PropertyMember("@ParamPageListPlacementInBookshelf", Tips = "@ParamPageListPlacementInBookshelfTips")]
        public bool IsPlacedInBookshelf
        {
            get { return _isPlacedInBookshelf; }
            set
            {
                if (SetProperty(ref _isPlacedInBookshelf, value))
                {
                    Update();
                }
            }
        }
#endif

        public void Update()
        {
            if (Config.Current.Layout.Bookshelf.IsPageListDocked)
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
                config.Layout.Bookshelf.IsPageListDocked = IsPlacedInBookshelf;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsPlacedInBookshelf = Config.Current.Layout.Bookshelf.IsPageListDocked;
            return memento;
        }

        [Obsolete]
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            ////this.IsPlacedInBookshelf = memento.IsPlacedInBookshelf;
        }

        #endregion

    }
}
