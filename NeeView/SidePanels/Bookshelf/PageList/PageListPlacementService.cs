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
        public static PageListPlacementService Current { get; } = new PageListPlacementService();

        private PageListPanel _panel;
        private bool _isPlacedInBookshelf;

        public PageListPlacementService()
        {
            _panel = new PageListPanel(PageList.Current);
            _isPlacedInBookshelf = true;
        }

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

        public void Update()
        {
            if (_isPlacedInBookshelf)
            {
                SidePanel.Current?.DetachPageListPanel();
                FolderPanelModel.Current?.SetVisual(_panel.View);
            }
            else
            {
                FolderPanelModel.Current?.SetVisual(null);
                SidePanel.Current?.AttachPageListPanel(_panel);
            }
        }

        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember, DefaultValue(true)]
            public bool IsPlacedInBookshelf { get; set; }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsPlacedInBookshelf = this.IsPlacedInBookshelf;
            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.IsPlacedInBookshelf = memento.IsPlacedInBookshelf;
        }

        #endregion

    }
}
