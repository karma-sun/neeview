using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class PageListConfig : BindableBase
    {
        private PanelListItemStyle _panelListItemStyle = PanelListItemStyle.Content;
        private PageNameFormat _format = PageNameFormat.Smart;


        /// <summary>
        /// ページリストのリスト項目表示形式
        /// </summary>
        [PropertyMember]
        public PanelListItemStyle PanelListItemStyle
        {
            get { return _panelListItemStyle; }
            set { SetProperty(ref _panelListItemStyle, value); }
        }

        /// <summary>
        /// ページ名表示形式
        /// </summary>
        [PropertyMember]
        public PageNameFormat Format
        {
            get { return _format; }
            set { SetProperty(ref _format, value); }
        }
    }

}



