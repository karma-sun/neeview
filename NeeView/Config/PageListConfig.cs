using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class PageListConfig : BindableBase
    {
        private bool _isVisible;
        private bool _isSelected;
        private PanelListItemStyle _panelListItemStyle;
        private PageNameFormat _format = PageNameFormat.Smart;

        [JsonIgnore]
        [PropertyMapReadOnly]
        [PropertyMember("@WordIsPanelVisible")]
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }

        [JsonIgnore]
        [PropertyMember("@WordIsPanelSelected")]
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        /// <summary>
        /// ページリストのリスト項目表示形式
        /// </summary>
        [PropertyMember("@ParamPageListItemStyle")]
        public PanelListItemStyle PanelListItemStyle
        {
            get { return _panelListItemStyle; }
            set { SetProperty(ref _panelListItemStyle, value); }
        }

        /// <summary>
        /// ページ名表示形式
        /// </summary>
        [PropertyMember("@ParamPageListFormat")]
        public PageNameFormat Format
        {
            get { return _format; }
            set { SetProperty(ref _format, value); }
        }
    }

}



