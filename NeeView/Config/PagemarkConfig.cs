using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class PagemarkConfig : BindableBase, IHasPanelListItemStyle
    {
        private PanelListItemStyle _panelListItemStyle = PanelListItemStyle.Content;
        private bool _isSavePagemark = true;
        private PagemarkOrder _pagemarkOrder;

        [JsonInclude, JsonPropertyName(nameof(PagemarkFilePath))]
        public string _pagemarkFilePath;


        [PropertyMember]
        public PanelListItemStyle PanelListItemStyle
        {
            get { return _panelListItemStyle; }
            set { SetProperty(ref _panelListItemStyle, value); }
        }

        [PropertyMember]
        public bool IsSavePagemark
        {
            get { return _isSavePagemark; }
            set { SetProperty(ref _isSavePagemark, value); }
        }

        // ページマークの保存場所
        [JsonIgnore]
        [PropertyPath(FileDialogType = FileDialogType.SaveFile, Filter = "JSON|*.json")]
        public string PagemarkFilePath
        {
            get { return _pagemarkFilePath ?? SaveData.DefaultPagemarkFilePath; }
            set { SetProperty(ref _pagemarkFilePath, (string.IsNullOrWhiteSpace(value) || value.Trim() == SaveData.DefaultPagemarkFilePath) ? null : value.Trim()); }
        }

        // ページマークの並び順
        [PropertyMember]
        public PagemarkOrder PagemarkOrder
        {
            get { return _pagemarkOrder; }
            set { SetProperty(ref _pagemarkOrder, value); }
        }
    }
}

