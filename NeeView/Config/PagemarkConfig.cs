using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class PagemarkConfig : BindableBase
    {
        private bool _isVisible;
        private bool _isSelected;
        private PanelListItemStyle _panelListItemStyle;
        private bool _isSavePagemark = true;
        private string _pagemarkFilePath;
        private PagemarkOrder _pagemarkOrder;

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

        [PropertyMember("@ParamPagemarkListItemStyle")]
        public PanelListItemStyle PanelListItemStyle
        {
            get { return _panelListItemStyle; }
            set { SetProperty(ref _panelListItemStyle, value); }
        }

        [PropertyMember("@ParamIsSavePagemark")]
        public bool IsSavePagemark
        {
            get { return _isSavePagemark; }
            set { SetProperty(ref _isSavePagemark, value); }
        }

        // ページマークの保存場所
        [PropertyPath("@ParamPagemarkFilePath", FileDialogType = FileDialogType.SaveFile, Filter = "JSON|*.json")]
        public string PagemarkFilePath
        {
            get { return _pagemarkFilePath; }
            set { SetProperty(ref _pagemarkFilePath, (string.IsNullOrWhiteSpace(value) || value == SaveData.DefaultPagemarkFilePath) ? null : value); }
        }

        // ページマークの並び順
        [PropertyMember("@ParamPagemarkOrder")]
        public PagemarkOrder PagemarkOrder
        {
            get { return _pagemarkOrder; }
            set { SetProperty(ref _pagemarkOrder, value); }
        }
    }
}

